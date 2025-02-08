using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    //private MyTurn MyTurn;

    // Start is called before the first frame update
    //private void Awake()
    //{
    //    MyTurn ??= new MyTurn();
    //}
    void Start()
    {
        TurnManager.Instance.StartBattle();
    }

    // Update is called once per frame


}
