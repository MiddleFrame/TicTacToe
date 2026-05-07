using Coin.Interfaces;
using SaveSystem;
using UnityEngine;

namespace Coin
{
    public class CoinController : ICoinService
    {
        private const string MONEY_KEY = "PlayerAllMoney";
        private const string COIN_SAVE_PATH = "CoinData";
        private const int COIN_PER_WIN = 20;
        private const int COIN_PER_UNLOCK = 50;
        private readonly BinarySaveSystem _coinSaveSystem;
        private CoinSaveData _coinSaveData;

        public CoinController()
        {
            _coinSaveSystem = new BinarySaveSystem(COIN_SAVE_PATH);
        }

        private void EnsureDataLoaded()
        {
            if (_coinSaveData != null) return;

            _coinSaveData = (CoinSaveData) _coinSaveSystem.Load();
            if (_coinSaveData != null) return;

            _coinSaveData = new CoinSaveData
            {
                CurrentMoney = PlayerPrefs.GetInt(MONEY_KEY, 0)
            };

            if (PlayerPrefs.HasKey(MONEY_KEY)) PlayerPrefs.DeleteKey(MONEY_KEY);
            _coinSaveSystem.Save(_coinSaveData);
        }

        public int GetCurrentMoney()
        {
            EnsureDataLoaded();
            return _coinSaveData.CurrentMoney;
        }

        public void SetCurrentMoney(int value)
        {
            EnsureDataLoaded();
            _coinSaveData.CurrentMoney = value;
            _coinSaveSystem.Save(_coinSaveData);
        }

        public int GetCoinPerWin()
        {
            return COIN_PER_WIN;
        }

        public int GetCoinPerUnlock()
        {
            return COIN_PER_UNLOCK;
        }
    }


}