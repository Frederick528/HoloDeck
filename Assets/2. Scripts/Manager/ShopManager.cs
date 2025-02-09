using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] Transform _shopCard;
    [SerializeField] Transform _shopCardPrice;

    int _shopCardIdx;
    void Awake() { Instance = this; }

    //private void Start()
    //{
    //    _shopCard = UiManager.Instance.ShopPanel.Find("Card");
    //    _shopCardPrice = UiManager.Instance.ShopPanel.Find("Price");
    //}

    public void SettingCardShop()
    {
        for (int i = 0; i < _shopCard.childCount; ++i)
        {
            CardData _cardData = InGameManager.Instance.FindCardData(Random.Range(100, 106));
            _shopCard.GetChild(i).GetComponent<UICard>().Setup(_cardData);       // 나중에 다 캐싱할 것
            _shopCardPrice.GetChild(i).GetComponent<TMP_Text>().text = _cardData.Price.ToString();
        }
    }
    public void BuyCard()
    {
        if (InGameManager.Instance.player.Coin.Value >= CardManager.Instance.GetCardData.Price)
        {
            InGameManager.Instance.player.Coin.Value -= CardManager.Instance.GetCardData.Price;
            CardManager.Instance.AddDeck(CardManager.Instance.GetCardData, EAddDeck.Main);
            _shopCard.GetChild(_shopCardIdx).GetComponent<UICard>().gameObject.SetActive(false);
            _shopCardPrice.GetChild(_shopCardIdx).GetComponent<TMP_Text>().text = "";
        }
        else
            print("돈부족");
    }

    public void BuyCardIdx(int idx)
    {
        _shopCardIdx = idx;
    }
}
