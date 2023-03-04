﻿using System.Collections.Generic;
using Cards.CustomType;

namespace Cards.Interfaces
{
    public interface IHandPoolManipulator
    {
        public void AddCard(PlayerInfo player);
        public void RemoveCard(PlayerInfo player, int id);
        public void RemoveCard(PlayerInfo player, CardModel cardModel);

        public void ResetHandPool(PlayerInfo player);

        public List<CardModel> CreateCardPull();
        public bool IsCurrentPlayerOnSlot();
    }
}