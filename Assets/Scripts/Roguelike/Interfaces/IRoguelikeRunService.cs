using System.Collections.Generic;
using Cards.CustomType;

namespace Roguelike.Interfaces
{
    public interface IRoguelikeRunService
    {
        bool IsRunActive { get; }
        int StageIndex { get; }
        int Victories { get; }
        int MaximumMana { get; }
        int CardsPlayed { get; }
        int PlayerFiguresPlaced { get; }
        int EnemyFiguresPlaced { get; }
        RoguelikeStageDefinition CurrentStage { get; }
        IReadOnlyList<CardInfo> Deck { get; }

        void StartNewRun();
        void EndRun();
        void RegisterVictory();
        void RegisterCardPlayed();
        void RegisterPlayerFigurePlaced();
        void RegisterEnemyFigurePlaced();
        bool CanIncreaseMaximumMana();
        bool TryIncreaseMaximumMana();
        IReadOnlyList<CardInfo> CreateRewardChoices();
        void ReplaceFirstDefaultCard(CardInfo reward);
        void ReplaceDeckCard(int deckIndex, CardInfo reward);
    }
}
