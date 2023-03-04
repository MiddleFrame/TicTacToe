using System.Threading.Tasks;
using Analytic;
using Cards.Interfaces;
using GameScene;
using GameState;
using Managers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Network
{
    public class MasterConectorManager : MonoBehaviourPunCallbacks
    {
        public static bool isConnected = false;

        private ICardList _cardList;
        
        [Inject]
        private void Construct(ICardList cardList)
        {
            _cardList = cardList;
        }
        
        void Start()
        {
            ConnectToMaster();
        }

        public static void ConnectToMaster()
        {
            isConnected = PhotonNetwork.IsConnectedAndReady;
            MainMenuUI.Instance.UpdateNetworkUI(isConnected);
            if (!isConnected && PhotonNetwork.NetworkClientState!= ClientState.Leaving)
            {
                PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
                PhotonNetwork.GameVersion = Application.version;
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.ConnectToRegion("us");
                //isConnected = PhotonNetwork.IsConnectedAndReady;
            }
        }

        public static void StartSearchRoom()
        {
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: Random.Range(1000, 9999).ToString(),
                roomOptions: new Photon.Realtime.RoomOptions {MaxPlayers = 2});
            Debug.Log("Start search!");
        }

        public static void StopSearch()
        {
            if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
            else CancelJoinRoom();
            isConnected = PhotonNetwork.IsConnectedAndReady;
            MainMenuUI.Instance.UpdateNetworkUI(isConnected);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            Debug.Log("Async enter room");
            Debug.Log("Connect player!");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.CurrentRoom.IsOpen = false;
                GameplayManager.TypeGame = GameplayManager.GameType.MultiplayerHuman;

                Vibration.Vibrate(500);
                AnalyticController.Player_Found_Match(SearchingEnemyWindow.TimePass);
                AnalyticController.Player_Start_Match(GameplayManager.GameType.MultiplayerHuman, _cardList.GetCardList());
                GameSceneManager.Instance.BeginLoadGameScene(GameSceneManager.GameScene.Game);
                GameSceneManager.Instance.BeginTransaction();
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Debug.Log("Async enter room");
            Debug.Log("Connect player!");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.CurrentRoom.IsOpen = false;
                GameplayManager.TypeGame = GameplayManager.GameType.MultiplayerHuman;

                Vibration.Vibrate(500);
                AnalyticController.Player_Found_Match(SearchingEnemyWindow.TimePass);
                AnalyticController.Player_Start_Match(GameplayManager.GameType.MultiplayerHuman, _cardList.GetCardList());
                GameSceneManager.Instance.BeginLoadGameScene(GameSceneManager.GameScene.Game);
                GameSceneManager.Instance.BeginTransaction();
            }
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("Connected" + PhotonNetwork.NickName);
            isConnected = PhotonNetwork.IsConnectedAndReady;
            MainMenuUI.Instance.UpdateNetworkUI(isConnected);
        }

        private static async void CancelJoinRoom()
        {
            Debug.Log("Async enter");
            while (!PhotonNetwork.InRoom)
            {
                Debug.Log("Async wait");
                await Task.Yield();
            }

            PhotonNetwork.LeaveRoom();
            Debug.Log("Async end");
            isConnected = PhotonNetwork.IsConnectedAndReady;
            MainMenuUI.Instance.UpdateNetworkUI(isConnected);
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
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"Pause {pauseStatus}");
            if (!pauseStatus) ConnectToMaster();
        }
    }
}