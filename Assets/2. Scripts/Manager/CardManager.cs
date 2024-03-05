
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

    public List<CardData> cardDatas;
    public List<Card> MainDeck;
    public List<Card> DrawDeck;  // 현재 내가 뽑을 수 있는 카드
    public List<Card> CardDummy;  // 카드 더미(사용 또는 버림)
    public List<Card> HandCard; // 내 손에 있는 카드

    [SerializeField] CardSO cardSO;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardDummyTr;

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] CardState cardState;

    Card selectCard;
    bool draggable;
    enum CardState { Nothing, CanMouseOver, CanMouseDrag }

    private void Awake() => Instance = this;

    public GameObject cardPrefab;

    private void Start()
    {
        SetupStartCardDeck();
    }
    private void Update()
    {
        SetCardState();
    }

    void SetupStartCardDeck()   // 나중에 무조건 고쳐야 함.
    {
        //MainDeck = new List<Card>();
        CardData cardData;
        for (int i = 0; i < cardSO.cards.Length; i++)
        {
            //Card setCard = cardPrefab.GetComponent<Card>();
            CardInfo cardInfo = cardSO.cards[i];
            cardData.Name = cardInfo.name;
            cardData.Cost = cardInfo.cost;
            cardData.Descript = cardInfo.description;
            cardData.Sprite = cardInfo.sprite;
            //setCard.Data.Name = cardInfo.name;
            //setCard.Data.Cost = cardInfo.cost;
            //setCard.Data.Descript = cardInfo.description;
            //setCard.Data.Sprite = cardInfo.sprite;
            //print(setCard.Data.Name);
            cardDatas.Add(cardData);
            MainDeck[i].Data = cardData;
            //print(MainDeck[i].Data.Name);
        }
    }

    public void StartBattle()
    {
        SetupCardDeck(true);
        TurnManager.OnAddCard += AddCard;
    }
    public void EndBattle()
    {
        TurnManager.OnAddCard -= AddCard;
        DrawDeck.Clear();
        CardDummy.Clear();
        HandCard.Clear();
    }
    void SetupCardDeck(bool start = false)
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
        for (int i = 0; i < DrawDeck.Count; i++)
        {
            int rand = Random.Range(0, DrawDeck.Count);
            Card temp = DrawDeck[i];
            DrawDeck[i] = DrawDeck[rand];
            DrawDeck[rand] = temp;
        }
    }
    public Card DrawCard()
    {
        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기
            SetupCardDeck();

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        Card card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        return card;

    }

    public void AddCard()
    {
        Card drawCard = DrawCard();
        if (drawCard == null)
            return;
        GameObject cardObject = PoolManager.instance.Pool.Get();
        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        Card card = cardObject.GetComponent<Card>();
        card.Setup(drawCard.Data);
        HandCard.Add(card);

        SetOriginOrder();
        CardAlignment();
    }

    public async UniTaskVoid ThrowAwayCard()
    {
        foreach (Card targetCard in HandCard)
        {
            targetCard.MoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardScale.cardScale * 0.5f), true, 0.3f);

            CardDummy.Add(targetCard);
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

        foreach (Card targetCard in HandCard)
        {
            targetCard.block = false;
            targetCard.Pool.Release(targetCard.gameObject);
            targetCard.transform.position = cardSpawnPoint.position;
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


        CardDummy.Add(throwCard);

        HandCard.Remove(throwCard);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardScale.cardScale * 0.5f), true, 0.3f);

        throwCard.block = false;
        throwCard.Pool.Release(throwCard.gameObject);
        throwCard.transform.position = cardSpawnPoint.position;
    }

    void SetOriginOrder()
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
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count, 0.5f, CardScale.cardScale);
        for (int i = 0; i < HandCard.Count; i++)
        {
            var targetCard = HandCard[i];

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
        if (cardState == CardState.Nothing || draggable)
            return;
        selectCard = card;
        LargeCard(true, card);
    }
    public void CardMouseExit(Card card)
    {
        if (cardState == CardState.Nothing || draggable)
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
        if (cardState != CardState.CanMouseDrag)
            return;
        draggable = true;
        card.block = true;
    }

    public async UniTask CardMouseUp(Card card)
    {
        draggable = false;
        if (cardState != CardState.CanMouseDrag)
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
        if (cardState != CardState.CanMouseDrag || !draggable)
            return;
        Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        card.transform.position = tempPos;
    }

    void SetCardState()
    {
        if (TurnManager.Instance.isLoading)
            cardState = CardState.Nothing;

        else if (!TurnManager.Instance.myTurn)
            cardState = CardState.CanMouseOver;

        else if (TurnManager.Instance.myTurn)
            cardState = CardState.CanMouseDrag;
    }

    #endregion
}
