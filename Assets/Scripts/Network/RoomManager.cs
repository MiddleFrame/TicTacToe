using System;
using GameState.Interfaces;
using Network.Interfaces;
using Photon.Pun;
using Photon.Realtime;
using Players.Interfaces;
using Score.Interfaces;
using UnityEngine;
using Zenject;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Network
{
    public class RoomManager : MonoBehaviourPunCallbacks, IRoomService
    {
        private Action<bool,int> _onPlayerLeaveAction;

        public bool GetIsOwnRoom()
        {
            return PhotonNetwork.IsMasterClient;
        }

        public int GetCurrentPlayerSide()
        {
            Debug.Log("Current Player side" + PhotonNetwork.LocalPlayer.ActorNumber);
            return PhotonNetwork.LocalPlayer.ActorNumber;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            _onPlayerLeaveAction?.Invoke(otherPlayer.CustomProperties["isPreExit"] == null || (bool) otherPlayer.CustomProperties["isPreExit"],otherPlayer.ActorNumber);
        }

        public void LeaveRoom(bool isPreExit)
        {
            if (!PhotonNetwork.InRoom) return;

            Hashtable hash = new Hashtable
            {
                {"isPreExit", isPreExit}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["isPreExit"]);
            PhotonConnectionLifecycle.LeaveRoomAndDisconnect();
        }

        public void SetPlayerLeaveAction(Action<bool,int> action)
        {
            _onPlayerLeaveAction = action;
        }
    }
}
