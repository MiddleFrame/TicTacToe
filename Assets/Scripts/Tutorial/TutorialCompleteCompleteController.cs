using Tutorial.Interfaces;
using SaveSystem;
using UnityEngine;

namespace Tutorial
{
    public class TutorialCompleteCompleteController : ITutorialCompleteService
    {
        private const string TUTORIAL_KEY = "IsTutorialShowed";
        private const string TUTORIAL_SAVE_PATH = "TutorialData";

        private bool _isTutorialShowed;
        private readonly BinarySaveSystem _tutorialSaveSystem;
        private TutorialSaveData _tutorialSaveData;

        private TutorialCompleteCompleteController()
        {
            _tutorialSaveSystem = new BinarySaveSystem(TUTORIAL_SAVE_PATH);
            _tutorialSaveData = (TutorialSaveData) _tutorialSaveSystem.Load();

            if (_tutorialSaveData == null)
            {
                _tutorialSaveData = new TutorialSaveData
                {
                    IsTutorialShowed = PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1
                };

                if (PlayerPrefs.HasKey(TUTORIAL_KEY)) PlayerPrefs.DeleteKey(TUTORIAL_KEY);
                _tutorialSaveSystem.Save(_tutorialSaveData);
            }

            _isTutorialShowed = _tutorialSaveData.IsTutorialShowed;
        }

        public bool GetIsTutorialComplete()
        {
            return _isTutorialShowed;
        }

        public void SetIsTutorialComplete(bool state)
        {
            _isTutorialShowed = state;
            _tutorialSaveData.IsTutorialShowed = _isTutorialShowed;
            _tutorialSaveSystem.Save(_tutorialSaveData);
        }
    }
}