using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemAbility
{

    public void SetPassiveItemAbility(Item item)
    {
        switch (item.Data.ID)
        {
            case 1:
                TurnManager.Instance.AddStartCardCount(1);
                break;
            case 2:
                InGameManager.Instance.player.AddMaxHealth(25);
                break;
            case 3:
                InGameManager.Instance.player.AddMaxHolo(1);
                break;
            case 4:
                InGameUIManager.Instance.ChangeRewardCardCount(true);
                break;
            //case 501:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await SingleAttackAB(item);
            //        await DrawAB();
            //    });
            //    break;
            //case 502:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await ShieldAB(item);
            //    });
            //    break;
            //case 503:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await HealAB(item);
            //    });
            //    break;
            //case 504:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await ContinuousDrawAB(item);
            //    });
            //    break;
            //case 1001:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await SingleAttackAB(item);
            //    });
            //    //InGameManager.Instance.player.Heal(10).Forget();
            //    //_potionBtns[idx].onClick.RemoveAllListeners();
            //    //_boolPotion[idx] = false;
            //    break;
            //case 1002:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        InGameManager.Instance.ChangeCoinValue(20);
            //    });
            //    //InGameManager.Instance.ChangeCoinValue(20);
            //    //_potionBtns[idx].onClick.RemoveAllListeners();
            //    //_boolPotion[idx] = false;
            //    break;
            //case 1003:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await ShieldAB(item);
            //    });
            //    //if (TurnManager.Instance.MyTurn)
            //    //{
            //    //    InGameManager.Instance.player.Shield(15).Forget();
            //    //    _potionBtns[idx].onClick.RemoveAllListeners();
            //    //    _boolPotion[idx] = false;
            //    //}
            //    break;
            //case 1004:
            //    item.ItemTask = () => UniTask.Create(async () =>
            //    {
            //        await DelayTask(0.5f);
            //        await SingleAttackAB(item);
            //    });
            //    //if (TurnManager.Instance.MyTurn)
            //    //{
            //    //    BattleManager.Instance.SetActiveArrowCursor(true, 2);
            //    //    //PotionBtns[idx].onClick.RemoveAllListeners();
            //    //    //boolPotion[idx] = false;
            //    //}
            //    break;
            //default:
            //    item.ItemTask = () => UniTask.CompletedTask;
            //    break;
        }
    }

    public void SetActiveItemAbility(ActiveItem item)
    {
        switch (item.Data.ID)
        {
            case 501:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAB(item);
                    await DrawAB();
                });
                break;
            case 502:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await ShieldAB(item);
                });
                break;
            case 503:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await HealAB(item);
                });
                break;
            case 504:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await ContinuousDrawAB(item);
                });
                break;
        }
    }

    public void SetPotionItemAbility(PotionItem item)
    {
        switch (item.Data.ID)
        {
            case 1001:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAB(item);
                });
                break;
            case 1002:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    InGameManager.Instance.ChangeCoinValue(20);
                });
                break;
            case 1003:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await ShieldAB(item);
                });
                break;
            case 1004:
                item.ItemTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    await SingleAttackAB(item);
                });
                break;
        }
    }

    async UniTask SingleAttackAB(UseItem item)
    {
        await item.TargetEnemy.TakeDamage(item.Data.Damage);        // 이 전 단계에서 null 검사를 하기 때문에 ?. 할 필요 없음.
        item.Target(null);                                         // missing 체크를 위한 거였으나.. 안 되나..??
    }
    async UniTask MultiAttackAB(UseItem item)
    {
        int enemyCount = EnemyManager.Instance.EnemyList.Count;
        await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
            Select(i => EnemyManager.Instance.EnemyList[(enemyCount - 1) - i].TakeDamage(item.Data.Damage)));
    }
    //async UniTask ContinuousSinglettackAB(Card card, float delay)
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
    //async UniTask ContinuousMultiAttackAB(Card card, float delay)
    //{
    //    await MultiAttackAB(card);
    //    for (int j = 1; j < card.Data.Count; ++j)
    //    {
    //        await DelayTask(delay);
    //        await MultiAttackAB(card);
    //    }
    //}
    async UniTask DrawAB()
    {
        await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
    }

    async UniTask ContinuousDrawAB(UseItem item)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    {
        await CardManager.Instance.DrawCard(item.Data.Draw);
    }
    async UniTask ShieldAB(UseItem item)
    {
        await InGameManager.Instance.player.Shield(item.Data.Shield);
    }

    async UniTask HealAB(UseItem item)
    {
        await InGameManager.Instance.player.Heal(item.Data.Heal);
    }

    //async UniTask<bool> ConditionDiscardAB(int discardCnt)
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
        await UniTask.WaitForSeconds(delay/*, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token*/);
    }
}
