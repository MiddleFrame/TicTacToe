using ActionInstaller;
using AI;
using Area;
using Cards;
using Cards.CustomType;
using Effects;
using Emotes;
using Field;
using FinishLine;
using FinishLine.Interfaces;
using GameState;
using History;
using ScreenScaler;
using Mana;
using Network;
using Players;
using Players.Interfaces;
using Score;
using ScreenScaler.Interfaces;
using Settings.Interfaces;
using Theme;
using TurnTimer;
using UIPages;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GameSceneLocationInstaller : MonoInstaller
    {
        [SerializeField]
        private FieldController _fieldController;

        [SerializeField]
        private CardPoolController _cardPoolController;

        [SerializeField]
        private AIController _aiController;

        [SerializeField]
        private NetworkEventManager _networkEventManager;

        [SerializeField]
        private EffectManager _effectManager;

        [SerializeField]
        private FinishLineManager _finishLineManager;

        [SerializeField]
        private ManaUIController _manaUIController;

        [SerializeField]
        private HistoryFactory _historyFactory;

        [SerializeField]
        private InGameUI _inGameUI;

        [SerializeField]
        private TurnTimerController _turnTimerController;

        [SerializeField]
        private RoomManager _roomManager;

        [SerializeField]
        private ThemeManager _themeManager;

        [SerializeField]
        private HistoryCardView _historyCardView;
        
        [SerializeField]
        private EmoteManager _emoteManager;
        
        public override void InstallBindings()
        {
            BindManaManager();
            BindCardFactory();
            BindScaler();
            BindScore();
            BindAreaService();
            BindFieldController();
            BindPlayerService();
            BindCardPoolController();
            BindGameplayController();
            BindAIController();
            BindNetworkEventService();
            BindEffectController();
            BindFinishLine();
            BindFinishLineMasterChecker();
            BindCellClearAnimation();
            BindManaUI();
            BindHistoryFactory();
            BindInGameUI();
            BindTurnTimer();
            BindRoomManager();
            BindThemeManager();
            BindCardActions();
            BindFieldFactory();
            BindLocationAction();
            BindFinishLineFactory();
            BindHistoryView();
            BindEmoteManager();
        }

        private void BindEmoteManager()
        {
            Container.BindInterfacesTo<EmoteManager>().FromInstance(_emoteManager).AsSingle();
        }

        private void BindHistoryView()
        {
            Container.BindInterfacesTo<HistoryCardView>().FromInstance(_historyCardView).AsSingle();
        }

        private void BindFinishLineFactory()
        {
            Container.BindInterfacesAndSelfTo<FinishLineFactory>().AsSingle();
        }

        private void BindLocationAction()
        {
            Container.BindInterfacesAndSelfTo<GameSceneActionBinder>().AsSingle();
        }

        private void BindFieldFactory()
        {
            Container.BindInterfacesAndSelfTo<FieldFactory>().AsSingle();
        }

        private void BindCardActions()
        {
            Container.BindInterfacesAndSelfTo<CardActions>().AsSingle();
        }

        private void BindThemeManager()
        {
            Container.BindInterfacesTo<ThemeManager>().FromInstance(_themeManager).AsSingle();
        }

        private void BindRoomManager()
        {
            Container.BindInterfacesTo<RoomManager>().FromInstance(_roomManager).AsSingle();
        }

        private void BindTurnTimer()
        {
            Container.BindInterfacesTo<TurnTimerController>().FromInstance(_turnTimerController).AsSingle();
        }

        private void BindInGameUI()
        {
            Container.BindInterfacesTo<InGameUI>().FromInstance(_inGameUI).AsSingle();
        }

        private void BindHistoryFactory()
        {
            Container.BindInterfacesTo<HistoryFactory>().FromInstance(_historyFactory).AsSingle();
        }

        private void BindManaUI()
        {
            Container.BindInterfacesTo<ManaUIController>().FromInstance(_manaUIController).AsSingle();
        }

        private void BindFinishLine()
        {
            Container.BindInterfacesTo<FinishLineManager>().FromInstance(_finishLineManager).AsSingle();
        }

        private void BindFinishLineMasterChecker()
        {
            Container.BindInterfacesAndSelfTo<FinishLineMasterChecker>().AsSingle();
        }

        private void BindCellClearAnimation()
        {
            ISettingsDataService settingsDataService = Container.Resolve<ISettingsDataService>();
            switch (settingsDataService.GetCellClearAnimationType())
            {
                case ISettingsDataService.CellClearAnimationType.FlyingToScore:
                    Container.Bind<ICellClearAnimationService>().To<FlyingToScoreCellAnimation>().AsSingle();
                    break;
                case ISettingsDataService.CellClearAnimationType.FinishLine:
                default:
                    Container.Bind<ICellClearAnimationService>().To<FinishLineLineCellAnimation>().AsSingle();
                    break;
            }
        }

        private void BindEffectController()
        {
            Container.BindInterfacesTo<EffectManager>().FromInstance(_effectManager).AsSingle();
        }

        private void BindNetworkEventService()
        {
            Container.BindInterfacesTo<NetworkEventManager>().FromInstance(_networkEventManager).AsSingle();
        }

        private void BindAIController()
        {
            Container.BindInterfacesTo<AIController>().FromInstance(_aiController).AsSingle();
        }

        private void BindGameplayController()
        {
            Container.BindInterfacesAndSelfTo<GameplayManager>().AsSingle();
        }

        private void BindCardPoolController()
        {
            Container.BindInterfacesTo<CardPoolController>().FromInstance(_cardPoolController).AsSingle();
        }

        private void BindPlayerService()
        {
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
        }

        private void BindFieldController()
        {
            Container.BindInterfacesTo<FieldController>().FromInstance(_fieldController).AsSingle();
        }

        private void BindAreaService()
        {
            Container.BindInterfacesAndSelfTo<AreaController>().AsSingle();
        }


        private void BindScore()
        {
            Container.BindInterfacesAndSelfTo<ScoreManager>().AsSingle();
        }

        private void BindScaler()
        {
            Container.BindInterfacesAndSelfTo<ScreenScalerService>().AsSingle();
        }

        private void BindCardFactory()
        {
            Container.BindInterfacesAndSelfTo<CardFactory>().AsSingle();
        }


        private void BindManaManager()
        {
            Container.BindInterfacesAndSelfTo<ManaController>().AsSingle();
        }
    }
}