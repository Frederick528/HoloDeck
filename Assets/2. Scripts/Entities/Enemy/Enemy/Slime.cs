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


    public override async UniTask Pattern()
    {
        turn++;
        switch (turn)
        {
            case 1:
                await Attack(enemyData.Damage);
                break;
            case 2:
                await base.Heal(enemyData.Damage);
                turn = 0;
                break;
        }
    }
}
