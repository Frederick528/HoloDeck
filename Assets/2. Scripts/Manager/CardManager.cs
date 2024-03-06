
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

    public List<Card> MainDeck;
    public List<Card> DrawDeck;  // ���� ���� ���� �� �ִ� ī��
    public List<Card> CardDummy;  // ī�� ����(��� �Ǵ� ����)
    public List<Card> HandCard; // �� �տ� �ִ� ī��

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
        for (int i = 0; i < cardSO.cards.Length; i++)
        {
            GameObject cardObject = PoolManager.instance.GetCard();
            Card card = cardObject.GetComponent<Card>();
            card.Setup(cardSO.cards[i]);
            PoolManager.instance.ReturnObjectToQueue(cardObject);
            MainDeck.Add(card);
        }
        StartBattle();
    }
    private void Update()
    {
        SetCardState();
        if (Input.GetKeyDown(KeyCode.K))
        {
            for (int i = 0; i < DrawDeck.Count; i++)
            {
                print(DrawDeck[i].Data.Name);
            }
        }
    }

    //void SetupStartCardDeck()   // ���߿� ������ ���ľ� ��.
    //{
    //    //MainDeck = new List<Card>();
    //    //CardData cardData;
    //    for (int i = 0; i < cardSO.cards.Length; i++)
    //    {
    //        Card startCard = new Card();
    //        //GameObject cardObject = PoolManager.instance.Pool.Get();
    //        //Card setCard = cardObject.GetComponent<Card>();
    //        //Card setCard = cardPrefab.GetComponent<Card>();
    //        //CardInfo cardInfo = cardSO.cards[i];

    //        //cardData.Name = cardInfo.name;
    //        //cardData.Cost = cardInfo.cost;
    //        //cardData.Descript = cardInfo.description;
    //        //cardData.Sprite = cardInfo.sprite;

    //        //startCard.Data.Name = cardInfo.name;
    //        //startCard.Data.Cost = cardInfo.cost;
    //        //startCard.Data.Descript = cardInfo.description;
    //        //startCard.Data.Sprite = cardInfo.sprite;
    //        //print(setCard.Data.Name);
    //        //cardDatas.Add(cardData);
    //        //MainDeck[i].Data = cardData;
    //        MainDeck.Add(startCard);
    //        //print(MainDeck[i].Data.Name);


    //        //card.Setup(cardSO.cards[i]);
    //        //card.Setup(card.Data);
    //        //MainDeck.Add(card);
    //    }
    //}

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
        if (DrawDeck.Count == 0)    // ���� ī�尡 ������ ������ ī�带 �ٽ� �ҷ�����, �� ����
            SetupCardDeck();

        if (DrawDeck.Count == 0)    // ���� ���� �Ŀ��� ���� ī�尡 ������ ����
            return null;

        Card card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        return card;

    }

    public Card DrawCard(CardData cardData)
    {
        if (DrawDeck.Count == 0)    // ���� ī�尡 ������ ������ ī�带 �ٽ� �ҷ�����, �� ����
            SetupCardDeck();

        if (DrawDeck.Count == 0)    // ���� ���� �Ŀ��� ���� ī�尡 ������ ����
            return null;

        for (int i = 0; i < DrawDeck.Count; i++)
        {
            if (DrawDeck[i].Data.Equals(cardData))
            {
                Card card = DrawDeck[i];
                DrawDeck.RemoveAt(i);
                return card;
            }
        }
        return null;
    }

    public void AddCard()
    {
        Card drawCard = DrawCard();

        if (drawCard == null)
            return;
        GameObject cardObject = PoolManager.instance.GetCard(/*drawCard.Data*/);
        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        Card card = cardObject.GetComponent<Card>();
        //if (string.IsNullOrEmpty(card.Data.Name))
        //{
        //    card.Data = drawCard.Data;
        //    card.Setup(drawCard.Data);
        //}
        //if (card.Data.Equals(drawCard.Data))

        //print(drawCard.Data.Name);
        //card.Setup(drawCard.Data);

        card.Setup(drawCard.Data);
        print(card.Data.Name);
        print(card.Data.Name);
        print(card.Data.Name);
        print(card.Data.Name);
        print(card.Data.Name);
        print(card.Data.Name);

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
            //targetCard.gameObject.SetActive(false);
            PoolManager.instance.ReturnObjectToQueue(targetCard.gameObject);
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
        PoolManager.instance.ReturnObjectToQueue(throwCard.gameObject);
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

            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(cardLerps[i] - 0.5f, 2));   // ���� ������
            //float curve = Mathf.Sqrt(Mathf.Pow(height, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // Ÿ���� ������

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
            ThrowAwayCard(card).Forget();   //card.block �� �ȿ� ����.
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
