using Analytic.Interfaces;
using Cards.Interfaces;
using Coroutine.Interfaces;
using FinishLine.Interfaces;
using GameScene.Interfaces;
using GameState.Interfaces;
using GameTypeService.Interfaces;
using Network.Interfaces;
using Players.Interfaces;
using Score.Interfaces;
using UIPages.Interfaces;
using Zenject;

namespace ActionInstaller
{
    public class GameSceneActionBinder: IInitializable
    {
        #region Dependecy

        private readonly IPlayerService _playerService;
        private readonly IScoreService _scoreService;
        private readonly ICheckEventNetworkService _checkEventNetworkService;
        private readonly IRoomService _roomService;
        private readonly IInGameUIService _inGameUIService;
        private readonly IMatchEventsAnalyticService _matchEventsAnalyticService;
        private readonly ICardList _cardList;
        private readonly IGameTypeService _gameTypeService;
        private readonly INetworkEventService _networkEventService;
        private readonly IGameStateService _gameStateService;
        private readonly IGameSceneService _gameSceneService;
        private readonly IFinishLineService _finishLineService;
        private readonly ICoroutineService _coroutineService;

        public GameSceneActionBinder
        (
            IPlayerService playerService,
            IScoreService scoreService,
            ICheckEventNetworkService checkEventNetworkService,
            IRoomService roomService,
            IInGameUIService inGameUIService,
            IMatchEventsAnalyticService matchEventsAnalyticService,
            ICardList cardList,
            IGameTypeService gameTypeService,
            INetworkEventService networkEventService,
            IGameStateService gameStateService,
            IGameSceneService gameSceneService,
            IFinishLineService finishLineService,
            ICoroutineService coroutineService
        )
        {
            _playerService = playerService;
            _scoreService = scoreService;
            _checkEventNetworkService = checkEventNetworkService;
            _roomService = roomService;
            _inGameUIService = inGameUIService;
            _matchEventsAnalyticService = matchEventsAnalyticService;
            _cardList = cardList;
            _gameTypeService = gameTypeService;
            _networkEventService = networkEventService;
            _gameStateService = gameStateService;
            _finishLineService = finishLineService;
            _gameSceneService = gameSceneService;
            _coroutineService = coroutineService;
        }

        #endregion


        private void BindServiceActions()
        {
            _inGameUIService.SetReturnHomeAction(() =>
            {
                if (!_gameStateService.IsCurrentGameplayState(GameplayState.GameOver))
                {
                    _matchEventsAnalyticService.Player_Lose_Match(_gameTypeService.GetGameType(),
                        _cardList.GetCardList());
                    _matchEventsAnalyticService.Player_Leave_Match(_gameTypeService.GetGameType(),
                        _cardList.GetCardList());
                }

                if (_gameStateService.GetIsOnline()) _roomService.LeaveRoom(true);
                _gameSceneService.BeginTransaction();
            });

            _inGameUIService.SetEndTurnAction(() =>
            {
                if (_gameStateService.GetIsOnline() && _playerService.GetCurrentPlayer().SideId !=
                    _roomService.GetCurrentPlayerSide()) return;
                if (!_coroutineService.GetIsQueueEmpty()) return;
                _gameStateService.SetGamePlayStateQueue(GameplayState.NewTurn);
                _checkEventNetworkService.RaiseEventEndTurn();
            });

            _inGameUIService.SetRestartGameAction(() =>
            {
                if (_gameStateService.GetIsOnline()) return;
                _gameStateService.SetGamePlayStateQueue(GameplayState.RestartGame);
                if (_inGameUIService.GetIsGameOverShowed()) _inGameUIService.StateGameOverPanel(false);
            });

            _roomService.SetPlayerLeaveAction((isPreExit, actorNumber) =>
                {
                    if (_gameStateService.IsCurrentGameplayState(GameplayState.GameOver)) return;
                    if (isPreExit)
                        _scoreService.RemovePlayer(actorNumber);
                    _gameStateService.SetGamePlayStateQueue(GameplayState.GameOver);
                }
            );

            _networkEventService.SetNewTurnAction(() =>
            {
                _gameStateService.SetGamePlayStateQueue(GameplayState.NewTurn);
            });
            _finishLineService.SetNetworkEventAction(() => { _checkEventNetworkService.RaiseEventMasterChecker(); });
            _finishLineService.SetNewGameState(state =>
            {
                _gameStateService.SetGamePlayStateQueue(state);
            });
            _finishLineService.SetPredicateIsEqualGameState(state => _gameStateService.IsCurrentGameplayState(state));
        }

        public void Initialize()
        {
            BindServiceActions();
        }
    }
}