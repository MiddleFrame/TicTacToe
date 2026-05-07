using System;

namespace DevMode.Interfaces
{
    public interface IDevModeService
    {
        event Action<bool> DevModeStateChanged;

        public bool GetIsDevModeEnabled();
        public bool TryEnableDevMode(string password);
        public void DisableDevMode();
    }
}
