using Analytic.Interfaces;
using Cards.Interfaces;
using GameScene;
using GameScene.Interfaces;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using Network.Interfaces;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Vibration.Interfaces;
using Zenject;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Network
{
    public class MasterConnectorManager : MonoBehaviourPunCallbacks, IMasterConnectorService
    {
        private bool isConnected;
        private bool _isSearchRequested;
        private bool _isJoinRequestSent;
        private bool _connectAfterDisconnect;
        private bool _isLoadingGame;

        #region Dependency

        private ICardList _cardList;
        private IVibrationService _vibrationService;
        private IMatchEventsAnalyticService _matchEventsAnalyticService;
        private IGameSceneService _gameSceneService;
        private IGameTypeService _gameTypeService;

        [Inject]
        private void Construct(ICardList cardList, IVibrationService vibrationService,
            IMatchEventsAnalyticService matchEventsAnalyticService, IGameSceneService gameSceneService,
            IGameTypeService gameTypeService)
        {
            _cardList = cardList;
            _vibrationService = vibrationService;
            _matchEventsAnalyticService = matchEventsAnalyticService;
            _gameSceneService = gameSceneService;
            _gameTypeService = gameTypeService;
        }

        #endregion

        private void Start()
        {
            isConnected = PhotonNetwork.IsConnectedAndReady;

            // A multiplayer connection should not remain active while the player is
            // simply sitting in the main menu.
            if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
                PhotonNetwork.Disconnect();
        }

        private void ConnectToMaster()
        {
            isConnected = PhotonNetwork.IsConnectedAndReady;

            if (isConnected)
            {
                StartMatchmaking();
                return;
            }

            ClientState state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.Disconnecting || state == ClientState.Leaving ||
                state == ClientState.DisconnectingFromGameServer)
            {
                _connectAfterDisconnect = true;
                return;
            }

            if (state != ClientState.PeerCreated && state != ClientState.Disconnected)
                return;

            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);

            // Use a copied settings object so ConnectUsingSettings initializes the
            // AppId/protocol correctly while still pinning matchmaking to one region.
            AppSettings appSettings = PhotonNetwork.PhotonServerSettings.AppSettings.CopyTo(new AppSettings());
            appSettings.FixedRegion = "us";
            appSettings.AppVersion = Application.version;

            if (!PhotonNetwork.ConnectUsingSettings(appSettings))
                Debug.LogError("Could not start connection to the Photon master server.");
        }

        public void StartSearchRoom()
        {
            if (_isSearchRequested || _isLoadingGame)
                return;

            _isSearchRequested = true;
            _isJoinRequestSent = false;
            ConnectToMaster();
            Debug.Log("Multiplayer search requested.");
        }

        private void StartMatchmaking()
        {
            if (!_isSearchRequested || _isJoinRequestSent || !PhotonNetwork.IsConnectedAndReady)
                return;

            _isJoinRequestSent = PhotonNetwork.JoinRandomOrCreateRoom(
                roomName: Random.Range(1000, 9999).ToString(),
                roomOptions: new RoomOptions {MaxPlayers = 2});

            if (!_isJoinRequestSent)
            {
                Debug.LogError("Photon rejected the matchmaking request.");
                StopSearch();
            }
        }

        public void StopSearch()
        {
            _isSearchRequested = false;
            _isJoinRequestSent = false;
            _connectAfterDisconnect = false;

            if (PhotonNetwork.InRoom)
                PhotonConnectionLifecycle.LeaveRoomAndDisconnect();
            else if (PhotonNetwork.NetworkClientState != ClientState.PeerCreated &&
                     PhotonNetwork.NetworkClientState != ClientState.Disconnected)
                PhotonNetwork.Disconnect();

            isConnected = PhotonNetwork.IsConnectedAndReady;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            TryStartMultiplayerMatch();
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            _isJoinRequestSent = false;

            if (!_isSearchRequested)
            {
                PhotonConnectionLifecycle.LeaveRoomAndDisconnect();
                return;
            }

            TryStartMultiplayerMatch();
        }

        private void TryStartMultiplayerMatch()
        {
            if (!_isSearchRequested || _isLoadingGame || !PhotonNetwork.InRoom ||
                PhotonNetwork.CurrentRoom.PlayerCount != 2)
                return;

            _isLoadingGame = true;
            _isSearchRequested = false;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CurrentRoom.IsOpen = false;

            // The value is changed to false only for a normal completed match.
            // A manual/abrupt exit therefore remains distinguishable to the opponent.
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                {"isPreExit", true}
            });

            _gameTypeService.SetGameType(GameType.MultiplayerHuman);
            _vibrationService.Vibrate(500);
            _matchEventsAnalyticService.Player_Found_Match(SearchingEnemyWindow.TimePass);
            _matchEventsAnalyticService.Player_Start_Match(GameType.MultiplayerHuman, _cardList.GetCardList());
            _gameSceneService.BeginLoadGameScene(GameSceneManager.GameScene.Game);
            _gameSceneService.BeginTransaction();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("Connected" + PhotonNetwork.NickName);
            isConnected = PhotonNetwork.IsConnectedAndReady;
            _connectAfterDisconnect = false;
            StartMatchmaking();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            isConnected = false;
            _isJoinRequestSent = false;

            if (_isSearchRequested && _connectAfterDisconnect)
            {
                _connectAfterDisconnect = false;
                ConnectToMaster();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            HandleMatchmakingFailure(returnCode, message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            HandleMatchmakingFailure(returnCode, message);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            HandleMatchmakingFailure(returnCode, message);
        }

        private void HandleMatchmakingFailure(short returnCode, string message)
        {
            Debug.LogError($"Matchmaking failed ({returnCode}): {message}");
            StopSearch();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log(
                    $"Is in master :{PhotonNetwork.IsConnectedAndReady}. Is local :{PhotonNetwork.IsConnectedAndReady}. Player count on master :{PhotonNetwork.CountOfPlayersOnMaster}");
                try
                {
                    Debug.Log(
                        $"Is in master :{PhotonNetwork.CurrentRoom} Player count :{PhotonNetwork.CurrentRoom.PlayerCount}");
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"Pause {pauseStatus}");
            if (!pauseStatus && _isSearchRequested)
                ConnectToMaster();
        }

        public bool GetIsConnectedToMaster()
        {
            return isConnected;
        }
    }
}
