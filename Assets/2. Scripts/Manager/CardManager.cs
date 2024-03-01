
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    //public List<Card> Deck { get; private set; }
    public List<Card> MainDeck;
    public List<Card> DrawDeck;  // 현재 내가 뽑을 수 있는 카드
    public List<Card> CardDummy;  // 카드 더미(사용 또는 버림)
    public List<Card> HandCard; // 내 손에 있는 카드

    [SerializeField] Transform cardSpawnPoint;

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;

    private void Awake() => Instance = this;

    public GameObject cardPrefab;

    public void StartBattle()
    {
        SetupCardDeck(true);
    }
    void SetupCardDeck(bool start = false)
    {
        if (!start)
        {
            for (int i = 0; i < CardDummy.Count; i++)
            {
                DrawDeck.Add(CardDummy[i]);
            }
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
        var drawCard = DrawCard();
        if (drawCard == null)
            return;
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        var card = cardObject.GetComponent<Card>();
        card.Setup(drawCard.Data);
        HandCard.Add(card);

        SetOriginOrder();
        CardAlignment();
    }

    void SetOriginOrder()
    {
        for (int i = 0; i < HandCard.Count; i++)
        {
            var targetCard = HandCard[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count, 0.5f, new Vector3(2f, 2.8f, 1f));
        for (int i = 0; i < HandCard.Count; i++)
        {
            var targetCard = HandCard[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
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
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, cardLerps[i]);

            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(cardLerps[i] - 0.5f, 2));   // 원의 방정식
            //float curve = Mathf.Sqrt(Mathf.Pow(height, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // 타원의 방정식

            targetPos.y += 2 * curve;
            Quaternion targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, cardLerps[i]);

            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }
}
