using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{

    public class EffectManager : Singleton<EffectManager>
    {

        public static bool IsEffectManagerDone = true;

        private List<Effect> _effectList = new List<Effect>();

        private List<Action> _serializedActions = new List<Action>();

        private void Awake()
        {
            _serializedActions.Add(AddBonusMana_Effect);
            _serializedActions.Add(AddFigure_Effect);
            _serializedActions.Add(Decrease2MaxMana_Effect);
            _serializedActions.Add(DecreaseIncrease2Mana_Effect);
            //actions.Add(FreezeCell_Effect);
            _serializedActions.Add(Increase2MaxMana_Effect);
            _serializedActions.Add(Random2Mana_Effect);
            _serializedActions.Add(Freeze3Cell_Effected);
            
        }

        public List<Effect> EffectList
        {
            get => _effectList;
            set => _effectList = value;
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

        public int GetIdSerializibleAction(Action action)
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
                effectList = _effectList.FindAll(x => x.EffectSideId == PlayerManager.Instance.GetCurrentPlayer().SideId);
                Debug.Log(PlayerManager.Instance.GetCurrentPlayer().EntityType);
                Debug.Log("Effect: list count" + effectList.Count);
                effectList.Sort(delegate (Effect effect1, Effect effect2)
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
                    Debug.Log("Effect: effect prior" + effectList[0].EffectPriority);
                    bool isAwaitNeed = lastPriority != effectList[0].EffectPriority;
                    lastPriority = effectList[0].EffectPriority;
                    if (isAwaitNeed)
                    {
                        //����������� ��� �������� �� ��������� ������� ����������
                        StartCoroutine(IEffectAwaitAsync(effectList, maxTime));
                        yield break;
                    }
                    Effect effect = effectList[0];
                    ActivateEffect(effect);
                    effectList.Remove(effect);
                    if (effect.EffectType == Effect.EffectTypes.Parallel)
                    {
                        maxTime = Mathf.Max(maxTime, effect.EffectTimeAction, (effect.EffectTurnCount == 0) ? effect.EffectTimeDisable : 0);
                    }
                    else
                    {
                        StartCoroutine(IEffectAwaitAsync(effectList));
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
                    NetworkEventManager.RaiseEventClearEffect(i);
                }
                else
                {
                    i+=1;
                }
            }
            yield return StartCoroutine(CoroutineManager.Instance.IAwaitProcess(maxTime));
            FinishLineManager.Instance.MasterChecker(PlayerManager.Instance.GetCurrentPlayer().SideId);
        }

        private IEnumerator IEffectAwaitAsync(List<Effect> effects, float startAwait = 0)
        {
            yield return StartCoroutine(CoroutineManager.Instance.IAwaitProcess(startAwait));
            while (!CoroutineManager.IsQueueEmpty) yield return null;
            CoroutineManager.Instance.AddCoroutine(UpdateEffectTurn(effects));
        }

        private void ActivateEffect(Effect effect)
        {

            effect.EffectTurnCount -= 1;
            NetworkEventManager.RaiseEventUpdateEffect(_effectList.IndexOf(effect), effect.EffectTurnCount);
            Debug.Log("Effect turn delete");


            effect.EffectAction.Invoke();
            if (effect.EffectTurnCount == 0) effect.OnEffectDisable.Invoke();

            Debug.Log("Effect Invoke");

        }

        public void AddFigure_Effect()
        {
            Action f = delegate ()
            {
                Vector2Int position = AIManager.Instance.GenerateRandomPosition(Field.Instance.FieldSize);
                if (position == new Vector2Int(-1, -1)) return;

                Field.Instance.PlaceInCell(position);

            };
            Effect effect = new Effect(f, 3, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Consistently, 1);
            AddEffect(effect);
        }

        public void AddBonusMana_Effect()
        {
            Action f = delegate ()
            {
                ManaManager.Instance.AddBonusMana(1);
                NetworkEventManager.RaiseEventAddBonusMana(1);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };
            Effect effect = new Effect(f, 1, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0);
            AddEffect(effect);
        }

        public void Decrease2MaxMana_Effect()
        {
            Action d = delegate ()
            {
                ManaManager.Instance.IncreaseMaxMana(2);
                NetworkEventManager.RaiseEventIncreaseMaxMana(2);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };
            Effect effect = new Effect(delegate () { }, 3, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0, d);
            AddEffect(effect);
        }
        public void Increase2MaxMana_Effect()
        {
            Action d = delegate ()
            {
                ManaManager.Instance.IncreaseMaxMana(-2);
                NetworkEventManager.RaiseEventIncreaseMaxMana(-2);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };
            Effect effect = new Effect(delegate () { }, 3, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0, d);
            AddEffect(effect);
        }

        public void Random2Mana_Effect()
        {
            Action f = delegate ()
            {
                int randValue = UnityEngine.Random.Range(0, 2) == 0 ? -2 : 2;
                ManaManager.Instance.AddBonusMana(randValue);
                NetworkEventManager.RaiseEventAddBonusMana(randValue);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };
            Effect effect = new Effect(f, 1, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0);
            AddEffect(effect);
        }

        public void DecreaseIncrease2Mana_Effect()
        {
            Action f = delegate ()
            {
                ManaManager.Instance.AddBonusMana(-2);
                NetworkEventManager.RaiseEventAddBonusMana(-2);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };
            Action d = delegate ()
            {


                ManaManager.Instance.AddBonusMana(4);
                NetworkEventManager.RaiseEventAddBonusMana(4);

                ManaManager.Instance.RestoreAllMana();
                ManaManager.Instance.UpdateManaUI();
            };


            Effect effect = new Effect(f, 2, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 0, d);
            AddEffect(effect);
        }

        public void FreezeCell_Effect(Vector2Int id)
        {
            Cell cell = Field.Instance.CellList[id.x][id.y];
            Action f = delegate ()
            {
            };
            Action d = delegate ()
            {
                Field.Instance.ResetSubStateWithPlaceFigure(cell.Id);
            };
            f.Invoke();
            Effect effect = new Effect(f, 2, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 2, d, Cell.AnimationTime, Cell.AnimationTime * 2);
            AddEffect(effect);
        }

        public void Freeze3Cell_Effected()
        {
            Action f = delegate ()
            {
                Vector2Int position = AIManager.Instance.GenerateRandomPosition(Field.Instance.FieldSize);
                if (position == new Vector2Int(-1, -1)) return;

                Field.Instance.FreezeCell(position);

                FreezeCell_Effect(position);
                NetworkEventManager.RaiseEventAddFreezeEffect(position);
                /**/
            };

            Effect effect = new Effect(f, 3, PlayerManager.Instance.GetCurrentPlayer().SideId, Effect.EffectTypes.Parallel, 3, null, Cell.AnimationTime);
            EffectManager.Instance.AddEffect(effect);
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

        public Effect(Action action, int turnCount, int SideId, EffectTypes effectType, int priority, Action onDisable = null, float effectTime = 0, float effectTimeDisable = 0)
        {
            if (onDisable == null) onDisable = delegate () { };

            EffectAction = action;
            OnEffectDisable = onDisable;
            EffectTurnCount = turnCount;
            EffectSideId = SideId;

            EffectTimeAction = effectTime;
            EffectTimeDisable = effectTimeDisable;
            EffectType = effectType;
            EffectPriority = priority;
        }
    }
}