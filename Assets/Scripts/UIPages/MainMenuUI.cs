using Analytic.Interfaces;
using Cards.Interfaces;
using Coin.Interfaces;
using DevMode.Interfaces;
using GameScene;
using GameScene.Interfaces;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using Settings.Interfaces;
using Roguelike.Interfaces;
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

        [Header("Settings properties"), SerializeField]
        private TMP_Dropdown _cellClearAnimationDropdown;

        [Header("Dev mode properties"), SerializeField]
        private Button _enableDevModeButton;

        [SerializeField]
        private Button _disableDevModeButton;

        [SerializeField]
        private DevModePasswordPopup _devModePasswordPopup;

        private static bool _isConnectedToMasterState = false;

        #region Dependency

        private ICardList _cardList;
        private ICoinService _coinService;
        private IMatchEventsAnalyticService _matchEventsAnalyticService;
        private IGameSceneService _gameSceneService;
        private IGameTypeService _gameTypeService;
        private ITutorialCompleteService _tutorialCompleteService;
        private ISettingsDataService _settingsDataService;
        private IDevModeService _devModeService;
        private IRoguelikeRunService _roguelikeRunService;

        [Inject]
        private void Construct(ICardList cardList, ICoinService coinService,
            IMatchEventsAnalyticService matchEventsAnalyticService, IGameSceneService gameSceneService,
            IGameTypeService gameTypeService,
            ITutorialCompleteService tutorialCompleteService,
            ISettingsDataService settingsDataService,
            IDevModeService devModeService,
            IRoguelikeRunService roguelikeRunService)
        {
            _cardList = cardList;
            _coinService = coinService;
            _matchEventsAnalyticService = matchEventsAnalyticService;
            _gameSceneService = gameSceneService;
            _gameTypeService = gameTypeService;
            _tutorialCompleteService = tutorialCompleteService;
            _settingsDataService = settingsDataService;
            _devModeService = devModeService;
            _roguelikeRunService = roguelikeRunService;
        }

        #endregion

        private void Awake()
        {
            if (_devModePasswordPopup == null) return;
            _devModePasswordPopup.PasswordSubmitted += OnDevModePasswordSubmitted;
        }

        private void Start()
        {
            UpdateNetworkUI(_isConnectedToMasterState);
            Debug.Log($"TutorialManager.IsTutorialShowed {_tutorialCompleteService.GetIsTutorialComplete()}");
            _tutorialShowedArea.SetActive(_tutorialCompleteService.GetIsTutorialComplete());
            _tutorialNotShowedArea.SetActive(!_tutorialCompleteService.GetIsTutorialComplete());
            SyncCellClearAnimationDropdown();
            SyncDevModeButtons();
        }

        private void OnEnable()
        {
            _devModeService.DevModeStateChanged += OnDevModeStateChanged;
            UpdateTexts();
            SyncCellClearAnimationDropdown();
            SyncDevModeButtons();
        }

        private void OnDisable()
        {
            _devModeService.DevModeStateChanged -= OnDevModeStateChanged;
        }

        private void OnDestroy()
        {
            if (_devModePasswordPopup == null) return;
            _devModePasswordPopup.PasswordSubmitted -= OnDevModePasswordSubmitted;
        }

        public void UpdateTexts()
        {
            _moneyValue.text = _coinService.GetCurrentMoney().ToString();
        }

        public void OnAIButtonStart()
        {
            _roguelikeRunService.EndRun();
            _matchEventsAnalyticService.Player_Start_Match(GameType.SingleAI, _cardList.GetCardList());
            _gameTypeService.SetGameType(GameType.SingleAI);
            _gameSceneService.LoadGameScene(GameSceneManager.GameScene.Game);
        }


        public void OnHumanButtonStart()
        {
            _roguelikeRunService.EndRun();
            _matchEventsAnalyticService.Player_Start_Match(GameType.SingleHuman, _cardList.GetCardList());
            _gameTypeService.SetGameType(GameType.SingleHuman);
            _gameSceneService.LoadGameScene(GameSceneManager.GameScene.Game);
        }

        public void OnRoguelikeButtonStart()
        {
            _roguelikeRunService.StartNewRun();
            if (!_roguelikeRunService.IsRunActive) return;
            _gameTypeService.SetGameType(GameType.Roguelike);
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

        public void ChangeCellClearAnimation(int animationType)
        {
            _settingsDataService.SetCellClearAnimationType(animationType);
            SyncCellClearAnimationDropdown();
        }

        public int GetCellClearAnimationType()
        {
            return (int) _settingsDataService.GetCellClearAnimationType();
        }

        public void SyncCellClearAnimationDropdown()
        {
            if (_cellClearAnimationDropdown == null) return;
            _cellClearAnimationDropdown.SetValueWithoutNotify(GetCellClearAnimationType());
        }

        public void EnableDevModeButtonClick()
        {
            if (_devModePasswordPopup == null) return;
            _devModePasswordPopup.Open();
        }

        public void DisableDevModeButtonClick()
        {
            _devModeService.DisableDevMode();
        }

        public void SyncDevModeButtons()
        {
            bool isDevModeEnabled = _devModeService.GetIsDevModeEnabled();
            if (_enableDevModeButton != null) _enableDevModeButton.gameObject.SetActive(!isDevModeEnabled);
            if (_disableDevModeButton != null) _disableDevModeButton.gameObject.SetActive(isDevModeEnabled);
        }

        private void OnDevModePasswordSubmitted(string password)
        {
            if (_devModeService.TryEnableDevMode(password))
            {
                _devModePasswordPopup.Close();
            }
            else
            {
                _devModePasswordPopup.ShowInvalidPassword();
            }
        }

        private void OnDevModeStateChanged(bool isEnabled)
        {
            SyncDevModeButtons();
        }
    }
}
