using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CardAbility
{

    private void SettingCondition(Card card, List<Func<UniTask<bool>>> conditionTasks, ref Action immediate, ref Action failure)
    {

        if ((card.Data.Discard != 0 || (card.Data.Discard == 0 && card.Data.DiscardOrder >= 0)) || (card.Data.Remove != 0 || (card.Data.Remove == 0 && card.Data.RemoveOrder >= 0)))
        {
            immediate += () => CardManager.Instance.SetCardState(1);
            failure += () => CardManager.Instance.SetCardState(2);

            if (card.Data.Discard != 0 && card.Data.DiscardOrder < 0) conditionTasks.Add(() => ConditionDiscardAB(card));
            if (card.Data.Remove != 0 && card.Data.RemoveOrder < 0) conditionTasks.Add(() => ConditionRemoveAB(card));
        }
        //if (card.Data.HP < 0 && card.Data.HPOrder < 0) conditionTasks.Add(() => ConditionHpCheck(card));
    }
    private void BuildBaseAbilities(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks)
    {
        // 엑셀에 있는 HP는 조건에서 확인하는 코드로 변경.
        // 회복 같은 경우에는 사용 효과이기에 스페셜에서 따로 적용
        // 1단계: 회복 (공격 전 생존 확보)

        //if (card.Data.HPOrder >= 0)
        //{
        //    tasks.Add((card.Data.HPOrder, AbilityTag.ChangeHP, () => HpEffectAB(card)));
        //}

        //// 2단계: 자해 (공격의 대가 지불)
        //if (card.Data.HP < 0)
        //{
        //    tasks.Add((2, AbilityTag.SelfDamage, () => HpEffectAB(card)));
        //}

        // 3단계: 메인 공격
        if (card.Data.DamageOrder >= 0)
        {
            if (card.Data.CardTag == CardTag.SingleAttack)
                tasks.Add((card.Data.DamageOrder, AbilityTag.Attack, () => SingleAttackAB(card, GetCrit())));
            else if (card.Data.CardTag == CardTag.AllAttack)
                tasks.Add((card.Data.DamageOrder, AbilityTag.Attack, () => AllAttackAB(card, GetCrit())));
            else if (card.Data.CardTag == CardTag.RandomAttack)
                tasks.Add((card.Data.DamageOrder, AbilityTag.Attack, () => RandomAttackAB(card, GetCrit())));
        }

        // 4단계: 방어 (공격 후 실드 형성)
        if (card.Data.ShieldOrder >= 0)
        {
            tasks.Add((card.Data.ShieldOrder, AbilityTag.Shield, () => ShieldAB(card)));
        }

        // 5단계: 드로우 (유틸리티)
        if (card.Data.DrawOrder >= 0)
        {
            tasks.Add((card.Data.DrawOrder, AbilityTag.Draw, () => DrawAB(card)));
        }

        if (card.Data.HPOrder >= 0)
        {
            tasks.Add((card.Data.HPOrder, AbilityTag.ChangeHP, () => HpEffectAB(card)));
        }

        if (card.Data.DiscardOrder >= 0)
        {
            tasks.Add((card.Data.DiscardOrder, AbilityTag.PostEffect, () => ConfirmedDiscardAB(card)));
        }

        if (card.Data.RemoveOrder >= 0)
        {
            tasks.Add((card.Data.RemoveOrder, AbilityTag.PostEffect, () => ConfirmedRemoveAB(card)));
        }
    }

    private bool GetCrit() => _player.GetStatusEffect(StatusEffect.UseCritical, out _);
}
