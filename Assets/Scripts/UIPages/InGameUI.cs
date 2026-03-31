using System;
using System.Collections;
using System.Collections.Generic;
using Analytic.Interfaces;
using Cards.Interfaces;
using Coroutine.Interfaces;
using GameScene.Interfaces;
using GameState.Interfaces;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using Network.Interfaces;
using Players.Interfaces;
using Score.Interfaces;
using Theme.Interfaces;
using TMPro;
using TurnTimer.Interfaces;
using UIElements;
using UIPages.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIPages
{
    public class InGameUI : MonoBehaviour, IInGameUIService
    {

        [SerializeField]
        private Slider _enemyHpSlider;
        [SerializeField]
        private Slider _playerHpSlider;

        [SerializeField]
        private TextMeshProUGUI _playerOneScoreText;

        [SerializeField]
        private List<GameObject> _playerOneRPList;

        [SerializeField]
        private TextMeshProUGUI _playerTwoScorText;

        [SerializeField]
        private List<GameObject> _playerTwoRPList;

        [SerializeField]
        private GameObject _newTurnBTN;


        [Space, Header("GameOver Properties"), SerializeField]
        private AnimationFading _gameOverPanel;

        [SerializeField]
        private Image _gameOverLogo;

        [SerializeField]
        private GameObject _windrawBG;

        [SerializeField]
        private GameObject _loseBG;

        [SerializeField]
        private List<GameObject> _bgList;

        [SerializeField]
        private GameObject _gameOverReplyBTN;

        [SerializeField]
        private TextMeshProUGUI _moneyValue;

        [SerializeField]
        private TextMeshProUGUI _winnerText;

        [SerializeField]
        private GameObject _winnerArea;

        [SerializeField]
        private GameObject _drawArea;

        [Space, Header("RoundOver Properties"), SerializeField]
        private AnimationFading _roundOverPanel;

        [SerializeField]
        private Image _roundOverLogo;

        [SerializeField]
        private GameObject _windrawRoundOverBG;

        [SerializeField]
        private GameObject _loseRoundOverBG;

        [SerializeField]
        private TextMeshProUGUI _winnerRoundOverText;

        [SerializeField]
        private GameObject _winnerRoundOverArea;

        [SerializeField]
        private GameObject _drawRoundOverArea;

        [SerializeField]
        private List<GameObject> _p1RoundOverPoints;

        [SerializeField]
        private List<GameObject> _p2RoundOverPoints;

        [Space, Header("Timer Properties"), SerializeField]
        private GameObject _timerPanel;

        [SerializeField]
        private Image _timerFilledImg;

        [SerializeField]
        private float _timerEnableTime;

        [SerializeField]
        private float _timerStartAnimationTime;

        private UnityEngine.Coroutine _timerCoroutine;

        [Header("New turn banner properties"), SerializeField]
        private Animator _animatorNewTurn;

        [Header("Pause menu Properties"), SerializeField]
        private GameObject _pauseMenuReplyBTN;

        [Header("Paper new turn properties"), SerializeField]
        private AnimationFading _paperNewTurnAnimation;

        [SerializeField]
        private Image _paperNewTurnPlayerImage;



        private static readonly int newTurn = Animator.StringToHash("NewTurn");
        private static readonly int xTurn = Animator.StringToHash("XTurn");
        private static readonly int oTurn = Animator.StringToHash("OTurn");

        private Action _replyButtonAction;
        private Action _returnMenuButtonAction;
        private Action _newTurnButtonAction;
        private bool _isOnlineGame;

        #region Dependecy

        private IHandPoolView _handPoolView;
        private IScoreWinnerService _scoreWinnerService;
        private IRoomService _roomService;
        private IPlayerService _playerService;
        private IThemeService _themeService;
        private ITurnTimerService _turnTimerService;
        private ICoroutineAwaitService _coroutineAwaitService;
        private IGameTypeService _gameTypeService;

        [Inject]
        private void Construct(IHandPoolView handPoolView, IScoreService scoreService,
            IScoreWinnerService scoreWinnerService, IRoomService roomService, IPlayerService playerService,
            IThemeService themeService,
            ITurnTimerService turnTimerService, ICoroutineAwaitService coroutineAwaitService,
            IGameTypeService gameTypeService)
        {
            _handPoolView = handPoolView;
            _scoreWinnerService = scoreWinnerService;
            _roomService = roomService;
            _playerService = playerService;
            _themeService = themeService;
            _turnTimerService = turnTimerService;
            _coroutineAwaitService = coroutineAwaitService;
            _gameTypeService = gameTypeService;
        }

        #endregion

        private void UpdateReplyState()
        {
            _pauseMenuReplyBTN.SetActive(!_isOnlineGame);
            _gameOverReplyBTN.SetActive(!_isOnlineGame);
        }

        public void NewTurn()
        {
            StopTimer();
            ClearTriggers();
            _animatorNewTurn.SetTrigger(newTurn);
            _timerCoroutine = StartCoroutine(ITimerProcess());

            _newTurnBTN.SetActive(_handPoolView.IsCurrentPlayerOnSlot());
            Debug.Log($"Current side : {_newTurnBTN.activeSelf}");
        }

        public void SetSideBannerTurn(int side)
        {
            ClearTriggers();
            _animatorNewTurn.SetTrigger((side == 1) ? "XTurn" : "OTurn");
        }

        public void SetRestartGameAction(Action action)
        {
            _replyButtonAction = action;
        }

        public void SetReturnHomeAction(Action action)
        {
            _returnMenuButtonAction = action;
        }

        public void SetEndTurnAction(Action action)
        {
            _newTurnButtonAction = action;
        }

        public void SetIsOnlineGame(bool state)
        {
            _isOnlineGame = state;
        }

        private void ClearTriggers()
        {
            _animatorNewTurn.ResetTrigger(newTurn);
            _animatorNewTurn.ResetTrigger(xTurn);
            _animatorNewTurn.ResetTrigger(oTurn);
        }
        public void SetupMaxScore(int firstPlayer, int secondPlayer)
        {
            _playerHpSlider.maxValue = firstPlayer;
            _enemyHpSlider.maxValue = secondPlayer;
        }

        public void UpdateScore(int score1Player, int score2Player)
        {
            _playerOneScoreText.text = score1Player+"/"+ _playerHpSlider.maxValue;
            _playerHpSlider.value = score1Player;
            _playerTwoScorText.text = score2Player+"/" + _enemyHpSlider.maxValue;
            _enemyHpSlider.value = score2Player;
        }

        public void UpdatePlayerRP(int score1Player, int score2Player)
        {
            for (int i = 0; i < _playerOneRPList.Count; i++)
            {
                _playerOneRPList[i].SetActive(i < score1Player);
            }

            for (int i = 0; i < _playerTwoRPList.Count; i++)
            {
                _playerTwoRPList[i].SetActive(i < score2Player);
            }
        }

        public void ReturnHome()
        {
            _returnMenuButtonAction?.Invoke();
        }

        public void EndButtonPressed()
        {
            _newTurnButtonAction?.Invoke();
        }

        public void StateGameOverPanel(bool state, int value = 0)
        {
            UpdateReplyState();
            if (state)
            {
                _gameOverPanel.FadeIn();
                int currentWinner = _scoreWinnerService.GetGameWinner();
                bool isWin = false;
                if (currentWinner != -1)
                {
                    switch (_gameTypeService.GetGameType())
                    {
                        case GameType.MultiplayerHuman:
                            isWin = currentWinner == _roomService.GetCurrentPlayerSide();
                            break;
                        case GameType.SingleAI:
                            isWin = _playerService.GetPlayers()[currentWinner - 1].EntityType == PlayerType.Human;
                            break;
                        case GameType.SingleHuman:
                            isWin = true;
                            break;
                    }
                }

                _windrawBG.SetActive(isWin || currentWinner == -1);
                _loseBG.SetActive(!(isWin || currentWinner == -1));
                _moneyValue.text = "+" + value.ToString();

                _winnerText.text = (currentWinner == -1) ? "Draw" :
                    (isWin) ? "You\nwin" : "You\nlose";

                _drawArea.SetActive(_scoreWinnerService.GetGameWinner() == -1);
                _winnerArea.SetActive(_scoreWinnerService.GetGameWinner() != -1);
                for (int i = 0; i < _bgList.Count; i++)
                {
                    _bgList[i].SetActive(i == Mathf.Max(0, currentWinner));
                }

                if (_scoreWinnerService.GetGameWinner() != -1)
                    _gameOverLogo.sprite =
                        _themeService.GetSprite((CellFigure) _scoreWinnerService.GetGameWinner());
            }
            else
            {
                _gameOverPanel.FadeOut();
            }
        }

        public void RestartGame()
        {
            _replyButtonAction?.Invoke();
        }


        public void StopTimer()
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        private IEnumerator ITimerProcess()
        {
            _timerPanel.SetActive(false);
            if (!_isOnlineGame)
                yield break;

            while (_timerEnableTime < _turnTimerService.GetTimeLeft()) yield return null;

            _timerFilledImg.fillAmount = 1;
            _timerPanel.SetActive(true);

            while (_timerStartAnimationTime < _turnTimerService.GetTimeLeft()) yield return null;

            while (_turnTimerService.GetTimeLeft() >= 0)
            {
                _timerFilledImg.fillAmount = _turnTimerService.GetTimeLeft() / _timerStartAnimationTime;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        public IEnumerator ShowRoundOverAnimation()
        {
            _roundOverPanel.FadeIn();
            int currentWinner = _scoreWinnerService.GetRoundWinner();
            bool isWin = false;
            if (currentWinner != -1)
            {
                switch (_gameTypeService.GetGameType())
                {
                    case GameType.MultiplayerHuman:
                        isWin = currentWinner == _roomService.GetCurrentPlayerSide();
                        break;
                    case GameType.SingleAI:
                        isWin = _playerService.GetPlayers()[currentWinner - 1].EntityType == PlayerType.Human;
                        break;
                    case GameType.SingleHuman:
                        isWin = true;
                        break;
                }
            }

            _windrawRoundOverBG.SetActive(isWin || currentWinner == -1);
            _loseRoundOverBG.SetActive(!(isWin || currentWinner == -1));

            _winnerRoundOverText.text = (currentWinner == -1) ? "Draw" :
                (isWin) ? "You\nwin" : "You\nlose";

            _drawRoundOverArea.SetActive(_scoreWinnerService.GetRoundWinner() == -1);
            _winnerRoundOverArea.SetActive(_scoreWinnerService.GetRoundWinner() != -1);
            for (int i = 0; i < _bgList.Count; i++)
            {
                _bgList[i].SetActive(i == Mathf.Max(0, currentWinner));
            }

            if (_scoreWinnerService.GetRoundWinner() != -1)
                _roundOverLogo.sprite =
                    _themeService.GetSprite((CellFigure) _scoreWinnerService.GetRoundWinner());

            int p1Count = _scoreWinnerService.GetCountRoundWin(1);
            int p2Count = _scoreWinnerService.GetCountRoundWin(2);

            for (int i = 0; i < _p1RoundOverPoints.Count; i++)
            {
                _p1RoundOverPoints[i].SetActive(i < p1Count);
            }

            for (int i = 0; i < _p2RoundOverPoints.Count; i++)
            {
                _p2RoundOverPoints[i].SetActive(i < p2Count);
            }

            Transform currentPoint = null;

            if (_scoreWinnerService.GetRoundWinner() == 1)
            {
                currentPoint = _p1RoundOverPoints[p1Count - 1].transform;
                currentPoint.localScale = Vector2.zero;
            }
            else if (_scoreWinnerService.GetRoundWinner() == 2)
            {
                currentPoint = _p2RoundOverPoints[p2Count - 1].transform;
                currentPoint.localScale = Vector2.zero;
            }


            yield return _coroutineAwaitService.AwaitTime((_roundOverPanel.FrameCount));
            yield return new WaitForSeconds(0.5f);
            if (currentPoint != null)
                yield return StartCoroutine(currentPoint.ScaleWithLerp(Vector2.zero, Vector2.one, 20));
            yield return new WaitForSeconds(1f);
            _roundOverPanel.FadeOut();
            yield return _coroutineAwaitService.AwaitTime((_roundOverPanel.FrameCount));
        }

        public IEnumerator IShowNewTurnAnimation(CellFigure cellFigure)
        {
            _paperNewTurnPlayerImage.sprite = _themeService.GetSprite(cellFigure);
            _paperNewTurnAnimation.FadeIn();
            Debug.Log("BegunAnim");
            yield return _coroutineAwaitService.AwaitTime((_paperNewTurnAnimation.FrameCount));
            yield return new WaitForSeconds(0.5f);
            _paperNewTurnAnimation.FadeOut();
            yield return _coroutineAwaitService.AwaitTime((_paperNewTurnAnimation.FrameCount));
        }

        public bool GetIsGameOverShowed()
        {
            return _gameOverPanel.gameObject.activeSelf;
        }
    }
}