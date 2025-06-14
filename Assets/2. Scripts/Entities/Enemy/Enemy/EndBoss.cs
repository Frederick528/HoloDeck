using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBoss : Enemy
{
    int turn;

    void Start()
    {
        AddStatusEffect((StatusEffect.Vampire, StatusEffectType.Perpetual), 5);
        AddStatusEffect((StatusEffect.Immunity, StatusEffectType.UseAmountInfiniteDuration), 3);
        AddStatusEffect((StatusEffect.Berserker, StatusEffectType.Perpetual), 2);
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
                SpecialPattern(enemyData.Damage);
                //_nextPattern = () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage / 2);
                //});
                turn = 0;
                break;
        }
    }

    protected override void SpecialPattern(int value, int repeat = 1, float delay = 0.3F)
    {
        _specialDesc = "해당 적은 {n}만큼의 공격과 회복과 쉴드를 준비 중입니다.";
        _nextPattern = () => UniTask.Create(async () =>
        {
            await Attack(value);
            await Heal(value);
            await Shield(value);
        });
        base.SpecialPattern(value, repeat, delay);
    }
}