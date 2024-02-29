
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    //private static GameObject _ogCard;

    public static CardManager Instance { get; private set; }
    //public List<Card> Deck { get; private set; }
    public List<Card> Deck;

    //public GameObject newerCardEffect;

    // Start is called before the first frame update
    private void Awake() => Instance = this;

    public GameObject cardPrefab;


    //public static bool TryGetCardsByID(int id, out Card[] cards)
    //{
    //    var result = true;

    //    var arr = Cards.Where(x => x.ID == id).Select(x => x);

    //    cards = arr.ToArray();

    //    return result;
    //}

    //public static bool TryGetCardsByLevel(int level, out Card[] cards)
    //{
    //    var result = true;

    //    var arr = Cards.Where(x => x.level == level).Select(x => x);

    //    cards = arr.ToArray();

    //    return result;
    //}

    //public static bool TryGetCardsByType(Card.CardType cardType, out Card[] cards)
    //{
    //    var result = true;

    //    var arr = Cards.Where(x => x.cardType == cardType).Select(x => x);

    //    cards = arr.ToArray();

    //    return result;
    //}

    //public static bool TryGetCards(out Card[] cards)
    //{
    //    var result = true;

    //    var arr = Cards.Where(x => x.ID < 5000).Select(x => x);

    //    cards = arr.ToArray();

    //    return result;
    //}

    //private void OnDestroy()
    //{
    //    Instance = null;
    //}
}
