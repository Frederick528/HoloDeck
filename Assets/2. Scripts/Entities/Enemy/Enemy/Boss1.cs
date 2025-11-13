using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1 : Enemy
{
    int turn;

    void Start()
    {
        BaseThickness = 2f;
        //_nextActImg.transform.localPosition = new Vector3(0, 2.5f);
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                AttackPattern(_defaultEnemyData.Damage);
                break;
            case 2:     // 중복 패턴일 경우, 가장 아래에 있는 함수가 가장 먼저 실행됨.
                HealPattern(_defaultEnemyData.Damage);
                DefensePattern(_defaultEnemyData.Damage, true);
                AttackPattern(_defaultEnemyData.Damage, true);
                //SpecialPattern(enemyData.Damage);
                //_nextPattern = () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage / 2);
                //});
                turn = 0;
                break;
        }
    }

    //protected override void SpecialPattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3F)
    //{
    //    _specialDesc = "해당 적은 {n}만큼의 공격과 회복과 쉴드를 준비 중입니다.";
    //    Func<UniTask> func = () => UniTask.Create(async () =>
    //    {
    //        await Attack(value);
    //        await Heal(value);
    //        await Shield(value);
    //    });

    //    if (addPattern)
    //    {
    //        _nextActText.text = "";
    //        _nextPattern += func;
    //    }
    //    else
    //    {
    //        _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
    //        _nextPattern = func;
    //    }

    //    //AddStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information), _criticalChance.Value * repeat);
    //    base.SpecialPattern(value, repeat, addPattern, delay);
    //}
}
