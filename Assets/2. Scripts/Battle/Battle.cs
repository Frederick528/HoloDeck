using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    //private MyTurn myTurn;

    // Start is called before the first frame update
    //private void Awake()
    //{
    //    myTurn ??= new MyTurn();
    //}
    void Start()
    {
        CardManager.Instance.StartBattle();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CardManager.Instance.AddCard();
        }
    }
    
}
