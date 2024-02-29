using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardGroup : MonoBehaviour
{
    private List<Card> _cards = new();
    public int Count => _cards.Count;
    public List<Card> Cards => _cards;

    public void AddCard(Card card)
    {
        // if (Cards.Contains(card))
        //     return;
        _cards.Add(card);
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
    }
    public void InsertCard(Card card)
    {
        _cards.Insert(0, card);
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
    }
    public void AddCardRange(IEnumerable<Card> cards)
    {
        _cards.AddRange(cards);
        foreach (var card in cards)
        {
            card.transform.SetParent(this.transform);
            card.transform.localPosition = Vector3.zero;
        }
    }
    public Card RemoveCard(Card card)
    {
        if (card.Equals(null)) return null;
        _cards.Remove(card);
        card.transform.SetParent(CardManager.Instance.transform, true);

        if (_cards.Count <= 1)
        {
            if (_cards.Count != 0)
            {
                _cards[0].transform.SetParent(CardManager.Instance.transform, true);
                Thread.MemoryBarrier();
            }
            Destroy(this.gameObject);
        }

        return card;
    }
    public Card RemoveCard(Card card, bool autoDestroyCardGroup)
    {
        if (autoDestroyCardGroup) return RemoveCard(card);
        _cards.Remove(card);
        card.transform.SetParent(CardManager.Instance.transform, true);

        return card;
    }

    public Card RemoveCard(int index)
    {
        try
        {
            if (Count > 0)
                return RemoveCard(_cards[index]);
        }
        catch (Exception e)
        {
            Debug.Log(index);
            Debug.Log(e);
        }

        return null;
    }

    public bool Contains(Card card)
    {
        return _cards.Contains(card);
    }

    public int IndexOf(Card card)
    {
        return _cards.IndexOf(card);
    }

    public bool IsLastElement(Card card)
    {
        return card.Equals(_cards[^1]);
    }

}
