using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : Enemy
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
                base.Heal(enemyData.damage);
                break;
            case 2:
                base.Heal(enemyData.damage);
                turn = 0;
                break;
        }
    }
}
