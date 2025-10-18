using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CardAbility
{
    //public UniTask Task;
    //public UniTask<Action<Card>> cardActionTask;

    int _startTask;
    int _endTask;
    int _checkTask;

    const float attackSpeed = 0.3f;
    public Action<T> Action<T>(Func<T, UniTaskVoid> asyncAction)
    {
        return (t1) => asyncAction(t1).Forget();
    }
    //public Action Action(Func<UniTask> asyncAction)
    //{
    //    return () => asyncAction().Forget();
    //}
    //public static class UniTaskHelper
    //{
    //    public static Action<T> Action<T>(Func<T, UniTask> asyncAction)
    //    {
    //        return (t1) => asyncAction(t1).Forget();
    //    }
    //}
    public Action cardAction;
    //public Action<Card> cardAction;

    //public CancellationTokenSource CancelSource = new CancellationTokenSource();
    //public Dictionary<CardTag, Action<int>> CardTask = new()
    //{
    //    {CardTag.SingleAttackAB, CardManager.Instance.targetEnemy.TakeDamage},
    //    {CardTag.MultiAttackAB},
    //    {CardTag.Skill, null}
    //};

    //public void MultiAttackAB(int dmg)
    //{
    //    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
    //    {
    //        EnemyManager.Instance.enemies[i].TakeDamage(dmg);
    //    }
    //}

    public void SetCardAbility(Card card)
    {
        card.UseConditions = null;          // 능력과 조건문은 초기화 해줘야 함. 능력은 모두 초기화 시키지만, 조건문은 일부 초기화가 안 되서 오류가 발생하는 경우 존재.
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
            switch (card.Data.ID)
            {
                case 105:
                    //card.UseConditions = () => UniTask.Create(async () =>
                    //{
                    //    CardManager.Instance.SetCardState(1);
                    //    await UniTask.CompletedTask;        // 사실 없어도 됨.
                    //    return true;

                    //});
                    card.CardTask = () => UniTask.Create(async () =>
                    {
                        CardManager.Instance.SetCardState(1);
                        await DelayTask(0.5f);
                        //await AfterDrawAB(card);
                        await DrawAB(card);
                        //await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
                        await ConfirmedDiscardAB();
                    });
                    break;
                case 503:
                    card.CardTask = () => UniTask.Create(async () =>
                    {
                        await DelayTask(0.5f);
                        InGameManager.Instance.Player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
                    });
                    break;
                case 801:
                    card.CardTask = () => UniTask.Create(async () =>
                    {
                        await DelayTask(0.5f);
                        card.Data.Count = card.Data.Cost;
                        await SingleAttackAB(card);
                    });
                    break;
                default:
                    card.CardTask = () => UniTask.CompletedTask;
                    break;
            }
        }
        //return cardActTask;
    }

    //public void SetCardAbility(Card card)
    //{
    //    //UniTask cardActTask;
    //    switch (card.Data.ID)       // Defer => await 한 번일 때 유용, Lazy => await 여러 번일 때 유용
    //    {
    //        case 100:
    //            card.UseConditions = () => UniTask.Create(/*(Func<UniTask<bool>>)(*/async () =>
    //            {
    //                return await ConditionDiscardAB();
    //            })/*)*/;
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await SingleAttackAB(card);
    //            });
    //            //return SingleAttackAB(card);
    //            //cardActTask = UniTask.Defer(async () => 
    //            //{
    //            //    await DelayTask(0.5f);
    //            //    SingleAttackAB(card);
    //            //}/*, true, TurnManager.Instance.CancelSource.Token*/);      // false면 await 이후 코드가 스레드 풀에서 진행된다고 하는데, 아직 정확히는 모르겠어서 이건 일단 좀 더 공부해봐야 할 듯.
    //            break;
    //        case 101:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await SingleAttackAB(card);         // whenAll 안 하는 이유는 때린 다음 드로우하는 게 좀 더 자연스럽기 때문
    //                await DrawAB();
    //            });

    //            //cardActTask = UniTask.Defer(async () =>
    //            //{
    //            //    await DelayTask(0.5f);
    //            //    SingleAttackAB(card);
    //            //    await DrawAB();

    //            //    //await UniTask.WhenAll(                    // WhenAll은 특정 상황에서만 사용
    //            //    //    UniTask.Create(async () =>
    //            //    //    {
    //            //    //        await DelayTask(1f);
    //            //    //        SingleAttackAB(card);
    //            //    //    }),
    //            //    //    UniTask.Create(async () =>
    //            //    //    {
    //            //    //        await DelayTask(1f);
    //            //    //        await DrawAB();
    //            //    //    }));


    //            //    //DrawAB(/*card*/)
    //            //});
    //            break;
    //        case 102:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ContinuousMultiAttackAB(card, 0.3f);
    //            });
    //            break;
    //        case 103:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ContinuousDrawAB(card);
    //            });
    //            break;
    //        case 104:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ShieldAB(card);
    //            });
    //            break;
    //        case 105:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await AfterDrawAB();
    //                await ConfirmedDiscardAB();
    //            });
    //            break;
    //        case 106:
    //            card.UseConditions = () => UniTask.Create(async () =>
    //            {
    //                return await ConditionDiscardAB();
    //            });
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ShieldAB(card);
    //            });
    //            break;
    //        case 107:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await UniTask.WhenAll(              // 이건 그냥 같이
    //                    SingleAttackAB(card),
    //                    ShieldAB(card)
    //                );
    //            });
    //            break;
    //        case 500:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ContinuousSinglettackAB(card, 0.3f);
    //            });
    //            break;
    //        case 501:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await SingleAttackAB(card);     // 떄린 다음에 쉴드랑 드로우 느낌.
    //                await UniTask.WhenAll(
    //                    ShieldAB(card),
    //                    DrawAB()
    //                );
    //            });
    //            break;
    //        case 502:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ContinuousMultiAttackAB(card);
    //            });
    //            break;
    //        case 800:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await SingleAttackAB(card);
    //            });
    //            break;
    //        case 1000:
    //            card.CardTask = () => UniTask.Create(async () =>
    //            {
    //                await DelayTask(0.5f);
    //                await ContinuousMultiAttackAB(card);
    //            });
    //            break;
    //        default:
    //            card.CardTask = () => UniTask.CompletedTask;
    //            break;
    //    }
    //    //return cardActTask;
    //}

    //public AsyncLazy SetCardLazyAbility(Card card)
    //{
    //    AsyncLazy cardLazy;
    //    switch (card.Data.ID)       // Defer => await 한 번일 때 유용, Lazy => await 여러 번일 때 유용
    //    {
    //        case 100:
    //            //return SingleAttackAB(card);
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAB(card);
    //            }/*, true, TurnManager.Instance.CancelSource.Token*/);      // false면 await 이후 코드가 스레드 풀에서 진행된다고 하는데, 아직 정확히는 모르겠어서 이건 일단 좀 더 공부해봐야 할 듯.
    //            break;
    //        case 101:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAB(card);
    //                await DrawAB();

    //                //await UniTask.WhenAll(
    //                //    UniTask.Create(async () =>
    //                //    {
    //                //        await DelayTask(1f);
    //                //        SingleAttackAB(card);
    //                //    }),
    //                //    UniTask.Create(async () =>
    //                //    {
    //                //        await DelayTask(1f);
    //                //        await DrawAB();
    //                //    }));


    //                //DrawAB(/*card*/)
    //            });
    //            break;
    //        case 102:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousSinglettackAB(card, 0.3f);
    //            });
    //            break;
    //        case 103:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousMultiAttackAB(card, 0.3f);
    //            });
    //            break;
    //        case 104:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousDrawAB(card);
    //            });
    //            break;
    //        case 105:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                ShieldAB(card);
    //            });
    //            break;
    //        default: cardLazy = UniTask.Lazy(() => UniTask.CompletedTask); break;
    //    }
    //    return cardLazy;
    //}
    //public Action SetCardActionAbility(Card card)
    //{
    //    switch (card.Data.ID)
    //    {
    //        case 100:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAB(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 101:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAB(card);
    //                await DrawAB(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 102:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                await ContinuousSinglettackAB(card, 0.3f);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 103:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await ContinuousMultiAttackAB(card, 0.3f);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 104:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await ContinuousDrawAB(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DrawAB(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 105:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                ShieldAB(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        default: cardAction = null; break;
    //    }
    //    return cardAction;
    //}

    //public UniTask SetCardTaskAbility(int id)
    //{
    //    UniTask cardActTask;
    //    switch (id)
    //    {
    //        case 100:
    //            cardActTask = (card) => UniTask.WhenAll(SingleAttackAB(card));
    //            //await UniTask.RunOnThreadPool((cardAction));
    //            cardAction += SingleAttackAB;
    //            break;
    //        case 101:
    //            cardAction += SingleAttackAB;
    //            cardAction += async (card) => await DrawAB(card);
    //            break;
    //        case 102:
    //            cardAction += async (card) =>
    //            {
    //                await DelayTask(1f);
    //                await ContinuousSinglettackAB(card, 0.3f);
    //            };
    //            break;
    //        case 103:
    //            cardAction += async (card) => await ContinuousMultiAttackAB(card, 0.3f);
    //            break;
    //        case 104:
    //            cardAction += async (card) => await ContinuousDrawAB(card);
    //            break;
    //        case 105:
    //            cardAction += ShieldAB;
    //            break;
    //        default: cardActTask = new(); break;
    //    }

    //    return cardActTask;
    //}

    //public Action<Card> SetCardTaskAbility(int id)
    //{
    //    switch (id)
    //    {
    //        case 100:
    //            cardAction += SingleAttackAB;
    //            break;
    //        case 101:
    //            cardAction += SingleAttackAB;
    //            cardAction += async (card) => await DrawAB(card);
    //            break;
    //        case 102:
    //            cardAction += async (card) =>
    //            {
    //                await DelayTask(1f);
    //                ContinuousSinglettackAB(card, 0.3f).Forget();
    //            };
    //            break;
    //        case 103:
    //            cardAction += (card) => ContinuousMultiAttackAB(card, 0.3f).Forget();
    //            break;
    //        case 104:
    //            cardAction += async (card) => await ContinuousDrawAB(card);
    //            //cardAction += UniTaskHelper.Action<Card>(DrawAB);
    //            break;
    //        case 105:
    //            cardAction += ShieldAB;
    //            break;
    //        default: cardAction = null; break;
    //    }
    //    return cardAction;
    //}

    void SettingCondition(Card card)
    {
        Func<UniTask<bool>?> uniTaskCondition = null;
        if (card.Data.Discard > 0)
        {
            uniTaskCondition = () => UniTask.Create(async () =>
            {
                return await ConditionDiscardAB();
            });
        }
        else if (card.Data.Remove > 0)
        {
            uniTaskCondition = () => UniTask.Create(async () =>
            {
                return await ConditionRemoveAB();
            });
        }
        card.UseConditions = uniTaskCondition;
    }

    void SettingSimpleAB(Card card, float firstDelay = 0.5f, float continuousDelay = 0.3f)
    {
        Func<UniTask> uniTaskAB = null;
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    uniTaskAB = () => UniTask.Create(async () =>
                    {
                        await DelayTask(firstDelay);      // 카드 쓰기 전 딜레이 확인
                        //await SingleAttackAB(card, continuousDelay);
                        await UniTask.WhenAll
                        (
                            SingleAttackAB(card, continuousDelay),
                            ShieldAB(card, continuousDelay, 1),
                            DrawAB(card, continuousDelay, 1)
                        );
                    });
                else
                    uniTaskAB = () => UniTask.Create(async () =>
                    {
                        await DelayTask(firstDelay);
                        await SingleAttackAB(card, continuousDelay);
                    });
                break;
            case CardTag.MultiAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    uniTaskAB = () => UniTask.Create(async () =>
                    {
                        await DelayTask(firstDelay);
                        //await MultiAttackAB(card, continuousDelay);
                        await UniTask.WhenAll
                        (
                            MultiAttackAB(card, continuousDelay),
                            ShieldAB(card, continuousDelay, 1),
                            DrawAB(card, continuousDelay, 1)
                        );
                    });
                else
                    uniTaskAB = () => UniTask.Create(async () =>
                    {
                        await DelayTask(firstDelay);
                        await MultiAttackAB(card, continuousDelay);
                    });
                break;
            case CardTag.Skill:
                uniTaskAB = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await UniTask.WhenAll
                    (
                        ShieldAB(card, continuousDelay),
                        DrawAB(card)
                    );
                });
                break;
        }
        card.CardTask = uniTaskAB;
        //card.CardTask = () => UniTask.Create(async () =>
        //{
        //    await DelayTask(0.5f);
        //    // 타격 이후 쉴드와 보호막 얻는 형태로 진행.
        //    if (card.Data.CardTag == CardTag.SingleAttack)
        //    {
        //        await SingleAttackAB(card, continuousDelay);
        //    }
        //    else if (card.Data.CardTag == CardTag.MultiAttack)
        //    {
        //        await MultiAttackAB(card, continuousDelay);
        //    }
        //    if (card.Data.Shield > 0 || card.Data.Draw > 0)
        //    {
        //        await UniTask.WhenAll
        //        (
        //            ShieldAB(card, continuousDelay),
        //            DrawAB(card)
        //        );
        //    }
        //});
    }
    //int CheckCritical(Card card, bool critical)
    //{
    //    int damage = card.Data.Damage;
    //    if (critical)
    //    {
    //        damage = Mathf.RoundToInt(damage * InGameManager.Instance.Player.CriticalDamage.Value * 0.01f + 0.0001f);
    //    }
    //    return damage;
    //}
    
    async UniTask CheckAllEndTask()
    {
        if (_endTask > 0)
            Debug.Log(--_endTask);
        await UniTask.WaitUntil(() => _endTask == 0);   // 다른 곳에서 게임 끝났는지 확인하고, 애초에 애는 다 끝났을 때, 값만 0이 됐는지 확인하는 거라 굳이 토큰 필요없음.
    }
    async UniTask CheckTaskOrder(int order, bool start = true)
    {
        if (start)
        {
            await UniTask.WaitUntil(() => _endTask == order, cancellationToken: TurnManager.Instance.CancelSource.Token);
            Debug.Log($"{order}번째 실행");
        }
        else
        {
            await CheckAllEndTask();
            await UniTask.WaitUntil(() => _endTask == order, cancellationToken: TurnManager.Instance.CancelSource.Token);
            Debug.Log($"{order}번째 실행");
        }
    }

    // 카드 사용 시, 사용하는 카드 이벤트 개수만큼 startTask가 증가함. 그 후, 각각의 이벤트가 끝날 때마다 endTask값을 올림.
    // endTask가 startTask만큼 즉, 모든 카드 이벤트가 끝났으면, 해당 카드의 공격타이밍을 초기화하고 초기화한 카드 개수가 startTask와 같을 때까지 기다림.
    // 초기화한 카드 개수를 구하지 않으면, 반복 카드일 경우, endTask를 낮췄다가 다시 올리기에 조건 검사에 문제가 생김.
    async UniTask CheckTaskCount(Card card)
    {
        _checkTask = 0;
        await UniTask.WaitUntil(() => _startTask == _endTask, cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
        if (card.CardUseTiming)
            card.UseTimingReset();
        ++_checkTask;
        // 밑에 내용 상관없음. 그냥 WaitUntil 쓰기로 함.
        // 작거나 같은 경우를 쓰는 이유: 각각의 카드 이벤트가 마무리 될 때마다 startTask를 1씩 감소시킴. 이 때, checkTask값이 startTask 값보다 커지게 되는데
        // 굳이 WaitUntil 써서 밑에 endTask가 0이 될 때까지 대기하는 것보단, 이 방식이 더 나을 것 같음. 
        await UniTask.WaitUntil(() => _startTask == _checkTask, cancellationToken: TurnManager.Instance.CancelSource.Token);
        //if (await UniTask.WaitUntil(() => _startTask == _endTask, cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow())
        //{
        //    Debug.Log("ASDASD");
        //    _startTask = 0;
        //    _endTask = 0;
        //}
        //if (card.RepeatEffect && card.PlayEffect != null)
        //{
        //    card.PlayEffect = null;
        //}
    }

    async UniTask SingleAttackAB(Card card, float delay = 0.3f, int order = 0)             // 컨티뉴 single이랑 그냥 single 합침.
    {
        Debug.Log(++_startTask);
        await CheckTaskOrder(order);
        if (EnemyManager.Instance.EnemyList.Count == 0)
        {
            Debug.Log(--_startTask);
            return;
        }
        //bool critical = InGameManager.Instance.Player.CheckCritical();
        bool critical = InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _);
        //await PoolManager.Instance.GetEffect(card.Data.Effect, card.TargetEnemy.transform.position, Quaternion.identity);
        //PoolManager.Instance.GetEffect(card.Data.Effect, card.TargetEnemy.transform.position, Quaternion.identity).Forget();

        var cts = new CancellationTokenSource();


        card.UseTimingReset();
        await UniTask.WhenAny(
            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
            , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
            );
        //await UniTask.WaitUntil(() => card.CardUseTiming);
        bool killEnemy = await card.TargetEnemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
        Debug.Log(++_endTask);
        if (!killEnemy)
        {
            for (int i = 1; i < card.Data.Count; ++i)
            {
                if (await CheckTaskCount(card).SuppressCancellationThrow())
                {
                    break;
                }
                if (await CheckTaskOrder(order, false).SuppressCancellationThrow())
                {
                    break;
                }
                //card.UseTimingReset();
                if (card.Data.Effect != null)
                {
                    if (card.RepeatEffect)
                    {
                        await DelayTask(delay);
                        await UniTask.WhenAny(
                            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
                            , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
                            );
                    }
                    else
                    {
                        await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
                    }
                }
                //damage = InGameManager.Instance.Player.CheckCritical(card.Data.Damage);
                if (await card.TargetEnemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical)))
                {
                    Debug.Log(++_endTask);
                    break;
                }
                else
                {
                    Debug.Log(++_endTask);
                }
            }
        }
        cts.Cancel();
        cts.Dispose();
        card.Target(null);
        InGameManager.Instance.Player.CheckCritical();
        await CheckTaskCount(card).SuppressCancellationThrow();
        await CheckAllEndTask();
        Debug.Log(--_startTask);
    }
    async UniTask MultiAttackAB(Card card, float delay = 0.3f, int order = 0)              // 컨티뉴 multi랑 그냥 multi 합침.
    {
        Debug.Log(++_startTask);
        await CheckTaskOrder(order);
        if (EnemyManager.Instance.EnemyList.Count == 0)
        {
            Debug.Log(--_startTask);
            return;
        }
        //bool critical = InGameManager.Instance.Player.CheckCritical();
        bool critical = InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _);
        //int damage = InGameManager.Instance.Player.CheckCritical(card.Data.Damage);
        var enemyList = EnemyManager.Instance.EnemyList.ToList();            // 무조건 한 번은 실행되게 함. 이러면 카운트 1를 따로 작성해주지 않아도 상관없음.

        var cts = new CancellationTokenSource();

        card.UseTimingReset();
        if (card.AllEnemies)
        {
            //if (card.PlayEffect == null)
            //{
                //card.PlayEffect = UniTask.Lazy(async () => await PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3)));
            await UniTask.WhenAny(
                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
                );
            //}
            //else
            //{
            //    await UniTask.WhenAny(
            //        card.PlayEffect.Task
            //        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
            //        );
            //}

            await UniTask.WhenAll(enemyList.Select(async enemy =>
            {
                if (enemy != null)
                {
                    await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
                }
            }));
            Debug.Log(++_endTask);

            for (int i = 1; i < card.Data.Count; ++i)
            {
                if (await CheckTaskCount(card).SuppressCancellationThrow())
                {
                    break;
                }
                if (await CheckTaskOrder(order, false).SuppressCancellationThrow())
                {
                    break;
                }

                if (EnemyManager.Instance.EnemyList.Count == 0)
                {
                    break;
                }
                else
                {
                    enemyList = EnemyManager.Instance.EnemyList.ToList();
                }
                //card.UseTimingReset();
                if (card.Data.Effect != null)
                {
                    if (card.RepeatEffect)
                    {
                        await DelayTask(delay);
                        await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
                        );
                    }
                    else
                    {
                        await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
                    }
                }

                await UniTask.WhenAll(enemyList.Select(async enemy =>
                {
                    if (enemy != null)
                    {
                        await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
                    }
                }));
                Debug.Log(++_endTask);
            }
        }
        else
        {
            await UniTask.WhenAll(enemyList.Select(async enemy =>
            {
                if (enemy != null)
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
                        );
                    //await UniTask.WaitUntil(() => card.CardUseTiming);
                    await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
                }
            }));

            Debug.Log(++_endTask);

            for (int i = 1; i < card.Data.Count; ++i)
            {
                if (await CheckTaskCount(card).SuppressCancellationThrow())
                {
                    break;
                }
                if (await CheckTaskOrder(order, false).SuppressCancellationThrow())
                {
                    break;
                }
                if (EnemyManager.Instance.EnemyList.Count == 0)
                {
                    break;
                }
                else
                {
                    enemyList = EnemyManager.Instance.EnemyList.ToList();
                }
                //enemyList = EnemyManager.Instance.EnemyList.ToList();
                //card.UseTimingReset();
                await UniTask.WhenAll(enemyList.Select(async enemy =>
                {
                    if (enemy != null && card.Data.Effect != null)
                    {
                        if (card.RepeatEffect)
                        {
                            await DelayTask(delay);
                            await UniTask.WhenAny(
                                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
                                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
                                );
                        }
                        else
                        {
                            await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
                        }
                        await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
                    }
                    else if (enemy != null)
                    {
                        await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
                    }
                }));

                Debug.Log(++_endTask);
            }
        }
        //await UniTask.WhenAll(Enumerable.Range(0, enemyCount)
        //    .Select(async j =>
        //    {
        //        await PoolManager.Instance.GetEffect(card.Data.Effect, EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].transform.position, Quaternion.identity);
        //        await EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));

        //    }));

        //for (int i = /*0*/1; i < card.Data.Count; ++i)
        //{
        //    //if (i != 0)
        //    //{
        //    //    await DelayTask(continuousDelay);
        //    //}
        //    enemyList = EnemyManager.Instance.EnemyList.ToList();
        //    card.UseTimingReset();
        //    await UniTask.WhenAll(enemyList.Select(async enemy =>
        //    {
        //        if (enemy != null)
        //        {
        //            if (card.RepeatEffect)
        //            {
        //                await DelayTask(delay);
        //                await UniTask.WhenAny(
        //                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
        //                    , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
        //                    );
        //            }
        //            else
        //            {
        //                await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
        //            }
        //            await enemy.TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical));
        //        }
        //    }));
        //    //damage = InGameManager.Instance.Player.CheckCritical(card.Data.Damage);
        //    //enemyCount = EnemyManager.Instance.EnemyList.Count;
        //    //await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
        //    //    Select(j => EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].TakeDamage(InGameManager.Instance.Player.CheckCriticalDamage(card.Data.Damage, critical))));
        //}
        cts.Cancel();
        cts.Dispose();
        InGameManager.Instance.Player.CheckCritical();
        await CheckTaskCount(card).SuppressCancellationThrow();
        await CheckAllEndTask();
        Debug.Log(--_startTask);
    }
    async UniTask ShieldAB(Card card, float delay = 0.3f, int order = 0)
    {
        Debug.Log(++_startTask);
        await CheckTaskOrder(order);
        if (EnemyManager.Instance.EnemyList.Count == 0)
        {
            Debug.Log(--_startTask);
            return;
        }
        if (card.Data.Effect != null)       // effect가 널이 아닐 경우를 확인하지만, 공격 모션이나 특수 모션이 있을 경우를 확인하는 것이고, 모션이 있다면, 해당 모션을 기다린 후 실행. 아니면 쉴드 사용(사용 시, 전용 이펙트 실행) 
        {
            await UniTask.WaitUntil(() => card.CardUseTiming);
        }
        // 쉴드가 다른 공격, 드로우에 비해 시간이 짧아서 같이 쓰려면 무조건 이펙트가 있어야 함. 안 그러면 순서가 이상해질 수 있음.
        await InGameManager.Instance.Player.Shield(card.Data.Shield);
        Debug.Log(++_endTask);
        for (int i = 1; i < card.Data.Count; ++i)
        {
            if (await CheckTaskCount(card).SuppressCancellationThrow())
            {
                break;
            }
            if (await CheckTaskOrder(order, false).SuppressCancellationThrow())
            {
                break;
            }
            if (card.Data.Effect != null)
            {
                await UniTask.WaitUntil(() => card.CardUseTiming);
            }
            else
            {
                await DelayTask(delay);
            }
            await InGameManager.Instance.Player.Shield(card.Data.Shield);
            Debug.Log(++_endTask);
        }
        await CheckTaskCount(card).SuppressCancellationThrow();
        await CheckAllEndTask();
        Debug.Log(--_startTask);
    }
    async UniTask DrawAB(Card card, float delay = 0.3f, int order = 0)
    {
        Debug.Log(++_startTask);
        await CheckTaskOrder(order);
        if (EnemyManager.Instance.EnemyList.Count == 0)
        {
            Debug.Log(--_startTask);
            return;
        }
        if (card.Data.Effect != null)
        {
            await UniTask.WaitUntil(() => card.CardUseTiming);
        }
        await CardManager.Instance.DrawCard(card.Data.Draw);
        Debug.Log(++_endTask);
        for (int i = 1; i < card.Data.Count; ++i)
        {
            if (await CheckTaskCount(card).SuppressCancellationThrow())
            {
                break;
            }
            if (await CheckTaskOrder(order, false).SuppressCancellationThrow())
            {
                break;
            }
            if (card.Data.Effect != null)
            {
                await UniTask.WaitUntil(() => card.CardUseTiming);
            }
            else
            {
                await DelayTask(delay);
            }
            await CardManager.Instance.DrawCard(card.Data.Draw);
            Debug.Log(++_endTask);
        }
        await CheckTaskCount(card).SuppressCancellationThrow();
        await CheckAllEndTask();
        Debug.Log(--_startTask);
    }

    //async UniTask SingleAttackAB(Card card)
    //{
    //    await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage);        // 이 전 단계에서 null 검사를 하기 때문에 ?. 할 필요 없음.
    //    card.Target(null);                                         // missing 체크를 위한 거였으나.. 안 되나..??
    //    //Enemy enemy = EnemyManager.Instance.targetEnemy;
    //    //enemy.TakeDamageEnemy(card.Data.Damage);
    //    //if (!card.Enhanced)
    //    //    enemy.TakeDamageEnemy(card.Data.Damage);
    //    //else
    //    //    enemy.TakeDamageEnemy(card.Data.EnhancedDamage);
    //    //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
    //}

    //async UniTask SingleAttackAB(object obj)       // 나중에 다시 체크해봐야 할 듯. 잘 하면 id로도 사용 가능할 듯?
    //{
    //    Card card = obj as Card;
    //    await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage);
    //    card.Target(null);
    //}
    //async UniTask MultiAttackAB(Card card)
    //{
    //    int enemyCount = EnemyManager.Instance.EnemyList.Count;
    //    await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
    //        Select(i => EnemyManager.Instance.EnemyList[(enemyCount - 1) - i].TakeDamageEnemy(card.Data.Damage)));
    //    //for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
    //    //{
    //    //    await EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage);
    //    //}
    //    //foreach (Enemy enemy in EnemyManager.Instance.enemies)
    //    //{
    //    //    enemy.TakeDamageEnemy(card.Data.Damage);
    //    //}
    //    //if (!card.Enhanced)
    //    //{
    //    //    for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
    //    //    {
    //    //        EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage);
    //    //        //EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage).Forget();
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
    //    //    {
    //    //        EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.EnhancedDamage);
    //    //    }
    //    //}
    //}
    //async UniTask ContinuousSinglettackAB(Card card, float continuousDelay = 0.3f)
    //{
    //    //Enemy enemy = EnemyManager.Instance.targetEnemy;
    //    if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //    {
    //        card.Target(null);
    //        return;
    //    }
    //    for (int i = 1; i < card.Data.Count; ++i)
    //    {
    //        await DelayTask(continuousDelay);
    //        if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //        {
    //            card.Target(null);
    //            return;
    //        }
    //    }
    //    card.Target(null);
    //    //if (!card.Enhanced)
    //    //{
    //    //    if (enemy.TakeDamageEnemy(card.Data.Damage))
    //    //        return;
    //    //    //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
    //    //    for (int i = 1; i < card.Data.Count; i++)
    //    //    {
    //    //        await DelayTask(continuousDelay);
    //    //        if (enemy.TakeDamageEnemy(card.Data.Damage))
    //    //            return;
    //    //        //if (enemy != null)
    //    //            //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
    //    //        //DelayTask().ContinueWith(() =>
    //    //        //{
    //    //        //    if (enemy != null)
    //    //        //        enemy.TakeDamage(card.Data.Damage);
    //    //        //});
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    if (enemy.TakeDamageEnemy(card.Data.EnhancedDamage))
    //    //        return;
    //    //    for (int i = 1; i < card.Data.EnhancedCount; i++)
    //    //    {
    //    //        await DelayTask(continuousDelay);
    //    //        if (enemy.TakeDamageEnemy(card.Data.EnhancedDamage))
    //    //            return;
    //    //    }
    //    //}
    //}
    //async UniTask ContinuousSinglettackAB(object obj, float continuousDelay)
    //{
    //    Card card = obj as Card;
    //    //Enemy enemy = EnemyManager.Instance.targetEnemy;
    //    if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //        return;
    //    for (int i = 1; i < card.Data.Count; ++i)
    //    {
    //        await DelayTask(continuousDelay);
    //        if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //            return;
    //    }
    //    card.Target(null);
    //}

    //async UniTask ContinuousMultiAttackAB(Card card, float continuousDelay = 0.3f)
    //{
    //    await MultiAttackAB(card);
    //    for (int j = 1; j < /*(!card.Enhanced ? card.Data.Count : card.Data.EnhancedCount)*/card.Data.Count; ++j)
    //    {
    //        await DelayTask(continuousDelay);
    //        await MultiAttackAB(card);
    //        //DelayTask().ContinueWith(() =>    //ContinueWith() 사용시 UniTask가 종종 최대 15초까지 안 끝나는 오류 발생
    //        //{
    //        //    MultiAttackAB(card);
    //        //});
    //    }
    //}
    //async UniTask DrawAB(/*Card card*/)
    //{
    //    //TurnManager.Instance.DrawTask().Forget();
    //    await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
    //}
    async UniTask AfterDrawAB(Card card)
    {
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.SetCardState(1);
        //await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
        await DrawAB(card);
        CardManager.Instance.SetCardState(2);
    }

    //async UniTask ContinuousDrawAB(Card card)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    //{
    //    //TurnManager.Instance.DrawTask(card.Data.Draw).Forget();
    //    await CardManager.Instance.DrawCard(card.Data.Draw);
    //    //await CardManager.Instance.DrawCards(card.Data.Draw);
    //    //if (!card.Enhanced)
    //    //    CardManager.Instance.DrawCards(card.Data.Draw).Forget();
    //    //else
    //    //    CardManager.Instance.DrawCards(card.Data.EnhancedDraw).Forget();
    //}
    //async UniTask ShieldAB(Card card)
    //{
    //    await InGameManager.Instance.player.Shield(card.Data.Shield);
    //    //if (!card.Enhanced)
    //    //    InGameManager.Instance.player.Shield(card.Data.Shield);
    //    //else
    //    //    InGameManager.Instance.player.Shield(card.Data.EnhancedDefence);
    //}

    async UniTask ConfirmedDiscardAB()
    {
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeDiscard(true);
        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionDiscardAB()
    {
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
        await UniTask.WhenAny(task1, task2, task3);
        cts.Cancel();
        //await UniTask.WhenAny(
        //    UniTask.Create(async () =>
        //    {
        //        await ButtonManager.Instance.DiscardButton.OnClickAsync();
        //        CardManager.Instance.ThrowAwaySelectedCard().Forget();
        //        discarded = true;


        //    }),
        //    UniTask.Create(async () =>
        //    {
        //        await ButtonManager.Instance.DiscardCancelButton.OnClickAsync();
        //        CardManager.Instance.ReturnSelectedCard();
        //        discarded = false;

        //    })
        //    );
        CardManager.Instance.ChangeDiscard(false);
        return discarded;

    }

    async UniTask ConfirmedRemoveAB()
    {
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeRemove(true);
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionRemoveAB()
    {
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
