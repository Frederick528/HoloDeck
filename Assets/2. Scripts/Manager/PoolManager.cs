using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using static InGameUIManager;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public int defaultCapacity = 10;
    //public int maxPoolSize = 10;
    public Card CardPrefab;
    public TMP_Text TextEffectPrefab;
    public UICard UICardPrefab;
    public GameObject[] EffectPrefabs;
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
    public Dictionary<GameObject, IObjectPool<GameObject>> EffectPool = new Dictionary<GameObject, IObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
    }

    private void Init()
    {
        _deck = InGameManager.Instance.PlayerTr.Find("Deck");
        _textParent = FindTransform.ContinueFindChildByName(InGameUIManager.Instance.Canvas(CanvasName.InGame), "TextEffect");
        //ViewDeckContent = InGameUIManager.Instance.ContinueFindChildByName(InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.ViewDeck), "Content");

        CardPool = new ObjectPool<Card>(CreateCardPooled, OnTakeFromPoolCard, OnReturnedToPoolCard, OnDestroyPoolCard, true, defaultCapacity);

        TextPool = new ObjectPool<TMP_Text>(CreateTextPooled, OnTakeFromPoolText, OnReturnedToPoolText, OnDestroyPoolText, true, defaultCapacity);

        //UICardPool = new ObjectPool<UICard>(CreateUICardPooled, OnTakeFromPoolUICard, OnReturnedToPoolUICard, OnDestroyPoolUICard, true, defaultCapacity);
        EffectPrefabs = InGameManager.Instance.CardSO.CardEffects;

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
    //    return Instantiate(UICardPrefab, InGameUIManager.Instance.ViewDeckContent);
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
    public async UniTask GetEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        if (!EffectPool.ContainsKey(prefab))
        {
            var pool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, transform),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: true,
                    defaultCapacity: 1,
                    maxSize: 5
                );

            EffectPool.Add(prefab, pool);
        }

        // 1. 풀에서 이펙트 오브젝트를 가져옴
        GameObject instance = EffectPool[prefab].Get();
        instance.transform.SetPositionAndRotation(position, rotation);

        // 2. 파티클 시스템의 재생 시간을 가져옴
        var ps = instance.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            // 파티클이 없다면 그냥 반납 처리
            EffectPool[prefab].Release(instance);
            return;
        }

        // 3. 파티클 재생 시간만큼 기다린 후 자동으로 반납하는 코루틴 시작
        await ReleaseEffect(prefab, instance, ps.main.duration);        // duration이랑 공격 타이밍 비교해서 딜 넣기.
    }

    private async UniTask ReleaseEffect(GameObject prefab, GameObject instance, float delay)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();

        if (prefab != null && EffectPool.ContainsKey(prefab))
        {
            EffectPool[prefab].Release(instance);
        }
        else
        {
            // 풀을 못찾는 경우 파괴
            Destroy(instance);
        }
    }
}