using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] Transform _shopCard;
    [SerializeField] Transform _shopCardPrice;

    UICard[] _shopCards;
    TMP_Text[] _shopCardPrices;

    int _shopCardIdx;

    CardData[][] _shopCardData;
    bool[,] _buyInfo;
    bool[] _isSaveShop;
    void Awake()
    {
        Instance = Instance != null ? Instance : this;
    }

    private void Start()
    {

        //_shopCard = InGameUIManager.Instance.FindChildByName(InGameUIManager.CanvasName.Shop, "Card");
        //_shopCardPrice = InGameUIManager.Instance.FindChildByName(InGameUIManager.CanvasName.Shop, "Price");
        SettingCardShop();
    }

    void SettingCardShop()
    {
        _isSaveShop = new bool[MapManager.Instance.IsSaveChapter.Length];
        _buyInfo = new bool[_isSaveShop.Length, _shopCard.childCount];
        _shopCardData = new CardData[_isSaveShop.Length][];
        _shopCards = new UICard[_shopCard.childCount];
        _shopCardPrices = new TMP_Text[_shopCard.childCount];
        for (int i = 0; i < _shopCards.Length; ++i)
        {
            _shopCards[i] = _shopCard.GetChild(i).GetComponent<UICard>();
            _shopCardPrices[i] = _shopCardPrice.GetChild(i).GetComponent<TMP_Text>();
        }
    }
    public void ChangeCardShop(bool reroll = false)
    {
        int nowChapterLV = GameManager.Instance.NowChapterLV;

        if (reroll || !_isSaveShop[nowChapterLV])
        {
            _shopCardData[nowChapterLV] = InGameManager.Instance.RandomCards(10, _shopCard.childCount);
            for (int col = 0; col < _shopCard.childCount; ++col)
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
