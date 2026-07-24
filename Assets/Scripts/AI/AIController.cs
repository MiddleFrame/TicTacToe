using System;
using System.Collections;
using AI.Interfaces;
using Cards.CustomType;
using Coroutine.Interfaces;
using Field.Interfaces;
using FinishLine.Interfaces;
using History.Interfaces;
using Players.Interfaces;
using UnityEngine;
using Zenject;
using Roguelike;
using Roguelike.Interfaces;
using Random = UnityEngine.Random;

namespace AI
{
    public class AIController : MonoBehaviour, IAIService
    {
        private BotGameType _botAggression;
        [SerializeField]
        private CardInfo _botCardDefault;

        #region Dependency

        private IPlayerService _playerService;
        private IHistoryService _historyService;
        private IFinishLineService _finishLineService;
        private IFieldService _fieldService;
        private IFieldFigureService _fieldFigureService;
        private ICoroutineService _coroutineService;
        private IRoguelikeRunService _roguelikeRunService;


        [Inject]
        private void Construct(
            IPlayerService playerService,
            IHistoryService historyService,
            IFinishLineService finishLineService,
            IFieldService fieldService,
            IFieldFigureService fieldFigureService,
            ICoroutineService coroutineService,
            IRoguelikeRunService roguelikeRunService)
        {
            _playerService = playerService;
            _historyService = historyService;
            _finishLineService = finishLineService;
            _fieldService = fieldService;
            _fieldFigureService = fieldFigureService;
            _coroutineService = coroutineService;
            _roguelikeRunService = roguelikeRunService;
        }

        #endregion

        private Vector2Int GenerateNewTurn(int sideId,int playerScore, int botScore)
        {
            _botAggression = botScore >playerScore ? BotGameType.Attack : BotGameType.Defense;

            Vector2Int maxId = new Vector2Int(-1, -1);
            int maxValue = -1;

            for (int x = 0; x < _fieldService.GetFieldSize().x; x++)
            {
                for (int y = 0; y < _fieldService.GetFieldSize().y; y++)
                {
                    Vector2Int currentId = new Vector2Int(x, y);
                    if (!_fieldFigureService.IsCellEnableToPlace(currentId)) continue;

                    int currentValue = GetCellValue(currentId, sideId);
                    if (currentValue == maxValue)
                    {
                        int randomId = Random.Range(0, 2);
                        maxId = (randomId == 0) ? maxId : currentId;
                        continue;
                    }

                    if (currentValue > maxValue)
                    {
                        maxValue = currentValue;
                        maxId = currentId;
                    }
                }
            }

            return maxId;
        }

        public Vector2Int GenerateRandomPosition(Vector2Int size)
        {
            if (!_fieldService.IsExistEmptyCell()) return new Vector2Int(-1, -1);
            Vector2Int result = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
            while (!_fieldFigureService.IsCellEnableToPlace(result))
            {
                result = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
            }

            return result;
        }

        private int GetCellValue(Vector2Int currentId, int sideId)
        {
            int allyValue = (_botAggression == BotGameType.Defense) ? 3 : 2;
            int enemyValue = (_botAggression == BotGameType.Attack) ? 3 : 2;

            int resultValue = 0;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Cell nextCell = _fieldService.GetNextCell(currentId, new Vector2Int(x, y));
                    //���������� �� ��������� ������
                    if (nextCell == null) continue;

                    resultValue = resultValue + ((sideId == (int) nextCell.Figure) ? allyValue : enemyValue);

                    //���� ����������� =0, �� ���������������
                    if (Mathf.Abs(nextCell.Id.x) + Mathf.Abs(nextCell.Id.y) == 0) continue;


                    //��������� ������������� ������
                    Cell postNextCell = _fieldService.GetNextCell(nextCell.Id, new Vector2Int(x, y));


                    //���������� �� ������������� ������
                    if (postNextCell == null) continue;
                    //������������ �� ������ 
                    if (postNextCell.Figure != nextCell.Figure) continue;

                    resultValue += (sideId == (int) nextCell.Figure) ? allyValue * 2 : enemyValue * 2;
                }
            }

            return resultValue;
        }

        public void StartBotTurn(int countFigure, int playerScore, int botScore, Action callback)
        {
            StartCoroutine(IBotTurnProcess(
                new RoguelikeEnemyTurn { SmartFigures = countFigure },
                playerScore, botScore, callback));
        }

        public void StartBotTurn(RoguelikeEnemyTurn turn, int playerScore, int botScore, Action callback)
        {
            StartCoroutine(IBotTurnProcess(turn, playerScore, botScore, callback));
        }

        private IEnumerator IBotTurnProcess(RoguelikeEnemyTurn turn, int playerScore, int botScore, Action callback)
        {
            for (int i = 0; i < turn.TotalFigures; i++)
            {
                bool useSmartMove = i < turn.SmartFigures;
                Vector2Int position = useSmartMove
                    ? GenerateNewTurn(_playerService.GetCurrentPlayer().SideId, playerScore, botScore)
                    : GenerateRandomPosition(_fieldService.GetFieldSize());
                if (position != new Vector2Int(-1, -1))
                {
                    _fieldFigureService.PlaceInCell(position);
                    if (_roguelikeRunService.IsRunActive) _roguelikeRunService.RegisterEnemyFigurePlaced();
                    Debug.Log($"Cell placing on {position}");

                    Debug.LogFormat("Current tactic : {0}. Is Empty: {1}", _botAggression,
                        _coroutineService.GetIsQueueEmpty());
                    _historyService.AddHistoryCard(_playerService.GetCurrentPlayer(), _botCardDefault);
                    _finishLineService.MasterChecker(_playerService.GetCurrentPlayer().SideId, isInQueue: true);
                    while (!_coroutineService.GetIsQueueEmpty()) yield return null;
                    
                }
                else
                {
                    yield break;
                }
            }
            callback?.Invoke();
        }

        public void StopBotTurnForce()
        {
            StopAllCoroutines();
        }

        public enum BotGameType
        {
            Defense,
            Attack
        }
    }
}
