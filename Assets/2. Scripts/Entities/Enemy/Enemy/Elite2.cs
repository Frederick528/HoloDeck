using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite2 : Enemy
{
    int turn;

    void Start()
    {
        AddStatusEffect((StatusEffect.Thievery, StatusEffectType.Perpetual), 15);
        //_nextActImg.transform.localPosition = new Vector3(0, 2.2f);
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                AttackPattern(_defaultEnemyData.Damage * 2);
                break;
            case 2:
                AttackPattern(_defaultEnemyData.Damage);
                turn = 0;
                break;
        }
    }
}
