using Cards.Enum;
using UnityEngine;

namespace Cards.CustomType
{
    [CreateAssetMenu(fileName = "New card", menuName = "Card")]
    public class CardInfo : ScriptableObject
    {
        private static int _currentId;

        public int CardId = _currentId;
        
        [Space, Header("Images")]
        public Sprite CardImageP1;

        public Sprite CardImageP2;
        public Sprite CardHighlightP1;
        public Sprite CardHighlightP2;
        public Color CardHighlightColor;

        [Space, Header("Name")]
        public string CardName;

        public string CardDescription;

        [Space, Space, Space]
        public bool IsDefaultUnlock;

        public CardTypeImpact CardType;
        public CardBonusType CardBonus;
        public Vector2Int CardAreaSize;
        public int CardCount;

        public bool IsNeedShowTip;
        public bool isEnabled = true;
        public string TipText;

        [Range(0, 5)]
        public int CardManacost;

        [HideInInspector]
        public int CardBonusManacost;

        public CardActionType СardActionId;


#if UNITY_EDITOR
       
        private void OnValidate()
        {
            if (CardId >= _currentId)
            {
                _currentId = CardId + 1;
            }

            if (CardId == 0)
            {
                CardId = _currentId;
            }
        }
#endif
    }
}