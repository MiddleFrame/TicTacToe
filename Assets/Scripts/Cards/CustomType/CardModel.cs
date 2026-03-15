using System.Diagnostics;
using Cards.Enum;
using Cards.Interfaces;
using Field.Interfaces;
using Players.Interfaces;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;


namespace Cards.CustomType
{
    [RequireComponent(typeof(CardView))]
    public class CardModel : MonoBehaviour
    {
        public CardInfo Info { get; private set; }

        private CardView _cardView;

        private readonly Stopwatch _stopWatch = new();

        private Vector2Int _chosenCell;
        private Vector2Int _prevChosenCell;
        private RectTransform _parentRect;
        private RectTransform _cardRect;
        private Vector2 _fingerOffset;

        public Stopwatch Stopwatch => _stopWatch;

        private bool _lastAlphaState = true;
        private bool _isSlotsReUpdatePositions;

        #region Dependency

        private IPlayerService _playerService;
        private IHandPoolView _handPoolView;
        private IFieldService _fieldService;
        private IFieldZoneService _fieldZoneService;
        private ICardActionService _cardActionService;

        [Inject]
        private void Construct(
            IPlayerService playerService,
            IHandPoolView handPoolView,
            IFieldService fieldService,
            IFieldZoneService fieldZoneService,
            ICardActionService cardActionService)
        {
            _playerService = playerService;
            _handPoolView = handPoolView;
            _fieldService = fieldService;
            _fieldZoneService = fieldZoneService;
            _cardActionService = cardActionService;
        }

        #endregion

        private void Awake()
        {
            _cardRect = GetComponent<RectTransform>();
            _parentRect = _cardRect.parent as RectTransform;

            _cardView = GetComponent<CardView>();
        }

        public void SetCardInfo(CardInfo ci)
        {
            Info = ci;
            _cardView.UpdateUI(ci, _playerService.GetCurrentPlayerOnDevice(), false);
        }

        public void SetCardInfo(CardInfo ci, int playerSide)
        {
            Info = ci;
            _cardView.UpdateUI(ci, playerSide, false);
        }

        /// <summary>
        /// Начало перетягивания карты
        /// </summary>
        public void BeginDrag()
        {
            _cardView.SetCanvasOverrideSorting(true);
            _cardView.SetTransformScale(1, false);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                Input.mousePosition,
                Camera.main, // если Canvas = Screen Space Overlay
                out var localPoint
            );

            _fingerOffset = (Vector2)_cardRect.localPosition - localPoint;

            _lastAlphaState = true;
            _chosenCell = new Vector2Int(-1, -1);
            SetTransformRotation(0);
            _isSlotsReUpdatePositions = false;
            _stopWatch.Reset();
            _stopWatch.Start();
            if (Info.IsNeedShowTip)
            {
                string textTip;
                textTip = I2.Loc.LocalizationManager.TryGetTranslation(Info.TipText, out textTip)
                    ? I2.Loc.LocalizationManager.GetTranslation(Info.TipText)
                    : Info.TipText;
                Debug.Log(textTip);
                _cardView.ShowTip(textTip, false);
            }

            
        }

        /// <summary>
        /// Перетягивание карты
        /// </summary>
        public void OnDrag()
        {
            if (!_handPoolView.IsCurrentPlayerOnSlot()) return;

            // Card position based on Finger position 
            //Vector2 vectorFigure = _cardView.GetPositionWithDistance(Input.mousePosition);

            // Card position based on Card center
            Vector2 vectorFigure = Camera.main.WorldToScreenPoint(_cardView.transform.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
     _parentRect,
     Input.mousePosition,
     Camera.main,
     out var localPoint
 );

            _cardRect.localPosition = localPoint + _fingerOffset;


            if (_fieldService.IsInFieldHeight(vectorFigure.y) && !_isSlotsReUpdatePositions)
            {
                _isSlotsReUpdatePositions = true;
                _handPoolView.UpdateCardPosition(false, this);
                transform.SetAsLastSibling();
                Debug.Log("Entered");
            }

            if (Info.CardType == CardTypeImpact.OnField) return;

            if (_fieldService.IsInFieldHeight(vectorFigure.y))
            {
                _cardView.HideTip(true);
            }

           /* 
           // card invisibility
           
           if (_lastAlphaState == _fieldService.IsInFieldHeight(vectorFigure.y))
            {
                _lastAlphaState = !_lastAlphaState;
                SetGroupAlpha(_lastAlphaState ? 1 : 0, false);
            }*/
            _chosenCell = _fieldService.GetIdFromPosition(vectorFigure+new Vector2(0,170), false);

            if (_prevChosenCell != _chosenCell)
            {
                if (_prevChosenCell != new Vector2Int(-1, -1))
                {
                    _fieldZoneService.UnhighlightZone(_prevChosenCell, Info.CardAreaSize);
                }

                if (_chosenCell != new Vector2Int(-1, -1))
                {
                    _fieldZoneService.HighlightZone(_chosenCell,
                        Info.CardAreaSize,
                        (_playerService.GetCurrentPlayer().SideId == 1)
                            ? Info.CardHighlightP1
                            : Info.CardHighlightP2,
                        Info.CardHighlightColor
                    );
                }

                _prevChosenCell = _chosenCell;
            }
        }

        /// <summary>
        /// Конец движения карты от пальца
        /// </summary>
        public void EndDragged()
        {
            if (_chosenCell != new Vector2Int(-1, -1))
            {
                _fieldZoneService.UnhighlightZone(_chosenCell, Info.CardAreaSize);
            }

            _stopWatch.Stop();
            _cardView.SetCanvasOverrideSorting(false);

            bool invokeState = _cardActionService.InvokeActionWithCheck(this, _chosenCell);
            if (invokeState)
            {
                Info.CardBonusManacost += 1;
                //SetTransformParent(null);
            }
            else
            {
                _lastAlphaState = true;
                SetGroupAlpha(1, false);
            }

            if (Info.IsNeedShowTip) _cardView.HideTip(invokeState);
            _handPoolView.UpdateCardPosition(false);
            _handPoolView.UpdateCardUI();
            _isSlotsReUpdatePositions = false;
        }

        public void CancelDragging()
        {
            if (FindObjectOfType<Field.FieldController>() != null)
            {
                if (_chosenCell != new Vector2Int(-1, -1))
                {
                    _fieldZoneService.UnhighlightZone(_chosenCell, Info.CardAreaSize);
                }
            }

            if (Info.IsNeedShowTip) _cardView.HideTip(true);
            _stopWatch.Stop();
            _cardView.SetCanvasOverrideSorting(false);


            _lastAlphaState = true;
            _handPoolView.UpdateCardUI();
            //SlotManager.Instance.UpdateCardPosition(!gameObject.activeInHierarchy);
        }

        public void SetSideCard(CardInfo info, int side)
        {
            _cardView.SetSideCard(info, side);
        }

        public void UpdateUI(CardInfo info, PlayerInfo playerInfo, bool isNeedLight)
        {
            _cardView.UpdateUI(info, playerInfo, isNeedLight);
        }

        public void SetTransformScale(float reals, bool instantly = true)
        {
            _cardView.SetTransformScale(reals, instantly);
        }

        public void SetTransform()
        {
            _cardView.SetTransform(Vector2.zero);
        }

        public void SetTransform(Vector2 position)
        {
            _cardView.SetTransform(position);
        }

        public void SetSibling(int id)
        {
            _cardView.SetSibling(id);
        }

        public void SetGroupAlpha(float init, float final, bool instantly = true)
        {
            _cardView.SetGroupAlpha(init, final, instantly);
        }

        private void SetGroupAlpha(float final, bool instantly = true)
        {
            _cardView.SetGroupAlpha(final, instantly);
        }

        public void SetTransformPosition(Vector2 position, bool instantly = true)
        {
            _cardView.SetTransformPosition(position, instantly);
        }

        public void SetTransformRotation(float x, bool instantly = true)
        {
            _cardView.SetTransformRotation(x, instantly);
        }

        public void SetCanvasOverrideSorting(bool state)
        {
            _cardView.SetCanvasOverrideSorting(state);
        }

        private void SetTransformPositionWithFingerDistance(Vector2 position, bool instantly = true)
        {
            _cardView.SetTransformPositionWithFingerDistance(position, instantly);
        }

        public Vector2 GetClearPosition()
        {
            return _cardView.GetPositionWithDistance(Input.mousePosition);
        }
    }
}