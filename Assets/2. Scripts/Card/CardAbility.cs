using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardAbility
{
    //public UniTask Task;
    //public UniTask<Action<Card>> cardActionTask;
    CancellationTokenSource _cts;

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
        if (card.Data.HasCondition)
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
                    card.CardTask = () => UniTask.Create(async () =>
                    {
                        await DelayTask(0.5f);
                        await AfterDrawAB();
                        await ConfirmedDiscardAB();
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
                        await SingleAttackAB(card, continuousDelay);
                        await UniTask.WhenAll
                        (
                            ShieldAB(card, continuousDelay),
                            DrawAB(card)
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
                        await MultiAttackAB(card, continuousDelay);
                        await UniTask.WhenAll
                        (
                            ShieldAB(card, continuousDelay),
                            DrawAB(card)
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
    async UniTask SingleAttackAB(Card card, float delay = 0.3f)             // 컨티뉴 single이랑 그냥 single 합침.
    {
        int damage = InGameManager.Instance.player.CheckCritical(card.Data.Damage);
        if (!await card.TargetEnemy.TakeDamageEnemy(damage) && card.Data.Count > 1)
        {
            for (int i = 1; i < card.Data.Count; ++i)
            {
                await DelayTask(delay);
                if (await card.TargetEnemy.TakeDamageEnemy(damage))
                    break;
            }
        }
        card.Target(null);
    }
    async UniTask MultiAttackAB(Card card, float delay = 0.3f)              // 컨티뉴 multi랑 그냥 multi 합침.
    {
        int damage = InGameManager.Instance.player.CheckCritical(card.Data.Damage);
        int enemyCount1 = EnemyManager.Instance.EnemyList.Count;            // 무조건 한 번은 실행되게 함. 이러면 카운트 1를 따로 작성해주지 않아도 상관없음.
        await UniTask.WhenAll(Enumerable.Range(0, enemyCount1).
            Select(j => EnemyManager.Instance.EnemyList[(enemyCount1 - 1) - j].TakeDamageEnemy(damage)));
        for (int i = /*0*/1; i < card.Data.Count; ++i)
        {
            //if (i != 0)
            //{
            //    await DelayTask(continuousDelay);
            //}
            await DelayTask(delay);
            int enemyCount2 = EnemyManager.Instance.EnemyList.Count;
            await UniTask.WhenAll(Enumerable.Range(0, enemyCount2).
                Select(j => EnemyManager.Instance.EnemyList[(enemyCount2 - 1) - j].TakeDamageEnemy(damage)));
        }
    }
    async UniTask ShieldAB(Card card, float delay = 0.3f)
    {
        await InGameManager.Instance.player.Shield(card.Data.Shield);
        for (int i = 1; i < card.Data.Count; ++i)
        {
            await DelayTask(delay);
            await InGameManager.Instance.player.Shield(card.Data.Shield);
        }
    }
    async UniTask DrawAB(Card card)
    {
        await CardManager.Instance.DrawCard(card.Data.Draw);
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
    async UniTask AfterDrawAB(/*Card card*/)
    {
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.SetCardState(1);
        await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
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
        _cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(_cts.Token);
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
            discarded = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(_cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        await UniTask.WhenAny(task1, task2);
        _cts.Cancel();
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
        _cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(_cts.Token);
            //CardManager.Instance.ThrowAwaySelectedCard().Forget();        // 제거하는 코드로 변경
            removed = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(_cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            removed = false;
        });
        await UniTask.WhenAny(task1, task2);
        _cts.Cancel();
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
        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    }
}
