using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : Enemy
{
    int turn;
    int _resistance = 1;

    void Start()
    {
        EntitySubScribe();
    }

    protected override async UniTask BeforeTakeDamage()
    {
        await base.Shield(_resistance);
        await base.BeforeTakeDamage();
    }

    protected override async UniTask AfterTakeDamage()
    {
        InGameManager.Instance.player.TakeDamagePlayer(enemyData.damage).Forget();
        await base.AfterTakeDamage();
    }

    //protected override int ResistDamage(int damage)
    //{
    //    int resistDamage;
    //    if (damage > _resistance)
    //    {
    //        resistDamage = damage - _resistance;      // BeforeTakeDamage로 얻을 shield 양만큼 빼서 계산.
    //        print(resistDamage);
    //    }
    //    else
    //    {
    //        resistDamage = 0;
    //    }
    //    return resistDamage;
    //}

    public override void CheckIfDead(int damage, int count)
    {
        int resistDamage;
        if (damage > _resistance)
        {
            resistDamage = damage - _resistance;      // BeforeTakeDamage로 얻을 shield 양만큼 빼서 계산.
        }
        else
        {
            resistDamage = 0;
        }
        base.CheckIfDead(resistDamage, count);
    }


    public override async UniTask Pattern()
    {
        turn++;
        switch (turn)
        {
            case 1:
                await base.Heal(enemyData.damage);
                break;
            case 2:
                await base.Heal(enemyData.damage);
                turn = 0;
                break;
        }
        //await base.Pattern();
    }
}
