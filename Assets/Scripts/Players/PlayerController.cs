using System.Collections.Generic;
using Cards.CustomType;
using GameTypeService.Enums;
using Players.Interfaces;
using UnityEngine;

namespace Players
{
    public class PlayerController : IPlayerService
    {
        private List<PlayerInfo> _players = new();
        private int _currentPlayer;

        private GameType _gameType;
        private int _playerOnlineId = -1;

        public void AddPlayer(PlayerType type, List<CardModel> deck)
        {
            PlayerInfo player = new PlayerInfo
            {
                EntityType = type,
                SideId = _players.Count + 1,
                FullDeckPool = deck
            };

            player.DeckPool = new List<CardModel>(player.FullDeckPool);
            player.HandPool = new List<CardModel>();
            _players.Add(player);
        }

        public void AddPlayer(PlayerType type, int side, List<CardModel> deck)
        {
            PlayerInfo player = new PlayerInfo
            {
                EntityType = type,
                SideId = side,
                FullDeckPool = deck
            };
            player.DeckPool = new List<CardModel>(player.FullDeckPool);
            player.HandPool = new List<CardModel>();
            _players.Add(player);
        }

        public void SetGameType(GameType gameType)
        {
            _gameType = gameType;
        }

        public void SetOnlineId(int id)
        {
            _playerOnlineId = id;
        }

        public PlayerInfo GetCurrentPlayer()
        {
            try
            {
                return _players[_currentPlayer];
            }
            catch
            {
                return null;
            }
        }

        public void NextPlayer()
        {
            _currentPlayer += 1;
            if (_currentPlayer == _players.Count) _currentPlayer = 0;
        }

        public PlayerInfo GetNextPlayer()
        {
            int nextPlayer = _currentPlayer + 1;
            if (nextPlayer == _players.Count) nextPlayer = 0;
            return _players[nextPlayer];
        }

        public void ResetCurrentPlayer()
        {
            _currentPlayer = 0;
        }

        public int GetCurrentSideOnDevice()
        {
            int result = (_gameType == GameType.MultiplayerHuman) ? _playerOnlineId :
                (_gameType == GameType.SingleHuman) ? GetCurrentPlayer().SideId : 1;

            return result;
        }

        public PlayerInfo GetCurrentPlayerOnDevice()
        {
            PlayerInfo result = (_gameType == GameType.MultiplayerHuman) ? _players[_playerOnlineId - 1] :
                (_gameType == GameType.SingleHuman) ? GetCurrentPlayer() : _players[0];

            return result;
        }

        public List<PlayerInfo> GetPlayers()
        {
            return _players;
        }
    }
}
