using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemAbility : MonoBehaviour
{

    public void SetItemAbility(Item item)
    {
        //UniTask cardActTask;
        switch (item.Data.Id)       // Defer => await 한 번일 때 유용, Lazy => await 여러 번일 때 유용
        {
            case 100:
                item.ItemTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAb(item);
                });
                break;
            case 101:
                item.ItemTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAb(item);
                    await DrawAb();
                });
                break;
            case 102:
                item.ItemTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ContinuousDrawAb(item);
                });
                break;
            case 103:
                item.ItemTask = UniTask.Defer(async () =>
                {
                    await DelayTask(0.5f);
                    await ShieldAb(item);
                });
                break;
            default:
                item.ItemTask = UniTask.CompletedTask;
                break;
        }
    }

    async UniTask SingleAttackAb(Item item)
    {
        await item.TargetEnemy.TakeDamageEnemy(item.Data.Damage);        // 이 전 단계에서 null 검사를 하기 때문에 ?. 할 필요 없음.
        item.Target(null);                                         // missing 체크를 위한 거였으나.. 안 되나..??
    }
    async UniTask MultiAttackAb(Item item)
    {
        int enemyCount = EnemyManager.Instance.EnemyList.Count;
        await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
            Select(i => EnemyManager.Instance.EnemyList[(enemyCount - 1) - i].TakeDamageEnemy(item.Data.Damage)));
    }
    //async UniTask ContinuousSinglettackAb(Card card, float delay)
    //{
    //    if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //        return;
    //    for (int i = 1; i < card.Data.Count; ++i)
    //    {
    //        await DelayTask(delay);
    //        if (await card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
    //            return;
    //    }
    //    card.Target(null);
    //}
    //async UniTask ContinuousMultiAttackAb(Card card, float delay)
    //{
    //    await MultiAttackAb(card);
    //    for (int j = 1; j < card.Data.Count; ++j)
    //    {
    //        await DelayTask(delay);
    //        await MultiAttackAb(card);
    //    }
    //}
    async UniTask DrawAb()
    {
        await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
    }

    async UniTask ContinuousDrawAb(Item item)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    {
        await CardManager.Instance.DrawCard(item.Data.Draw);
    }
    async UniTask ShieldAb(Item item)
    {
        await InGameManager.Instance.player.Shield(item.Data.Shield);
    }

    //async UniTask<bool> DiscardAb(int discardCnt)
    //{
    //    bool discarded = false;
    //    ButtonManager.Instance.DiscardBtnInvert(false);
    //    CardManager.Instance.ChangeDiscard(true);
    //    await UniTask.WhenAny(
    //        UniTask.Create(async () =>
    //        {
    //            await ButtonManager.Instance.DiscardButton.OnClickAsync();
    //            CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //            discarded = true;


    //        }),
    //        UniTask.Create(async () =>
    //        {
    //            await ButtonManager.Instance.DiscardCancelButton.OnClickAsync();
    //            CardManager.Instance.ReturnSelectedCard();
    //            discarded = false;

    //        })
    //        );
    //    CardManager.Instance.ChangeDiscard(false);
    //    return discarded;

    //}

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    }
}
