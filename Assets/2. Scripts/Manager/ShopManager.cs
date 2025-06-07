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
        _shopCards = new UICard[_shopCard.childCount];
        _shopCardPrices = new TMP_Text[_shopCard.childCount];
        for (int i = 0; i < _shopCards.Length; ++i)
        {
            _shopCards[i] = _shopCard.GetChild(i).GetComponent<UICard>();
            _shopCardPrices[i] = _shopCardPrice.GetChild(i).GetComponent<TMP_Text>();
        }
        ChangeCardShop();
    }
    public void ChangeCardShop()
    {
        for (int i = 0; i < _shopCards.Length; ++i)
        {
            //CardData _cardData = InGameManager.Instance.FindCardData(Random.Range(100, 106));
            CardData cardData = InGameManager.Instance.RandomCard(10);
            _shopCards[i].Setup(cardData);
            _shopCardPrices[i].text = cardData.Price.ToString();
        }
    }
    public void BuyCard()
    {
        if (InGameManager.Instance.Player.Coin.Value >= CardManager.Instance.GetCardData.Price)
        {
            InGameManager.Instance.Player.Coin.Value -= CardManager.Instance.GetCardData.Price;
            CardManager.Instance.AddDeck(CardManager.Instance.GetCardData, EAddDeck.Main);
            _shopCard.GetChild(_shopCardIdx).GetComponent<UICard>().gameObject.SetActive(false);
            _shopCardPrice.GetChild(_shopCardIdx).GetComponent<TMP_Text>().text = "";
        }
        else
            print("µ∑∫Œ¡∑");
    }

    public void BuyCardIdx(int idx)
    {
        _shopCardIdx = idx;
    }
}
