using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardAbility
{
    CancellationTokenSource _cts;

    Action _cardImmediately;
    Func<UniTask> _cardTask;
    Func<UniTask<bool>?> _conditionTask;

    Player _player;
    public void SetCardAbility(Card card)
    {
        _player = InGameManager.Instance.Player;
        SettingImmediately(card);

        if (card.Data.HasSimpleCondition)
        {
            SettingCondition(card);
        }

        if (card.Data.IsSimpleAB)
        {
            SettingSimpleAB(card);
        }
        else
        {
            SettingCardAB(card);
        }
        card.SetCardImmediately(_cardImmediately);
        card.SetCardTask(_cardTask);
        card.SetUseConditions(_conditionTask);
    }
    void SettingImmediately(Card card)
    {
        switch (card.Data.ID)
        {
            case 105:
                _cardImmediately = () =>
                {
                    CardManager.Instance.SetCardState(1);
                };
                break;
            default:
                _cardImmediately = null ;
                break;
        }
    }

    void SettingCondition(Card card)
    {
        //Func<UniTask<bool>?> uniTaskCondition = null;
        if (card.Data.Discard > 0)
        {
            _conditionTask = () => UniTask.Create(async () =>
            {
                return await ConditionDiscardAB(card);
            });
        }
        else if (card.Data.Remove > 0)
        {
            _conditionTask = () => UniTask.Create(async () =>
            {
                return await ConditionRemoveAB(card);
            });
        }
        //card.SetUseConditions(uniTaskCondition);
    }

    void SettingSimpleAB(Card card, float delay = 0.3f)
    {
        //Func<UniTask> uniTaskAB = null;
        bool critical = _player.GetStatusEffect(StatusEffect.UseCritical, out _);
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                            (0, () => SingleAttackAB(card, critical)),
                            (1, () => ShieldAB(card)),
                            (1, () => DrawAB(card))
                        );
                    });
                else
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                            (0, () => SingleAttackAB(card, critical))
                        );
                    });
                break;
            case CardTag.MultiAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                             (0, () => MultiAttackAB(card, critical)),
                             (1, () => ShieldAB(card)),
                             (1, () => DrawAB(card))
                         );
                    });
                else
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                             (0, () => MultiAttackAB(card, critical))
                         );
                    });
                break;
            case CardTag.Skill:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await AddCardEvent(card, delay,
                             (0, () => ShieldAB(card)),
                             (0, () => DrawAB(card))
                         );
                });
                break;
        }
        //card.SetCardTask(uniTaskAB);

    }

    void SettingCardAB(Card card)
    {
        switch (card.Data.ID)
        {
            case 105:
                _cardTask = () => UniTask.Create(async () =>
                {
                    //CardManager.Instance.SetCardState(1);
                    await AddCardEvent(card, 0.5f,
                        (0, () => DrawAB(card)),
                        (1, () => ConfirmedDiscardAB(card))
                    );
                });
                break;
            case 503:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    _player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
                });
                break;
            case 801:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await AddCardEvent(card, 0.5f,
                        (0, async () =>
                        {
                            card.Data.Count = card.Data.Cost;
                            await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
                        }
                    )
                    );
                });
                break;
            default:
                _cardTask = () => UniTask.CompletedTask;
                break;
        }
    }

    async UniTask PlayCardEvent(Card card, SortedDictionary<int, List<Func<UniTask>>> cardEvent, float delay)
    {
        
        await UniTask.Create(async () =>
        {
            _cts = new CancellationTokenSource();

            bool isStart = true;
            for (int i = 0; i < card.Data.Count; i++) // 카드 횟수만큼 반복
            {
                if (i != 0)
                {
                    isStart = false;
                }
                await DelayTask(delay);
                await SpawnEffect(card, isStart);
                foreach (var eventTask in cardEvent) // 0, 1, 2... 순서대로 실행
                {
                    var tasksToRun = eventTask.Value.Select(func => func()).ToList();
                    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow(); // 동시 실행 및 대기
                }
                if (_cts.IsCancellationRequested)
                    break;
            }

            switch (card.Data.CardTag)
            {
                case CardTag.SingleAttack:
                    card.Target(null);
                    _player.CheckCritical();
                    break;
                case CardTag.MultiAttack:
                    _player.CheckCritical();
                    break;
                case CardTag.Skill:
                    break;
            }
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        });
    }

    async UniTask SpawnEffect(Card card, bool start = true)
    {
        if (card.Data.Effect == null)
        {
            //await DelayTask(0.5f);
            return;
        }
        card.UseTimingReset();
        if (!start && !card.RepeatEffect)
        {
            await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
            return;
        }
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                await UniTask.WhenAny(
                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
                    , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                    );
                break;
            case CardTag.MultiAttack:
                if (card.AllEnemies)
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                        );
                }
                else
                {
                    await UniTask.WhenAll(EnemyManager.Instance.EnemyList.Select(async enemy =>
                    {
                        if (enemy != null)
                        {
                            await UniTask.WhenAny(
                                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
                                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                                );
                            //await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
                        }
                    }));
                }
                break;  
            case CardTag.Skill:
                break;
        }
    }

    async UniTask AddCardEvent(Card card, float delay = 0.3f, params (int, Func<UniTask>)[] taskOrder)
    {
        var cardEventDict = new SortedDictionary<int, List<Func<UniTask>>>();
        foreach (var (order, effectTask) in taskOrder)
        {
            if (!cardEventDict.ContainsKey(order))
            {
                cardEventDict[order] = new List<Func<UniTask>>();
            }
            cardEventDict[order].Add(effectTask);
        }
        await PlayCardEvent(card, cardEventDict, delay);
    }

    async UniTask SingleAttackAB(Card card, bool critical)             // 컨티뉴 single이랑 그냥 single 합침. -> 2025년 10월 말에 코드 다 바꾸면서 그냥 단일 타켓 코드로만 작동.
    {
        
        bool killEnemy = await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
        if (killEnemy)
        {
            //Debug.Log(_cts);
            _cts.Cancel();
            _cts.Dispose();
        }
    }
    async UniTask MultiAttackAB(Card card, bool critical)              // 컨티뉴 multi랑 그냥 multi 합침. -> 2025년 10월 말에 코드 다 바꾸면서 그냥 멀티 타켓 코드로만 작동.
    {


        var enemyList = EnemyManager.Instance.EnemyList.ToList();

        await UniTask.WhenAll(enemyList.Select(async enemy =>
        {
            if (enemy != null)
            {
                await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
            }
        }));
        if (EnemyManager.Instance.NoEnemy) // 여긴 InBattle로 체크 안 함. InBattle은 적 죽는 모션 끝나는 것까지 기다려야 함. (+클리어 판정이 아닌, 현재 필드에 남아있는 적이 없다는 뜻)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

    }
    async UniTask ShieldAB(Card card)
    {
        
        // 쉴드가 다른 공격, 드로우에 비해 시간이 짧아서 같이 쓰려면 무조건 이펙트가 있어야 함. 안 그러면 순서가 이상해질 수 있음.
        await _player.Shield(card.Data.Shield);
        
    }
    async UniTask DrawAB(Card card)
    {
        
        await CardManager.Instance.DrawCard(card.Data.Draw);
        
    }
    async UniTask AfterDrawAB(Card card)
    {
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.SetCardState(1);
        //await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
        await DrawAB(card);
        CardManager.Instance.SetCardState(2);
    }


    async UniTask ConfirmedDiscardAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);

        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeDiscard(true);
        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionDiscardAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        bool discarded = false;
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeDiscard(true);
        CancellationTokenSource cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
            discarded = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        var task3 = UniTask.Create(async () =>
        {
            await UniTask.WaitUntil(() => !InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf, cancellationToken: cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        var task4 = UniTask.Create(async () =>
        {
            // 버리는 와중에 전투가 끝나면(전투 bool이 변경되면) 초기화
            await UniTask.WaitUntil(() => !TurnManager.Instance.InBattle, cancellationToken: cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        await UniTask.WhenAny(task1, task2, task3, task4);
        cts.Cancel();
        
        CardManager.Instance.ChangeDiscard(false);
        return discarded;

    }

    async UniTask ConfirmedRemoveAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeRemove(true);
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionRemoveAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        bool removed = false;
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeRemove(true);
        CancellationTokenSource cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
            //CardManager.Instance.ThrowAwaySelectedCard().Forget();        // 제거하는 코드로 변경
            removed = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            removed = false;
        });
        await UniTask.WhenAny(task1, task2);
        cts.Cancel();
        CardManager.Instance.ChangeRemove(false);
        return removed;
    }

    void ReduceHpAB(Card card)
    {

    }

    void HealAB(Card card)
    {

    }

    void CureAB(Card card)
    {

    }

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    }
}


