using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;

public partial class CardAbility
{
    private void InitSpecialAbilities()
    {
        _specialAbilityMap = new()
        {
            // args[0]: Type, args[1]: Amount, args[2]: Duration
            { "ATKUp", (card, tasks, data) => {
                var type = GetEnum<StatusEffectType>(data.Type); // 헬퍼 사용
                //int amount = GetVariable(card, data.Amount);
                //int duration = GetVariable(card, data.Duration);

                AddAttackUP(card, tasks, type, data.Amount, data.Duration);
            }},

            { "Vulnerable", (card, tasks, data) => {
                var type = GetEnum<StatusEffectType>(data.Type); // 헬퍼 사용
                //int amount = GetVariable(card, data.Amount);
                //int duration = GetVariable(card, data.Duration);

                AddVulnerable(card, tasks, type, data.Amount, data.Duration);
            }},

            //{ "XValue", (card, tasks, data) => {
            //    // data.Type에 "Count", "Damage" 등이 들어있으므로 그대로 전달
            //    if (!string.IsNullOrEmpty(data.Type))
            //        AddXValue(card, tasks, data.Type);
            //}}
        };
        //_specialAbilityMap = new()
        //{
        //    { "ATKUp", (card, tasks, args) => AddAttackUP(card, tasks, GetAmount(card, args)) },
        //    { "DEFUp", (card, tasks, args) => AddDefenseUP(card, tasks, GetAmount(card, args)) },

        //    // 인자가 문자열(Target 등)인 경우는 그대로 사용
        //    { "XValue", (card, tasks, args) => {
        //        if (args.Length > 0) AddXValue(card, tasks, args[0]);
        //    }},
        //    //{ "VulnerableAB", VulnerableAB}
        //    //{ "Burn", (card, tasks) => AddBurnEffect(card, tasks, 3) } // 인자가 필요하면 람다로 래핑
        //};
    }

    private int GetVariable(Card card, string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        string val = input.Trim().ToLower();
        if (val == "x") return card.Data.Cost;
        if (int.TryParse(val, out int result)) return result;

        return 0;
    }


    //private int GetAmount(Card card, string[] args)
    //{
    //    // 1. 인자가 아예 없는 경우 기본값 반환
    //    if (args == null || args.Length == 0) return 0;

    //    string input = args[0].Trim().ToLower();

    //    // 2. "x"인 경우 카드의 코스트 반환
    //    if (input == "x") return card.Data.Cost;

    //    // 3. 숫자로 변환 가능한 경우 해당 숫자 반환
    //    if (int.TryParse(input, out int result)) return result;

    //    // 4. 변환 실패 시 기본값 반환
    //    return 0;
    //}

    private void SettingImmediately(Card card, ref Action immediate, ref Action failure)
    {
        switch (card.Data.ID)
        {
            //case 105:
            //    immediate += () => CardManager.Instance.SetCardState(1);
            //    failure += () => CardManager.Instance.SetCardState(2);
            //    break;
        }
    }

    private void SettingSpecialCondition(Card card, List<Func<UniTask<bool>>> conditionTasks, ref Action immediate, ref Action failure)
    {
        switch (card.Data.ID)
        {
            //case 503:
            //    //conditionTasks.Insert(0, () => UniTask.FromResult(card.Data.Cost >= 3));
            //    conditionTasks.Add(() => ConditionHpCheckXValue(card));     // 체력을 깎는 조건은 맨 마지막에 확인해야 문제 없음.
            //    break;
            case 999: // 예: 내 손패가 3장 이하일 때만 사용 가능
                conditionTasks.Insert(0, () => UniTask.FromResult(CardManager.Instance.HandCard.Count <= 3));
                break;
                // 필요 없으면 비워두면 됨 (Simple에서 추가한 기본 조건만 작동)
        }
    }

    private void BuildSpecialAbilities(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks)
    {
        if (card.Data.SpecialTags == null || card.Data.SpecialTags.Count == 0) return;

        foreach (var tagData in card.Data.SpecialTags)
        {
            if (tagData.Tag == "XValue") continue;
            // 3. 맵에서 해당 태그가 있는지 확인
            if (_specialAbilityMap.TryGetValue(tagData.Tag, out var addAction))
            {

                addAction(card, tasks, tagData);
            }
        }
        //switch (card.Data.ID)
        //{
        //    case 105: AddConfirmedDiscard(card, tasks); break;
        //    case 108: AddEnemyStatusDamage(card, tasks); break;
        //    case 503: AddAttackUP(card, tasks); break;
        //    //case 801: AddXValue(card, tasks); break;

        //    case 110: VulnerableAB(card, tasks); break;
        //}
    }

    

    // 105: 확정 버리기 (6번) (조건이 아닌 카드 효과라서 여기서 적용됨.)
    //void AddConfirmedDiscard(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks)
    //{
    //    tasks.Add((6, AbilityTag.PostEffect, () => ConfirmedDiscardAB(card)));
    //}

    // 108: 상태이상 비례 데미지 (스마트 버전)
    void AddEnemyStatusDamage(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks)
    {
        if (card.Data.Damage > 0)
        {
            // Case A: 기존 데미지가 있는 경우 (0번에서 더하고 6번에서 뺌)
            int bonus = 0;
            tasks.Add((0, AbilityTag.PrePreparation, () => {
                bonus = card.TargetEnemy.CurStatusEffectList.Count + card.TargetEnemy.CurStatusEffectPerpetualList.Count;
                card.Data.Damage += bonus;
                return UniTask.CompletedTask;
            }
            ));
            tasks.Add((6, AbilityTag.PostEffect, () => {
                card.Data.Damage -= bonus;
                return UniTask.CompletedTask;
            }
            ));
        }
        else
        {
            // Case B: 데미지가 없는 경우 (3번에서 직접 계산해서 공격)
            tasks.Add((3, AbilityTag.Attack, async () => {
                int count = card.TargetEnemy.CurStatusEffectList.Count + card.TargetEnemy.CurStatusEffectPerpetualList.Count;
                card.Data.Damage = count;
                await SingleAttackAB(card, GetCrit());
                card.Data.Damage = 0;
            }
            ));
        }
    }

    // 503: 공격력 버프 (0번)
    void AddAttackUP(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks, StatusEffectType type, string amountText, string durationText)
    {
        tasks.Add((0, AbilityTag.PrePreparation, () => {

            int amount = GetVariable(card, amountText);
            int duration = GetVariable(card, durationText);

            _player.AddStatusEffect((StatusEffect.ATKUp, type), amount, duration);
            return UniTask.CompletedTask;
        }));
    }

    void AddDefenseUP(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks, StatusEffectType type, string amountText, string durationText)
    {
        tasks.Add((0, AbilityTag.PrePreparation, () => {
            int amount = GetVariable(card, amountText);
            int duration = GetVariable(card, durationText);

            _player.AddStatusEffect((StatusEffect.DEFUp, StatusEffectType.InfiniteDuration), amount);
            return UniTask.CompletedTask;
        }
        ));
    }

    // [장착] 카드를 내기 직전, 모든 XValue 태그를 찾아 수치에 더함
    
    // 801: 횟수 세팅 (0번)
    //private void AddXValue(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks, string target)
    //{
    //    int bonusValue = 0; // 원복을 위해 저장해둘 변수

    //    // 1. 사전 준비(0번): X값을 타겟에 적용
    //    tasks.Add((0, AbilityTag.PrePreparation, () => {
    //        int xValue = card.Data.Cost;
    //        bonusValue = xValue;

    //        switch (target)
    //        {
    //            case "Count":
    //                card.Data.Count += xValue;
    //                break;
    //            case "Damage":
    //                card.Data.Damage += xValue;
    //                break;
    //            case "Shield":
    //                card.Data.Shield += xValue;
    //                break;
    //            case "Draw":
    //                card.Data.Draw += xValue;
    //                break;
    //            case "Discard":
    //                card.Data.Discard += xValue;
    //                break;
    //            case "Remove":
    //                card.Data.Remove += xValue;
    //                break;
    //            case "HealHP":
    //                card.Data.HP += xValue;
    //                break;
    //            case "DamageHP":
    //                card.Data.HP -= xValue;
    //                break;
    //        }
    //        return UniTask.CompletedTask;
    //    }
    //    ));

    //    // 2. 사후 처리(6번): 데미지나 횟수처럼 '이번 장'에만 해당되는 값은 원복
    //    tasks.Add((6, AbilityTag.PostEffect, () => {
    //        switch (target)
    //        {
    //            case "Count":
    //                card.Data.Count -= bonusValue;
    //                break;
    //            case "Damage":
    //                card.Data.Damage -= bonusValue;
    //                break;
    //            case "Shield":
    //                card.Data.Shield -= bonusValue;
    //                break;
    //            case "Draw":
    //                card.Data.Draw -= bonusValue;
    //                break;
    //            case "Discard":
    //                card.Data.Discard -= bonusValue;
    //                break;
    //            case "Remove":
    //                card.Data.Remove -= bonusValue;
    //                break;
    //            case "HealHP":
    //                card.Data.HP -= bonusValue;
    //                break;
    //            case "DamageHP":
    //                card.Data.HP += bonusValue;
    //                break;
    //        }
    //        return UniTask.CompletedTask;
    //    }));
    //}

    void AddVulnerable(Card card, List<(int order, AbilityTag tag, Func<UniTask> task)> tasks, StatusEffectType type, string amountText, string durationText)
    {
        tasks.Add((10, AbilityTag.PrePreparation, () => {
            // 헬퍼를 사용하여 적군 전체 혹은 단일 타겟에게 적용
            int amount = GetVariable(card, amountText);
            int duration = GetVariable(card, durationText);

            ForEachEnemyTarget(card, (targetEnemy) => {
                targetEnemy.AddStatusEffect((StatusEffect.Vulnerable, type), amount, duration);
            });
            return UniTask.CompletedTask;
        }
        ));
    }

}