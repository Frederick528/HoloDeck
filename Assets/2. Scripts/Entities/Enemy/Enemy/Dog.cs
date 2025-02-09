using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Enemy
{
    int turn;

    void Start()
    {
        EntitySubScribe();
    }


    public override void Pattern()
    {
        turn++;
        switch (turn)
        {
            case 1:
                InGameManager.Instance.player.TakeDamagePlayer(enemyData.damage).Forget();
                break;
            case 2:
                InGameManager.Instance.player.TakeDamagePlayer((int)(enemyData.damage/2)).Forget();
                turn = 0;
                break;
        }
    }
}
