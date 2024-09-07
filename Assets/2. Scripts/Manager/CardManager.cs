using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    public Dictionary<int, CardData> CardDatas {  get; private set; } = new Dictionary<int, CardData>();
    //public List<Card> Deck { get; private set; }

    public List<Card> MainDeck;   // 덱 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X)
    //public List<Card> MainCardDeck;     // 덱에 있는 카드 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X) (MainDeck과 같이 카드 추가)
    public List<Card> DrawDeck;   // 현재 내가 뽑을 수 있는 카드
    public List<Card> CardDummy;  // 카드 더미(사용 또는 버림)
    public List<Card> HandCard;   // 내 손에 있는 카드

    public bool isSingleTarget;
    public bool useSingleTargetCard;
    
    public ECardState cardState;

    [SerializeField] CardSO cardSO;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardDummyTr;

    [SerializeField] Transform Deck;    // 소환된 덱 카드들

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;

    [SerializeField] GameObject arrow;
    
    [SerializeField] GameObject cardPrefab;

    [SerializeField] Transform cardRewardContent;

    Card selectCard;
    bool draggable;
    bool isUseCard;     // 카드 사용존에 카드가 올라왔을 경우(카드를 놓으면 카드가 사용되는 위치)
    
    bool canPush = true;
    public enum ECardState { Nothing, CanMouseOver, CanMouseDrag }


    
    private void Awake() => Instance = this;


    private void Start()
    {
        SetupStartCardDeck();
        //StartBattle();    // 현재 Battle.cs에서 진행중
    }
    //private void Update()
    //{
    //    SetCardState();     // UniRx 이용해서 따로 처리할 것

    //}

    public void ShowRewardCard(int[] reward)
    {
        for (int i = 0; i < reward.Length; ++i)
            cardRewardContent.GetChild(i).GetComponent<UICard>().Setup(FindCardData(reward[i]));
    }

    public CardData FindCardData(int id)   // id 값으로 카드데이터 가져오기
    {
        CardData _cardData;
        if (CardDatas.TryGetValue(id, out _cardData))
        {
            return _cardData;
        }
        else
        {
            _cardData = Array.Find(cardSO.cards, x => x.id == id);
            CardDatas.Add(id, _cardData);
            return _cardData;
        }
        //return cardSO.cards.Find(x => x.id == id);
        //return Array.Find(cardSO.cards, x => x.id == id);
    }

    void SetupStartCardDeck()   // 시작할 때, 메인덱을 설정하는 함수 (게임 시작 이후에는 사용하지 않음.)
    {
        int _startDeck = 6;     // 여기 밑 코드 변경해야 함. 캐릭터별로 얻는 카드와 카드 ID가 달라지기 때문에 switch로 구별.
        for (int i = 0; i < _startDeck; i++)
            AddDeck(cardSO.cards[i], EAddDeck.Main);

        //AddDeck(FindCardInCardSO(1000), EAddDeck.Main);     // 이 부분은 제거할 것
        //AddDeck(FindCardInCardSO(1001), EAddDeck.Main);     // 이 부분은 제거할 것
    }

    public void AddDeck(CardData cardData, EAddDeck eAddDeck)     // 덱에 카드를 추가할 때 사용, 핸드로 카드를 가져올 때는 AddCard 함수 사용.
    {
        GameObject cardObject = PoolManager.instance.CardPool.Get();
            /*Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, Deck);*/
        Card setCard = cardObject.GetComponent<Card>();

        setCard.name = cardData.name;
        cardObject.name = cardData.name;    // 시각화 용도

        setCard.Setup(cardData);
        switch (eAddDeck)
        {
            case EAddDeck.Main:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                MainDeck.Add(setCard);
                //MainCardDeck.Add(setCard);
                break;

            case EAddDeck.Draw:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                DrawDeck.Add(setCard);
                ShuffleDeck();
                break;

            case EAddDeck.Dummy:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                CardDummy.Add(setCard);
                break;

            case EAddDeck.Hand:                 // 핸드로 가져오는 건 카드 정렬 때문에 AddCard 함수를 이용해서만 접근할 것.
                if (HandCard.Count < 10)
                {
                    setCard.block = true;
                    cardObject.transform.position = Vector3.zero;
                    HandCard.Add(setCard);
                    StartCoroutine(WaitUnblock(setCard, CardUtils.CardAlignmentDelay));
                }
                else
                {
                    cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                    CardDummy.Add(setCard);
                }
                break;

        }
    }

    IEnumerator WaitUnblock(Card setCard, float waitTime)       //  .Forget() 추가하기 귀찮아서 그냥 코루틴으로 작성함.
    {
        yield return new WaitForSeconds(waitTime);
        setCard.block = false;
    }

    public void ClearCard()
    {
        DrawDeck.Clear();
        CardDummy.Clear();
        //HandCard.Clear();
        for (int i = 0; i < Deck.childCount; i++)
        {
            if (!Deck.GetChild(i).gameObject.activeSelf)
                continue;
            Card card = Deck.GetComponentsInChildren<Card>(true)[i];
            //GameObject cardObject = Deck.GetChild(i).gameObject;
            if (MainDeck.Contains(card))
            {
                card.MoveTransform(new PRS(cardSpawnPoint.position, Quaternion.identity, CardUtils.CardScale), false);
                continue;
            }
            card.CardRelease();
            //MainCardDeck.Add(card);
            //cardObject.GetComponent<Card>().CardRelease();

        }
    }
    public void SetupDrawDeck(bool start = false)  // 드로우덱 섞기(start가 true일 경우, 메인덱에서 가져옴. false일 경우, 카드더미에서 가져옴.)
    {
        if (!start)
        {
            DrawDeck = CardDummy.ToList();
            //foreach (Card card in CardDummy)
            //{
            //    DrawDeck.Add(card);
            //}
            CardDummy.Clear();
        }
        else
        {
            DrawDeck = MainDeck.ToList();
            //foreach (Card card in MainDeck)
            //{
            //    DrawDeck.Add(card);
            //}
        }
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < DrawDeck.Count; i++)
        {
            int rand = Random.Range(0, DrawDeck.Count);
            Card temp = DrawDeck[i];
            DrawDeck[i] = DrawDeck[rand];
            DrawDeck[rand] = temp;
        }
    }
    
    public async UniTask<Card> DrawCard()
    {
        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기. 이 경우에는 카드 뽑기가 0.5초 후 가능 (카드 버려지는 시간인 0.3초보단 높게 잡아야 함.)
        {
            SetupDrawDeck();
            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));
        }

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        Card card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        return card;
    }

    public async UniTask<Card> DrawCard(Card drawCard)
    {
        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기. 이 경우에는 카드 뽑기가 LoadCardDummyDelay초 후 가능 (카드 버려지는 시간인 ThrowAwayCardDelay초보단 높게 잡아야 함.)
        {
            SetupDrawDeck();
            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));
        }

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        Card card = DrawDeck.Find(x => x == drawCard);
        DrawDeck.Remove(card);
        return card;

        //for (int i = 0; i < DrawDeck.Count; i++)
        //{
        //    if (DrawDeck[i] == drawCard)
        //    {
        //        Card card = DrawDeck[i];
        //        DrawDeck.RemoveAt(i);
        //        return card;
        //    }
        //}
        //return null;
    }

    public async UniTask<Card[]> DrawCards(int count)
    {
        Card[] card = new Card[count];
        int tempDraw = DrawDeck.Count;
        if (DrawDeck.Count < count)    // 덱에 있는 카드가 뽑을 카드보다 적으면, 일단 덱에 있는 카드를 뽑고 덱 섞기. 이 경우에는 카드 뽑기가 LoadCardDummyDelay초 후 가능 (카드 버려지는 시간인 ThrowAwayCardDelay초보단 높게 잡아야 함.)
        {
            for (int i = 0; i < DrawDeck.Count; ++i)
            {
                card[i] = DrawDeck[0];
                DrawDeck.RemoveAt(0);
            }
            SetupDrawDeck();
            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));
        }


        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return card;
        
        //if (DrawDeck.Count < count)
        //    count = DrawDeck.Count;

        //Card[] card = new Card[count];
        for (int i = tempDraw; i < count; ++i)      // 위에서 리턴이 걸리지 않으면, 남은 카드를 뽑음. 남은 카드를 뽑던 중, 덱에 있는 카드가 없을 경우, 리턴
        {
            if (DrawDeck.Count == 0)
                return card;
            card[i] = DrawDeck[0];
            DrawDeck.RemoveAt(0);
        }
        return card;
    }

    public async UniTask AddCard()   // 손패로 드로우할 카드
    {
        Card drawCard = await DrawCard();
        if (drawCard == null)
            return;
        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        //Card card = drawCard.GetComponent<Card>();
        //drawCard.Setup(drawCard.Data);
        HandCard.Add(drawCard);

        SetOriginOrder();
        CardAlignment();
    }

    public async UniTask AddCards(int count)   // 손패로 드로우할 카드
    {
        Card[] drawCard = await DrawCards(count);
        //if (drawCard == null)
        //    return;

        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        //Card card = drawCard.GetComponent<Card>();
        //drawCard.Setup(drawCard.Data);
        for (int i = 0; i < drawCard.Length; ++i)
        {
            if (drawCard[i] == null)
                return;
            HandCard.Add(drawCard[i]);

            SetOriginOrder();
            CardAlignment();
            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.CardAlignmentDelay));
        }
    }

    public async UniTask AddCard(Card addCard)    // 덱에서 손패로 카드를 가져옴.
    {
        Card drawCard = await DrawCard(addCard);
        if (drawCard == null)
            return;

        //Card card = drawCard.GetComponent<Card>();
        //drawCard.Setup(drawCard.Data);
        HandCard.Add(drawCard);

        SetOriginOrder();
        CardAlignment();

    }

    public void AddCard(CardData addCard)  // 카드 생성
    {
        AddDeck(addCard, EAddDeck.Hand);

        SetOriginOrder();
        CardAlignment();
    }

    public async UniTask ThrowAwayCard()
    {
        foreach (Card targetCard in HandCard)
        {
            targetCard.block = true;
            targetCard.MoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);

            CardDummy.Add(targetCard);
        }

        
        await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.ThrowAwayCardDelay));

        foreach (Card targetCard in HandCard)
        {
            targetCard.block = false;
            targetCard.MoveTransform(new PRS(cardSpawnPoint.position, Quaternion.identity, CardUtils.CardScale), false);
        }
        HandCard.Clear();


        //foreach (Card dummyCard in CardDummy)
        //{
        //    dummyCard.block = false;
        //    dummyCard.CardPool.Release(dummyCard.gameObject);
        //}
    }


    public async UniTask UsedCard(Card usedCard)
    {
        usedCard.cardAction?.Invoke(usedCard);
        
        HandCard.Remove(usedCard);

        //await UniTask.Delay(TimeSpan.FromSeconds(usedCard.Data.cardUseDelay));

        //if (MapManager.Instance.currStage.cleared)
        //    return;

        CardDummy.Add(usedCard);

        SetOriginOrder();
        CardAlignment();

        await usedCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);

        //throwCard.block = false;
        usedCard.transform.position = cardSpawnPoint.position;
        usedCard.block = false;

        //await UniTask.Delay(TimeSpan.FromSeconds(usedCard.Data.cardUseDelay));
        //CardDummy.Add(usedCard);
    }

    public async UniTask ThrowAwayCard(Card throwCard)
    {
        CardDummy.Add(throwCard);

        HandCard.Remove(throwCard);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);

        //throwCard.block = false;
        throwCard.transform.position = cardSpawnPoint.position;
    }

    void SetOriginOrder()       // 카드가 보이는 순서 설정
    {
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count/*, 0.5f*/, CardUtils.CardScale);
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];

            targetCard.originPRS = originCardPRSs[i];
            if (targetCard == selectCard)
                continue;
            targetCard.MoveTransform(targetCard.originPRS, true, CardUtils.CardAlignmentDelay);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int cardCount/*, float height*/, Vector3 scale)
    {
        float[] cardLerps = new float[cardCount];
        List<PRS> results = new List<PRS>(cardCount);

        if (cardCount == 1)
        {
            cardLerps = new float[] { 0.5f };
        }
        else
        {
            float interval = cardCount < 7f ? 1f / 7 : 1f / cardCount;
            float cardPos = 0f;
            for (int i = 0; i < cardCount; i++)
            {
                if (i == 0)
                    cardPos += cardCount < 7f ? (interval * (4f - cardCount * 0.5f)) : interval * 0.5f;
                else
                    cardPos += interval;
                cardLerps[i] = cardPos;
            }
        }

        for (int i = 0; i < cardCount; i++)
        {
            Vector3 targetPos = Vector3.Lerp(leftTr.position, rightTr.position, cardLerps[i]);
            targetPos.z = -i * 5;

            float curve = Mathf.Sqrt(Mathf.Pow(/*height*/0.5f, 2) - Mathf.Pow(cardLerps[i] - 0.5f, 2));   // 원의 방정식
            //float curve = Mathf.Sqrt(Mathf.Pow(/*height*/0.5f, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // 타원의 방정식

            targetPos.y += 3f * curve - 1.5f;
            Quaternion targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, cardLerps[i]);

            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (cardState == ECardState.Nothing || draggable)
            return;
        selectCard = card;
        LargeCard(true, card);
        PushCard(card);
    }
    public void CardMouseExit(Card card)
    {
        if (cardState == ECardState.Nothing || draggable)
            return;
        selectCard = null;
        LargeCard(false, card);
        PullCard();
    }


    void LargeCard(bool isLarge, Card card)
    {
        if (isLarge)
        {
            card.transform.DOKill();
            Vector3 largePos = new Vector3(card.originPRS.pos.x, -3.32f, -100f);
            card.MoveTransform(new PRS(largePos, Quaternion.identity, CardUtils.CardScale * 1.2f), false);
        }
        else
            card.MoveTransform(card.originPRS, true, CardUtils.CardAlignmentDelay);

        card.GetComponent<Order>().SetMostFrontOrder(isLarge);
    }
    void PushCard(Card card)
    {
        if (!canPush)
            return;
        //int cardIndex = -1;
        //for (int i = 0; i < HandCard.Count; i++)
        //{
        //    if (HandCard[i] == card)
        //    {
        //        cardIndex = i;
        //        break;
        //    }
        //}
        int cardIndex = HandCard.IndexOf(card);
        if (cardIndex == -1)
            return;

        for (int i = 1; i < HandCard.Count; i++)    // 나중에 수정 필요해보임.
        {
            if (cardIndex - i >= 0)
                HandCard[cardIndex - i].transform.DOMoveX(HandCard[cardIndex - i].originPRS.pos.x - 0.5f/i, CardUtils.CardAlignmentDelay);
            if (cardIndex + i < HandCard.Count)
                HandCard[cardIndex + i].transform.DOMoveX(HandCard[cardIndex + i].originPRS.pos.x + 0.5f/i, CardUtils.CardAlignmentDelay);
        }
        canPush = false;
    }

    void PullCard()     // PushCard()보다 움직임 속도가 빨라야 함. 즉, dotweenTime 값은 더 작아야 함.
    {   
        canPush = true;
        foreach (Card card in HandCard)
        {
            card.transform.DOKill();
            card.MoveTransform(card.originPRS, true, 0.2f);
        }
        
    }
    public void CardMouseDown(Card card)
    {
        if (cardState != ECardState.CanMouseDrag)
            return;
        draggable = true;
        card.block = true;
    }

    public void CardMouseUp(Card card)
    {
        //draggable = false;
        //arrow.SetActive(false); => ArrowCursor(false);
        if (cardState != ECardState.CanMouseDrag)
        {
            //card.block = false;
            return;
        }
        draggable = false;
        ArrowCursor(false);
        selectCard = null;
        if (isUseCard && card.Data.cardTag != CardTag.SingleAttack)     // 단일타격을 제외한 나머지
        {
            UseCard(card);
        }
        else if (isUseCard /*&& card.Data.cardTag != CardTag.SingleAttack */&& useSingleTargetCard)     // 단일타격이 성공했을 경우
        {
            UseCard(card);
        }
        else        // 사용되지 않은 경우
        {
            PutDownCard(card).Forget();
        }

    }

    //async UniTask UseCard(Card card)
    //{
    //    //card.cardAction?.Invoke(card);
    //    if (GameManager.Instance.player.CurHolo < card.Data.cost)
    //    {
    //        PutDownCard(card).Forget();
    //        return;
    //    }
    //    GameManager.Instance.player.ChangeHoloValue(-card.Data.cost);
    //    await UsedCard(card);
    //    //await UsedCard(card);
    //    //card.block = false;
    //    //GameManager.Instance.blockClick = false;
    //}

    void UseCard(Card card)
    {
        //card.cardAction?.Invoke(card);
        if (GameManager.Instance.player.CurHolo < card.Data.cost)
        {
            PutDownCard(card).Forget();
            return;
        }
        GameManager.Instance.player.ChangeHoloValue(-card.Data.cost);
        UsedCard(card).Forget();
        //await UsedCard(card);
        //card.block = false;
        //GameManager.Instance.blockClick = false;
    }


    async UniTask PutDownCard(Card card)
    {
        //comeBackCard = true;
        card.GetComponent<Order>().SetMostFrontOrder(false);
        PullCard();
        await card.TaskMoveTransform(card.originPRS, true, CardUtils.CardAlignmentDelay);
        card.block = false;
        //draggable = false;
        //GameManager.Instance.blockClick = false;
    }

    public void CardDrag(Card card)
    {
        if (cardState != ECardState.CanMouseDrag || !draggable)
            return;

        DetectCardArea();

        if (isUseCard && card.Data.cardTag == CardTag.SingleAttack && !isSingleTarget)
        {
            ArrowCursor(true);
            PullCard();
            card.transform.DOKill();        // 마우스 커서가 카드를 나갈 때 카드 크기가 원래대로 돌아가는 코드를 멈춰주는 함수.
            card.transform.position = new Vector2(0, -3.32f);
            isSingleTarget = true;
        }
        else if (isUseCard && card.Data.cardTag != CardTag.SingleAttack)
        {
            Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            card.transform.position = tempPos;
        }
        else if (!isUseCard && isSingleTarget)
        {
            ArrowCursor(false);
            isSingleTarget = false;
        }
        else if (!isUseCard)
        {
            Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            card.transform.position = tempPos;
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        int layer = LayerMask.NameToLayer("UsedCardArea");
        isUseCard = Array.Exists(hits, x => x.collider.gameObject.layer == layer);

    }

    //void SetCardState()
    //{
    //    if (TurnManager.Instance.isLoading.Value)
    //        cardState = ECardState.Nothing;

    //    else if (!TurnManager.Instance.myTurn)
    //        cardState = ECardState.CanMouseOver;

    //    else if (TurnManager.Instance.myTurn)
    //        cardState = ECardState.CanMouseDrag;
    //}

    void ArrowCursor(bool isOn)
    {
        arrow.SetActive(isOn);
        Cursor.visible = !isOn;
    }
    #endregion
}
