using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
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
                GameManager.Instance.player.TakeDamagePlayer(enemyData.damage).Forget();
                break;
            case 2:
                base.Heal(enemyData.damage);
                turn = 0;
                break;
        }
    }
}
