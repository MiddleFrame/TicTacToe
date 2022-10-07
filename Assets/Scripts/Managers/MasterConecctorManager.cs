using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace Managers {

    public class MasterConecctorManager : MonoBehaviourPunCallbacks
    {

        public static bool IsConnected = false;
        void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
                PhotonNetwork.GameVersion = Application.version;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public static void StartSearchRoom()
        {
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: Random.Range(1000, 9999).ToString(), roomOptions: new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
            Debug.Log("Start search!");
        }

        public static void StopSearh()
        {
            PhotonNetwork.LeaveRoom();
        }

        public  override  void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.CurrentRoom.IsOpen = false;
                GameplayManager.TypeGame = GameplayManager.GameType.MultiplayerHuman;
                GameSceneManager.Instance.SetGameScene(GameSceneManager.GameScene.Game);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.CurrentRoom.IsOpen = false;
                GameplayManager.TypeGame = GameplayManager.GameType.MultiplayerHuman;
                GameSceneManager.Instance.SetGameScene(GameSceneManager.GameScene.Game);
            }
        }

        public override void OnConnectedToMaster()
        {
            IsConnected = true;
            base.OnConnectedToMaster();
            Debug.Log("Connected" + PhotonNetwork.NickName);
            MainMenuUI.Instance.UpdateNetworkUI(true);
        }
    }

}