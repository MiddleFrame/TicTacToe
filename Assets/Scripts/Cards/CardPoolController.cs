using System;
using System.Collections;
using System.Collections.Generic;
using Cards.CustomType;
using Cards.Interfaces;
using Coroutine.Interfaces;
using Mana.Interfaces;
using Players.Interfaces;
using ScreenScaler.Interfaces;
using UIElements;
using UnityEngine;
using Zenject;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

namespace Cards
{
    public class CardPoolController : MonoBehaviour, IHandPoolManipulator, IHandPoolView, IRechangerService
    {
        #region Field

        [SerializeField]
        private Transform _cardTransformParent;

        [Header("Border properties"), SerializeField]
        private float _buttonBorder;

        [SerializeField]
        private float _widthBorder;

        [SerializeField]
        private float _heightLift;

        [SerializeField]
        private Vector2 _startPointDelta;

        [Header("Card properties"), SerializeField]
        private float _widthCard;

        [SerializeField]
        private float _angleDelta;

        [SerializeField]
        private float _heightDelta;

        private const int SLOTS_COUNT = 5;
        private const int CARD_PER_TURN = 2;

        private Vector2 _startPoint;

        [SerializeField]
        private AnimationFading _cardRechanger;

        [SerializeField]
        private RectTransform _rechangerRect;

        [SerializeField]
        private Button _cardRechangerBTN;


        private PlayerInfo _currentPlayerView;

        #endregion

        #region Dependency

        private IScreenScaler _screenScaler;
        private ICardFactory _cardFactory;
        private IPlayerService _playerService;
        private ICoroutineService _coroutineService;
        private ICoroutineAwaitService _coroutineAwaitService;
        private IManaService _manaService;

        [Inject]
        private void Construct(
            IScreenScaler screenScaler,
            ICardFactory cardFactory,
            IPlayerService playerService,
            ICoroutineService coroutineService,
            ICoroutineAwaitService coroutineAwaitService,
            IManaService manaService
        )
        {
            _screenScaler = screenScaler;
            _cardFactory = cardFactory;
            _playerService = playerService;
            _coroutineService = coroutineService;
            _coroutineAwaitService = coroutineAwaitService;
            _manaService = manaService;
        }

        #endregion

        public int MaxCardHand => SLOTS_COUNT;


        public void AddCard(PlayerInfo player)
        {
            if (player.HandPool.Count >= SLOTS_COUNT) return;
            if (player.DeckPool.Count == 0) return;

            int endBorder = player.DeckPool.Count;
            if (endBorder != 1) endBorder -= 1;

            int card = Random.Range(0, endBorder);
            player.HandPool.Add(player.DeckPool[card]);
            player.DeckPool.RemoveRange(card, 1);
            card = player.HandPool.Count - 1;
            player.HandPool[card].gameObject.SetActive(true);
            player.HandPool[card].SetTransformPosition(_startPoint);
        }

        public void RemoveCard(PlayerInfo player, int id)
        {
            if (player.HandPool.Count == 0) return;
            if (id >= player.HandPool.Count) return;

            player.DeckPool.Add(player.HandPool[id]);
            player.HandPool.RemoveRange(id, 1);
            player.DeckPool[^1].gameObject.SetActive(false);
        }

        public void RemoveCard(PlayerInfo player, CardModel cardModel)
        {
            if (player.HandPool.Count == 0) return;
            if (!player.HandPool.Contains(cardModel)) return;

            player.DeckPool.Add(cardModel);
            player.HandPool.Remove(cardModel);
            player.DeckPool[^1].gameObject.SetActive(false);
        }

        public void UpdateCardPosition(bool instantly = true, CardModel cardModel = null)
        {
            Debug.Log("Update card pool");

            float currentCount = _currentPlayerView.HandPool.Count;
            int curIndex = _currentPlayerView.HandPool.IndexOf(cardModel);
            if (curIndex != -1)
            {
                currentCount -= 1;
                _currentPlayerView.HandPool.Remove(cardModel);

                _currentPlayerView.HandPool.Add(cardModel);
            }

            float count = currentCount;

            float positionY = 
                _buttonBorder + (IsCurrentPlayerOnSlot() ? _heightLift : 0);

            float overlapPercent = 1f;
            float step = _widthCard * overlapPercent;


            float totalWidth = step * (count - 1);
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float x = startX + step * i;

                float curveStrength = 0.4f;
                float curveOffset = 0f;
                if (count > 1)
                {
                    curveOffset = Mathf.Sin(Mathf.PI * i / (count - 1f)) * _heightDelta * curveStrength;
                }

                float posY = positionY + curveOffset;


                Vector2 finalPos = new Vector2(x, posY);

                var card = _currentPlayerView.HandPool[i];

                card.SetTransformScale(0.7f, instantly);
                card.SetSibling(i);
                card.SetTransformPosition(finalPos, instantly);

                float angle;
                if (count <= 1)
                {
                    angle = 0f;
                }
                else
                {
                    angle = _angleDelta - (_angleDelta * 2f / (count - 1f)) * i;
                }
                card.SetTransformRotation(angle, instantly);
            }
        }

        public void UpdateCardUI()
        {
            foreach (CardModel card in _currentPlayerView.HandPool)
            {
                card.UpdateUI(card.Info, _currentPlayerView,
                    _manaService.IsEnoughMana(card.Info.CardManacost + card.Info.CardBonusManacost) &&
                    _playerService.GetCurrentPlayerOnDevice() == _playerService.GetCurrentPlayer());
            }
        }


        public void ChangeCurrentPlayerView(PlayerInfo player)
        {
            if (player.EntityType == PlayerType.AI)
            {
                for (int i = 0; i < _currentPlayerView.HandPool.Count; i++)
                {
                    _currentPlayerView.HandPool[i].CancelDragging();
                }

                UpdateCardPosition(false);
                return;
            }

            if (_currentPlayerView != null)
            {
                foreach (var t in _currentPlayerView.HandPool)
                {
                    t.gameObject.SetActive(false);
                }
            }

            _currentPlayerView = player;
            Debug.Log("Current side id:" + _currentPlayerView.SideId);
            foreach (var t in _currentPlayerView.HandPool)
            {
                t.gameObject.SetActive(true);
            }

            for (int i = 0; i < CARD_PER_TURN; i++)
            {
                AddCard(_currentPlayerView);
            }

            UpdateCardPosition(false);
            UpdateCardUI();
        }


        public void ResetHandPool(PlayerInfo player)
        {
            while (player.HandPool.Count > 0)
            {
                RemoveCard(player, player.HandPool[0]);
            }
        }

        public void UseRechanger()
        {
            if (_playerService.GetCurrentPlayer() != _currentPlayerView) return;
            CardModel cardModel = _currentPlayerView.HandPool[Random.Range(0, _currentPlayerView.HandPool.Count)];
            _cardRechanger.FadeOut();
            _cardRechangerBTN.interactable = false;
            _coroutineService.AddCoroutine(IRechanherProcess(cardModel));
        }


        private IEnumerator IRechanherProcess(CardModel cardModel)
        {
            cardModel.SetGroupAlpha(1, 0, false);
            yield return _coroutineAwaitService.AwaitTime(5f);
            RemoveCard(_playerService.GetCurrentPlayer(), cardModel);
            AddCard(_playerService.GetCurrentPlayer());
            UpdateCardPosition(false);
        }

        public List<CardModel> CreateCardPull(int side)
        {
            List<CardModel> list = _cardFactory.CreateDeck(side, _cardTransformParent);
            _startPoint = _screenScaler.GetVector(new Vector2(_widthBorder, _buttonBorder) + _startPointDelta);
            foreach (CardModel card in list)
                card.SetTransform(_startPoint);
            return list;
        }

        public void SynchronizeDeck(PlayerInfo player, IReadOnlyList<CardInfo> cardInfos)
        {
            if (player == null || cardInfos == null) return;

            ResetHandPool(player);
            if (player.FullDeckPool.Count != cardInfos.Count)
            {
                Debug.LogError(
                    $"Cannot synchronize deck: models={player.FullDeckPool.Count}, cards={cardInfos.Count}.");
                return;
            }

            for (int i = 0; i < player.FullDeckPool.Count; i++)
            {
                player.FullDeckPool[i].SetCardInfoWithoutVisualUpdate(cardInfos[i]);
                player.FullDeckPool[i].Info.CardBonusManacost = 0;
            }

            player.DeckPool = new List<CardModel>(player.FullDeckPool);
        }

        public void ResetRechanger()
        {
            if (_cardRechangerBTN.interactable) return;
            _cardRechanger.FadeIn();
            _cardRechangerBTN.interactable = true;
        }

        public bool IsCurrentPlayerOnSlot() =>
            _currentPlayerView.SideId == _playerService.GetCurrentPlayer().SideId;
    }
}
