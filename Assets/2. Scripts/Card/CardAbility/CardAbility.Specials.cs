using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class CardAbility
{

    private void InitSpecialAbilities()
    {
        _specialAbilityMap = new()
        {
            // args[0]: Type, args[1]: Amount, args[2]: Duration
            { StatusEffect.ATKUp, (card, tasks, data) => {
                //var type = GetEnum<StatusEffectType>(data.Type); // 헬퍼 사용
                //int amount = GetVariable(card, data.Amount);
                //int duration = GetVariable(card, data.Duration);

                AddAttackUP(card, tasks, data);
            }},

            { StatusEffect.Vulnerable, (card, tasks, data) => {
                //var type = GetEnum<StatusEffectType>(data.Type); // 헬퍼 사용
                //int amount = GetVariable(card, data.Amount);
                //int duration = GetVariable(card, data.Duration);

                AddVulnerable(card, tasks, data);
            }},

            { SpecialTag.Cost, (card, tasks, data) => {
                AddCost(card, tasks, data);
            }},

            { SpecialTag.Kill, (card, tasks, data) => {
                KillEnemy(card, tasks, data);
            }},

            { SpecialTag.DamageDealt, (card, tasks, data) => {
                DamageDealt(card, tasks, data);
            }},

            { SpecialTag.ChangeAllDebuffValue, (card, tasks, data) => {
                ChangeAllDebuffValue(card, tasks, data);
            }},

            { SpecialTag.RandomCardDraw, (card, tasks, data) => {
                RandomCardDraw(card, tasks, data);
            }},

            { SpecialTag.DrawCheck, (card, tasks, data) => {
                DrawCheck(card, tasks, data);
            }},

            { SpecialTag.PermanentUpgrade, (card, tasks, data) => {
                PermanentUpgrade(card, tasks, data);
            }},

            { SpecialTag.AddAbilityUpgrade, (card, tasks, data) => {
                AddAbilityUpgrade(card, tasks, data);
            }},

            //{ SpecialTag.CheckUsedCardCount, (card, tasks, data) => {
            //    CheckUsedCardCount(card, tasks, data);
            //}},       카드에서 바로 적용되도록 변경함.

            //{ SpecialTag.CardCountValue, (card, tasks, data) => {
            //    CardCountValue(card, tasks, data);
            //}},


            { StatusEffect.Weaking, (card, tasks, data) => {
                AddWeaking(card, tasks, data);
            }},

            { StatusEffect.Wildness, (card, tasks, data) => {
                AddWildness(card, tasks, data);
            }},

            { StatusEffect.DoubleAttack, (card, tasks, data) => {
                AddDoubleAttack(card, tasks, data);
            }},

            { StatusEffect.AddDamage, (card, tasks, data) => {
                AddAddDamage(card, tasks, data);
            }},

            { StatusEffect.CostZero, (card, tasks, data) => {
                AddCostZero(card, tasks, data);
            }},

            { StatusEffect.Vampire, (card, tasks, data) => {
                AddVampire(card, tasks, data);
            }},

            { StatusEffect.ZeroCostDamage, (card, tasks, data) => {
                AddZeroCostDamage(card, tasks, data);
            }},

            { SpecialTag.XValue, (card, tasks, data) => {
                AddXValue(card, tasks, data);
                //// data.Type에 "Count", "Damage" 등이 들어있으므로 그대로 전달
                //if (!string.IsNullOrEmpty(data.Type))
                //    AddXValue(card, tasks, data.Type);
            }},


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

    //private int GetVariable(Card card, string input)
    //{
    //    if (string.IsNullOrEmpty(input)) return 0;

    //    string val = input.Trim().ToLower();
    //    if (val == "x") return card.Data.Cost;
    //    if (int.TryParse(val, out int result)) return result;

    //    return 0;
    //}

    //private float GetMultipleValue(Card card, string input)
    //{
    //    if (string.IsNullOrEmpty(input)) return 1f;

    //    string val = input.Trim().ToLower();
    //    if (val == "x") return (float)card.Data.Cost;
    //    if (float.TryParse(val, out float result)) return result;

    //    return 1f;
    //}


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

    private void SettingImmediately(Card card, ref Action immediate, ref Action failure, ref Action success)
    {
        switch (card.Data.ID)
        {
            //case 105:
            //    immediate += () => CardManager.Instance.SetCardState(1);
            //    failure += () => CardManager.Instance.SetCardState(2);
            //    break;
        }
    }

    private void SettingSpecialCondition(Card card, List<Func<UniTask<bool>>> conditionTasks, ref Action immediate, ref Action failure, ref Action success)
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

    private void BuildSpecialAbilities(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks)
    {
        if (card.Data.MasterTags == null || card.Data.MasterTags.Count == 0) return;

        foreach (var tagData in card.Data.MasterTags)
        {
            //if (tagData.Tag.Is(SpecialTag.XValue)) continue;
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
    void AddEnemyStatusDamage(Card card, List<(int order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks)
    {
        if (card.Data.Damage > 0)
        {
            // Case A: 기존 데미지가 있는 경우 (0번에서 더하고 6번에서 뺌)
            int bonus = 0;
            tasks.Add((0, AbilityTag.PrePreparation, (ctx) => {
                bonus = card.TargetEnemy.CurStatusEffectList.Count + card.TargetEnemy.CurStatusEffectPerpetualList.Count;
                card.Data.Damage += bonus;
                return UniTask.CompletedTask;
            }
            ));
            tasks.Add((6, AbilityTag.PostEffect, (ctx) => {
                card.Data.Damage -= bonus;
                return UniTask.CompletedTask;
            }
            ));
        }
        else
        {
            // Case B: 데미지가 없는 경우 (3번에서 직접 계산해서 공격)
            tasks.Add((3, AbilityTag.Attack, async (ctx) => {
                int count = card.TargetEnemy.CurStatusEffectList.Count + card.TargetEnemy.CurStatusEffectPerpetualList.Count;
                card.Data.Damage = count;
                await SingleAttackAB(card, GetCrit(), ctx);
                card.Data.Damage = 0;
            }
            ));
        }
    }

    // 503: 공격력 버프 (0번)
    void AddAttackUP(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            int amount, duration;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            if (data.XDuration) duration = card.Data.Cost;
            else duration = (int)data.Duration;

            _player.AddStatusEffect((StatusEffect.ATKUp, data.Type.Status), amount, duration);
            return UniTask.CompletedTask;
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void AddDefenseUP(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            int amount, duration;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            if (data.XDuration) duration = card.Data.Cost;
            else duration = (int)data.Duration;

            _player.AddStatusEffect((StatusEffect.DEFUp, data.Type.Status), amount);
            return UniTask.CompletedTask;
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void AddCost(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            int amount;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            switch (data.Type.Special)
            {
                case SpecialTagType.AddAmount:
                    _player.AddCurHolo(amount);
                    break;
                case SpecialTagType.MultipleAmount:
                    _player.AddCurHolo(_player.CurHolo * (amount - 1));
                    break;
            }

            return UniTask.CompletedTask;
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void KillEnemy(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = async (ctx) =>
        {
            if (!ctx.KilledEnemie) return;
            int amount;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            await TypeCardTask(data.Type.Special, amount);
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }


    void DamageDealt(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = async (ctx) =>
        {
            float multiple;
            if (data.XAmount) multiple = card.Data.Cost;
            else multiple = data.Amount;

            int amount = MathUtil.MultiplierToInt(ctx.LastDamageDealt, multiple);
            await TypeCardTask(data.Type.Special, amount);
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));

    }

    void ChangeAllDebuffValue(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            int amount, duration;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            if (data.XDuration) duration = card.Data.Cost;
            else duration = (int)data.Duration;
            ApplyToStatus(card, data.Type.Special, amount, duration);
            return UniTask.CompletedTask;
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void RandomCardDraw(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = async (ctx) =>
        {
            int amount;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            switch (data.Type.Special)
            {
                case SpecialTagType.DrawAttack:
                    await CardManager.Instance.DrawAttackCardsFromDeck(amount);
                    break;
                case SpecialTagType.DrawSkill:
                    //await CardManager.Instance.DrawAttackCardsFromDeck(amount);
                    break;
            }
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void DrawCheck(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            if (ctx.DrawnCards.Count == 0) return UniTask.CompletedTask;

            // 2. 이 카드의 마스터 태그 리스트에서 'DrawCardEffect' 태그를 찾아옵니다.
            var effectData = card.Data.MasterTags.Find(t => t.Tag.Special == SpecialTag.DrawCardEffect);

            // 만약 강화 수치를 담은 DrawCardEffect 태그가 없다면 실행 불가
            if (effectData.Tag.Is(SpecialTag.None)) return UniTask.CompletedTask;

            foreach (var drawnCard in ctx.DrawnCards)
            {
                bool isMatch = false;

                // 3. DrawCardEffect의 타입에 따라 조건 검사
                switch (data.Type.Special)
                {
                    case SpecialTagType.IsDamage:
                        // 공격 카드인가? (DamageOrder가 0 이상이면 공격 능력이 있는 카드)
                        if (drawnCard.Data.DamageOrder >= 0) isMatch = true;
                        break;

                    case SpecialTagType.IsShield:
                        // 방어 카드인가?
                        if (drawnCard.Data.ShieldOrder >= 0) isMatch = true;
                        break;
                }

                // 4. 조건에 맞다면 해당 카드의 수치를 강화!
                if (isMatch)
                {
                    int bonusAmount = effectData.XAmount ? card.Data.Cost : (int)effectData.Amount;

                    //ApplyToCard(drawnCard, data.Type.Special, bonusAmount);
                    drawnCard.AddCardBuff(false, effectData.Type.Special, bonusAmount);


                    // 시각적으로 수치가 변했음을 알림 (카드 텍스트 갱신)
                    //drawnCard.RefreshAllDesc();
                }
            }
            return UniTask.CompletedTask;
        };
        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void PermanentUpgrade(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = (ctx) =>
        {
            int amount;
            if (data.XAmount) amount = card.Data.Cost;
            else amount = (int)data.Amount;

            card.AddCardBuff(true, data.Type.Special, amount);

            return UniTask.CompletedTask;
        };

        tasks.Add((10, AbilityTag.PostEffect, taskLogic));
    }

    void AddAbilityUpgrade(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        int amount;
        if (data.XAmount) amount = card.Data.Cost;
        else amount = (int)data.Amount;

        card.AbilityBuff(data.Type.Special, amount);
    }

    //void CheckUsedCardCount(Card card, List<(float order, AbilityTag tag, Func<UniTask> task)> tasks, MasterTagData data)
    //{
    //    if (!card.Data.MasterTags.Any(t => t.Tag.Special == SpecialTag.CardCountValue)) return;
    //    var valueTag = card.Data.MasterTags.Find(t => t.Tag.Special == SpecialTag.CardCountValue);

    //    // 짝꿍이 없으면 이 카드는 그냥 체크만 하고 끝나는 무의미한 카드거나 에러입니다.
    //    if (valueTag.Tag.Special == SpecialTag.None) return;

    //    // 2. 짝꿍이 있다면, 두 태스크를 여기서 한 번에 생성합니다.
    //    // 'amount'를 여기서 클로저로 잡아두면 0.1f에서 0.2f로 아주 안전하게 전달됩니다.
    //    int calculatedCount = 0;
    //    int appliedAmount = 0;

    //    // [Step 1: 계산 - 0.1f]
    //    Func<UniTask> calculateTask = () =>
    //    {
    //        var tm = TurnManager.Instance;
    //        if (tm == null) return UniTask.CompletedTask;

    //        calculatedCount = data.Type.Special switch
    //        {
    //            SpecialTagType.TurnAttack => tm.GetTurnAttackCount(),
    //            SpecialTagType.TurnSkill => tm.GetTurnSkillCount(),
    //            SpecialTagType.TurnAll => tm.GetTurnAttackCount() + tm.GetTurnSkillCount(),
    //            SpecialTagType.BattleAttack => tm.GetBattleAttackCount(),
    //            SpecialTagType.BattleSkill => tm.GetBattleSkillCount(),
    //            SpecialTagType.BattleAll => tm.GetBattleAttackCount() + tm.GetBattleSkillCount(),
    //            SpecialTagType.BattleZeroCost => tm.GetBattleZeroCostCount(),
    //            _ => 0
    //        };
    //        return UniTask.CompletedTask;
    //    };
    //    tasks.Add((0.1f, AbilityTag.PrePreparation, calculateTask));

    //    // [Step 2: 적용 - 0.2f]
    //    // 여기서 valueTag.Amount를 바로 써버립니다!
    //    Func<UniTask> applyTask = () =>
    //    {
    //        appliedAmount = MathUtil.MultiplierToInt(calculatedCount, valueTag.Amount);
    //        ApplyToCard(card, valueTag.Type.Special, appliedAmount);
    //        return UniTask.CompletedTask;
    //    };
    //    tasks.Add((0.2f, AbilityTag.PrePreparation, applyTask));

    //    // [Step 3: 복구 - 999.9f]
    //    // 복구 로직까지 여기서 한 번에 예약해버리면 완벽합니다.
    //    Func<UniTask> restoreTask = () =>
    //    {
    //        RestoreCard(card, valueTag.Type.Special, appliedAmount);
    //        return UniTask.CompletedTask;
    //    };
    //    tasks.Add((999.8f, AbilityTag.PostEffect, restoreTask));

    //}

    async UniTask TypeCardTask(SpecialTagType type, int amount)
    {
        switch (type)
        {
            case SpecialTagType.AddHealHP:
                await _player.Heal(amount);
                break;
            case SpecialTagType.AddCost:
                _player.AddCurHolo(amount);
                break;
            case SpecialTagType.AddShield:
                await _player.Shield(amount);
                break;
            case SpecialTagType.AddDraw:
                await CardManager.Instance.DrawCard(amount);
                break;
        }
    }

    void AddVulnerable(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(false, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    void AddWeaking(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(false, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    void AddWildness(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(true, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    void AddDoubleAttack(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(true, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    void AddAddDamage(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(false, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    void AddCostZero(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(true, card, data);
        tasks.Add((999.1f, AbilityTag.PostEffect, taskLogic));
    }
    void AddVampire(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(true, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }
    void AddZeroCostDamage(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic = CreateTaskLogic(true, card, data);
        tasks.Add((10f, AbilityTag.PostEffect, taskLogic));
    }

    Func<PlayContext, UniTask> CreateTaskLogic(bool isApplyPlayer, Card card, MasterTagData data)
    {
        Func<PlayContext, UniTask> taskLogic;
        if (isApplyPlayer)
        {
            taskLogic = (ctx) =>
            {
                // 헬퍼를 사용하여 적군 전체 혹은 단일 타겟에게 적용
                int amount, duration;
                if (data.XAmount) amount = card.Data.Cost;
                else amount = (int)data.Amount;

                if (data.XDuration) duration = card.Data.Cost;
                else duration = (int)data.Duration;

                _player.AddStatusEffect((data.Tag.Status, data.Type.Status), amount, duration);
                return UniTask.CompletedTask;
            };
        }
        else
        {
            taskLogic = (ctx) =>
            {
                // 헬퍼를 사용하여 적군 전체 혹은 단일 타겟에게 적용
                int amount, duration;
                if (data.XAmount) amount = card.Data.Cost;
                else amount = (int)data.Amount;

                if (data.XDuration) duration = card.Data.Cost;
                else duration = (int)data.Duration;

                ForEachEnemyTarget(card, (targetEnemy) => {
                    targetEnemy.AddStatusEffect((data.Tag.Status, data.Type.Status), amount, duration);
                });
                return UniTask.CompletedTask;
            };
        }

        return taskLogic;
    }

    private void AddXValue(Card card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)> tasks, MasterTagData data)
    {
        if (card.DefaultData.Cost != -1) return;
        int amount = 0;
        Func<PlayContext, UniTask> taskLogic1 = (ctx) =>
        {
            int xValue = card.Data.Cost;

            amount = MathUtil.MultiplierToInt(xValue, data.Amount);

            ApplyToCard(card, data.Type.Special, amount);
            return UniTask.CompletedTask;
        };

        tasks.Add((0, AbilityTag.PrePreparation, taskLogic1));

        Func<PlayContext, UniTask> taskLogic2 = (ctx) =>
        {
            RestoreCard(card, data.Type.Special, amount);
            card.Data.Cost = -1;
            return UniTask.CompletedTask;
        };

        tasks.Add((999.9f, AbilityTag.PostEffect, taskLogic2));

    }

    void ApplyToStatus(Card card, SpecialTagType type, int amount, int duration)
    {
        if (type == SpecialTagType.None) return;
        ForEachEnemyTarget(card, (targetEnemy) =>
        {
            switch (type)
            {
                // [1] 더하기/빼기 계열 (amount, duration이 변화량 자체)
                case SpecialTagType.AddAmount:
                case SpecialTagType.AddDuration:
                    AdjustAllStatusEffects(targetEnemy, amount, duration);
                    break;

                // [2] 배수 계열 (amount, duration이 곱할 배수)
                case SpecialTagType.MultipleAmount:
                case SpecialTagType.MultipleDuration:
                    // MultipleAmount일 때 amount는 배수, MultipleDuration일 때 duration이 배수
                    // 만약 시트에서 하나만 쓴다면 기본값 1을 넘겨서 변화 없게 처리
                    int amtMult = (type == SpecialTagType.MultipleAmount) ? amount : 1;
                    int durMult = (type == SpecialTagType.MultipleDuration) ? duration : 1;

                    MultiplyAllStatusEffects(targetEnemy, amtMult, durMult);
                    break;
            }
        });
    }

    // --- [내부 헬퍼 1: 더하기/빼기] ---
    private void AdjustAllStatusEffects(Enemy enemy, int amountDelta, int durationDelta)
    {
        var targets = GetAllStatusTargets(enemy);
        foreach (var target in targets)
        {
            // 수치 처리
            if (amountDelta > 0) enemy.AddStatusEffect(target, amountDelta, 0);
            else if (amountDelta < 0) enemy.ReduceStatusEffect(target, Mathf.Abs(amountDelta), 0);

            // 시간 처리
            if (durationDelta > 0) enemy.AddStatusEffect(target, 0, durationDelta);
            else if (durationDelta < 0) enemy.ReduceStatusEffect(target, 0, Mathf.Abs(durationDelta));
        }
    }

    // --- [내부 헬퍼 2: 배수 곱하기] ---
    private void MultiplyAllStatusEffects(Enemy enemy, int amountMultiplier, int durationMultiplier)
    {
        var targets = GetAllStatusTargets(enemy);
        foreach (var target in targets)
        {
            // 현재 정보 가져오기
            var info = enemy.CurStatusEffectDict[target.effect][target.type];
            int curAmt = info.amount;
            int curDur = info.duration;

            // 1. 수치 배수 계산: (현재값 * 배수) - 현재값 = 추가/감소할 양
            if (amountMultiplier != 1)
            {
                int delta = (curAmt * amountMultiplier) - curAmt;
                if (delta > 0) enemy.AddStatusEffect(target, delta, 0);
                else if (delta < 0) enemy.ReduceStatusEffect(target, Mathf.Abs(delta), 0);
            }

            // 2. 지속시간 배수 계산 (무한 지속 -1은 제외)
            if (durationMultiplier != 1 && curDur != -1)
            {
                int delta = (curDur * durationMultiplier) - curDur;
                if (delta > 0) enemy.AddStatusEffect(target, 0, delta);
                else if (delta < 0) enemy.ReduceStatusEffect(target, 0, Mathf.Abs(delta));
            }
        }
    }

    // 공통 키 추출 함수
    private List<(StatusEffect effect, StatusEffectType type)> GetAllStatusTargets(Enemy enemy)
    {
        List<(StatusEffect effect, StatusEffectType type)> targets = new();
        foreach (var outer in enemy.CurStatusEffectDict)
            foreach (var inner in outer.Value)
                targets.Add((outer.Key, inner.Key));
        return targets;
    }

    private void ApplyToCard(Card card, SpecialTagType type, int amount)
    {
        switch (type)
        {
            case SpecialTagType.AddCount: card.Data.Count = amount; break;
            case SpecialTagType.AddDamage: card.Data.Damage += amount; break;
            case SpecialTagType.AddShield: card.Data.Shield += amount; break;
            case SpecialTagType.AddDraw: card.Data.Draw += amount; break;
            case SpecialTagType.AddHealHP: card.Data.HP += amount; break;
            case SpecialTagType.AddDamageHP: card.Data.HP -= amount; break;
        }
    }

    private void RestoreCard(Card card, SpecialTagType type, int amount)
    {
        switch (type)
        {
            // Count는 1이 아니라 기본값으로 복구하는 것이 안전합니다.
            case SpecialTagType.AddCount: card.Data.Count = card.DefaultData.Count; break;
            case SpecialTagType.AddDamage: card.Data.Damage -= amount; break;
            case SpecialTagType.AddShield: card.Data.Shield -= amount; break;
            case SpecialTagType.AddDraw: card.Data.Draw -= amount; break;
            case SpecialTagType.AddHealHP: card.Data.HP -= amount; break;
            case SpecialTagType.AddDamageHP: card.Data.HP += amount; break;
        }
    }


}