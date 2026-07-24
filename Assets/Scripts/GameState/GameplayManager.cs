using System.Collections;
using AI.Interfaces;
using Analytic.Interfaces;
using Cards.CustomType;
using Cards.Interfaces;
using Coin.Interfaces;
using Coroutine.Interfaces;
using Effects.Interfaces;
using Field.Interfaces;
using GameScene;
using GameScene.Interfaces;
using GameState.Interfaces;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using History.Interfaces;
using Mana.Interfaces;
using Network.Interfaces;
using Players.Interfaces;
using Score.Interfaces;
using TurnTimer.Interfaces;
using UIPages.Interfaces;
using UnityEngine;
using Zenject;
using Roguelike.Interfaces;

namespace GameState
{
    public class GameplayManager : IGameStateService, IInitializable
    {
        private GameplayState _gameplayState = GameplayState.NewGame;

        public GameplayState GetCurrentGameplayState()
        {
            return _gameplayState;
        }

        private bool _isNewStateInQueue;

        private bool _isOnline;

        private int _figureCount;

        private const int MAX_FIGURE_COUNT = 3;

        #region Dependecy

        private readonly IAIService _aiService;
        private readonly IPlayerService _playerService;
        private readonly IScoreService _scoreService;
        private readonly ICoroutineService _coroutineService;
        private readonly IHandPoolView _handPoolView;
        private readonly IHandPoolManipulator _handPoolManipulator;
        private readonly IHistoryService _historyService;
        private readonly IEffectService _effectService;
        private readonly ITurnTimerService _turnTimerService;
        private readonly ICheckEventNetworkService _checkEventNetworkService;
        private readonly IRoomService _roomService;
        private readonly IScoreWinnerService _scoreWinnerService;
        private readonly ICoinService _coinService;
        private readonly IInGameUIService _inGameUIService;
        private readonly IMatchEventsAnalyticService _matchEventsAnalyticService;
        private readonly ICardList _cardList;
        private readonly IGameTypeService _gameTypeService;
        private readonly IManaService _manaService;
        private readonly IManaUIService _manaUIService;
        private readonly IFieldService _fieldService;
        private readonly IGameSceneService _gameSceneService;
        private readonly INetworkEventService _networkEventService;
        private readonly IRechangerService _rechangerService;
        private readonly IRoguelikeRunService _roguelikeRunService;
        private readonly IRoguelikeUIService _roguelikeUIService;

        public GameplayManager
        (
            IAIService aiService,
             IGameSceneService gameSceneService,
             IPlayerService playerService,
             IScoreService scoreService,
             ICoroutineService coroutineService,
             IHandPoolManipulator handPoolManipulator,
             IHistoryService historyService,
             IHandPoolView handPoolView,
             IEffectService effectService,
             ITurnTimerService turnTimerService,
             ICheckEventNetworkService checkEventNetworkService,
             IRoomService roomService,
             IScoreWinnerService scoreWinnerService,
             ICoinService coinService,
             IInGameUIService inGameUIService,
             IMatchEventsAnalyticService matchEventsAnalyticService,
             ICardList cardList,
             IGameTypeService gameTypeService,
             IManaService manaService,
             IManaUIService manaUIService,
             IFieldService fieldService,
            INetworkEventService networkEventService,
            IRechangerService rechangerService,
            IRoguelikeRunService roguelikeRunService,
            IRoguelikeUIService roguelikeUIService
            )
        {
            _aiService = aiService;
            _playerService = playerService;
            _scoreService = scoreService;
            _coroutineService = coroutineService;
            _historyService = historyService;
            _handPoolView = handPoolView;
            _effectService = effectService;
            _turnTimerService = turnTimerService;
            _checkEventNetworkService = checkEventNetworkService;
            _roomService = roomService;
            _scoreWinnerService = scoreWinnerService;
            _coinService = coinService;
            _inGameUIService = inGameUIService;
            _matchEventsAnalyticService = matchEventsAnalyticService;
            _cardList = cardList;
            _handPoolManipulator = handPoolManipulator;
            _gameTypeService = gameTypeService;
            _manaService = manaService;
            _manaUIService = manaUIService;
            _fieldService = fieldService;
            _gameSceneService = gameSceneService;
            _networkEventService = networkEventService;
            _rechangerService = rechangerService;
            _roguelikeRunService = roguelikeRunService;
            _roguelikeUIService = roguelikeUIService;
        }

        #endregion

        private void CheckGameplayState()
        {
            switch (_gameplayState)
            {
                case GameplayState.None:
                    break;

                case GameplayState.NewGame:
                    switch (_gameTypeService.GetGameType())
                    {
                        case GameType.SingleAI:
                            _isOnline = false;
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(1));
                            _scoreService.AddPlayer(1);
                            _playerService.AddPlayer(PlayerType.AI, _handPoolManipulator.CreateCardPull(2));
                            _scoreService.AddPlayer(2);
                            break;
                        case GameType.SingleHuman:
                            _isOnline = false;
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(1));
                            _scoreService.AddPlayer(1);
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(2));
                            _scoreService.AddPlayer(2);
                            break;
                        case GameType.MultiplayerHuman:
                            _isOnline = true;
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(1));
                            _scoreService.AddPlayer(1);
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(2));
                            _scoreService.AddPlayer(2);
                            _playerService.SetOnlineId(_roomService.GetCurrentPlayerSide());
                            break;
                        case GameType.Roguelike:
                            _isOnline = false;
                            _playerService.AddPlayer(PlayerType.Human, _handPoolManipulator.CreateCardPull(1));
                            _scoreService.AddPlayer(1);
                            _playerService.AddPlayer(PlayerType.AI, _handPoolManipulator.CreateCardPull(2));
                            _scoreService.AddPlayer(2);
                            break;
                    }

                    _inGameUIService.SetIsOnlineGame(_isOnline);
                    _networkEventService.SetIsOnline(_isOnline);

                    _playerService.SetGameType(_gameTypeService.GetGameType());

                    SetGameplayState(GameplayState.NewRound);
                    _coroutineService.AddCoroutine(
                        _inGameUIService.IShowNewTurnAnimation(
                            (CellFigure)_playerService.GetCurrentSideOnDevice()));

                    break;

                case GameplayState.NewTurn:
                    _isNewStateInQueue = false;
                    _historyService.AddHistoryNewTurn(_playerService.GetCurrentPlayer());
                    foreach (CardModel card in _playerService.GetCurrentPlayer().FullDeckPool)
                    {
                        card.Info.CardBonusManacost = 0;
                    }

                    _playerService.NextPlayer();
                    _manaService.SetBonusMana(0);

                    _coroutineService.AddCoroutine(
                        _inGameUIService.IShowNewTurnAnimation((CellFigure)_playerService.GetCurrentPlayer()
                            .SideId));


                    if (!_isOnline || _playerService.GetCurrentPlayer().SideId ==
                        _roomService.GetCurrentPlayerSide())
                    {
                        _handPoolView.ChangeCurrentPlayerView(_playerService.GetCurrentPlayer());
                        _coroutineService.AddCoroutine(_effectService.UpdateEffectTurn());
                    }

                    if (_isOnline)
                        _turnTimerService.StartNewTurnTimer(_playerService.GetCurrentPlayer().EntityType,
                        (!_isOnline || _playerService.GetCurrentPlayer().SideId ==
                            Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber)
                            ? () =>
                            {
                                SetGamePlayStateQueue(GameplayState.NewTurn);
                                _checkEventNetworkService.RaiseEventEndTurn();
                            }
                        : null);

                    _manaService.RestoreAllMana();
                    _manaUIService.UpdateManaUI();

                    _handPoolView.UpdateCardPosition(false);
                    _handPoolView.UpdateCardUI();
                    _rechangerService.ResetRechanger();

                    _inGameUIService.NewTurn();

                    if (_playerService.GetCurrentPlayer().EntityType.Equals(PlayerType.AI))
                    {
                        Debug.Log("AI TURN START");
                        _figureCount = Mathf.Min(_figureCount + 1, MAX_FIGURE_COUNT);
                        _coroutineService.AddCoroutine(StartBotTurnAfterTurnAnimation());
                    }
                    else
                    {
                        SetGameplayState(GameplayState.None);
                    }

                    break;
                case GameplayState.GameOver:
                    if (_inGameUIService.GetIsGameOverShowed()) return;
                    if (_isOnline) _roomService.LeaveRoom(false);
                    int valueMoney = 0;
                    if (_gameTypeService.GetGameType() != GameType.SingleHuman &&
                        _scoreWinnerService.GetRoundWinner() != -1 &&
                        _playerService.GetCurrentSideOnDevice() == _scoreWinnerService.GetRoundWinner())
                        valueMoney = _coinService.GetCoinPerWin();
                    else if (_gameTypeService.GetGameType() != GameType.SingleHuman &&
                             _scoreWinnerService.GetRoundWinner() == -1) valueMoney = _coinService.GetCoinPerWin() / 2;
                    _coinService.SetCurrentMoney(_coinService.GetCurrentMoney() + valueMoney);
                    _inGameUIService.StateGameOverPanel(true, valueMoney);
                    _coroutineService.ClearQueue();

                    int currentWinner = _scoreWinnerService.GetGameWinner();
                    if (currentWinner == -1)
                    {
                        _matchEventsAnalyticService.Player_Draw_Match(_gameTypeService.GetGameType(),
                            _cardList.GetCardList());
                    }
                    else
                    {
                        bool isWin = false;
                        switch (_gameTypeService.GetGameType())
                        {
                            case GameType.MultiplayerHuman:
                                isWin = currentWinner == _roomService.GetCurrentPlayerSide();
                                break;
                            case GameType.SingleAI:
                                isWin = _playerService.GetPlayers()[currentWinner - 1].EntityType ==
                                        PlayerType.Human;
                                break;
                        }

                        if (isWin)
                            _matchEventsAnalyticService.Player_Win_Match(_gameTypeService.GetGameType(),
                                _cardList.GetCardList());
                        else
                            _matchEventsAnalyticService.Player_Lose_Match(_gameTypeService.GetGameType(),
                                _cardList.GetCardList());
                    }

                    break;
                case GameplayState.RestartGame:
                    _scoreWinnerService.ClearRoundWinners();
                    SetGameplayState(GameplayState.NewRound);

                    break;
                case GameplayState.RoundOver:
                    _turnTimerService.StopTimer();
                    _inGameUIService.StopTimer();
                    _coroutineService.ClearQueue();
                    if (_roguelikeRunService.IsRunActive)
                    {
                        HandleRoguelikeRoundOver();
                        break;
                    }
                    _scoreWinnerService.AddRoundWinner(_scoreWinnerService.GetRoundWinner());
                    if (_scoreWinnerService.IsExistGameWinner())
                    {
                        SetGameplayState(GameplayState.GameOver);
                    }
                    else
                    {
                        _coroutineService.AddCoroutine(_inGameUIService.ShowRoundOverAnimation());
                        _coroutineService.AddCoroutine(SetStateQueueProcess(GameplayState.NewRound));
                    }

                    break;
                case GameplayState.NewRound:
                    _figureCount = 0;
                    _aiService.StopBotTurnForce();
                    _playerService.ResetCurrentPlayer();
                    if (_roguelikeRunService.IsRunActive)
                    {
                        _fieldService.InitializationWithSize(_roguelikeRunService.CurrentStage.BoardSize);
                        _scoreService.SetMaxScore(1, _roguelikeRunService.CurrentStage.ScoreToWin);
                        _scoreService.SetMaxScore(2, _roguelikeRunService.CurrentStage.ScoreToWin);
                        _handPoolManipulator.SynchronizeDeck(
                            _playerService.GetPlayers()[0], _roguelikeRunService.Deck);
                    }
                    else
                    {
                        _fieldService.Initialization(_scoreWinnerService.GetRoundCount());
                    }
                    _coroutineService.ClearQueue();
                    _historyService.ClearHistory();
                    _effectService.ClearEffect();
                    foreach (var t in _playerService.GetPlayers())
                    {
                        _handPoolManipulator.ResetHandPool(t);
                        foreach (CardModel card in t.FullDeckPool)
                        {
                            card.Info.CardBonusManacost = 0;
                        }
                    }

                    _manaService.SetBonusMana(0);
                    if (_roguelikeRunService.IsRunActive)
                    {
                        _manaService.SetMaximumMana(_roguelikeRunService.MaximumMana);
                    }
                    else
                    {
                        _manaService.ResetMana(_scoreWinnerService.GetRoundCount());
                    }
                    _manaService.RestoreAllMana();
                    _manaUIService.UpdateManaUI();

                    _handPoolView.ChangeCurrentPlayerView(_playerService.GetCurrentPlayerOnDevice());
                    _scoreService.ClearAllScore();
                    _handPoolView.UpdateCardPosition(false);
                    if (_isOnline)
                        _turnTimerService.StartNewTurnTimer(_playerService.GetCurrentPlayer().EntityType,
                            (!_isOnline || _playerService.GetCurrentPlayer().SideId ==
                                Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber)
                                ? () =>
                                {
                                    SetGamePlayStateQueue(GameplayState.NewTurn);
                                    _checkEventNetworkService.RaiseEventEndTurn();
                                }
                        : null);


                    _inGameUIService.NewTurn();
                    _inGameUIService.SetSideBannerTurn(1);
                    _inGameUIService.SetupMaxScore(_scoreService.GetMaxScore(1), _scoreService.GetMaxScore(2));
                    _inGameUIService.UpdateScore(0, 0);
                    _inGameUIService.UpdatePlayerRP(
                        _scoreWinnerService.GetCountRoundWin(1),
                        _scoreWinnerService.GetCountRoundWin(2));
                    if (_roguelikeRunService.IsRunActive)
                    {
                        SetGameplayState(GameplayState.None);
                    }
                    break;
                case GameplayState.RoguelikeReward:
                case GameplayState.RoguelikeGameOver:
                    break;
            }
        }

        private void HandleRoguelikeRoundOver()
        {
            _aiService.StopBotTurnForce();
            int winner = _scoreWinnerService.GetRoundWinner();
            if (winner == -1)
            {
                _coroutineService.AddCoroutine(_inGameUIService.ShowRoundOverAnimation());
                _coroutineService.AddCoroutine(SetStateQueueProcess(GameplayState.NewRound));
                return;
            }

            if (winner == 1)
            {
                _roguelikeRunService.RegisterVictory();
                bool firstVictory = _roguelikeRunService.Victories == 1;
                SetGameplayState(GameplayState.RoguelikeReward);
                _roguelikeUIService.ShowVictoryReward(firstVictory,
                    () => SetGameplayState(GameplayState.NewRound));
                return;
            }

            SetGameplayState(GameplayState.RoguelikeGameOver);
            _roguelikeUIService.ShowDefeatSummary(() => _gameSceneService.BeginTransaction());
        }

        private IEnumerator StartBotTurnAfterTurnAnimation()
        {
            if (_roguelikeRunService.IsRunActive)
            {
                _aiService.StartBotTurn(_roguelikeRunService.CurrentStage.EnemyTurn,
                    _scoreService.GetScore(_playerService.GetPlayers()[0].SideId),
                    _scoreService.GetScore(_playerService.GetPlayers()[1].SideId),
                    () => { SetGameplayState(GameplayState.NewTurn); });
            }
            else
            {
                _aiService.StartBotTurn(_figureCount,
                    _scoreService.GetScore(_playerService.GetPlayers()[0].SideId),
                    _scoreService.GetScore(_playerService.GetPlayers()[1].SideId),
                    () => { SetGameplayState(GameplayState.NewTurn); });
            }
            yield return null;
        }



        public void SetGameplayState(GameplayState state)
        {
            _gameplayState = state;
            CheckGameplayState();
            Debug.Log($"Current state :{state}");
        }

        public void SetGamePlayStateQueue(GameplayState state)
        {
            if (!_isNewStateInQueue)
            {
                _isNewStateInQueue = true;
                _coroutineService.AddCoroutine(SetStateQueueProcess(state));
            }
        }

        private IEnumerator SetStateQueueProcess(GameplayState state)
        {
            SetGameplayState(state);
            _isNewStateInQueue = false;
            yield return null;
        }

        public bool IsCurrentGameplayState(GameplayState state)
        {
            return _gameplayState == state;
        }

        public bool GetIsOnline()
        {
            return _isOnline;
        }

        public void Initialize()
        {
            SetGameplayState(GameplayState.NewGame);
            _gameSceneService.BeginLoadGameScene(GameSceneManager.GameScene.MainMenu);
        }
    }
}
