using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public int defaultCapacity = 10;
    //public int maxPoolSize = 10;
    public GameObject cardPrefab;
    //public GameObject _mapPrefab;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform deck;

    //[SerializeField] Transform map;
    //[SerializeField] Transform HandCard;

    public IObjectPool<System.Tuple<GameObject, Card>> CardPool { get; private set; }
    //public IObjectPool<GameObject> MapPool { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);


        Init();
    }

    private void Init()
    {
        //MapPool = new ObjectPool<GameObject>(CreateMapPooledItem, OnTakeFromPool, OnReturnedToPool,
        //OnDestroyPoolObject, true, defaultCapacity/*, maxPoolSize*/);
        CardPool = new ObjectPool<System.Tuple<GameObject, Card>>(CreateCardPooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, defaultCapacity/*, maxPoolSize*/);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < defaultCapacity; i++)
        {
            //Map map = CreateMapPooledItem().GetComponent<Map>();
            //map.MapRelease();
            ReleaseCard(CreateCardPooledItem());
        }
    }

    // 생성
    private System.Tuple<GameObject, Card> CreateCardPooledItem()
    {
        GameObject cardObj = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, deck);
        Card card = cardObj.GetComponent<Card>();
        //card.CardPool = this.CardPool;

        return new System.Tuple<GameObject, Card>(cardObj, card);
    }
    //private GameObject CreateMapPooledItem()
    //{
    //    GameObject mapPoolGo = Instantiate(_mapPrefab, Vector3.one * 0.5f, Quaternion.identity, map);
    //    mapPoolGo.GetComponent<Map>().MapPool = this.MapPool;
    //    return mapPoolGo;
    //}

    // 사용
    private void OnTakeFromPool(System.Tuple<GameObject, Card> card)
    {
        card.Item1.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(System.Tuple<GameObject, Card> card)
    {
        card.Item1.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(System.Tuple<GameObject, Card> card)
    {
        Destroy(card.Item1 );
    }

    public void GetCard(out GameObject obj, out Card card)
    {
        var poolObject = CardPool.Get();
        obj = poolObject.Item1;
        card = poolObject.Item2;
    }
    public void ReleaseCard(System.Tuple<GameObject, Card> card)
    {
        CardPool.Release(card);
    }
    public void ReleaseCard(GameObject obj, Card card)
    {
        CardPool.Release(new System.Tuple<GameObject, Card>(obj, card));
    }
}