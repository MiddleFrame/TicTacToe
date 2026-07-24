using System.Collections.Generic;
using Cards.CustomType;
using Cards.Interfaces;
using UnityEngine;
using Zenject;
using GameTypeService.Enums;
using GameTypeService.Interfaces;
using Roguelike.Interfaces;

namespace Cards
{
    public class CardFactory : ICardFactory
    {
        private CardModel _cardModelPrefab;
        private const string CARD_PREFAB_PATH = "Card";

        private readonly DiContainer _diContainer;

        private void Load()
        {
            _cardModelPrefab = Resources.Load<CardModel>(CARD_PREFAB_PATH);
        }

        #region Dependecy

        private readonly ICardList _cardList;
        private readonly IRoguelikeRunService _roguelikeRunService;
        private readonly IGameTypeService _gameTypeService;

        public CardFactory(DiContainer diContainer, ICardList cardList,
            IRoguelikeRunService roguelikeRunService, IGameTypeService gameTypeService)
        {
            _diContainer = diContainer;
            _cardList = cardList;
            _roguelikeRunService = roguelikeRunService;
            _gameTypeService = gameTypeService;
            Load();
        }

        #endregion

        public List<CardModel> CreateDeck(int side, Transform parent)
        {
            List<CardModel> newDeck = new List<CardModel>();
            if (_gameTypeService.GetGameType() == GameType.Roguelike && side == 1)
            {
                foreach (CardInfo cardInfo in _roguelikeRunService.Deck)
                {
                    newDeck.Add(CreateCard(cardInfo, side, parent));
                }

                return newDeck;
            }

            foreach (CardInfo cardInfo in _cardList.GetCardList())
            {
                for (int i = 0; i < cardInfo.CardCount; i++)
                {
                    newDeck.Add(CreateCard(cardInfo, side, parent));
                }
            }

            return newDeck;
        }

        private CardModel CreateCard(CardInfo cardInfo, int side, Transform parent)
        {
            CardModel cardModel = _diContainer.InstantiatePrefabForComponent<CardModel>(_cardModelPrefab, parent);
            cardModel.SetCardInfo(cardInfo, side);
            cardModel.Info.CardBonusManacost = 0;
            cardModel.gameObject.SetActive(false);
            return cardModel;
        }
    }
}
