using System;
using Analytic.Interfaces;
using Cards.Interfaces;
using Coin.Interfaces;
using IAPurchasing.Interfaces;
using SaveSystem;
using TMPro;
using UIElements;
using UnityEngine;
using Zenject;

namespace UIPages
{
    public class StorePage : MonoBehaviour
    {
        private const string WARNING_POPUP_KEY = "IsWarningPopupShowed";
        private const string STORE_SAVE_PATH = "StoreData";

        [Header("Store properties"), SerializeField]
        private AnimationFading _warningPopup;

        [SerializeField]
        private TextMeshProUGUI _randomCardPrice;
        
        #region Dependency

        private IStoreEventsAnalyticService _storeEventsAnalyticService;
        private IIAPService _iapService;
        private IAdEventsAnalyticService _adEventsAnalyticService;
        private ICollectionService _collectionService;
        private ICoinService _coinService;

        [Inject]
        private void Construct(
            IStoreEventsAnalyticService storeEventsAnalyticService,
            IIAPService iapService,
            IAdEventsAnalyticService adEventsAnalyticService,
            ICollectionService collectionService,
            ICoinService coinService)
        {
            _storeEventsAnalyticService = storeEventsAnalyticService;
            _iapService = iapService;
            _adEventsAnalyticService = adEventsAnalyticService;
            _collectionService = collectionService;
            _coinService = coinService;
        }

        #endregion

        private void Start()
        {
            _randomCardPrice.text = _coinService.GetCoinPerUnlock().ToString();
        }

        private static BinarySaveSystem _storeSaveSystem;
        private static StoreSaveData _storeSaveData;

        private static void EnsureStoreDataLoaded()
        {
            _storeSaveSystem ??= new BinarySaveSystem(STORE_SAVE_PATH);
            if (_storeSaveData != null) return;

            _storeSaveData = (StoreSaveData) _storeSaveSystem.Load();
            if (_storeSaveData != null) return;

            _storeSaveData = new StoreSaveData
            {
                IsWarningPopupShowed = PlayerPrefs.GetInt(WARNING_POPUP_KEY, 0) == 1
            };

            if (PlayerPrefs.HasKey(WARNING_POPUP_KEY)) PlayerPrefs.DeleteKey(WARNING_POPUP_KEY);
            _storeSaveSystem.Save(_storeSaveData);
        }

        private bool _isWarningPopupShowed
        {
            get
            {
                EnsureStoreDataLoaded();
                return _storeSaveData.IsWarningPopupShowed;
            }
            set
            {
                EnsureStoreDataLoaded();
                _storeSaveData.IsWarningPopupShowed = value;
                _storeSaveSystem.Save(_storeSaveData);
            }
        }

        public void ShowWarningPopup()
        {
            if (_isWarningPopupShowed) return;
            _isWarningPopupShowed = true;
            _warningPopup.FadeIn();
        }

        public void BuyBetaBundle()
        {
            _storeEventsAnalyticService.Player_Try_Purchase(_iapService.GetBetatestBundleId());
            _iapService.BuyProductID(_iapService.GetBetatestBundleId(),
                delegate
                {
                    _collectionService.UnlockAllCard();
                    _storeEventsAnalyticService.Player_Bought_Bundle(_iapService.GetBetatestBundleId());
                });
        }

        public void UnlockCardWithRewardAd()
        {
            for (int i = 0; i < 5; i++)
            {
                _collectionService.UnlockRandomCard(false);
            }
        }

        public void FindAdd() => _adEventsAnalyticService.Player_Try_Watch_Add();

        public void WatchedAdd() => _adEventsAnalyticService.Player_Watched_Add();

        public void Player_Open_Store()
        {
            _storeEventsAnalyticService.Player_Open_Store();
        }
    }
}