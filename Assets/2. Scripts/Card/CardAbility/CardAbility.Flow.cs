using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public partial class CardAbility
{
    async UniTask AddCardEvent(Card card, (float order, AbilityTag tag, Func<PlayContext, UniTask> task)[] taskOrder, PlayContext context)
    {
        // Order(번호표)만 정렬해주는 딕셔너리
        var cardEventDict = new SortedDictionary<float, List<Func<PlayContext, UniTask>>>();

        // 입력받은 순서 그대로(Order가 같다면 Add한 순서대로) 처리
        foreach (var (order, tag, effectTask) in taskOrder)
        {
            if (!cardEventDict.ContainsKey(order))
            {
                cardEventDict[order] = new List<Func<PlayContext, UniTask>>();
            }

            // 리스트에 추가 (사용자가 Build 함수에서 Add한 순서가 유지됨)
            cardEventDict[order].Add(effectTask);
        }

        await PlayCardEvent(card, cardEventDict, context);
    }

    async UniTask PlayCardEvent(Card card, SortedDictionary<float, List<Func<PlayContext, UniTask>>> cardEvent, PlayContext context)
    {
        _cts = new CancellationTokenSource();

        //card.IndividualUseDamage = 0;
        //card.TotalUseDamage = 0;
        //card.IsKillEnemy = false;

        if (!card.UnableEffect)
        {
            // [1] Order 0 처리: 효과 발동 전 딱 1번만 실행
            foreach (var eventTask in cardEvent)
            {
                if (card == null) break;
                if (eventTask.Key >= 0f && eventTask.Key < 1f)
                {
                    var tasksToRun = eventTask.Value.Select(func => func(context)).ToList();
                    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
                }
            }
            //if (cardEvent.TryGetValue(0, out var startTasks))
            //{
            //    var tasksToRun = startTasks.Select(func => func()).ToList();
            //    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
            //}

            for (int i = 0; i < card.Data.Count; i++)
            {
                if (card == null) break;
                // 1. 이펙트 소환 (루프 안에서 딜레이 및 타이밍 체크)
                await EffectManager.Instance.SpawnEffect(card, i == 0, _cts.Token);

                EffectManager.Instance.SlowSppedEffect(card);

                // 2. 해당 순서(Order)의 능력치 동시 실행
                foreach (var eventTask in cardEvent)
                {
                    if (!(eventTask.Key >= 1f && eventTask.Key < 999f)) continue;
                    var tasksToRun = eventTask.Value.Select(func => func(context)).ToList();
                    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
                }

                EffectManager.Instance.OriginSpeedEffect();

                if (_cts.IsCancellationRequested) break;
            }

            // [3]. 카드 태그별 후처리 (타겟 해제 및 크리티컬 체크)
            PostProcess(card, context);

            // [4] Order 999 처리: 모든 반복이 끝나고 카드 후처리가 끝난 다음 딱 1번만 실행
            foreach (var eventTask in cardEvent)
            {
                if (card == null) break;
                if (eventTask.Key >= 999f && eventTask.Key < 1000f)
                {
                    var tasksToRun = eventTask.Value.Select(func => func(context)).ToList();
                    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
                }
            }

        }
        else
        {
            PostProcess(card, context);
        }

        //if (cardEvent.TryGetValue(999, out var endTasks))
        //{
        //    if (!_cts.IsCancellationRequested)
        //    {
        //        var tasksToRun = endTasks.Select(func => func()).ToList();
        //        await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
        //    }
        //}


        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        await UniTask.WaitForSeconds(CardUtils.NextCardUseDelay);
    }

    void PostProcess(Card card, PlayContext context)
    {
        if (card == null) return;
        if (card is ActionCard actionCard)
        {
            return;
        }
        if (card.Data.CardTag == CardTag.SingleAttack || card.Data.CardTag == CardTag.SkillTargetSingle) card.Target(null);
        if (card.Data.DamageOrder >= 0)
        {
            _player.CheckCritical();
            TurnManager.Instance.AddAttackCardsPlayed();
            if (card.Data.Cost == 0)
            {
                _player.ApplyStatusEffect(StatusEffect.ZeroCostDamage, out _);
            }

            if (context.KilledEnemie)
            {
                // _player.OnKillEffect(); 
            }
            //_player.ApplyStatusEffect(StatusEffect.Weaking, out _);
            //ForEachEnemyTarget(card, (targetEnemy) => {
            //    targetEnemy.ApplyStatusEffect(StatusEffect.Vulnerable, out _);
            //});
        }
        if (IsSkillTag(card.Data.CardTag))
        {
            TurnManager.Instance.AddSkillCardsPlayed();
        }
        if (card.Data.Cost == 0)
        {
            TurnManager.Instance.AddZeroCardsPlayed();
        }
    }
    bool IsSkillTag(CardTag tag) =>
        tag == CardTag.SkillTargetSelf || tag == CardTag.SkillTargetSingle ||
        tag == CardTag.SkillTargetAll || tag == CardTag.SkillTargetRandom;

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    }
}
