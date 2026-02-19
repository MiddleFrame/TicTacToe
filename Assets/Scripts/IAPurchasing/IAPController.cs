using System;
using System.Collections;
using IAPurchasing.Interfaces;
using UnityEngine;
#if UNITY_IAP
using UnityEngine.Purchasing;
#endif

namespace IAPurchasing
{
    public class IAPController: IIAPService
        #if UNITY_IAP
        , IStoreListener
#endif
    {
        private const string BETATEST_BUNDLE = "betatest_bundle";

        public void BuyProductID(string productId, Action action)
        {
            throw new NotImplementedException();
        }

        public bool CheckBuyState(string id)
        {
            throw new NotImplementedException();
        }

        public string GetBetatestBundleId()
        {
            return BETATEST_BUNDLE;
        }

        public void IAPInitializate()
        {
            throw new NotImplementedException();
        }

        #if UNITY_IAP

        private Action _purchased;
        private IStoreController _storeController;

        public IStoreController StoreController => _storeController;

        private IExtensionProvider _storeExtensionProvider;

        private bool _isSubscribeEnable;

        
        public void IAPInitializate()
        {
            if (IsIAPInitialized())
                return;
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder
                .AddProduct(BETATEST_BUNDLE, ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsIAPInitialized()
        {
            return _storeController != null && _storeExtensionProvider != null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
        }
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (_purchased != null) _purchased();
            _purchased = null;
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            _purchased = null;
        }

        /// <summary>
        /// Ïðîâåðèòü, êóïëåí ëè òîâàð.
        /// </summary>
        /// <param name="id">Èíäåêñ òîâàðà â ñïèñêå.</param>
        /// <returns></returns>
        public bool CheckBuyState(string id)
        {
            Product product = _storeController.products.WithID(id);
            if (product.hasReceipt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _storeExtensionProvider = extensions;
            //I2.Loc.GlobalParameters.SetString("PriceWeek", _storeController.products.WithStoreSpecificID("weekly_subscription").metadata.localizedPrice.ToString());
        }

        public void BuyProductID(string productId, Action action)
        {
            Debug.Log("Try to buy: " + productId);
            if (IsIAPInitialized())
            {
                _purchased = action;
                Product product = _storeController.products.WithID(productId);
                if (product is {availableToPurchase: true})
                {
                    _storeController.InitiatePurchase(product);
                }
            }
        }
#endif
    }
}
