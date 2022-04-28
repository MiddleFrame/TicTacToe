using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomController : MonoBehaviourPunCallbacks
{
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            TurnController.Restart();
            Debug.Log("2 players!!");
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            TurnController.Restart();
            Debug.Log("2 players!!");
        }
    }
}