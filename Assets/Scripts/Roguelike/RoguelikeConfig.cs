using System;
using System.Collections.Generic;
using Cards.CustomType;
using UnityEngine;

namespace Roguelike
{
    [CreateAssetMenu(fileName = "RoguelikeConfig", menuName = "TicTacToe/Roguelike Config")]
    public class RoguelikeConfig : ScriptableObject
    {
        [SerializeField]
        private CardInfo _defaultCard;

        [SerializeField, Min(1)]
        private int _deckSize = 10;

        [SerializeField, Min(1)]
        private int _rewardChoiceCount = 3;

        [SerializeField, Min(0)]
        private int _maximumMana = 6;

        [SerializeField]
        private List<RoguelikeStageDefinition> _stages = new();

        [Header("Cycle after configured stages")]
        [SerializeField]
        private RoguelikeEnemyTurn _smartCycleEnemy = new() { SmartFigures = 4 };

        [SerializeField]
        private RoguelikeEnemyTurn _randomCycleEnemy = new() { RandomFigures = 5 };

        public CardInfo DefaultCard => _defaultCard;
        public int DeckSize => _deckSize;
        public int RewardChoiceCount => _rewardChoiceCount;
        public int MaximumMana => _maximumMana;

        public RoguelikeStageDefinition GetStage(int stageIndex)
        {
            if (_stages.Count == 0)
            {
                return RoguelikeStageDefinition.CreateFallback(stageIndex);
            }

            if (stageIndex < _stages.Count)
            {
                return _stages[Mathf.Max(0, stageIndex)];
            }

            RoguelikeStageDefinition last = _stages[^1];
            RoguelikeEnemyTurn enemy = (stageIndex - _stages.Count) % 2 == 0
                ? _smartCycleEnemy
                : _randomCycleEnemy;

            return new RoguelikeStageDefinition
            {
                BoardSize = last.BoardSize,
                ScoreToWin = last.ScoreToWin,
                EnemyTurn = enemy
            };
        }
    }

    [Serializable]
    public class RoguelikeStageDefinition
    {
        [Min(3)]
        public int BoardSize = 3;

        [Min(1)]
        public int ScoreToWin = 3;

        public RoguelikeEnemyTurn EnemyTurn = new();

        public static RoguelikeStageDefinition CreateFallback(int stageIndex)
        {
            int boardSize = Mathf.Min(6, 3 + stageIndex / 2);
            int score = stageIndex == 0 ? 3 : stageIndex <= 2 ? 10 : stageIndex == 3 ? 15 : 20;
            RoguelikeEnemyTurn enemy = stageIndex switch
            {
                0 => new RoguelikeEnemyTurn { SmartFigures = 1 },
                1 => new RoguelikeEnemyTurn { SmartFigures = 2 },
                2 => new RoguelikeEnemyTurn { RandomFigures = 3 },
                3 => new RoguelikeEnemyTurn { SmartFigures = 2, RandomFigures = 1 },
                4 => new RoguelikeEnemyTurn { RandomFigures = 4 },
                _ when stageIndex % 2 == 1 => new RoguelikeEnemyTurn { SmartFigures = 4 },
                _ => new RoguelikeEnemyTurn { RandomFigures = 5 }
            };

            return new RoguelikeStageDefinition
            {
                BoardSize = boardSize,
                ScoreToWin = score,
                EnemyTurn = enemy
            };
        }
    }

    [Serializable]
    public struct RoguelikeEnemyTurn
    {
        [Min(0)]
        public int SmartFigures;

        [Min(0)]
        public int RandomFigures;

        public int TotalFigures => SmartFigures + RandomFigures;
    }
}
