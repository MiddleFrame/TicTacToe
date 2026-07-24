using System.Collections.Generic;
using System.Linq;
using Cards.CustomType;
using Roguelike.Interfaces;
using UnityEngine;

namespace Roguelike
{
    public class RoguelikeRunController : IRoguelikeRunService
    {
        private const string ConfigPath = "RoguelikeConfig";
        private const string CardsPath = "Cards";

        private readonly RoguelikeConfig _config;
        private readonly List<CardInfo> _deck = new();
        private readonly List<CardInfo> _rewardPool;

        private CardInfo _defaultCard;

        public bool IsRunActive { get; private set; }
        public int StageIndex { get; private set; }
        public int Victories { get; private set; }
        public int MaximumMana { get; private set; }
        public int CardsPlayed { get; private set; }
        public int PlayerFiguresPlaced { get; private set; }
        public int EnemyFiguresPlaced { get; private set; }
        public RoguelikeStageDefinition CurrentStage => _config.GetStage(StageIndex);
        public IReadOnlyList<CardInfo> Deck => _deck;

        public RoguelikeRunController()
        {
            _config = Resources.Load<RoguelikeConfig>(ConfigPath);
            if (_config == null)
            {
                Debug.LogError($"Roguelike config is missing at Resources/{ConfigPath}.asset");
                _config = ScriptableObject.CreateInstance<RoguelikeConfig>();
            }

            _rewardPool = Resources.LoadAll<CardInfo>(CardsPath)
                .Where(card => card != null && card.isEnabled && card != _config.DefaultCard)
                .OrderBy(card => card.CardId)
                .ToList();
        }

        public void StartNewRun()
        {
            IsRunActive = true;
            StageIndex = 0;
            Victories = 0;
            MaximumMana = 0;
            CardsPlayed = 0;
            PlayerFiguresPlaced = 0;
            EnemyFiguresPlaced = 0;
            _deck.Clear();

            CardInfo defaultCard = _config.DefaultCard;
            if (defaultCard == null)
            {
                defaultCard = Resources.LoadAll<CardInfo>(CardsPath)
                    .FirstOrDefault(card => card.СardActionId == Cards.Enum.CardActionType.PlaceFigure);
            }

            if (defaultCard == null)
            {
                Debug.LogError("Roguelike cannot start: the default place-figure card is missing.");
                IsRunActive = false;
                return;
            }

            _defaultCard = defaultCard;
            for (int i = 0; i < _config.DeckSize; i++)
            {
                _deck.Add(defaultCard);
            }
        }

        public void EndRun()
        {
            IsRunActive = false;
        }

        public void RegisterVictory()
        {
            Victories++;
            StageIndex++;
            if (Victories == 1)
            {
                MaximumMana = Mathf.Max(MaximumMana, 1);
            }
        }

        public void RegisterCardPlayed()
        {
            CardsPlayed++;
        }

        public void RegisterPlayerFigurePlaced()
        {
            PlayerFiguresPlaced++;
        }

        public void RegisterEnemyFigurePlaced()
        {
            EnemyFiguresPlaced++;
        }

        public bool CanIncreaseMaximumMana()
        {
            return MaximumMana < _config.MaximumMana;
        }

        public bool TryIncreaseMaximumMana()
        {
            if (!CanIncreaseMaximumMana()) return false;
            MaximumMana++;
            return true;
        }

        public IReadOnlyList<CardInfo> CreateRewardChoices()
        {
            int count = Mathf.Min(_config.RewardChoiceCount, _rewardPool.Count);
            List<CardInfo> shuffled = new(_rewardPool);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int swapIndex = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }

            return shuffled.Take(count).ToList();
        }

        public void ReplaceFirstDefaultCard(CardInfo reward)
        {
            if (reward == null || _deck.Count == 0) return;
            int defaultIndex = _deck.FindIndex(card => card == _defaultCard);
            ReplaceDeckCard(defaultIndex < 0 ? 0 : defaultIndex, reward);
        }

        public void ReplaceDeckCard(int deckIndex, CardInfo reward)
        {
            if (reward == null || deckIndex < 0 || deckIndex >= _deck.Count) return;
            _deck[deckIndex] = reward;
        }
    }
}
