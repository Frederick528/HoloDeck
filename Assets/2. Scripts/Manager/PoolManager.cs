using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance { get; private set; }

    public int defaultCapacity = 10;
    //public int maxPoolSize = 10;
    public GameObject cardPrefab;
    public GameObject mapPrefab;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform deck;

    [SerializeField] Transform map;
    //[SerializeField] Transform HandCard;

    public IObjectPool<GameObject> CardPool { get; private set; }
    public IObjectPool<GameObject> MapPool { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        Init();
    }

    private void Init()
    {
        MapPool = new ObjectPool<GameObject>(CreateMapPooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, defaultCapacity/*, maxPoolSize*/);
        CardPool = new ObjectPool<GameObject>(CreateCardPooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, defaultCapacity/*, maxPoolSize*/);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < defaultCapacity; i++)
        {
            Map map = CreateMapPooledItem().GetComponent<Map>();
            map.MapRelease();
            Card card = CreateCardPooledItem().GetComponent<Card>();
            card.CardRelease();
        }
    }

    // 생성
    private GameObject CreateCardPooledItem()
    {
        GameObject cardPoolGo = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, deck);
        cardPoolGo.GetComponent<Card>().CardPool = this.CardPool;
        return cardPoolGo;
    }
    private GameObject CreateMapPooledItem()
    {
        GameObject mapPoolGo = Instantiate(mapPrefab, Vector3.one * 0.5f, Quaternion.identity, map);
        mapPoolGo.GetComponent<Map>().MapPool = this.MapPool;
        return mapPoolGo;
    }

    // 사용
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
}