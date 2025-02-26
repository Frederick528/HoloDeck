using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public int defaultCapacity = 10;
    //public int maxPoolSize = 10;
    public Card cardPrefab;
    public TMP_Text TextEffect;
    //public GameObject _mapPrefab;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform deck;

    [SerializeField] Transform _textParent;

    //[SerializeField] Transform map;
    //[SerializeField] Transform HandCard;

    public IObjectPool<Card> CardPool { get; private set; }
    public IObjectPool<TMP_Text> TextPool { get; private set; }
    //public IObjectPool<GameObject> MapPool { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        Init();
    }

    //private void Start()
    //{
    //    _textParent = UiManager.Instance.ContinueFindChildByName(UiManager.Instance.Canvas(UiManager.CanvasName.InGame), "TextEffect");
    //    Init();
    //}

    private void Init()
    {
        //MapPool = new ObjectPool<GameObject>(CreateMapPooledItem, OnTakeFromPool, OnReturnedToPool,
        //OnDestroyPoolObject, true, defaultCapacity/*, maxPoolSize*/);
        CardPool = new ObjectPool<Card>(CreateCardPooledItem, OnTakeFromPoolCard, OnReturnedToPoolCard,
        OnDestroyPoolCard, true, defaultCapacity/*, maxPoolSize*/);

        TextPool = new ObjectPool<TMP_Text>(CreateTextPooledItem, OnTakeFromPoolText, OnReturnedToPoolText, OnDestroyPoolText, true, defaultCapacity);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < defaultCapacity; i++)
        {
            //Map map = CreateMapPooledItem().GetComponent<Map>();
            //map.MapRelease();
            ReleaseCard(CreateCardPooledItem());
            ReleaseText(CreateTextPooledItem());
        }
    }

    // 생성
    private Card CreateCardPooledItem()
    {
        Card cardObj = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, deck);
        //Card card = cardObj.GetComponent<Card>();
        //card.CardPool = this.CardPool;

        return cardObj;
    }
    //private GameObject CreateMapPooledItem()
    //{
    //    GameObject mapPoolGo = Instantiate(_mapPrefab, Vector3.one * 0.5f, Quaternion.identity, map);
    //    mapPoolGo.GetComponent<Map>().MapPool = this.MapPool;
    //    return mapPoolGo;
    //}
    private TMP_Text CreateTextPooledItem()
    {
        return Instantiate(TextEffect, _textParent);
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

    public void ReleaseText(TMP_Text text)
    {
        TextPool.Release(text);
    }
    public void GetText(out TMP_Text text)
    {
        text = TextPool.Get();
    }

    public void GetCard(/*out GameObject obj, */out Card card)
    {
        card = CardPool.Get();
    }
    public void ReleaseCard(Card card)
    {
        CardPool.Release(card);
    }
    //public void ReleaseCard(GameObject obj, Card card)
    //{
    //    CardPool.Release(new System.Tuple<GameObject, Card>(obj, card));
    //}
}