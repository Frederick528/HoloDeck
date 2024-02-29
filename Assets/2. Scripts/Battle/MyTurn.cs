using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTurn
{
    private int _turnCnt = 0;
    private bool _isTurnProceeding = false;
    public int TurnCnt => _turnCnt;
    public bool IsTurnProceeding => _isTurnProceeding;
    public List<Card> MyDeck { get; private set; }  // 현재 내가 뽑을 수 있는 카드
    public List<Card> BattleDeck { get; private set; }  // 카드매니저 덱 + 배틀 중 추가된 카드
    public List<Card> MyCard { get; private set; } // 내 손에 있는 카드
    public void StartFight()
    {
        //When the fight monsters, call this method.
        BattleDeck = CardManager.Instance.Deck;
        SetupCardDeck();
        _turnCnt = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        //When the start my turn, call this method.
        _turnCnt++;
        _isTurnProceeding = true;
        //curAp = maxAp
        //drawCard
    }

    public void EndTurn()
    {
        //When the click end turn, call this method.
        _isTurnProceeding = false;
        //throwCard
    }

    public void EndFight()
    {
        //When the clear room, call this method.
        _turnCnt = 0;
        _isTurnProceeding = false;
        //reward
    }
    void SetupCardDeck()
    {
        MyDeck = new List<Card>();
        for (int i = 0; i < BattleDeck.Count; i++)
        {
            MyDeck.Add(BattleDeck[i]);
        }
        for (int i = 0; i < MyDeck.Count; i++)
        {
            int rand = Random.Range(0, MyDeck.Count);
            Card temp = MyDeck[i];
            MyDeck[i] = MyDeck[rand];
            MyDeck[rand] = temp;
        }
    }

    public Card DrawCard()
    {
        // Deck에서 카드를 뽑습니다.
        if (MyDeck.Count == 0)
            SetupCardDeck();
        
        Card card = MyDeck[0];
        MyDeck.RemoveAt(0);
        return card;

    }
}
