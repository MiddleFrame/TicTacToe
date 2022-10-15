using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{

    public class TurnTimerManager : Singleton<TurnTimerManager>
    {
        public const float PlayerTurnTime = 20f;
        public const float BotTurnTime = 6f;

        private float _timeLeft = 0f;

        private IEnumerator _timerCoroutine;

        public float TimeLeft
        {
            get { return _timeLeft; }
        }

        public void StartNewTurnTimer(float TurnTime)
        {


            StopCoroutine(_timerCoroutine);
            _timeLeft = TurnTime;
            StartCoroutine(_timerCoroutine);

        }
        public void StartNewTurnTimer(PlayerType player, bool isNeedAddToQueue = true)
        {

            Debug.Log("Started timer");
            if (_timerCoroutine!=null)StopCoroutine(_timerCoroutine);
            switch (player)
            {
                case PlayerType.AI:
                    _timeLeft = BotTurnTime;
                    break;
                case PlayerType.Human:
                    _timeLeft = PlayerTurnTime;
                    break;
            }
            _timerCoroutine = ITurnTimer(isNeedAddToQueue);
            StartCoroutine(_timerCoroutine);

        }

        private IEnumerator ITurnTimer(bool isNeedAddtoQueue)
        {
            while (_timeLeft > 0)
            {
                yield return null;
                _timeLeft -= Time.deltaTime;
            }
            /*GameplayManager.Instance.SetGameplayState(GameplayState.NewTurn);*/
            if (isNeedAddtoQueue) { GameplayManager.Instance.SetGamePlayStateQueue(GameplayManager.GameplayState.NewTurn);
                NetworkEventManager.RaiseEventEndTurn();
            
            }
        }
    }
}