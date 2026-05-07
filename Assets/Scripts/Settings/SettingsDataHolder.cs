using Settings.Interfaces;
using SaveSystem;
using UnityEngine;

namespace Settings
{
    public class SettingsDataHolder : ISettingsDataService
    {
        private const string LANGUAGE_KEY = "LanguageSettings";
        private const string CELL_CLEAR_ANIMATION_KEY = "CellClearAnimationSettings";
        private const string SETTINGS_SAVE_PATH = "SettingsData";

        private string _language;
        private ISettingsDataService.CellClearAnimationType _cellClearAnimationType =
            ISettingsDataService.CellClearAnimationType.FlyingToScore;
        private bool _isDevModeEnabled;
        private readonly BinarySaveSystem _settingsSaveSystem;
        private SettingsSaveData _settingsSaveData;

        public SettingsDataHolder()
        {
            _settingsSaveSystem = new BinarySaveSystem(SETTINGS_SAVE_PATH);
        }

        private void EnsureDataLoaded()
        {
            if (_settingsSaveData != null) return;

            _settingsSaveData = (SettingsSaveData) _settingsSaveSystem.Load();
            if (_settingsSaveData != null) return;

            _settingsSaveData = new SettingsSaveData
            {
                Language = PlayerPrefs.GetString(LANGUAGE_KEY, I2.Loc.LocalizationManager.CurrentLanguageCode),
                CellClearAnimationType = PlayerPrefs.GetInt(
                    CELL_CLEAR_ANIMATION_KEY,
                    (int) ISettingsDataService.CellClearAnimationType.FlyingToScore)
            };

            if (PlayerPrefs.HasKey(LANGUAGE_KEY)) PlayerPrefs.DeleteKey(LANGUAGE_KEY);
            if (PlayerPrefs.HasKey(CELL_CLEAR_ANIMATION_KEY)) PlayerPrefs.DeleteKey(CELL_CLEAR_ANIMATION_KEY);

            SaveData();
        }

        private void SaveData()
        {
            _settingsSaveSystem.Save(_settingsSaveData);
        }

        public void SetLanguage(string language)
        {
            EnsureDataLoaded();
            _language = language;
            _settingsSaveData.Language = _language;
            SaveData();
            I2.Loc.LocalizationManager.CurrentLanguageCode = _language;
        }

        public void SetLanguage(ISettingsDataService.Language language)
        {
            EnsureDataLoaded();
            _language = language.ToString();
            _settingsSaveData.Language = _language;
            SaveData();
            I2.Loc.LocalizationManager.CurrentLanguageCode = _language;
        }

        public void LoadLanguage()
        {
            EnsureDataLoaded();
            _language = string.IsNullOrEmpty(_settingsSaveData.Language)
                ? I2.Loc.LocalizationManager.CurrentLanguageCode
                : _settingsSaveData.Language;
            I2.Loc.LocalizationManager.CurrentLanguageCode = _language;
        }

        public string GetLanguage()
        {
            return _language;
        }

        public void SetCellClearAnimationType(ISettingsDataService.CellClearAnimationType animationType)
        {
            EnsureDataLoaded();
            _cellClearAnimationType = animationType;
            _settingsSaveData.CellClearAnimationType = (int) _cellClearAnimationType;
            SaveData();
        }

        public void SetCellClearAnimationType(int animationType)
        {
            if (animationType < (int) ISettingsDataService.CellClearAnimationType.FinishLine ||
                animationType > (int) ISettingsDataService.CellClearAnimationType.FlyingToScore)
            {
                animationType = (int) ISettingsDataService.CellClearAnimationType.FlyingToScore;
            }

            SetCellClearAnimationType((ISettingsDataService.CellClearAnimationType) animationType);
        }

        public void LoadCellClearAnimationType()
        {
            EnsureDataLoaded();
            int animationType = _settingsSaveData.CellClearAnimationType;
            SetCellClearAnimationType(animationType);
        }

        public ISettingsDataService.CellClearAnimationType GetCellClearAnimationType()
        {
            return _cellClearAnimationType;
        }

        public void SetDevModeState(bool isEnabled)
        {
            EnsureDataLoaded();
            _isDevModeEnabled = isEnabled;
            _settingsSaveData.IsDevModeEnabled = _isDevModeEnabled;
            SaveData();
        }

        public void LoadDevModeState()
        {
            EnsureDataLoaded();
            _isDevModeEnabled = _settingsSaveData.IsDevModeEnabled;
        }

        public bool GetIsDevModeEnabled()
        {
            EnsureDataLoaded();
            _isDevModeEnabled = _settingsSaveData.IsDevModeEnabled;
            return _isDevModeEnabled;
        }
    }
}