using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }


    RectTransform _shop;
    RectTransform _shopPanel;
    RectTransform _shopEnlargePanel;

    int _shopCount;

    UICard[] _shopCards;
    TMP_Text[] _shopCardPrices;

    int _shopCardIdx;

    CardData[][] _shopCardData;
    bool[,] _buyInfo;
    bool[] _isSaveShop;
    void Awake()
    {
        Instance = Instance != null ? Instance : this;
        _shop = InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.Shop).Find("Shop") as RectTransform;
        _shopPanel = InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.Shop).Find("ShopPanel") as RectTransform;
        _shopEnlargePanel = InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.Shop).Find("ShopEnlargePanel") as RectTransform;

        Button shopBtn, backBtn, buyBtn, enlargeBackBtn;
        if (_shop.TryGetComponent<Button>(out shopBtn))
        {
            shopBtn.onClick.AddListener(() =>
            {
                OutGameUIManager.Instance.AddOpenUIOreder(InGameUIManager.CanvasName.Shop.ToString(), () =>
                {
                    OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.Shop.ToString());
                    _shopPanel.gameObject.SetActive(false);
                });
                _shopPanel.gameObject.SetActive(true);
            });
        }
        if (_shopPanel.Find("BackBtn").TryGetComponent<Button>(out backBtn))
        {
            backBtn.onClick.AddListener(() =>
            {
                OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.Shop.ToString());
                _shopPanel.gameObject.SetActive(false);
            });
        }
        if (_shopEnlargePanel.Find("BuyBtn").TryGetComponent<Button>(out buyBtn))
        {
            buyBtn.onClick.AddListener(() =>
            {
                BuyCard();
                OutGameUIManager.Instance.RemoveOpenUIOrder("EnlargeCard");
                _shopEnlargePanel.gameObject.SetActive(false);
            });
        }
        if (_shopEnlargePanel.Find("BackBtn").TryGetComponent<Button>(out enlargeBackBtn))
        {
            enlargeBackBtn.onClick.AddListener(() =>
            {
                OutGameUIManager.Instance.RemoveOpenUIOrder("EnlargeCard");
                _shopEnlargePanel.gameObject.SetActive(false);
            });
        }

        Transform shopCard = _shopPanel.Find("Card");
        Transform shopCardPrice = _shopPanel.Find("Price");

        _shopCount = shopCard.childCount;

        _isSaveShop = new bool[MapManager.Instance.IsSaveChapter.Length];
        _buyInfo = new bool[_isSaveShop.Length, _shopCount];
        _shopCardData = new CardData[_isSaveShop.Length][];
        _shopCards = new UICard[_shopCount];
        _shopCardPrices = new TMP_Text[_shopCount];

        UICard enlargeShopCard = _shopEnlargePanel.Find("UICardImg").GetComponent<UICard>();

        for (int i = 0; i < _shopCards.Length; ++i)
        {
            _shopCards[i] = shopCard.GetChild(i).GetComponent<UICard>();
            _shopCards[i].EnlargeCard = enlargeShopCard;
            _shopCardPrices[i] = shopCardPrice.GetChild(i).GetComponent<TMP_Text>();
        }

        //_cardRewardContent = FindTransform.ContinueFindChildUIByName(Canvas(CanvasName.CardReward), "Content");

        //for (int i = 0; i < _uiCards.Length; ++i)
        //{
        //    _uiCards[i] = _cardRewardContent.GetChild(i).GetComponent<UICard>();
        //    _uiCards[i].EnlargeCard = enlargeRewardCard;
        //}

        //_itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel") as RectTransform;
        //_itemRewardContent = FindTransform.ContinueFindChildUIByName(Canvas(CanvasName.ItemReward), "Content");

        //for (int i = 0; i < _uiItems.Length; ++i)
        //{
        //    _uiItems[i] = _itemRewardContent.GetChild(i).GetComponent<UIItem>();
        //}
    }

    //private void Start()
    //{

    //    //_shopCard = InGameUIManager.Instance.FindChildByName(InGameUIManager.CanvasName.Shop, "Card");
    //    //_shopCardPrice = InGameUIManager.Instance.FindChildByName(InGameUIManager.CanvasName.Shop, "Price");
    //    SettingCardShop();
    //}

    //void SettingCardShop()
    //{
    //    Transform shopCard = _shopPanel.Find("Card");
    //    Transform shopCardPrice = _shopPanel.Find("Price");

    //    _shopCount = shopCard.childCount;

    //    _isSaveShop = new bool[MapManager.Instance.IsSaveChapter.Length];
    //    _buyInfo = new bool[_isSaveShop.Length, _shopCount];
    //    _shopCardData = new CardData[_isSaveShop.Length][];
    //    _shopCards = new UICard[_shopCount];
    //    _shopCardPrices = new TMP_Text[_shopCount];

    //    UICard enlargeShopCard = _shopEnlargePanel.Find("UICardImg").GetComponent<UICard>();

    //    for (int i = 0; i < _shopCards.Length; ++i)
    //    {
    //        _shopCards[i] = shopCard.GetChild(i).GetComponent<UICard>();
    //        _shopCards[i].EnlargeCard = enlargeShopCard;
    //        _shopCardPrices[i] = shopCardPrice.GetChild(i).GetComponent<TMP_Text>();
    //    }
    //}
    public void ChangeCardShop(bool reroll = false)
    {
        int nowChapterLV = GameManager.Instance.NowChapterLV;

        if (reroll || !_isSaveShop[nowChapterLV])
        {
            _shopCardData[nowChapterLV] = InGameManager.Instance.RandomCards(10, _shopCount);
            for (int col = 0; col < _shopCount; ++col)
            {
                _buyInfo[nowChapterLV, col] = false;
            }
            _isSaveShop[nowChapterLV] = true;
        }
        //InGameManager.Instance.ReturnRandomCard(_shopCardData);

        int i = 0;
        foreach (var cardData in _shopCardData[nowChapterLV])
        {
            if (!_buyInfo[nowChapterLV, i])
            {
                _shopCards[i].gameObject.SetActive(true);
                if (cardData != null)
                {
                    _shopCards[i].Setup(cardData);
                    _shopCardPrices[i].text = cardData.Price.ToString();
                }
                else
                {
                    _shopCards[i].Setup(cardData);
                    _shopCardPrices[i].text = "0";
                }
            }
            else
            {
                print(i);
                _shopCards[i].gameObject.SetActive(false);
                _shopCardPrices[i].text = "";
            }
            ++i;
        }

        //for (int i = 0; i < _shopCards.Length; ++i)
        //{
        //    //CardData _cardData = InGameManager.Instance.FindCardData(Random.Range(100, 106));
        //    CardData cardData = InGameManager.Instance.RandomCard(10);
        //    _shopCards[i].Setup(cardData);
        //    _shopCardPrices[i].text = cardData.Price.ToString();
        //}
    }

    public void CloseShop()
    {
        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.Shop.ToString());
        _shopPanel.gameObject.SetActive(false);
        OutGameUIManager.Instance.RemoveOpenUIOrder("EnlargeCard");
        _shopEnlargePanel.gameObject.SetActive(false);
    }
    public void BuyCard()
    {
        if (CardManager.Instance.GetCardData == null) return;
        if (InGameManager.Instance.Player.Coin.Value >= CardManager.Instance.GetCardData.Price)
        {
            InGameManager.Instance.Player.Coin.Value -= CardManager.Instance.GetCardData.Price;
            CardManager.Instance.AddDeck(CardManager.Instance.GetCardData, EAddDeck.Main);
            _buyInfo[GameManager.Instance.NowChapterLV, _shopCardIdx] = true;
            _shopCards[_shopCardIdx].gameObject.SetActive(false);
            _shopCardPrices[_shopCardIdx].text = "";
        }
        else
            print("µ∑∫Œ¡∑");
    }

    public void BuyCardIdx(int idx)
    {
        _shopCardIdx = idx;
    }
}
