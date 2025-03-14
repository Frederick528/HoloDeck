using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using static UIManager;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public int defaultCapacity = 10;
    //public int maxPoolSize = 10;
    public Card CardPrefab;
    public TMP_Text TextEffectPrefab;
    public UICard UICardPrefab;
    //public GameObject _mapPrefab;

    Transform _deck;
    Transform _textParent;
    //Transform ViewDeckContent;

    //[SerializeField] Transform map;
    //[SerializeField] Transform HandCard;
    int _setActiveUICard;


    public IObjectPool<Card> CardPool { get; private set; }
    public IObjectPool<TMP_Text> TextPool { get; private set; }
    public IObjectPool<UICard> UICardPool { get; private set; }

    private void Awake()
    {
        Instance = Instance != null ? Instance : this;

        Init();
    }

    private void Init()
    {
        _deck = UIManager.Instance.Player.Find("Deck");
        _textParent = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(CanvasName.InGame), "TextEffect");
        //ViewDeckContent = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.ViewDeck), "Content");

        CardPool = new ObjectPool<Card>(CreateCardPooled, OnTakeFromPoolCard, OnReturnedToPoolCard, OnDestroyPoolCard, true, defaultCapacity);

        TextPool = new ObjectPool<TMP_Text>(CreateTextPooled, OnTakeFromPoolText, OnReturnedToPoolText, OnDestroyPoolText, true, defaultCapacity);

        //UICardPool = new ObjectPool<UICard>(CreateUICardPooled, OnTakeFromPoolUICard, OnReturnedToPoolUICard, OnDestroyPoolUICard, true, defaultCapacity);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < defaultCapacity; ++i)
        {
            ReleaseCard(CreateCardPooled());
            ReleaseText(CreateTextPooled());
            //ReleaseUICard(CreateUICardPooled());
        }
    }

    // 생성
    private Card CreateCardPooled()
    {
        Card cardObj = Instantiate(CardPrefab, CardManager.Instance.CardSpawnPoint.position, Quaternion.identity, _deck);
        //Card card = cardObj.GetComponent<Card>();
        //card.CardPool = this.CardPool;

        return cardObj;
    }

    // 사용
    private void OnTakeFromPoolCard(Card card)
    {
        card.gameObject.SetActive(true);
    }

    // 반환
    private void OnReturnedToPoolCard(Card card)
    {
        card.gameObject.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolCard(Card card)
    {
        Destroy(card.gameObject);
    }

    public Card GetCard(/*out Card card*/)
    {
       return CardPool.Get();
    }
    public void ReleaseCard(Card card)
    {
        CardPool.Release(card);
    }
    //public void ReleaseCard(GameObject obj, Card card)
    //{
    //    CardPool.Release(new System.Tuple<GameObject, Card>(obj, card));
    //}
    private TMP_Text CreateTextPooled()
    {
        return Instantiate(TextEffectPrefab, _textParent);
    }
    private void OnTakeFromPoolText(TMP_Text text)
    {
        text.gameObject.SetActive(true);
    }
    private void OnReturnedToPoolText(TMP_Text text)
    {
        text.gameObject.SetActive(false);
    }
    private void OnDestroyPoolText(TMP_Text text)
    {
        Destroy(text.gameObject);
    }
    public void ReleaseText(TMP_Text text)
    {
        TextPool.Release(text);
    }
    public TMP_Text GetText()
    {
        return TextPool.Get();
    }

    //UICard CreateUICardPooled()
    //{
    //    ++_setActiveUICard;
    //    return Instantiate(UICardPrefab, UIManager.Instance.ViewDeckContent);
    //}
    //private void OnTakeFromPoolUICard(UICard uiCard)
    //{
    //    uiCard.gameObject.SetActive(true);
    //}
    //private void OnReturnedToPoolUICard(UICard uiCard)
    //{
    //    uiCard.gameObject.SetActive(false);
    //}
    //private void OnDestroyPoolUICard(UICard uiCard)
    //{
    //    Destroy(uiCard.gameObject);
    //}
    //public UICard GetUICard()
    //{
    //    ++_setActiveUICard;
    //    return UICardPool.Get();
    //}
    //public UICard GetUICard(int count)
    //{
    //    if (_setActiveUICard < count)
    //    {
    //        ++_setActiveUICard;
    //        return UICardPool.Get();
    //    }
    //    return null;
    //}
    //public void ReleaseUICard(UICard uiCard)
    //{
    //    --_setActiveUICard;
    //    UICardPool.Release(uiCard);
    //}
    //public void ReleaseUICard(UICard uiCard, int count)
    //{
    //    if (_setActiveUICard > count)
    //    {
    //        --_setActiveUICard;
    //        UICardPool.Release(uiCard);
    //    }
    //}

}