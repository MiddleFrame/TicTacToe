using System.Collections.Generic;
using Score.Interfaces;
using UIPages.Interfaces;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreManager : IScoreService, IScoreWinnerService
    {
        private readonly Dictionary<int, int> _currentScoreList = new Dictionary<int, int>();

        private readonly Dictionary<int, int> _winScoreList = new Dictionary<int, int>();

        private List<int> _roundWinner = new List<int>();

        private const int ROUND_FOR_WIN = 2;
        private const int MAX_ROUNDS = 3;

        public int GetScore(int player)
        {
            return _currentScoreList[player];
        }

        public int GetMaxScore(int player)
        {
            return _winScoreList[player];
        }

        public void SetScore(int player, int value)
        {
            _currentScoreList[player] = value;
        }

        public void AddScore(int player, int value)
        {
            _currentScoreList[player] += value;
        }


        public void AddPlayer(int player)
        {
            _currentScoreList.Add(player, 0);
            _winScoreList.Add(player, 20);
        }

        public void RemovePlayer(int player)
        {
            _currentScoreList.Remove(player);
            _winScoreList.Remove(player);
        }

        public void ClearAllScore()
        {
            List<int> keyList = new List<int>(_currentScoreList.Keys);

            foreach (int key in keyList)
            {
                _currentScoreList[key] = 0;
            }
        }

        public bool IsExistRoundWinner()
        {
            foreach (int res in _currentScoreList.Keys)
            {
                if (_currentScoreList[res] >= _winScoreList[res]) return true;
            }

            return false;
        }


        public int GetRoundWinner()
        {
            int maxScore = -1;
            int maxKey = -1;
            foreach (int res in _currentScoreList.Keys)
            {
                Debug.Log($"res: {res}. score: {_currentScoreList[res]}");
                if (_currentScoreList[res] > maxScore)
                {
                    maxScore = _currentScoreList[res];
                    maxKey = res;
                }
                else if (_currentScoreList[res] == maxScore)
                {
                    maxKey = -1;
                }
            }

            return maxKey;
        }

        public bool IsExistGameWinner()
        {
            int p1Count = GetCountRoundWin(1);
            int p2Count = GetCountRoundWin(2);
            return (p1Count == ROUND_FOR_WIN || p2Count == ROUND_FOR_WIN || _roundWinner.Count == MAX_ROUNDS);
        }

        public int GetGameWinner()
        {
            int p1Count = _roundWinner.FindAll(x => x == 1).Count;
            int p2Count = _roundWinner.FindAll(x => x == 2).Count;
            if (p1Count == ROUND_FOR_WIN || (_roundWinner.Count == MAX_ROUNDS && p1Count > p2Count)) return 1;
            else if (p2Count == ROUND_FOR_WIN || (_roundWinner.Count == MAX_ROUNDS && p2Count > p1Count)) return 2;
            else return -1;
        }

        public void AddRoundWinner(int Side)
        {
            _roundWinner.Add(Side);
        }

        public void ClearRoundWinners()
        {
            _roundWinner = new List<int>();
        }

        public int GetCountRoundWin(int side)
        {
            return _roundWinner.FindAll(x => x == side).Count;
        }

        public int GetRoundCount()
        {
            return _roundWinner.Count;
        }
    }
}