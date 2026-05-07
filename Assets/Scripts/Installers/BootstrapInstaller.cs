using Analytic;
using CardCollection;
using Cards;
using Coin;
using Coroutine;
using DevMode;
using GameScene;
using GameTypeService;
using IAPurchasing;
using IAPurchasing.Interfaces;
using Network;
using Settings;
using Settings.Interfaces;
using Tutorial;
using UnityEngine;
using Vibration.Interfaces;
using Zenject;

public class BootstrapInstaller : MonoInstaller, IInitializable
{
    [SerializeField]
    private CoroutineQueueController _coroutineQueueController;
    
    public override void InstallBindings()
    {
        BindInstaller();
        BindDeckData();
        BindGameScene();
        BindVibration();
        BindGameType();
        BindCoins();
        BindAnalyticEvents();
        BindTutorialData();
        BindIAPurchase();
        BindCoroutineQueue();
        BindLanguage();
        BindDevMode();
        BindCollectionData();
    }

    private void BindCollectionData()
    {
        Container.BindInterfacesAndSelfTo<CollectionData>().AsSingle();
    }

    private void BindLanguage()
    {
        Container.BindInterfacesAndSelfTo<SettingsDataHolder>().AsSingle();
    }

    private void BindDevMode()
    {
        Container.BindInterfacesAndSelfTo<DevModeService>().AsSingle();
    }

    private void BindCoroutineQueue()
    {
        Container.BindInterfacesTo<CoroutineQueueController>().FromInstance(_coroutineQueueController).AsSingle();
    }

    private void BindIAPurchase()
    {
        Container.BindInterfacesAndSelfTo<IAPController>().AsSingle();
    }

    private void BindTutorialData()
    {
        Container.BindInterfacesAndSelfTo<TutorialCompleteCompleteController>().AsSingle();
    }

    private void BindAnalyticEvents()
    {
        Container.BindInterfacesAndSelfTo<AnalyticController>().AsSingle();
    }

    private void BindInstaller()
    {
        Container.BindInterfacesTo<BootstrapInstaller>().FromInstance(this).AsSingle();
    }

    private void BindCoins()
    {
        Container.BindInterfacesAndSelfTo<CoinController>().AsSingle();
    }

    private void BindGameType()
    {
        Container.BindInterfacesAndSelfTo<GameTypeController>().AsSingle();
    }
    
    private void BindVibration()
    {
        Container.BindInterfacesAndSelfTo<VibrationService>().AsSingle().NonLazy();
    }

    private void BindGameScene()
    {
        Container.BindInterfacesAndSelfTo<GameSceneManager>().AsSingle();
    }

    private void BindDeckData()
    {
        Container.BindInterfacesAndSelfTo<DeckData>().AsSingle();
    }

    public void Initialize()
    {
        SetApplicationSettings();
    }
    
    private void SetApplicationSettings()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Container.Resolve<IVibrationService>().Init();
        Container.Resolve<ISettingsDataService>().LoadLanguage();
        Container.Resolve<ISettingsDataService>().LoadCellClearAnimationType();
        Container.Resolve<ISettingsDataService>().LoadDevModeState();
#if UNITY_IAP
        Container.Resolve<IIAPService>().IAPInitializate();
#endif
    }
}