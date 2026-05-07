namespace Settings.Interfaces
{
    public interface ISettingsDataService
    {
        public void SetLanguage(string language);
        public void SetLanguage(ISettingsDataService.Language language);
        public void LoadLanguage();
        public string GetLanguage();

        public void SetCellClearAnimationType(ISettingsDataService.CellClearAnimationType animationType);
        public void SetCellClearAnimationType(int animationType);
        public void LoadCellClearAnimationType();
        public ISettingsDataService.CellClearAnimationType GetCellClearAnimationType();

        public void SetDevModeState(bool isEnabled);
        public void LoadDevModeState();
        public bool GetIsDevModeEnabled();
        
        public enum Language
        {
            ru,
            en
        }

        public enum CellClearAnimationType
        {
            FinishLine = 0,
            FlyingToScore = 1
        }
    }
}