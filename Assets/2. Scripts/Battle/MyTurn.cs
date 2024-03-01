using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTurn
{
    public int TurnCnt { get; private set; }
    public bool IsTurnProceeding { get; private set; }
    //public List<Card> MyDeck { get; private set; }  // 현재 내가 뽑을 수 있는 카드
    //public List<Card> BattleDeck { get; private set; }  // 카드매니저 덱 + 배틀 중 추가된 카드
    //public void StartFight()
    //{
    //    //When the fight monsters, call this method.
    //    BattleDeck = CardManager.Instance.MainDeck;
    //    MyDeck = new List<Card>();
    //    SetupCardDeck();
    //    TurnCnt = 0;
    //    StartTurn();
    //}

    //public void StartTurn()
    //{
    //    //When the start my turn, call this method.
    //    TurnCnt++;
    //    IsTurnProceeding = true;
    //    //curAp = maxAp
    //    //drawCard
    //}

    //public void EndTurn()
    //{
    //    //When the click end turn, call this method.
    //    IsTurnProceeding = false;
    //    //throwCard
    //}

    //public void EndFight()
    //{
    //    //When the clear room, call this method.
    //    TurnCnt = 0;
    //    IsTurnProceeding = false;
    //    //reward
    //}
    //void SetupCardDeck()
    //{
    //    for (int i = 0; i < BattleDeck.Count; i++)
    //    {
    //        MyDeck.Add(BattleDeck[i]);
    //    }
    //    for (int i = 0; i < MyDeck.Count; i++)
    //    {
    //        int rand = Random.Range(0, MyDeck.Count);
    //        Card temp = MyDeck[i];
    //        MyDeck[i] = MyDeck[rand];
    //        MyDeck[rand] = temp;
    //    }
    //}

    //public Card DrawCard()
    //{
    //    // Deck에서 카드를 뽑습니다.
    //    if (MyDeck.Count == 0)
    //        SetupCardDeck();
        
    //    Card card = MyDeck[0];
    //    MyDeck.RemoveAt(0);
    //    return card;

    //}
}
