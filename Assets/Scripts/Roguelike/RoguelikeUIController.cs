using System;
using System.Collections.Generic;
using CardCollection;
using Cards.CustomType;
using Roguelike.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Roguelike
{
    public class RoguelikeUIController : MonoBehaviour, IRoguelikeUIService
    {
        [Header("Reward choice")]
        [SerializeField]
        private GameObject _upgradeChoicePanel;

        [SerializeField]
        private Button _increaseManaButton;

        [SerializeField]
        private GameObject _cardPanel;

        [SerializeField]
        private TextMeshProUGUI _cardPanelTitle;

        [SerializeField]
        private Transform _rewardCardParent;

        [SerializeField]
        private Transform _deckCardParent;

        [SerializeField, Min(0.01f)]
        private float _rewardCardScale = 1.63f;

        [SerializeField, Min(0.01f)]
        private float _deckCardScale = 1.15f;

        [SerializeField]
        private Button _cardPanelCloseButton;

        [Header("Defeat")]
        [SerializeField]
        private GameObject _summaryPanel;

        [SerializeField]
        private TextMeshProUGUI _summaryText;

        [Header("Shared card prefab")]
        [SerializeField]
        private CardCollectionUIObject _cardChoicePrefab;

        private readonly List<GameObject> _spawnedCards = new();
        private IReadOnlyList<CardInfo> _rewardChoices;
        private CardInfo _pendingReward;
        private Action _rewardCompleteAction;
        private Action _returnToMenuAction;
        private bool _isFirstVictory;

        private IRoguelikeRunService _runService;
        private DiContainer _container;

        [Inject]
        private void Construct(IRoguelikeRunService runService, DiContainer container)
        {
            _runService = runService;
            _container = container;
        }

        private void Awake()
        {
            HideAll();
        }

        public void HideAll()
        {
            ClearSpawnedCards();
            SetActive(_upgradeChoicePanel, false);
            SetActive(_cardPanel, false);
            SetActive(_summaryPanel, false);
            SetActive(_rewardCardParent?.gameObject, false);
            SetActive(_deckCardParent?.gameObject, false);
        }

        public void ShowVictoryReward(bool isFirstVictory, Action onComplete)
        {
            HideAll();
            _isFirstVictory = isFirstVictory;
            _rewardCompleteAction = onComplete;
            _pendingReward = null;

            if (isFirstVictory || !_runService.CanIncreaseMaximumMana())
            {
                ShowCardReward();
                return;
            }

            _increaseManaButton.interactable = _runService.CanIncreaseMaximumMana();
            _upgradeChoicePanel.SetActive(true);
        }

        public void ChooseManaReward()
        {
            if (!_runService.TryIncreaseMaximumMana()) return;
            CompleteReward();
        }

        public void ChooseCardReward()
        {
            ShowCardReward();
        }

        private void ShowCardReward()
        {
            ClearSpawnedCards();
            SetActive(_upgradeChoicePanel, false);
            SetActive(_summaryPanel, false);
            _rewardChoices = _runService.CreateRewardChoices();
            SetActive(_rewardCardParent.gameObject, true);
            SetActive(_deckCardParent.gameObject, false);

            for (int i = 0; i < _rewardChoices.Count; i++)
            {
                SpawnCard(_rewardChoices[i], i, _rewardCardParent, SelectRewardCard, _rewardCardScale);
            }

            _cardPanelTitle.text = "Выберите новую карту";
            _cardPanelCloseButton.gameObject.SetActive(false);
            _cardPanel.SetActive(true);
        }

        private void SelectRewardCard(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= _rewardChoices.Count) return;
            _pendingReward = _rewardChoices[rewardIndex];
            if (_isFirstVictory)
            {
                _runService.ReplaceFirstDefaultCard(_pendingReward);
                CompleteReward();
                return;
            }

            ShowReplacementChoice();
        }

        private void ShowReplacementChoice()
        {
            ClearSpawnedCards();
            SetActive(_rewardCardParent.gameObject, false);
            SetActive(_deckCardParent.gameObject, true);

            IReadOnlyList<CardInfo> deck = _runService.Deck;
            for (int i = 0; i < deck.Count; i++)
            {
                SpawnCard(deck[i], i, _deckCardParent, SelectReplacement, _deckCardScale);
            }

            _cardPanelTitle.text = "Выберите карту, которую надо заменить";
            _cardPanelCloseButton.gameObject.SetActive(false);
            _cardPanel.SetActive(true);
        }

        private void SelectReplacement(int deckIndex)
        {
            _runService.ReplaceDeckCard(deckIndex, _pendingReward);
            CompleteReward();
        }

        private void CompleteReward()
        {
            HideAll();
            Action action = _rewardCompleteAction;
            _rewardCompleteAction = null;
            action?.Invoke();
        }

        public void ShowDefeatSummary(Action returnToMenu)
        {
            HideAll();
            _returnToMenuAction = returnToMenu;
            RoguelikeStageDefinition stage = _runService.CurrentStage;

            _summaryText.text =
                $"ЗАБЕГ ОКОНЧЕН\n\n" +
                $"Побед: {_runService.Victories}\n" +
                $"Сыграно карт: {_runService.CardsPlayed}\n" +
                $"Фигур поставлено: {_runService.PlayerFiguresPlaced}\n" +
                $"Фигур врага: {_runService.EnemyFiguresPlaced}\n" +
                $"Макс. мана: {_runService.MaximumMana}\n" +
                $"Итоговое поле: {stage.BoardSize}×{stage.BoardSize}";
            _summaryPanel.SetActive(true);
        }

        public void OpenDeckPreview()
        {
            ClearSpawnedCards();
            SetActive(_summaryPanel, false);
            SetActive(_rewardCardParent.gameObject, false);
            SetActive(_deckCardParent.gameObject, true);

            IReadOnlyList<CardInfo> deck = _runService.Deck;
            for (int i = 0; i < deck.Count; i++)
            {
                SpawnCard(deck[i], i, _deckCardParent, null, _deckCardScale);
            }

            _cardPanelTitle.text = "Итоговая колода";
            _cardPanelCloseButton.gameObject.SetActive(true);
            _cardPanel.SetActive(true);
        }

        public void CloseDeckPreview()
        {
            ClearSpawnedCards();
            SetActive(_cardPanel, false);
            SetActive(_summaryPanel, true);
        }

        public void ReturnToMenu()
        {
            Action action = _returnToMenuAction;
            _returnToMenuAction = null;
            _runService.EndRun();
            action?.Invoke();
        }

        private void SpawnCard(CardInfo info, int index, Transform parent, Action<int> clickAction, float visualScale)
        {
            CardCollectionUIObject view =
                _container.InstantiatePrefabForComponent<CardCollectionUIObject>(_cardChoicePrefab, parent);
            view.BindRoguelike(info, index, clickAction, visualScale);
            _spawnedCards.Add(view.gameObject);
        }

        private void ClearSpawnedCards()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] == null) continue;
                _spawnedCards[i].SetActive(false);
                Destroy(_spawnedCards[i]);
            }

            _spawnedCards.Clear();
        }

        private static void SetActive(GameObject target, bool state)
        {
            if (target != null) target.SetActive(state);
        }
    }
}
