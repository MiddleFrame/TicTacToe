using System.Collections.Generic;
using Cards.CustomType;
using Cards.Interfaces;
using UnityEngine;
using Zenject;

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

        public CardFactory(DiContainer diContainer, ICardList cardList)
        {
            _diContainer = diContainer;
            _cardList = cardList;
            Load();
        }

        #endregion

        public List<CardModel> CreateDeck(int side, Transform parent)
        {
            List<CardModel> newDeck = new List<CardModel>();
            foreach (CardInfo cardInfo in _cardList.GetCardList())
            {
                for (int i = 0; i < cardInfo.CardCount; i++)
                {
                    CardModel cardModel = _diContainer.InstantiatePrefabForComponent<CardModel>(_cardModelPrefab, parent);
                    //card.name = card.Info.CardName;
                    Debug.Log(cardModel);
                    cardModel.SetCardInfo(cardInfo, side);
                    cardModel.Info.CardBonusManacost = 0;
                    cardModel.gameObject.SetActive(false);
                    //card.SetTransformParent(transform);
                    newDeck.Add(cardModel);
                }
            }

            return newDeck;
        }
    }
}