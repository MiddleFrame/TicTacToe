using System;

namespace Roguelike.Interfaces
{
    public interface IRoguelikeUIService
    {
        void HideAll();
        void ShowVictoryReward(bool isFirstVictory, Action onComplete);
        void ShowDefeatSummary(Action returnToMenu);
    }
}
