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
    //    {CardTag.SingleAttackAb, CardManager.Instance.targetEnemy.TakeDamage},
    //    {CardTag.MultiAttackAb},
    //    {CardTag.Skill, null}
    //};

    //public void MultiAttackAb(int dmg)
    //{
    //    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
    //    {
    //        EnemyManager.Instance.enemies[i].TakeDamage(dmg);
    //    }
    //}

    public void SetCardAbility(Card card)
    {
        //UniTask cardActTask;
        switch (card.Data.Id)       // Defer => await 한 번일 때 유용, Lazy => await 여러 번일 때 유용
        {
            case 100:
                card.UseConditions = UniTask.Defer(async () =>
                {
                    return await DiscardAb(2);
                });
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAb(card);
                });
                //return SingleAttackAb(card);
                //cardActTask = UniTask.Defer(async () => 
                //{
                //    await DelayTask(0.5f);
                //    SingleAttackAb(card);
                //}/*, true, TurnManager.Instance.CancelSource.Token*/);      // false면 await 이후 코드가 스레드 풀에서 진행된다고 하는데, 아직 정확히는 모르겠어서 이건 일단 좀 더 공부해봐야 할 듯.
                break;
            case 101:
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAb(card);
                    await DrawAb();
                });

                //cardActTask = UniTask.Defer(async () =>
                //{
                //    await DelayTask(0.5f);
                //    SingleAttackAb(card);
                //    await DrawAb();

                //    //await UniTask.WhenAll(                    // WhenAll은 특정 상황에서만 사용
                //    //    UniTask.Create(async () =>
                //    //    {
                //    //        await DelayTask(1f);
                //    //        SingleAttackAb(card);
                //    //    }),
                //    //    UniTask.Create(async () =>
                //    //    {
                //    //        await DelayTask(1f);
                //    //        await DrawAb();
                //    //    }));


                //    //DrawAb(/*card*/)
                //});
                break;
            case 102:
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ContinuousSinglettackAb(card, 0.3f);
                });
                break;
            case 103:
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ContinuousMultiAttackAb(card, 0.3f);
                });
                break;
            case 104:
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ContinuousDrawAb(card);
                });
                break;
            case 105:
                card.CardTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ShieldAb(card);
                });
                break;
            default:
                card.CardTask = UniTask.CompletedTask;
                break;
        }
        //return cardActTask;
    }
    //public AsyncLazy SetCardLazyAbility(Card card)
    //{
    //    AsyncLazy cardLazy;
    //    switch (card.Data.Id)       // Defer => await 한 번일 때 유용, Lazy => await 여러 번일 때 유용
    //    {
    //        case 100:
    //            //return SingleAttackAb(card);
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAb(card);
    //            }/*, true, TurnManager.Instance.CancelSource.Token*/);      // false면 await 이후 코드가 스레드 풀에서 진행된다고 하는데, 아직 정확히는 모르겠어서 이건 일단 좀 더 공부해봐야 할 듯.
    //            break;
    //        case 101:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAb(card);
    //                await DrawAb();

    //                //await UniTask.WhenAll(
    //                //    UniTask.Create(async () =>
    //                //    {
    //                //        await DelayTask(1f);
    //                //        SingleAttackAb(card);
    //                //    }),
    //                //    UniTask.Create(async () =>
    //                //    {
    //                //        await DelayTask(1f);
    //                //        await DrawAb();
    //                //    }));


    //                //DrawAb(/*card*/)
    //            });
    //            break;
    //        case 102:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousSinglettackAb(card, 0.3f);
    //            });
    //            break;
    //        case 103:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousMultiAttackAb(card, 0.3f);
    //            });
    //            break;
    //        case 104:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await ContinuousDrawAb(card);
    //            });
    //            break;
    //        case 105:
    //            cardLazy = UniTask.Lazy(async () =>
    //            {
    //                await DelayTask(1f);
    //                ShieldAb(card);
    //            });
    //            break;
    //        default: cardLazy = UniTask.Lazy(() => UniTask.CompletedTask); break;
    //    }
    //    return cardLazy;
    //}
    //public Action SetCardActionAbility(Card card)
    //{
    //    switch (card.Data.Id)
    //    {
    //        case 100:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAb(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 101:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                SingleAttackAb(card);
    //                await DrawAb(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 102:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                await ContinuousSinglettackAb(card, 0.3f);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 103:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await ContinuousMultiAttackAb(card, 0.3f);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 104:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await ContinuousDrawAb(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DrawAb(card);
    //                CardManager.Instance._eventQueue.DoNext().Forget();
    //            });
    //            break;
    //        case 105:
    //            cardAction += UniTask.Action(async () =>
    //            {
    //                await DelayTask(1f);
    //                ShieldAb(card);
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
    //            cardActTask = (card) => UniTask.WhenAll(SingleAttackAb(card));
    //            //await UniTask.RunOnThreadPool((cardAction));
    //            cardAction += SingleAttackAb;
    //            break;
    //        case 101:
    //            cardAction += SingleAttackAb;
    //            cardAction += async (card) => await DrawAb(card);
    //            break;
    //        case 102:
    //            cardAction += async (card) =>
    //            {
    //                await DelayTask(1f);
    //                await ContinuousSinglettackAb(card, 0.3f);
    //            };
    //            break;
    //        case 103:
    //            cardAction += async (card) => await ContinuousMultiAttackAb(card, 0.3f);
    //            break;
    //        case 104:
    //            cardAction += async (card) => await ContinuousDrawAb(card);
    //            break;
    //        case 105:
    //            cardAction += ShieldAb;
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
    //            cardAction += SingleAttackAb;
    //            break;
    //        case 101:
    //            cardAction += SingleAttackAb;
    //            cardAction += async (card) => await DrawAb(card);
    //            break;
    //        case 102:
    //            cardAction += async (card) =>
    //            {
    //                await DelayTask(1f);
    //                ContinuousSinglettackAb(card, 0.3f).Forget();
    //            };
    //            break;
    //        case 103:
    //            cardAction += (card) => ContinuousMultiAttackAb(card, 0.3f).Forget();
    //            break;
    //        case 104:
    //            cardAction += async (card) => await ContinuousDrawAb(card);
    //            //cardAction += UniTaskHelper.Action<Card>(DrawAb);
    //            break;
    //        case 105:
    //            cardAction += ShieldAb;
    //            break;
    //        default: cardAction = null; break;
    //    }
    //    return cardAction;
    //}

    async UniTask SingleAttackAb(Card card)
    {
        await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage);        // 이 전 단계에서 null 검사를 하기 때문에 ?. 할 필요 없음.
        card.Target(null);                                         // missing 체크를 위한 거였으나.. 안 되나..??
        //Enemy enemy = EnemyManager.Instance.targetEnemy;
        //enemy.TakeDamageEnemy(card.Data.Damage);
        //if (!card.Enhanced)
        //    enemy.TakeDamageEnemy(card.Data.Damage);
        //else
        //    enemy.TakeDamageEnemy(card.Data.EnhancedDamage);
        //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
    }

    async UniTask SingleAttackAb(object obj)       // 나중에 다시 체크해봐야 할 듯. 잘 하면 id로도 사용 가능할 듯?
    {
        Card card = obj as Card;
        await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage);
        card.Target(null);
    }
    async UniTask MultiAttackAb(Card card)
    {
        int enemyCount = EnemyManager.Instance.EnemyList.Count;
        await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
            Select(i => EnemyManager.Instance.EnemyList[(enemyCount - 1) - i].TakeDamageEnemy(card.Data.Damage)));
        //for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
        //{
        //    await EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage);
        //}
        //foreach (Enemy enemy in EnemyManager.Instance.enemies)
        //{
        //    enemy.TakeDamageEnemy(card.Data.Damage);
        //}
        //if (!card.Enhanced)
        //{
        //    for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
        //    {
        //        EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage);
        //        //EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage).Forget();
        //    }
        //}
        //else
        //{
        //    for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
        //    {
        //        EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.EnhancedDamage);
        //    }
        //}
    }
    async UniTask ContinuousSinglettackAb(Card card, float delay)
    {
        //Enemy enemy = EnemyManager.Instance.targetEnemy;
        if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
            return;
        for (int i = 1; i < card.Data.Count; ++i)
        {
            await DelayTask(delay);
            if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
                return;
        }
        card.Target(null);
        //if (!card.Enhanced)
        //{
        //    if (enemy.TakeDamageEnemy(card.Data.Damage))
        //        return;
        //    //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
        //    for (int i = 1; i < card.Data.Count; i++)
        //    {
        //        await DelayTask(delay);
        //        if (enemy.TakeDamageEnemy(card.Data.Damage))
        //            return;
        //        //if (enemy != null)
        //            //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
        //        //DelayTask().ContinueWith(() =>
        //        //{
        //        //    if (enemy != null)
        //        //        enemy.TakeDamage(card.Data.Damage);
        //        //});
        //    }
        //}
        //else
        //{
        //    if (enemy.TakeDamageEnemy(card.Data.EnhancedDamage))
        //        return;
        //    for (int i = 1; i < card.Data.EnhancedCount; i++)
        //    {
        //        await DelayTask(delay);
        //        if (enemy.TakeDamageEnemy(card.Data.EnhancedDamage))
        //            return;
        //    }
        //}
    }
    async UniTask ContinuousSinglettackAb(object obj, float delay)
    {
        Card card = obj as Card;
        //Enemy enemy = EnemyManager.Instance.targetEnemy;
        if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
            return;
        for (int i = 1; i < card.Data.Count; ++i)
        {
            await DelayTask(delay);
            if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
                return;
        }
        card.Target(null);
    }

    async UniTask ContinuousMultiAttackAb(Card card, float delay)
    {
        await MultiAttackAb(card);
        for (int j = 1; j < /*(!card.Enhanced ? card.Data.Count : card.Data.EnhancedCount)*/card.Data.Count; ++j)
        {
            await DelayTask(delay);
            await MultiAttackAb(card);
            //DelayTask().ContinueWith(() =>    //ContinueWith() 사용시 UniTask가 종종 최대 15초까지 안 끝나는 오류 발생
            //{
            //    MultiAttackAb(card);
            //});
        }
    }
    async UniTask DrawAb(/*Card card*/)
    {
        //TurnManager.Instance.DrawTask().Forget();
        await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
    }

    async UniTask ContinuousDrawAb(Card card)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    {
        //TurnManager.Instance.DrawTask(card.Data.Draw).Forget();
        await CardManager.Instance.DrawCard(card.Data.Draw);
        //await CardManager.Instance.DrawCards(card.Data.Draw);
        //if (!card.Enhanced)
        //    CardManager.Instance.DrawCards(card.Data.Draw).Forget();
        //else
        //    CardManager.Instance.DrawCards(card.Data.EnhancedDraw).Forget();
    }
    async UniTask ShieldAb(Card card)
    {
        await InGameManager.Instance.player.Shield(card.Data.Shield);
        //if (!card.Enhanced)
        //    InGameManager.Instance.player.Shield(card.Data.Shield);
        //else
        //    InGameManager.Instance.player.Shield(card.Data.EnhancedDefence);
    }

    async UniTask<bool> DiscardAb(int discardCnt)
    {
        bool discarded = false;
        ButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeDiscard(true);
        await UniTask.WhenAny(
            UniTask.Create(async () =>
            {
                await ButtonManager.Instance.DiscardButton.OnClickAsync();
                CardManager.Instance.ThrowAwaySelectedCard().Forget();
                discarded = true;


            }),
            UniTask.Create(async () =>
            {
                await ButtonManager.Instance.DiscardCancelButton.OnClickAsync();
                CardManager.Instance.ReturnSelectedCard();
                discarded = false;

            })
            );
        CardManager.Instance.ChangeDiscard(false);
        return discarded;

    }

    void RemoveAb(Card card)
    {
        CardManager.Instance.ChangeRemove(true);
        CardManager.Instance.SetCardState(3);   // Click
    }

    void ReduceHpAb(Card card)
    {

    }

    void HealAb(Card card)
    {

    }

    void CureAb(Card card)
    {

    }

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    }
}
