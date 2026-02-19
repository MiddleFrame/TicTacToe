using Analytic.Interfaces;
using Cards.Interfaces;
using Coin.Interfaces;
using GameScene;
using GameScene.Interfaces;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using Settings.Interfaces;
using TMPro;
using Tutorial.Interfaces;
using UIPages.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIPages
{
    public class MainMenuUI : MonoBehaviour, IMainMenuState
    {
        [SerializeField]
        private TextMeshProUGUI _moneyValue;

        [Header("Multiplayer button properties"), SerializeField]
        private Button _multiplayerButton;

        [SerializeField]
        private GameObject _multiplayerNotAvailableObject;

        [Header("Tutorial area properties"), SerializeField]
        private GameObject _tutorialShowedArea;

        [SerializeField]
        private GameObject _tutorialNotShowedArea;

        private static bool _isConnectedToMasterState = false;

        #region Dependency

        private ICardList _cardList;
        private ICoinService _coinService;
        private IMatchEventsAnalyticService _matchEventsAnalyticService;
        private IGameSceneService _gameSceneService;
        private IGameTypeService _gameTypeService;
        private ITutorialCompleteService _tutorialCompleteService;
        private ISettingsDataService _settingsDataService;

        [Inject]
        private void Construct(ICardList cardList, ICoinService coinService,
            IMatchEventsAnalyticService matchEventsAnalyticService, IGameSceneService gameSceneService,
            IGameTypeService gameTypeService,
            ITutorialCompleteService tutorialCompleteService,
            ISettingsDataService settingsDataService)
        {
            _cardList = cardList;
            _coinService = coinService;
            _matchEventsAnalyticService = matchEventsAnalyticService;
            _gameSceneService = gameSceneService;
            _gameTypeService = gameTypeService;
            _tutorialCompleteService = tutorialCompleteService;
            _settingsDataService = settingsDataService;
        }

        #endregion

        private void Start()
        {
            UpdateNetworkUI(_isConnectedToMasterState);
            Debug.Log($"TutorialManager.IsTutorialShowed {_tutorialCompleteService.GetIsTutorialComplete()}");
            _tutorialShowedArea.SetActive(_tutorialCompleteService.GetIsTutorialComplete());
            _tutorialNotShowedArea.SetActive(!_tutorialCompleteService.GetIsTutorialComplete());
        }

        private void OnEnable()
        {
            UpdateTexts();
        }

        public void UpdateTexts()
        {
            _moneyValue.text = _coinService.GetCurrentMoney().ToString();
        }

        public void OnAIButtonStart()
        {
            _matchEventsAnalyticService.Player_Start_Match(GameType.SingleAI, _cardList.GetCardList());
            _gameTypeService.SetGameType(GameType.SingleAI);
            _gameSceneService.LoadGameScene(GameSceneManager.GameScene.Game);
        }


        public void OnHumanButtonStart()
        {
            _matchEventsAnalyticService.Player_Start_Match(GameType.SingleHuman, _cardList.GetCardList());
            _gameTypeService.SetGameType(GameType.SingleHuman);
            _gameSceneService.LoadGameScene(GameSceneManager.GameScene.Game);
        }

        public void OnTutorialButtonClick()
        {
            _gameSceneService.LoadGameScene(GameSceneManager.GameScene.Tutorial);
        }

        private void ChangeMultiplayerButtonState(bool state)
        {
            _multiplayerButton.interactable = state;
            _multiplayerNotAvailableObject.SetActive(!state);
        }

        public void UpdateNetworkUI(bool isConnected)
        {
            ChangeMultiplayerButtonState(isConnected);
        }

        public void PlayButtonClick()
        {
            OnAIButtonStart();
        }

        public void OnDiscordClick()
        {
            Application.OpenURL("https://discord.gg/4PJbjRZtkU");
        }

        public void SetIsConnectedToMaster(bool state)
        {
            _isConnectedToMasterState = state;
            UpdateNetworkUI(_isConnectedToMasterState);
        }

        public void ChangeLanguage(string language)
        {
            _settingsDataService.SetLanguage(language);
        }

        public void ChangeLanguage()
        {
            Debug.Log("We Are here!! " +_settingsDataService.GetLanguage());
            _settingsDataService.SetLanguage((_settingsDataService.GetLanguage()) == "ru"
                ? ISettingsDataService.Language.en
                : ISettingsDataService.Language.ru);
        }
    }
}