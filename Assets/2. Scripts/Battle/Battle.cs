using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    private MyTurn myTurn;
    public Transform canvas;
    // Start is called before the first frame update
    private void Awake()
    {
        myTurn ??= new MyTurn();
    }
    void Start()
    {
        myTurn.StartFight();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print(myTurn.DrawCard().name);
            AddCard();
        }
    }
    void AddCard()
    {
        var cardObject = Instantiate(CardManager.Instance.cardPrefab, Vector3.zero, Quaternion.identity, canvas);
        var card = cardObject.GetComponent<Card>();
        card.Setup(myTurn.DrawCard().Data);

        SetOriginOrder();
    }

    void SetOriginOrder()
    {
        int count = myTurn.MyCard.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = myTurn.MyCard[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
}
