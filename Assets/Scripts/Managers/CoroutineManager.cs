using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{

    public class CoroutineManager : Singleton<CoroutineManager>
    {

        private Queue<IEnumerator> _coroutineQueue = new Queue<IEnumerator>();

        public static bool IsQueueEmpty = true;

        void Start()
        {
            StartCoroutine(CoroutineCoordinator());
        }

        IEnumerator CoroutineCoordinator()
        {
            while (true)
            {
                while (_coroutineQueue.Count > 0)
                {
                    IsQueueEmpty = false;
                    Debug.Log(_coroutineQueue);
                    yield return StartCoroutine(_coroutineQueue.Dequeue());

                }
                yield return new WaitForEndOfFrame();
                IsQueueEmpty = true;
            }
        }

        public void AddCoroutine(IEnumerator coroutine)
        {
            IsQueueEmpty = false;
            _coroutineQueue.Enqueue(coroutine);            
        }

        public void AddAwaitTime(float time)
        {
            AddCoroutine(IAwaitProcess(time));
        }

        public IEnumerator IAwaitProcess(float time)
        {
            for (int i = 0; i < time; i++) yield return null;
        }

        public void ClearQueue()
        {
            _coroutineQueue.Clear();
        }
    }
}