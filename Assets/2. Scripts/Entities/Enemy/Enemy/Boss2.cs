using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : Enemy
{
    int turn;

    void Start()
    {
        //_nextActImg.transform.localPosition = new Vector3(0, 2.5f);
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                AttackPattern(enemyData.Damage);
                break;
            case 2:
                AttackPattern(enemyData.Damage);
                turn = 0;
                break;
        }
    }
}
