using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    private List<PlayerInfo> _players = new List<PlayerInfo>();

    private int _currentPlayer=0;


    private void Awake()
    {
        AddPlayer(PlayerType.Human);
        AddPlayer(PlayerType.AI);

    }

    public void AddPlayer(PlayerType type)
    {
        PlayerInfo player = new PlayerInfo();
        player.EntityType = type;
        _players.Add(player);
        player.SideId = _players.Count;
    }

    public PlayerInfo GetCurrentPlayer()
    {
        return _players[_currentPlayer];
    }

    public void NextPlayer()
    {
        _currentPlayer += 1;
        if (_currentPlayer == _players.Count) _currentPlayer = 0;
    }

}