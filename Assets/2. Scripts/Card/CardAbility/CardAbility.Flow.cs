using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public partial class CardAbility
{
    async UniTask AddCardEvent(Card card, params (int order, AbilityTag tag, Func<UniTask> task)[] taskOrder)
    {
        // Order(번호표)만 정렬해주는 딕셔너리
        var cardEventDict = new SortedDictionary<int, List<Func<UniTask>>>();

        // 입력받은 순서 그대로(Order가 같다면 Add한 순서대로) 처리
        foreach (var (order, tag, effectTask) in taskOrder)
        {
            if (!cardEventDict.ContainsKey(order))
            {
                cardEventDict[order] = new List<Func<UniTask>>();
            }

            // 리스트에 추가 (사용자가 Build 함수에서 Add한 순서가 유지됨)
            cardEventDict[order].Add(effectTask);
        }

        await PlayCardEvent(card, cardEventDict);
    }

    async UniTask PlayCardEvent(Card card, SortedDictionary<int, List<Func<UniTask>>> cardEvent)
    {
        _cts = new CancellationTokenSource();

        // [1] Order 0 처리: 효과 발동 전 딱 1번만 실행
        if (cardEvent.TryGetValue(0, out var startTasks))
        {
            var tasksToRun = startTasks.Select(func => func()).ToList();
            await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
        }

        for (int i = 0; i < card.Data.Count; i++)
        {
            // 1. 이펙트 소환 (루프 안에서 딜레이 및 타이밍 체크)
            await EffectManager.Instance.SpawnEffect(card, i == 0, _cts.Token);

            EffectManager.Instance.SlowSppedEffect(card);

            // 2. 해당 순서(Order)의 능력치 동시 실행
            foreach (var eventTask in cardEvent)
            {
                if (eventTask.Key == 0 || eventTask.Key == 999) continue;
                var tasksToRun = eventTask.Value.Select(func => func()).ToList();
                await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
            }

            EffectManager.Instance.OriginSpeedEffect();

            if (_cts.IsCancellationRequested) break;
        }

        // [3] Order 999 처리: 모든 반복이 끝난 후 딱 1번만 실행
        if (cardEvent.TryGetValue(999, out var endTasks))
        {
            var tasksToRun = endTasks.Select(func => func()).ToList();
            await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow();
        }

        // [4]. 카드 태그별 후처리 (타겟 해제 및 크리티컬 체크)
        PostProcess(card);

        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        await UniTask.WaitForSeconds(CardUtils.NextCardUseDelay);
    }

    void PostProcess(Card card)
    {
        if (card.Data.CardTag == CardTag.SingleAttack || card.Data.CardTag == CardTag.SkillTargetSingle) card.Target(null);
        if (card.Data.CardTag == CardTag.SingleAttack || card.Data.CardTag == CardTag.AllAttack || card.Data.CardTag == CardTag.RandomAttack)
            _player.CheckCritical();
    }

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    }
}
