using Cysharp.Threading.Tasks;
using System;
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
                AttackPattern(_defaultEnemyData.Damage);
                break;
            case 2:
                //DefensePattern(enemyData.Damage, 1, true);
                //HealPattern(enemyData.Damage, 1, true);
                AttackPattern(_defaultEnemyData.Damage);
                SpecialPattern(1,1,true);
                //_nextPattern = () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage / 2);
                //});
                turn = 0;
                break;
        }
    }

    protected override void SpecialPattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3F)
    {
        _specialDesc = "해당 적은 당신의 상태 효과 중 1개를 랜덤하게 제거 할 예정입니다";

        //if (addPattern)
        //{
        //    _nextActText.text = "";
        //    //_nextPattern += func;
        //}
        //else
        //{
        //    _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        //    //_nextPattern = func;
        //}
        _nextPattern.Add(async () => await UniTask.Create(async () =>
        {
            await UniTask.WaitForSeconds(delay/*, cancellationToken: TurnManager.Instance.CancelSource.Token*/);
            if (this.CurHP.Value > 0)
                player.RemoveStatusEffect(player.GetRandomStatusEffect());
        }));
        //_nextPattern = () => UniTask.Create(async () =>
        //{
        //    await UniTask.CompletedTask;
        //    player.RemoveStatusEffect(player.GetRandomStatusEffect());
        //    //await Attack(value);
           
        //});
        //AddStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information), _criticalChance.Value * repeat);
        base.SpecialPattern(value, repeat, addPattern, delay);
    }
}