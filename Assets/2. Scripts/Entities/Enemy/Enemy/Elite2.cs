using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite2 : Enemy
{
    int turn;

    void Start()
    {
        BaseThickness = 0.005f;
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
                SetRepeat(2);
                AttackPattern(_defaultEnemyData.Damage);
                break;
            case 2:
                SetRepeat();
                AttackPattern(_defaultEnemyData.Damage);
                turn = 0;
                break;
        }
    }
}
