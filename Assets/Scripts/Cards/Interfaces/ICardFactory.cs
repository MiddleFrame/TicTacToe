using System.Collections.Generic;
using Cards.CustomType;
using UnityEngine;

namespace Cards.Interfaces
{
    public interface ICardFactory
    {
        public List<CardModel> CreateDeck(int side, Transform parent);
    }
}