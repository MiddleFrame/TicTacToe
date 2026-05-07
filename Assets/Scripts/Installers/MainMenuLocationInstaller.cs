using CardCollection;
using Cards;
using DevMode;
using Network;
using UIElements;
using UIPages;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainMenuLocationInstaller : MonoInstaller
    {
    [SerializeField]
        private MasterConnectorManager _masterConnectorManager;

        [SerializeField]
        private MainMenuUI _mainMenuUI;

        [SerializeField]
        private CollectionManager _collectionManager;
        
        [SerializeField]
        private BuyingAnimationController _buyingAnimationController;
        
        public override void InstallBindings()
        {
            BindMasterConnector();
            BindMainMenuUI();
            BindCollectionManager();
            BindCollectionDevMode();
            BindCollectionFactory();
            BindBuyingAnimation();
        }

        private void BindCollectionDevMode()
        {
            Container.BindInterfacesAndSelfTo<DevCollectionService>().AsSingle();
        }

        private void BindBuyingAnimation()
        {
            Container.BindInterfacesTo<BuyingAnimationController>().FromInstance(_buyingAnimationController).AsSingle();
        }

        private void BindCollectionFactory()
        {
            Container.BindInterfacesAndSelfTo<CardCollectionFactory>().AsSingle();
        }

        private void BindCollectionManager()
        {
            Container.BindInterfacesTo<CollectionManager>().FromInstance(_collectionManager).AsSingle();
        }

        private void BindMainMenuUI()
        {
            Container.BindInterfacesTo<MainMenuUI>().FromInstance(_mainMenuUI).AsSingle();
        }


        private void BindMasterConnector()
        {
            Container.BindInterfacesTo<MasterConnectorManager>().FromInstance(_masterConnectorManager).AsSingle();
        }
    }
}