using System;
using System.Collections;
using System.Collections.Generic;
using Coroutine.Interfaces;
using Field.Interfaces;
using FinishLine.Interfaces;
using ScreenScaler.Interfaces;
using Score.Interfaces;
using UnityEngine;
using Zenject;

namespace FinishLine
{
    public class FinishLineManager : MonoBehaviour, IFinishLineService
    {
        private const float LINE_WIDTH_PERCENT = 0.2f;

        [SerializeField]
        private Transform _lineParent;

        private readonly List<List<Vector2Int>> _lineForClearing = new List<List<Vector2Int>>();
        private readonly List<FinishLineObject> _lineFinishEnabled = new();

        private Action _networkEventAction;
        private Action<GameplayState> _newGameplayStateAction;
        private Predicate<GameplayState> _isGameplayEqual;

        #region Dependency

        private ICoroutineService _coroutineService;
        private ICoroutineAwaitService _coroutineAwaitService;
        private IFieldService _fieldService;
        private IFieldFigureService _fieldFigureService;
        private IScreenScaler _screenScaler;
        private IFinishLineFactoryService _finishLineFactoryService;
        private IFinishLineMasterCheckerService _finishLineMasterCheckerService;
        private ICellClearAnimationService _cellClearAnimationService;
        private IScoreWinnerService _scoreWinnerService;

        [Inject]
        private void Construct(
            ICoroutineService coroutineService,
            ICoroutineAwaitService coroutineAwaitService,
            IFieldService fieldService,
            IFieldFigureService fieldFigureService,
            IScreenScaler screenScaler,
            IFinishLineFactoryService finishLineFactoryService,
            IFinishLineMasterCheckerService finishLineMasterCheckerService,
            ICellClearAnimationService cellClearAnimationService,
            IScoreWinnerService scoreWinnerService)
        {
            _coroutineService = coroutineService;
            _coroutineAwaitService = coroutineAwaitService;
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
            _screenScaler = screenScaler;
            _finishLineFactoryService = finishLineFactoryService;
            _finishLineMasterCheckerService = finishLineMasterCheckerService;
            _cellClearAnimationService = cellClearAnimationService;
            _scoreWinnerService = scoreWinnerService;
        }

        #endregion


        public void MasterChecker(int figure, bool isInQueue = true, bool isNeedEvent = true)
        {
            _ = isInQueue;
            _coroutineService.AddCoroutine(MasterCheckerCoroutine((CellFigure) figure, isNeedEvent));
        }

        public void SetNetworkEventAction(Action action)
        {
            _networkEventAction = action;
        }

        public void SetNewGameState(Action<GameplayState> action)
        {
            _newGameplayStateAction = action;
        }

        public void SetPredicateIsEqualGameState(Predicate<GameplayState> action)
        {
            _isGameplayEqual = action;
        }

        private IEnumerator MasterCheckerCoroutine(CellFigure figure, bool isNeedEvent)
        {
            if (isNeedEvent) _networkEventAction?.Invoke();
            List<List<Vector2Int>> linesRes =
                _finishLineMasterCheckerService.FindLinesToClear(figure, _lineForClearing);

            if (linesRes.Count > 0)
            {
                foreach (List<Vector2Int> line in linesRes)
                {
                    _lineForClearing.Add(line);
                    foreach (Vector2Int cell in line)
                    {
                        _fieldFigureService.SetIsCellClear(cell, true);
                    }
                }

                yield return StartCoroutine(PlayClearAnimation(linesRes));
            }
            else
            {
                if (!_fieldService.IsExistEmptyCell() && _lineForClearing.Count == 0 &&
                    !_isGameplayEqual(GameplayState.GameOver))
                {
                    _newGameplayStateAction(GameplayState.RoundOver);
                }
            }

            yield return null;
        }

        private IEnumerator PlayClearAnimation(List<List<Vector2Int>> lines)
        {
            foreach (List<Vector2Int> line in lines)
            {
                foreach (Vector2Int cell in line)
                {
                    _fieldFigureService.SetIsCellClear(cell, false);
                }
            }

            yield return StartCoroutine(_cellClearAnimationService.Play(lines, DrawFinishLine));

            foreach (List<Vector2Int> line in lines)
            {
                _lineForClearing.Remove(line);
            }

            yield return _coroutineAwaitService.AwaitTime(_cellClearAnimationService.AnimationFrames);

            if (_scoreWinnerService.IsExistRoundWinner() &&
                !_isGameplayEqual(GameplayState.GameOver))
            {
                _newGameplayStateAction(GameplayState.RoundOver);
            }
        }

        public void DrawFinishLine(List<Vector2Int> ids, int score = 0)
        {
            _ = score;
            if (_lineFinishEnabled.Count == 0) CreateFinishLine();
            FinishLineObject fl = _lineFinishEnabled[0];
            _lineFinishEnabled.Remove(fl);
            fl.SetAlphaFinishLine(0);
            fl.SetWidthScreenCord(_fieldService.GetCellSize() * LINE_WIDTH_PERCENT / _screenScaler.GetWidthRatio());
            fl.SetPositions(_fieldService.GetCellPosition(ids[0]), _fieldService.GetCellPosition(ids[^1]));
            StartCoroutine(PlayLineVisual(fl));
        }

        private void CreateFinishLine()
        {
            FinishLineObject lr = _finishLineFactoryService.InstantiateFinishLine();
            lr.SetTransformParent(_lineParent.transform);
            lr.SetPositions(Vector2Int.zero, Vector2.zero);
            _lineFinishEnabled.Add(lr);
        }

        private IEnumerator PlayLineVisual(FinishLineObject finishLineObject)
        {
            float frame = 0f;
            while (frame < FinishLineObject.FINISH_COUNT_FRAME)
            {
                frame++;
                finishLineObject.SetAlphaFinishLine(frame / FinishLineObject.FINISH_COUNT_FRAME);
                yield return null;
            }

            while (frame > 0f)
            {
                frame--;
                finishLineObject.SetAlphaFinishLine(frame / FinishLineObject.FINISH_COUNT_FRAME);
                yield return null;
            }

            _lineFinishEnabled.Add(finishLineObject);
        }
    }
}