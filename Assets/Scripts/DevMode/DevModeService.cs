using System;
using DevMode.Interfaces;
using Settings.Interfaces;

namespace DevMode
{
    public class DevModeService : IDevModeService
    {
        private const string DEV_MODE_PASSWORD = "tttp_dev_mode";

        private readonly ISettingsDataService _settingsDataService;
        private bool _isDevModeEnabled;

        public event Action<bool> DevModeStateChanged;

        public DevModeService(ISettingsDataService settingsDataService)
        {
            _settingsDataService = settingsDataService;
            _settingsDataService.LoadDevModeState();
            _isDevModeEnabled = _settingsDataService.GetIsDevModeEnabled();
        }

        public bool GetIsDevModeEnabled()
        {
            return _isDevModeEnabled;
        }

        public bool TryEnableDevMode(string password)
        {
            if (!string.Equals(password, DEV_MODE_PASSWORD, StringComparison.Ordinal))
            {
                return false;
            }

            SetDevModeState(true);
            return true;
        }

        public void DisableDevMode()
        {
            SetDevModeState(false);
        }

        private void SetDevModeState(bool isEnabled)
        {
            if (_isDevModeEnabled == isEnabled) return;

            _isDevModeEnabled = isEnabled;
            _settingsDataService.SetDevModeState(_isDevModeEnabled);
            DevModeStateChanged?.Invoke(_isDevModeEnabled);
        }
    }
}
