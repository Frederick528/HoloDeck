using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    int turn;

    void Start()
    {
        EnemySubScribe();
        _nextActImg.transform.localPosition = new Vector3(0, 1.8f);
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
    }


    public override async UniTask Pattern()
    {
        turn++;
        switch (turn)
        {
            case 1:
                await Attack(enemyData.Damage);
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
                break;
            case 2:
                await Heal(enemyData.Damage);
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                turn = 0;
                break;
        }
    }
}
