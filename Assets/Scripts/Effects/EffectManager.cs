using System;
using System.Collections;
using System.Collections.Generic;
using AI.Interfaces;
using Cards.Interfaces;
using Coroutine.Interfaces;
using Effects.Interfaces;
using Field.Interfaces;
using FinishLine.Interfaces;
using Mana.Interfaces;
using Network.Interfaces;
using Players.Interfaces;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class EffectManager : MonoBehaviour, IEffectService, ISerializableEffects, IFreezeEffectService
    {
        #region Dependency

        private IHandPoolView _handPoolView;
        private IPlayerService _playerService;
        private IAIService _aiService;
        private ICoroutineAwaitService _coroutineAwaitService;
        private ICoroutineService _coroutineService;
        private IEffectEventNetworkService _effectEventNetworkService;
        private IFinishLineService _finishLineService;
        private IManaEventNetworkService _manaEventNetworkService;
        private IFieldService _fieldService;
        private IFieldFigureService _fieldFigureService;
        private IManaService _manaService;
        private IManaUIService _manaUIService;

        [Inject]
        private void Construct(
            IHandPoolView handPool,
            IPlayerService playerService,
            IAIService aiService,
            ICoroutineService coroutineService,
            ICoroutineAwaitService coroutineAwaitService,
            IEffectEventNetworkService effectEventNetworkService,
            IFinishLineService finishLineService,
            IManaEventNetworkService manaEventNetworkService,
            IFieldService fieldService,
            IFieldFigureService fieldFigureService,
            IManaService manaService,
            IManaUIService manaUIService)
        {
            _handPoolView = handPool;
            _playerService = playerService;
            _aiService = aiService;
            _coroutineService = coroutineService;
            _coroutineAwaitService = coroutineAwaitService;
            _effectEventNetworkService = effectEventNetworkService;
            _finishLineService = finishLineService;
            _manaEventNetworkService = manaEventNetworkService;
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
            _manaService = manaService;
            _manaUIService = manaUIService;
        }

        #endregion

        private readonly List<Effect> _effectList = new();

        private readonly List<Action> _serializedActions = new();

        private readonly List<Cell> _freezeCellList = new ();
        private readonly List<Effect> _freezeEffectList = new ();

        private void Awake()
        {
            _serializedActions.Add(AddBonusMana_Effect);
            _serializedActions.Add(AddFigure_Effect);
            _serializedActions.Add(Decrease2MaxMana_Effect);
            _serializedActions.Add(DecreaseIncrease2Mana_Effect);
            //_serializedActions.Add(FreezeCell_Effect);
            _serializedActions.Add(Increase2MaxMana_Effect);
            _serializedActions.Add(Random2Mana_Effect);
            _serializedActions.Add(Freeze3Cell_Effected);
        }

        public List<Effect> GetEffectList()
        {
            return _effectList;
        }

        public void AddEffect(Effect effect)
        {
            _effectList.Add(effect);
        }

        public void AddEffect(int effect)
        {
            _serializedActions[effect].Invoke();
        }

        public void ClearEffect()
        {
            _effectList.Clear();
        }

        public void ClearEffect(int id)
        {
            _effectList.RemoveAt(id);
        }

        public void UpdateEffectState(int id, int value)
        {
            _effectList[id].EffectTurnCount = value;
        }

        public int GetIdSerializableAction(Action action)
        {
            return _serializedActions.IndexOf(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effects"></param>
        /// <returns></returns>
        public IEnumerator UpdateEffectTurn(List<Effect> effects = null)
        {
            List<Effect> effectList = effects;
            if (effects == null)
            {
                Debug.Log("Effect: create list");
                effectList = _effectList.FindAll(x => x.EffectSideId == _playerService.GetCurrentPlayer().SideId);
                Debug.Log(_playerService.GetCurrentPlayer().EntityType);
                Debug.Log("Effect: list count" + effectList.Count);
                effectList.Sort(delegate(Effect effect1, Effect effect2)
                {
                    return (effect1.EffectPriority >= effect2.EffectPriority) ? 1 : -1;
                });
            }

            float maxTime = 0;
            if (effectList.Count != 0)
            {
                int lastPriority = effectList[0].EffectPriority;
                while (effectList.Count > 0)
                {
                    bool isAwaitNeed = lastPriority != effectList[0].EffectPriority;
                    lastPriority = effectList[0].EffectPriority;
                    if (isAwaitNeed)
                    {
                        //Выполняется при переходе на следующий уровень приоритета
                        _coroutineService.AddCoroutine(IEffectAwaitAsync(effectList, maxTime));
                        yield break;
                    }

                    Effect effect = effectList[0];
                    Debug.Log($"Effect {effect}. Priority {effect.EffectPriority}. Effect type {effect.EffectType}");
                    ActivateEffect(effect);
                    effectList.Remove(effect);
                    if (effect.EffectType == Effect.EffectTypes.Parallel)
                    {
                        maxTime = Mathf.Max(maxTime, effect.EffectTimeAction,
                            (effect.EffectTurnCount == 0) ? effect.EffectTimeDisable : 0);
                    }
                    else
                    {
                        _coroutineService.AddCoroutine(IEffectAwaitAsync(effectList,
                            Mathf.Max(effect.EffectTimeAction,
                                (effect.EffectTurnCount == 0) ? effect.EffectTimeDisable : 0)));
                        yield break;
                    }
                }
            }

            int i = 0;
            while (i < _effectList.Count)
            {
                if (_effectList[i].EffectTurnCount == 0)
                {
                    _effectList.RemoveAt(i);
                    _effectEventNetworkService.RaiseEventClearEffect(i);
                }
                else
                {
                    i += 1;
                }
            }

            yield return _coroutineAwaitService.AwaitTime(maxTime);
            _finishLineService.MasterChecker(_playerService.GetCurrentPlayer().SideId, isInQueue: true);
            _handPoolView.UpdateCardUI();
        }

        private IEnumerator IEffectAwaitAsync(List<Effect> effects, float startAwait = 0)
        {
            yield return _coroutineAwaitService.AwaitTime(startAwait);
            _coroutineService.AddCoroutine(UpdateEffectTurn(effects));
        }

        private void ActivateEffect(Effect effect)
        {
            effect.EffectTurnCount -= 1;
            _effectEventNetworkService.RaiseEventUpdateEffect(_effectList.IndexOf(effect), effect.EffectTurnCount);
            Debug.Log("Effect turn delete");


            effect.EffectAction.Invoke();
            if (effect.EffectTurnCount == 0) effect.OnEffectDisable.Invoke();

            Debug.Log("Effect Invoke");
        }

        public void AddFigure_Effect()
        {
            Action f = delegate
            {
                Vector2Int position = _aiService.GenerateRandomPosition(_fieldService.GetFieldSize());
                if (position == new Vector2Int(-1, -1)) return;

                _fieldFigureService.PlaceInCell(position);
            };
            Effect effect = new Effect(f, 3, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Consistently,
                1);
            AddEffect(effect);
        }

        public void AddBonusMana_Effect()
        {
            Action f = delegate
            {
                _manaService.AddBonusMana(1);
                _manaEventNetworkService.RaiseEventAddBonusMana(1);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };
            Effect effect = new Effect(f, 1, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0);
            AddEffect(effect);
        }

        public void Decrease2MaxMana_Effect()
        {
            Action d = delegate
            {
                _manaService.IncreaseMaxMana(2);
                _manaEventNetworkService.RaiseEventIncreaseMaxMana(2);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };
            Effect effect = new Effect(delegate { }, 3, _playerService.GetCurrentPlayer().SideId,
                Effect.EffectTypes.Parallel, 0, d);
            AddEffect(effect);
        }

        public void Increase2MaxMana_Effect()
        {
            Action d = delegate
            {
                _manaService.IncreaseMaxMana(-2);
                _manaEventNetworkService.RaiseEventIncreaseMaxMana(-2);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };
            Effect effect = new Effect(delegate { }, 3, _playerService.GetCurrentPlayer().SideId,
                Effect.EffectTypes.Parallel, 0, d);
            AddEffect(effect);
        }

        public void Random2Mana_Effect()
        {
            Action f = delegate
            {
                int randValue = UnityEngine.Random.Range(0, 2) == 0 ? -2 : 2;
                _manaService.AddBonusMana(randValue);
                _manaEventNetworkService.RaiseEventAddBonusMana(randValue);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };
            Effect effect = new Effect(f, 1, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0);
            AddEffect(effect);
        }

        public void DecreaseIncrease2Mana_Effect()
        {
            Action f = delegate
            {
                _manaService.AddBonusMana(-2);
                _manaEventNetworkService.RaiseEventAddBonusMana(-2);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };
            Action d = delegate
            {
                _manaService.AddBonusMana(4);
                _manaEventNetworkService.RaiseEventAddBonusMana(4);

                _manaService.RestoreAllMana();
                _manaUIService.UpdateManaUI();
            };


            Effect effect = new Effect(f, 2, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0,
                d);
            AddEffect(effect);
        }

        public void FreezeCell_Effect(Vector2Int id)
        {
            Cell cell = _fieldService.GetCellLink(id);
            Action f = delegate { };
            Action d = delegate
            {
                _fieldFigureService.ResetSubStateWithPlaceFigure(cell.Id);
                _freezeEffectList.RemoveAt(_freezeCellList.IndexOf(cell));
                _freezeCellList.Remove(cell);
                
            };
            f.Invoke();
            Effect effect = new Effect(f, 2, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Consistently,
                2,
                d, Cell.AnimationTime, Cell.AnimationTime * 2);
            _freezeCellList.Add(cell);
            _freezeEffectList.Add(effect);
            Debug.Log($"EFFECT FreezeList {_freezeEffectList.Count}. CellList {_freezeCellList.Count}");
            AddEffect(effect);
        }

        public void Freeze3Cell_Effected()
        {
            Action f = delegate
            {
                Vector2Int position = _aiService.GenerateRandomPosition(_fieldService.GetFieldSize());
                if (position == new Vector2Int(-1, -1)) return;

                _fieldFigureService.FreezeCell(position);
                /**/
            };

            Effect effect = new Effect(f, 3, _playerService.GetCurrentPlayer().SideId, Effect.EffectTypes.Consistently,
                3,
                null, Cell.AnimationTime, Cell.AnimationTime);
            AddEffect(effect);
        }

        public List<Cell> GetFreezeCell()
        {
            return _freezeCellList;
        }
        
        public List<Effect> GetFreezeEffect()
        {
            return _freezeEffectList;
        }

        public void ReleaseFreeze(Cell cell)
        {
            Debug.Log($"FreezeList {_freezeEffectList.Count}. CellList {_freezeCellList.Count}");
            Effect effect = _freezeEffectList[_freezeCellList.IndexOf(cell)];
            effect.OnEffectDisable?.Invoke();
            _freezeEffectList.Remove(effect);
            _freezeCellList.Remove(cell);
            _effectEventNetworkService.RaiseEventClearEffect(_effectList.IndexOf(effect));
            _effectList.Remove(effect);
        }

        public void ReleaseFreeze(Effect effect)
        {
            effect.OnEffectDisable?.Invoke();
            _freezeCellList.RemoveAt(_freezeEffectList.IndexOf(effect));
            _freezeEffectList.Remove(effect);
            _effectEventNetworkService.RaiseEventClearEffect(_effectList.IndexOf(effect));
            _effectList.Remove(effect);
        }
    }

    public class Effect
    {
        public Action EffectAction;
        public Action OnEffectDisable;
        public int EffectTurnCount;
        public int EffectSideId;
        public float EffectTimeAction;
        public float EffectTimeDisable;
        public EffectTypes EffectType;
        public int EffectPriority;


        public enum EffectTypes
        {
            Parallel,
            Consistently
        }

        public Effect(Action action, int turnCount, int sideId, EffectTypes effectType, int priority,
            Action onDisable = null, float effectTime = 0, float effectTimeDisable = 0)
        {
            if (onDisable == null) onDisable = delegate() { };

            EffectAction = action;
            OnEffectDisable = onDisable;
            EffectTurnCount = turnCount;
            EffectSideId = sideId;

            EffectTimeAction = effectTime;
            EffectTimeDisable = effectTimeDisable;
            EffectType = effectType;
            EffectPriority = priority;
        }
    }
}