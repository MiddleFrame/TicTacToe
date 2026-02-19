using GameScene.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScene
{
    public class GameSceneManager : IGameSceneService
    {
        private static AsyncOperation _asyncOperation = null;

        private string _currentAsyncLoad = "";

        public void BeginLoadGameScene(GameScene state)
        {
            switch (state)
            {
                case GameScene.Game:
                    _currentAsyncLoad = "GameplayScene";
                    break;
                case GameScene.MainMenu:
                    _currentAsyncLoad = "MainMenu";
                    break;
                case GameScene.Tutorial:
                    _currentAsyncLoad = "TutorialScene";
                    break;
            }

            _asyncOperation = SceneManager.LoadSceneAsync(_currentAsyncLoad);
            _asyncOperation.allowSceneActivation = false;
        }

        public void BeginTransaction()
        {
            _currentAsyncLoad = "";
            _asyncOperation.allowSceneActivation = true;
            _asyncOperation = null;
        }


        public void LoadGameScene(GameScene state)
        {
            switch (state)
            {
                case GameScene.Game:
                    SceneManager.LoadScene("GameplayScene");
                    break;
                case GameScene.MainMenu:
                    SceneManager.LoadScene("MainMenu");
                    break;
                case GameScene.Tutorial:
                    SceneManager.LoadScene("TutorialScene");
                    break;
            }

        }

        public enum GameScene
        {
            MainMenu,
            Game,
            Tutorial
        }
    }
}