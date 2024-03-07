
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    //public List<Card> Deck { get; private set; }

    public List<GameObject> MainDeck;   // 덱 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X)
    //public List<Card> MainCardDeck;     // 덱에 있는 카드 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X) (MainDeck과 같이 카드 추가)
    public List<GameObject> DrawDeck;   // 현재 내가 뽑을 수 있는 카드
    public List<GameObject> CardDummy;  // 카드 더미(사용 또는 버림)
    public List<GameObject> HandCard;   // 내 손에 있는 카드

    [SerializeField] CardSO cardSO;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardDummyTr;

    [SerializeField] Transform Deck;    // 소환된 덱 카드들

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState cardState;

    Card selectCard;
    bool draggable;
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    enum EAddDeck { 
        Main, Draw, Dummy, Hand,
        MainNDraw, MainNDummy, MainNHand, DrawNHand, DummyNHand,
        MainNDrawNDummy, MainNDrawNHand, DrawNDummyNHand,
        MainNDrawNDummyNHand
    }

    private void Awake() => Instance = this;

    public GameObject cardPrefab;

    private void Start()
    {
        SetupStartCardDeck();
        //StartBattle();    // 현재 Battle.cs에서 진행중
    }
    private void Update()
    {
        SetCardState();     // UniRx 이용해서 따로 처리할 것


        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AddDeck(cardSO.cards[0], EAddDeck.Main);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddDeck(cardSO.cards[0], EAddDeck.Draw);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddDeck(cardSO.cards[1], EAddDeck.Draw);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddDeck(cardSO.cards[0], EAddDeck.Dummy);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))       // 카드 찾아서 뽑기 (수정 필요해보임. 덱에서 인덱스로 GameObject를 지정해서 넣어줄거면 굳이 drawCard 함수에서 카드를 확인해 볼 필요가 없음.)
        {
            AddCard(Deck.GetComponentsInChildren<Card>()[2].gameObject);  // 전투덱에서 가져오는 경우
            //AddCard(DrawDeck[2]);   // 드로우덱에서 가져오는 경우
            //AddCard(CardDummy[0]);  // 버린 카드덱에 있는 카드가 드로우덱에도 있을 경우 => 적용 안됨. 주소 문제인 듯
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))       // 카드 생성
        {
            AddCard(cardSO.cards[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            EndBattle();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            StartBattle();
        }
    }

    void SetupStartCardDeck()   // 시작할 때, 메인덱을 설정하는 함수 (게임 시작 이후에는 사용하지 않음.)
    {
        for (int i = 0; i < cardSO.cards.Length; i++)
            AddDeck(cardSO.cards[i], EAddDeck.Main);
    }

    void AddDeck(CardData cardData, EAddDeck eAddDeck)     // 덱에 카드를 추가할 때 사용, 핸드로 카드를 가져올 때는 AddCard 함수 사용.
    {
        GameObject cardObject = PoolManager.instance.Pool.Get();
            /*Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, Deck);*/
        Card setCard = cardObject.GetComponent<Card>();

        cardObject.name = cardData.Name;    // 시각화 용도

        setCard.Setup(cardData);
        switch (eAddDeck)
        {
            case EAddDeck.Main:
                cardObject.transform.localScale = CardScale.cardScale * 0.5f;
                MainDeck.Add(cardObject);
                //MainCardDeck.Add(setCard);
                break;

            case EAddDeck.Draw:
                cardObject.transform.localScale = CardScale.cardScale * 0.5f;
                DrawDeck.Add(cardObject);
                ShuffleDeck();
                break;

            case EAddDeck.Dummy:
                cardObject.transform.localScale = CardScale.cardScale * 0.5f;
                CardDummy.Add(cardObject);
                break;

            case EAddDeck.Hand:                 // 핸드로 가져오는 건 카드 정렬 때문에 AddCard 함수를 이용해서만 접근할 것.
                if (HandCard.Count < 10)
                {
                    cardObject.transform.position = Vector3.zero;
                    HandCard.Add(cardObject);
                }
                else
                {
                    cardObject.transform.localScale = CardScale.cardScale * 0.5f;
                    CardDummy.Add(cardObject);
                }
                break;

        }
    }

    public void StartBattle()       // 배틀 시작시, 덱 섞기 및 액션 추가
    {
        SetupDrawDeck(true);
        TurnManager.OnAddCard += AddCard;
        TurnManager.Instance.StartTurnTask().Forget();
    }
    public void EndBattle()         // 리팩토링 필요해보임.
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.Instance.EndTurn();
        DrawDeck.Clear();
        CardDummy.Clear();
        //HandCard.Clear();
        for (int i = 0; i < Deck.childCount; i++)
        {
            if (!Deck.GetChild(i).gameObject.activeSelf)
                continue;
            Card card = Deck.GetComponentsInChildren<Card>(true)[i];
            //GameObject cardObject = Deck.GetChild(i).gameObject;
            if (MainDeck.Contains(card.gameObject))
                continue;
            card.Pool.Release(card.gameObject);
            //MainCardDeck.Add(card);
            //cardObject.GetComponent<Card>().CardRelease();

        }
        //foreach (Card card in MainCardDeck)
        //{
        //    card.Pool.Release(card.gameObject);
        //}
        //MainCardDeck.Clear();

        //foreach (Card card in MainCardDeck)
        //{
        //    AddDeck(card.Data, EAddDeck.Main);
        //}

    }
    void SetupDrawDeck(bool start = false)  // 드로우덱 섞기(start가 true일 경우, 메인덱에서 가져옴. false일 경우, 카드더미에서 가져옴.)
    {
        if (!start)
        {
            for (int i = 0; i < CardDummy.Count; i++)
            {
                DrawDeck.Add(CardDummy[i]);
            }
            CardDummy.Clear();
        }
        else
        {
            for (int i = 0; i < MainDeck.Count; i++)
            {
                DrawDeck.Add(MainDeck[i]);
            }
        }
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < DrawDeck.Count; i++)
        {
            int rand = Random.Range(0, DrawDeck.Count);
            GameObject temp = DrawDeck[i];
            DrawDeck[i] = DrawDeck[rand];
            DrawDeck[rand] = temp;
        }
    }
    
    public GameObject DrawCard()
    {
        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기
            SetupDrawDeck();

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        GameObject card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        return card;
    }

    GameObject DrawCard(GameObject drawCard)
    {
        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기
            SetupDrawDeck();

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        for (int i = 0; i < DrawDeck.Count; i++)
        {
            if (DrawDeck[i] == drawCard)
            {
                GameObject card = DrawDeck[i];
                DrawDeck.RemoveAt(i);
                return card;
            }
        }
        return null;
    }

    public void AddCard()   // 손패로 드로우할 카드
    {
        GameObject drawCard = DrawCard();
        if (drawCard == null)
            return;
        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        Card card = drawCard.GetComponent<Card>();
        card.Setup(card.Data);
        HandCard.Add(drawCard);

        SetOriginOrder();
        CardAlignment();
    }

    void AddCard(GameObject addCard)    // 덱에서 손패로 카드를 가져옴.
    {
        GameObject drawCard = DrawCard(addCard);
        if (drawCard == null)
            return;

        Card card = drawCard.GetComponent<Card>();
        card.Setup(card.Data);
        HandCard.Add(drawCard);

        SetOriginOrder();
        CardAlignment();

    }

    void AddCard(CardData addCard)  // 카드 생성
    {
        AddDeck(addCard, EAddDeck.Hand);

        SetOriginOrder();
        CardAlignment();
    }

    public async UniTaskVoid ThrowAwayCard()
    {
        foreach (GameObject targetCard in HandCard)
        {
            targetCard.GetComponent<Card>().MoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardScale.cardScale * 0.5f), true, 0.3f);

            CardDummy.Add(targetCard);
        }

        
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

        foreach (GameObject targetCard in HandCard)
        {
            targetCard.GetComponent<Card>().block = false;
            targetCard.GetComponent<Card>().transform.position = cardSpawnPoint.position;
        }
        HandCard.Clear();


        //foreach (Card dummyCard in CardDummy)
        //{
        //    dummyCard.block = false;
        //    dummyCard.Pool.Release(dummyCard.gameObject);
        //}
    }

    public async UniTask ThrowAwayCard(Card throwCard)
    {


        CardDummy.Add(throwCard.gameObject);

        HandCard.Remove(throwCard.gameObject);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardScale.cardScale * 0.5f), true, 0.3f);

        throwCard.block = false;
        throwCard.transform.position = cardSpawnPoint.position;
    }

    void SetOriginOrder()
    {
        for (int i = 0; i < HandCard.Count; i++)
        {
            GameObject targetCard = HandCard[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count, 0.5f, CardScale.cardScale);
        for (int i = 0; i < HandCard.Count; i++)
        {
            var targetCard = HandCard[i].GetComponent<Card>();

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.3f);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int cardCount, float height, Vector3 scale)
    {
        float[] cardLerps = new float[cardCount];
        List<PRS> results = new List<PRS>(cardCount);

        if (cardCount == 1)
        {
            cardLerps = new float[] { 0.5f };
        }
        else
        {
            float interval = cardCount < 7f ? 1f / 6 : 1f / cardCount;
            float cardPos = 0f;
            for (int i = 0; i < cardCount; i++)
            {
                if (i == 0)
                    cardPos += cardCount < 7f ? (interval * (3.5f - cardCount * 0.5f)) : interval * 0.5f;
                else
                    cardPos += interval;
                cardLerps[i] = cardPos;
            }
        }

        for (int i = 0; i < cardCount; i++)
        {
            Vector3 targetPos = Vector3.Lerp(leftTr.position, rightTr.position, cardLerps[i]);
            targetPos.z = -i * 5;

            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(cardLerps[i] - 0.5f, 2));   // 원의 방정식
            //float curve = Mathf.Sqrt(Mathf.Pow(height, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // 타원의 방정식

            targetPos.y += 2 * curve - 0.5f;
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
    }
    public void CardMouseExit(Card card)
    {
        if (cardState == ECardState.Nothing || draggable)
            return;
        selectCard = null;
        LargeCard(false, card);
    }


    void LargeCard(bool isLarge, Card card)
    {
        if (isLarge)
        {
            Vector3 largePos = new Vector3(card.originPRS.pos.x, -3.2f, -100f);
            card.MoveTransform(new PRS(largePos, Quaternion.identity, CardScale.cardScale * 1.2f), false);
        }
        else
            card.MoveTransform(card.originPRS, true, 0.3f);

        card.GetComponent<Order>().SetMostFrontOrder(isLarge);
    }

    public void CardMouseDown(Card card)
    {
        if (cardState != ECardState.CanMouseDrag)
            return;
        draggable = true;
        card.block = true;
    }

    public async UniTask CardMouseUp(Card card)
    {
        draggable = false;
        if (cardState != ECardState.CanMouseDrag)
        {
            card.block = false;
            return;
        }
        if (GameManager.Instance.throwAwayCard)
        {
            ThrowAwayCard(card).Forget();   //card.block 이 안에 있음.
            //GameManager.Instance.blockClick = false;
        }
        else
        {
            //comeBackCard = true;
            card.GetComponent<Order>().SetMostFrontOrder(false);
            await card.TaskMoveTransform(card.originPRS, true, 0.3f);
            card.block = false;
            //draggable = false;
            //GameManager.Instance.blockClick = false;
        }
    }

    public void CardDrag(Card card)
    {
        if (cardState != ECardState.CanMouseDrag || !draggable)
            return;
        Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        card.transform.position = tempPos;
    }

    void SetCardState()
    {
        if (TurnManager.Instance.isLoading)
            cardState = ECardState.Nothing;

        else if (!TurnManager.Instance.myTurn)
            cardState = ECardState.CanMouseOver;

        else if (TurnManager.Instance.myTurn)
            cardState = ECardState.CanMouseDrag;
    }

    #endregion
}
