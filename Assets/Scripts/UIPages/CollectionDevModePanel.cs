using DevMode.Interfaces;
using UnityEngine;
using Zenject;

namespace UIPages
{
    public class CollectionDevModePanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject _devButtonsRoot;

        private IDevModeService _devModeService;
        private IDevCollectionService _devCollectionService;

        [Inject]
        private void Construct(IDevModeService devModeService, IDevCollectionService devCollectionService)
        {
            _devModeService = devModeService;
            _devCollectionService = devCollectionService;
        }

        private void OnEnable()
        {
            _devModeService.DevModeStateChanged += OnDevModeStateChanged;
            SyncVisibility();
        }

        private void OnDisable()
        {
            _devModeService.DevModeStateChanged -= OnDevModeStateChanged;
        }

        public void UnlockAllCardsButtonClick()
        {
            if (!_devModeService.GetIsDevModeEnabled()) return;
            _devCollectionService.UnlockAllCards();
        }

        public void LockAllCardsButtonClick()
        {
            if (!_devModeService.GetIsDevModeEnabled()) return;
            _devCollectionService.LockAllCards();
        }

        private void OnDevModeStateChanged(bool isEnabled)
        {
            SyncVisibility();
        }

        private void SyncVisibility()
        {
            if (_devButtonsRoot == null) return;
            _devButtonsRoot.SetActive(_devModeService.GetIsDevModeEnabled());
        }
    }
}
