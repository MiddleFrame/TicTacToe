using Settings.Interfaces;
using UnityEngine;

namespace Settings
{
    public class SettingsDataHolder : ISettingsDataService
    {
        private const string LANGUAGE_KEY = "LanguageSettings";
        private const string CELL_CLEAR_ANIMATION_KEY = "CellClearAnimationSettings";

        private string _language;
        private ISettingsDataService.CellClearAnimationType _cellClearAnimationType =
            ISettingsDataService.CellClearAnimationType.FlyingToScore;

        public void SetLanguage(string language)
        {
            _language = language;
            PlayerPrefs.SetString(LANGUAGE_KEY, _language);
            PlayerPrefs.Save();
            I2.Loc.LocalizationManager.CurrentLanguageCode  = _language;
        }

        public void SetLanguage(ISettingsDataService.Language language)
        {
            _language = language.ToString();
            PlayerPrefs.SetString(LANGUAGE_KEY, _language);
            PlayerPrefs.Save();
            I2.Loc.LocalizationManager.CurrentLanguageCode  = _language;
        }

        public void LoadLanguage()
        {
            _language = PlayerPrefs.GetString(LANGUAGE_KEY, I2.Loc.LocalizationManager.CurrentLanguageCode );
            SetLanguage(_language);
        }

        public string GetLanguage()
        {
            return _language;
        }

        public void SetCellClearAnimationType(ISettingsDataService.CellClearAnimationType animationType)
        {
            _cellClearAnimationType = animationType;
            PlayerPrefs.SetInt(CELL_CLEAR_ANIMATION_KEY, (int) _cellClearAnimationType);
            PlayerPrefs.Save();
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
            int animationType = PlayerPrefs.GetInt(
                CELL_CLEAR_ANIMATION_KEY,
                (int) ISettingsDataService.CellClearAnimationType.FlyingToScore);

            SetCellClearAnimationType(animationType);
        }

        public ISettingsDataService.CellClearAnimationType GetCellClearAnimationType()
        {
            return _cellClearAnimationType;
        }
    }
}