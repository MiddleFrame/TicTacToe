using System;
using Cards.CustomType;
using Cards.Interfaces;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace CardCollection
{
    public class CardCollectionUIObject : CardViewBase, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Информация карты как информационной сущности
        /// </summary>
        public CardInfo Info;

        /// <summary>
        /// Обьект карты
        /// </summary>
        [Header("Objects"), SerializeField]
        private GameObject _cardObj;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Обьект карты (закрытое состояние)
        /// </summary>
        [SerializeField]
        private GameObject _cardObjClose;
        
        #region Dependency

        private ICollectionUIService _collectionUIService;

        private bool _isRoguelikeView;
        private int _roguelikeDeckIndex;
        private Action<int> _roguelikeClickAction;

       [Inject]
        private void Construct([InjectOptional] ICollectionUIService collectionUIService)
        {
            _collectionUIService = collectionUIService;
        }

        #endregion
        
        private void Awake()
        {
            if (Info != null) UpdateUI(true);
        }

        public void BindRoguelike(CardInfo info, int deckIndex, Action<int> clickAction, float visualScale)
        {
            Info = info;
            _roguelikeDeckIndex = deckIndex;
            _roguelikeClickAction = clickAction;
            _isRoguelikeView = true;
            float safeScale = Mathf.Max(0.01f, visualScale);
            Vector3 cardScale = new(safeScale, safeScale, 1f);
            _cardObj.transform.localScale = cardScale;
            _cardObjClose.transform.localScale = cardScale;
            UpdateUI(true);
            SetDeckState(true);
        }

        public void UpdateUI(bool isUnlock)
        {

            _cardObj.SetActive(isUnlock);
            _cardObjClose.SetActive(!isUnlock);
            if (isUnlock)
            {            
              base.UpdateCardViewImage(Info,new PlayerInfo(){SideId=1});
            }
        }

        public void SetDeckState(bool state)
        {
            _canvasGroup.alpha = (state) ? 1 : 0.2f;
        }
    
        public void OnDrag(PointerEventData eventData)
        {
            if (_isRoguelikeView) return;
            _collectionUIService?.OnDrag(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isRoguelikeView) return;
            _collectionUIService?.StartTap(this,eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isRoguelikeView)
            {
                _roguelikeClickAction?.Invoke(_roguelikeDeckIndex);
                return;
            }

            _collectionUIService?.EndTap(this,eventData);
        }
    }
}
