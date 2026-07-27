using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

namespace Network
{
    /// <summary>
    /// Completes the room leave handshake before closing the Photon connection.
    /// The callback target is not tied to a scene, so it also survives a scene
    /// transition started immediately after LeaveRoom.
    /// </summary>
    public static class PhotonConnectionLifecycle
    {
        private static readonly DisconnectAfterLeaveCallback Callback = new DisconnectAfterLeaveCallback();
        private static bool _isWaitingForLeave;

        public static void LeaveRoomAndDisconnect()
        {
            if (!PhotonNetwork.InRoom)
            {
                Disconnect();
                return;
            }

            if (_isWaitingForLeave)
                return;

            _isWaitingForLeave = true;
            PhotonNetwork.NetworkingClient.AddCallbackTarget(Callback);

            if (!PhotonNetwork.LeaveRoom(false))
            {
                StopWaitingForLeave();
                Disconnect();
            }
        }

        private static void Disconnect()
        {
            if (PhotonNetwork.NetworkClientState != ClientState.PeerCreated &&
                PhotonNetwork.NetworkClientState != ClientState.Disconnected &&
                PhotonNetwork.NetworkClientState != ClientState.Disconnecting)
                PhotonNetwork.Disconnect();
        }

        private static void StopWaitingForLeave()
        {
            if (!_isWaitingForLeave)
                return;

            _isWaitingForLeave = false;
            PhotonNetwork.NetworkingClient.RemoveCallbackTarget(Callback);
        }

        private sealed class DisconnectAfterLeaveCallback : IMatchmakingCallbacks
        {
            public void OnLeftRoom()
            {
                StopWaitingForLeave();
                Disconnect();
            }

            public void OnFriendListUpdate(List<FriendInfo> friendList)
            {
            }

            public void OnCreatedRoom()
            {
            }

            public void OnCreateRoomFailed(short returnCode, string message)
            {
            }

            public void OnJoinedRoom()
            {
            }

            public void OnJoinRoomFailed(short returnCode, string message)
            {
            }

            public void OnJoinRandomFailed(short returnCode, string message)
            {
            }
        }
    }
}
