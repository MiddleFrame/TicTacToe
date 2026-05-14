using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Analytic.Interfaces;
using CardCollection.Interfaces;
using Cards.CustomType;
using Cards.Interfaces;
using Coin.Interfaces;
using ExitGames.Client.Photon;
using SaveSystem;
using TMPro;
using UIElements.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using ICollectionUIService = Cards.Interfaces.ICollectionUIService;

namespace CardCollection
{
    public class CollectionManager : MonoBehaviour, ICollectionUIService, ICollectionService
    {
        [Header("Card view"), SerializeField]
        private CanvasGroup _viewCardCanvas;

        [SerializeField]
        private TextMeshProUGUI _viewManapoints;

        [SerializeField]
        private TextMeshProUGUI _viewCardDescription;

        [SerializeField]
        private Image _viewCardImage;

        [SerializeField]
        private List<GameObject> _viewBonusImageList = new List<GameObject>();

        [SerializeField]
        private ScrollRectEx _collectionScrollRect;
        
        [SerializeField]
        private ScrollRectEx _deckScrollRect;

        private DateTime _startTimeTap = DateTime.MinValue;
        private const float TIME_TAP_VIEW = 0.5f;
        private const float ALPHA_PER_STEP = 0.05f;

        private const string CARD_INFO_PATH = "Cards/";

        [Header("Deck properties"), SerializeField]
        private Transform _deckParent;

        [SerializeField]
        private int _minDeckPool;

        private List<CardCollectionUIObject> _cardDeck = new List<CardCollectionUIObject>();

        private List<int> _redactedDeck;

        private static CardBoolData _cardBoolUnlockData;
        private static BinarySaveSystem _cardUnlockSaveSystem;
        private const string CARD_UNLOCK_PATH = "UnlockData";

        private static CardListData _currentDeckData;
        private static BinarySaveSystem _currentDeckSaveSystem;
        private const string CARD_DECK_PATH = "DeckData";

        [Header("Collection properties"), SerializeField]
        private Transform _collectionParent;

        private List<CardCollectionUIObject> _cardCollections = new List<CardCollectionUIObject>();

        [Header("Texts"), SerializeField]
        private TextMeshProUGUI _cardUnlockValue;

        [SerializeField]
        private TextMeshProUGUI _moneyValue;

        private IEnumerator _fadeInCoroutine;

        private bool _isEdit;
        private bool _isDrag;

        #region Dependency

        private ICardList _cardList;
        private ICoinService _coinService;
        private ICollectionEventsAnalyticService _collectionEventsAnalyticService;
        private IStoreEventsAnalyticService _storeEventsAnalyticService;
        private ICardCollectionFactory _cardCollectionFactory;
        private IBuyingAnimationController _buyingAnimationController;
        private ICollectionData _collectionData;

        [Inject]
        private void Construct(
            ICardList cardList,
            ICoinService coinService,
            ICollectionEventsAnalyticService collectionEventsAnalyticService,
            IStoreEventsAnalyticService storeEventsAnalyticService,
            ICardCollectionFactory cardCollectionFactory,
            IBuyingAnimationController buyingAnimationController,
            ICollectionData collectionData)
        {
            _cardList = cardList;
            _coinService = coinService;
            _collectionEventsAnalyticService = collectionEventsAnalyticService;
            _storeEventsAnalyticService = storeEventsAnalyticService;
            _cardCollectionFactory = cardCollectionFactory;
            _buyingAnimationController = buyingAnimationController;
            _collectionData = collectionData;
        }

        #endregion

        private void Start()
        {
            _viewCardCanvas.gameObject.SetActive(false);

            if (_collectionData.IsInit() == false)
            {
                _collectionData.Initialize();
                var resourcesCard = Resources.LoadAll<CardInfo>(CARD_INFO_PATH);
                foreach (var card in resourcesCard)
                {
                    if(card.isEnabled)
                        _collectionData.AddCard(card);
                }
            }


            if (_cardCollections.Count == 0)
                _cardCollections =
                    _cardCollectionFactory.CreateCollectionUIList(_collectionData.GetCardList(), _collectionParent);
            if (_cardDeck.Count == 0)
                _cardDeck = _cardCollectionFactory.CreateCollectionUIList(_collectionData.GetCardList(), _deckParent);

            LoadCardUnlockData();

            LoadCurrentDeckData();

            _redactedDeck = _currentDeckData.List;

            CreateCardPull();

            UpdateCollectionCardState();
            UpdateDeckCardState();
            UpdateTexts();
        }

        public int CountCardInDeck()
        {
            return _redactedDeck.Count;
        }

        public void UpdateCollectionCardState()
        {
            foreach (CardCollectionUIObject card in _cardCollections)
            {
                card.UpdateUI(IsCardUnlock(card.Info));
                card.SetDeckState(true);
            }
        }

        public void UpdateDeckCardState()
        {
            foreach (CardCollectionUIObject card in _cardDeck)
            {
                card.UpdateUI(IsCardUnlock(card.Info));
                card.gameObject.SetActive(IsCardUnlock(card.Info));
                card.SetDeckState(IsOnDeck(card.Info));
            }
        }

        public void UpdateTexts()
        {
            _cardUnlockValue.text = _coinService.GetCoinPerUnlock().ToString();
            _moneyValue.text = _coinService.GetCurrentMoney().ToString();
        }

        private void CreateCardPull()
        {
            _cardList.CardListClear();

            foreach (int card in _currentDeckData.List)
            {
                _cardList.CardListAdd(_collectionData.GetCardFromId(card));
            }
        }

        public void UnlockRandomCard(bool isNeedUsekCoin = true)
        {
            if (isNeedUsekCoin && _coinService.GetCurrentMoney() < _coinService.GetCoinPerUnlock()) return;
            var lockedList = _cardCollections.Where(card => !IsCardUnlock(card.Info)).ToList();
            if (lockedList.Count == 0) return;
            int valueRand = UnityEngine.Random.Range(0, lockedList.Count);
            CardCollectionUIObject currentCard = lockedList[valueRand];

            _cardBoolUnlockData.BoolData[currentCard.Info.CardId] = true;
            SaveCardUnlockData();

            currentCard.UpdateUI(true);
            _buyingAnimationController.ShowBuyingAnimation(currentCard.Info);
            if (isNeedUsekCoin)
            {
                _coinService.SetCurrentMoney(_coinService.GetCurrentMoney() - _coinService.GetCoinPerUnlock());
                _moneyValue.text = _coinService.GetCurrentMoney().ToString();
            }

            _currentDeckData.List.Add(currentCard.Info.CardId);
            SaveCurrentDeckData();
            CreateCardPull();
        }

        public void UnlockAllCard()
        {
            List<KeyValuePair<int, bool>> keyList = _cardBoolUnlockData.BoolData.Where(x => x.Value == false).ToList();

            for (int i = 0; i < keyList.Count; i++)
            {
                _buyingAnimationController.ShowBuyingAnimation(_collectionData.GetCardFromId(keyList[i].Key));
                _cardBoolUnlockData.BoolData[keyList[i].Key] = true;
                if (!_currentDeckData.List.Contains(keyList[i].Key))
                {
                    _currentDeckData.List.Add(keyList[i].Key);
                }
            }

            SaveCurrentDeckData();
            SaveCardUnlockData();
            CreateCardPull();
            UpdateCollectionCardState();
            UpdateDeckCardState();
        }

        public void LockAllCard()
        {
            foreach (int cardId in _cardBoolUnlockData.BoolData.Keys.ToList())
            {
                _cardBoolUnlockData.BoolData[cardId] = false;
            }

            _currentDeckData.List.Clear();
            _redactedDeck = _currentDeckData.List;

            SaveCurrentDeckData();
            SaveCardUnlockData();
            CreateCardPull();
            UpdateCollectionCardState();
            UpdateDeckCardState();
        }

        private void LoadCardUnlockData()
        {
            _cardUnlockSaveSystem ??= new(CARD_UNLOCK_PATH);
            _cardBoolUnlockData = (CardBoolData) _cardUnlockSaveSystem.Load();
            if (_cardBoolUnlockData == null)
            {
                _cardBoolUnlockData = new CardBoolData();
                foreach (var card in _collectionData.GetCardList())
                {
                    //Синхронизация со старыми сейвами
                    if (PlayerPrefs.HasKey("IsCard" + card.CardName + "Unlocked"))
                    {
                        _cardBoolUnlockData.BoolData[card.CardId] =
                            PlayerPrefs.GetInt("IsCard" + card.name + "Unlocked", 0) == 1;
                        PlayerPrefs.DeleteKey("IsCard" + card.name + "Unlocked");
                    }
                    else
                    {
                        _cardBoolUnlockData.BoolData[card.CardId] = card.IsDefaultUnlock;
                    }
                }

                SaveCardUnlockData();
            }
        }

        private void SaveCardUnlockData()
        {
            _cardUnlockSaveSystem ??= new(CARD_UNLOCK_PATH);
            _cardUnlockSaveSystem.Save(_cardBoolUnlockData);
        }

        private void LoadCurrentDeckData()
        {
            _currentDeckSaveSystem ??= new BinarySaveSystem(CARD_DECK_PATH);
            _currentDeckData = (CardListData) _currentDeckSaveSystem.Load();

            if (_currentDeckData != null) return;

            _currentDeckData = new CardListData();
            foreach (var card in _cardBoolUnlockData.BoolData.Where(card => card.Value))
            {
                _currentDeckData.List.Add(card.Key);
            }
            SaveCurrentDeckData();
        }

        private void SaveCurrentDeckData()
        {
            _currentDeckSaveSystem ??= new BinarySaveSystem(CARD_DECK_PATH);
            _currentDeckSaveSystem.Save(_currentDeckData);
        }

        public bool IsCardUnlock(CardInfo cardInfo)
        {
            return _cardBoolUnlockData.BoolData[cardInfo.CardId];
        }

        private void PickCard(CardInfo card)
        {
            if (_redactedDeck.Exists(x => x == card.CardId))
            {
                _redactedDeck.Remove(card.CardId);
            }
            else
            {
                _redactedDeck.Add(card.CardId);
            }
        }

        private bool IsOnRedactedDeck(CardInfo card)
        {
            return _redactedDeck.Exists(x => x == card.CardId);
        }

        private bool IsOnDeck(CardInfo card)
        {
            return _currentDeckData.List.Exists(x => x == card.CardId);
        }

        public void TrySaveDeck()
        {
            if (CountCardInDeck() >= _minDeckPool)
            {
                _isEdit = false;
                _currentDeckData.List = _redactedDeck;
                SaveCurrentDeckData();
                CreateCardPull();
            }
            else throw new Exception("Card count less minimum");
        }

        public void StartTap(CardCollectionUIObject cardCollectionUIObject, PointerEventData eventData)
        {
            _collectionScrollRect.OnBeginDrag(eventData);
            _deckScrollRect.OnBeginDrag(eventData);
            
            _startTimeTap = DateTime.Now;
            _fadeInCoroutine = StartTapProcess(cardCollectionUIObject);
            _isDrag = false;
            StartCoroutine(_fadeInCoroutine);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _collectionScrollRect.OnDrag(eventData);
            _deckScrollRect.OnDrag(eventData);

            if (_isDrag == false)
            {
                StopCoroutine(_fadeInCoroutine);
                if ((DateTime.Now - _startTimeTap).TotalSeconds > TIME_TAP_VIEW)
                {
                    StartCoroutine(EndTapProcess());
                }
            }

            _isDrag = true;
        }

        public void EndTap(CardCollectionUIObject cardCollectionUIObject, PointerEventData eventData)
        {
            _collectionScrollRect.OnEndDrag(eventData);
            _deckScrollRect.OnEndDrag(eventData);
            StopCoroutine(_fadeInCoroutine);
            
            if (_isDrag) 
                return;
            
            if ((DateTime.Now - _startTimeTap).TotalSeconds > TIME_TAP_VIEW)
            {
                StartCoroutine(EndTapProcess());
            }
            else
            {
                int value = IsOnRedactedDeck(cardCollectionUIObject.Info) ? 1 : -1;

                if (!_isEdit || CountCardInDeck() - value < _minDeckPool) return;

                PickCard(cardCollectionUIObject.Info);
                cardCollectionUIObject.SetDeckState(IsOnRedactedDeck(cardCollectionUIObject.Info));
            }
        }

        private void UpdateCardViewImage(CardInfo info)
        {
            string desc;
            desc = I2.Loc.LocalizationManager.TryGetTranslation(info.CardDescription, out desc)
                ? I2.Loc.LocalizationManager.GetTranslation(info.CardDescription)
                : info.CardDescription;
            _viewCardDescription.text = desc;
            _viewCardImage.sprite = info.CardImageP1;
            _viewManapoints.text = info.CardManacost.ToString();
            for (int i = 0; i < _viewBonusImageList.Count; i++)
            {
                _viewBonusImageList[i].SetActive(i == (int) info.CardBonus);
            }
        }

        public void StartEditPull()
        {
            _isEdit = true;
        }

        public void Player_Open_Collection()
        {
            _collectionEventsAnalyticService.Player_Open_Collection();
        }

        public void Player_Bought_Random_Card()
        {
            if (_coinService.GetCurrentMoney() < _coinService.GetCoinPerUnlock()) return;

            _storeEventsAnalyticService.Player_Bought_Random_Card();
        }

        public void Player_Open_Deckbuild()
        {
            _collectionEventsAnalyticService.Player_Open_Deckbuild();
        }


        private IEnumerator StartTapProcess(CardCollectionUIObject cardCollectionUIObject)
        {
            yield return new WaitForSeconds(TIME_TAP_VIEW);

            UpdateCardViewImage(cardCollectionUIObject.Info);
            _viewCardCanvas.gameObject.SetActive(true);
            _viewCardCanvas.alpha = 0;
            while (_viewCardCanvas.alpha < 1)
            {
                _viewCardCanvas.alpha += ALPHA_PER_STEP;
                yield return null;
            }
        }

        private IEnumerator EndTapProcess()
        {
            while (_viewCardCanvas.alpha > 0)
            {
                _viewCardCanvas.alpha -= ALPHA_PER_STEP;
                yield return null;
            }

            _viewCardCanvas.alpha = 0;
            _viewCardCanvas.gameObject.SetActive(false);
        }
        
        
    }
}