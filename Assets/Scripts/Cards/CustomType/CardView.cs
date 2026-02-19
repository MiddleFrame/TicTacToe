using System;
using System.Collections.Generic;
using Cards.Enum;
using ScreenScaler;
using ScreenScaler.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Cards.CustomType
{
    [RequireComponent(typeof(CardModel))]
    public class CardView : MonoBehaviour
    {
        [Serializable]
        public class BonusImageType
        {
            public GameObject BonusImage;
            public CardBonusType BonusType;
        }

        #region Fields

        #endregion

        private const int ROTATION_COUNT_FRAME = 12;

        private const int SCALE_COUNT_FRAME = 12;

        private const int POSITION_COUNT_FRAME = 12;

        private const int LIGHT_COUNT_FRAME = 12;

        private const int CANVAS_ALPHA_COUNT_FRAME = 12;


        private RectTransform _cardTransformRect;

        private bool _isSlotsReUpdatePositions;

        private Vector2Int _prevChosedCell;

        private Canvas _cardCanvas;

        private readonly Vector2 _fingerToCardDistance = new(0, 200);

        private CanvasGroup _cardCanvasGroup;

        [Header("Texts"), SerializeField]
        private TextMeshProUGUI _manapointsText;

        [SerializeField]
        private TextMeshProUGUI _cardDescription;

        [Header("Images"), SerializeField]
        private Image _cardBacklight;

        [SerializeField]
        private Image _cardMainImage;

        [SerializeField]
        private CardTips _cardTip;

        [SerializeField]
        private List<BonusImageType> _bonusImageList = new();

        private Vector2 _initScale;

        #region Coroutines

        private UnityEngine.Coroutine _alphaCoroutine;

        private bool _isAlphaCoroutineWork;

        private UnityEngine.Coroutine _positionCoroutine;

        private bool _isPositionCoroutineWork;

        private UnityEngine.Coroutine _rotationCoroutine;

        private bool _isRotationCoroutineWork;

        private UnityEngine.Coroutine _scaleCoroutine;

        private bool _isScaleCoroutineWork;

        #endregion


        #region Dependecy

        private IScreenScaler _screenScaler;

        [Inject]
        private void Construct(IScreenScaler screenScaler)
        {
            _screenScaler = screenScaler;
        }

        #endregion

        void Awake()
        {
            _cardTransformRect = GetComponent<RectTransform>();
            _cardCanvas = GetComponent<Canvas>();
            _cardCanvasGroup = GetComponent<CanvasGroup>();
            _initScale = new Vector2(1,
                    1);
        }

        private void OnDisable()
        {
            _isPositionCoroutineWork = false;
            _isScaleCoroutineWork = false;
            _isRotationCoroutineWork = false;
        }

        private void OnEnable()
        {
            _cardCanvas.overrideSorting = false;
            SetGroupAlpha(_cardCanvasGroup.alpha, 1);
            _cardTip.HideTip(true);
        }

        public void SetSideCard(CardInfo info, int side)
        {
            _cardMainImage.sprite = (side == 1) ? info.CardImageP1 : info.CardImageP2;
        }


        public void UpdateUI(CardInfo info, PlayerInfo playerInfo, bool isNeedLight)
        {
            _manapointsText.text = (info.CardManacost + info.CardBonusManacost).ToString();
            SetSideCard(info, playerInfo.SideId);

            string desc;
            desc = I2.Loc.LocalizationManager.TryGetTranslation(info.CardDescription, out desc)
                ? I2.Loc.LocalizationManager.GetTranslation(info.CardDescription)
                : info.CardDescription;

            StartCoroutine(_cardBacklight.AlphaWithLerp(
                _cardBacklight.color.a,
                (isNeedLight) ? 1 : 0,
                LIGHT_COUNT_FRAME));
            _cardDescription.text = desc;

            foreach (BonusImageType bonus in _bonusImageList)
            {
                bonus.BonusImage.SetActive(bonus.BonusType == info.CardBonus);
            }
        }
        
        public void UpdateUI(CardInfo info, int playerSide, bool isNeedLight)
        {
            _manapointsText.text = (info.CardManacost + info.CardBonusManacost).ToString();
            SetSideCard(info, playerSide);

            string desc;
            desc = I2.Loc.LocalizationManager.TryGetTranslation(info.CardDescription, out desc)
                ? I2.Loc.LocalizationManager.GetTranslation(info.CardDescription)
                : info.CardDescription;

            StartCoroutine(_cardBacklight.AlphaWithLerp(
                _cardBacklight.color.a,
                (isNeedLight) ? 1 : 0,
                LIGHT_COUNT_FRAME));
            _cardDescription.text = desc;

            foreach (BonusImageType bonus in _bonusImageList)
            {
                bonus.BonusImage.SetActive(bonus.BonusType == info.CardBonus);
            }
        }

        public void SetTransformScale(float reals, bool instantly = true)
        {
            Vector2 finScale = _initScale * reals;
            if (instantly) transform.localScale = finScale;
            else
            {
                if (_isScaleCoroutineWork) StopCoroutine(_scaleCoroutine);
                _isScaleCoroutineWork = true;
                _scaleCoroutine = StartCoroutine(_cardTransformRect.ScaleWithLerp
                    (
                        _cardTransformRect.localScale,
                        finScale,
                        SCALE_COUNT_FRAME,
                        () => { _isScaleCoroutineWork = false; }
                    )
                );
            }
        }


        public void SetTransform(Vector2 position)
        {
            SetTransformPosition(position);
            _cardTransformRect.localScale = Vector3.one;
        }

        public void SetSibling(int id)
        {
            _cardTransformRect.SetSiblingIndex(id);
        }

        public void SetGroupAlpha(float init, float final, bool instantly = true)
        {
            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            if (instantly) _cardCanvasGroup.alpha = final;
            else
                _alphaCoroutine = StartCoroutine(_cardCanvasGroup.AlphaWithLerp(init, final, CANVAS_ALPHA_COUNT_FRAME));
        }

        public void SetGroupAlpha(float final, bool instantly = true)
        {
            if (_alphaCoroutine != null) StopCoroutine(_alphaCoroutine);
            if (instantly) _cardCanvasGroup.alpha = final;
            else
                _alphaCoroutine =
                    StartCoroutine(_cardCanvasGroup.AlphaWithLerp(_cardCanvasGroup.alpha, final,
                        CANVAS_ALPHA_COUNT_FRAME));
        }

        public void SetTransformPosition(float x, float y, bool instantly = true)
        {
         //   if (instantly) _cardTransformRect.localPosition = new Vector2(x, y);
        //    else
            // {
            //     if (_isPositionCoroutineWork) StopCoroutine(_positionCoroutine);
            //     _isPositionCoroutineWork = true;
            //     _positionCoroutine = StartCoroutine(_cardTransformRect.LocalPositionWithLerp
            //         (
            //             _cardTransformRect.localPosition,
            //             new Vector2(x, y),
            //             POSITION_COUNT_FRAME,
            //             () => { _isPositionCoroutineWork = false; }
            //         )
            //     );
            // }
        }

        public void SetTransformPosition(Vector2 position, bool instantly = true)
        {
            if (instantly) _cardTransformRect.localPosition = position;
            else
            {
                if (_isPositionCoroutineWork) StopCoroutine(_positionCoroutine);
                _isPositionCoroutineWork = true;
                Debug.Log($"Set new position init {_cardTransformRect.localPosition} fin {position}");
                _positionCoroutine = StartCoroutine(_cardTransformRect.LocalPositionWithLerp
                    (
                        _cardTransformRect.localPosition,
                        position,
                        POSITION_COUNT_FRAME,
                        () => { _isPositionCoroutineWork = false; }
                    )
                );
            }
        }

        public void SetTransformRotation(float x, bool instantly = true)
        {
            if (instantly) _cardTransformRect.localRotation = Quaternion.Euler(0, 0, x);
            else
            {
                if (_isRotationCoroutineWork) StopCoroutine(_rotationCoroutine);
                _isRotationCoroutineWork = true;
                _rotationCoroutine = StartCoroutine(_cardTransformRect.RotationWithLerp
                    (
                        _cardTransformRect.rotation,
                        Quaternion.Euler(0, 0, x),
                        ROTATION_COUNT_FRAME,
                        () => { _isRotationCoroutineWork = false; }
                    )
                );
            }
        }

        public void SetCanvasOverrideSorting(bool state)
        {
            _cardCanvas.overrideSorting = state;
        }

        public void SetTransformPositionWithFingerDistance(Vector2 position, bool instantly = true)
        {
            SetTransformPosition(GetPositionWithDistance(position), instantly);
        }

        public void ShowTip(string text, bool instantly)
        {
            _cardTip.ShowTip(text, instantly);
        }
        
        public void HideTip(bool instantly)
        {
            _cardTip.HideTip(instantly);
        }

        public Vector2 GetFingerDistance()
        {
            return _fingerToCardDistance;
        }

        public Vector2 GetPositionWithDistance(Vector2 position)
        {
            return position + _screenScaler.GetVector(_fingerToCardDistance);
        }

        public Vector2 GetClearPosition()
        {
            return (Vector2)_cardTransformRect.localPosition - _screenScaler.GetVector(_fingerToCardDistance);
        }
    }
}