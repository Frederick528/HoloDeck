using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardAbility
{
    public Action<Card> cardAction;
    const float attackSpeed = 0.3f;

    //public CancellationTokenSource CancelSource = new CancellationTokenSource();
    //public Dictionary<CardTag, Action<int>> CardAction = new()
    //{
    //    {CardTag.SingleAttack, CardManager.Instance.targetEnemy.TakeDamage},
    //    {CardTag.MultiAttack},
    //    {CardTag.Skill, null}
    //};

    //public void MultiAttack(int dmg)
    //{
    //    for (int i = 0; i < EnemyManager.Instance.enemies.Count; i++)
    //    {
    //        EnemyManager.Instance.enemies[i].TakeDamage(dmg);
    //    }
    //}


    public Action<Card> SetCardAbility(int id)
    {
        switch (id)
        {
            case 100:
                cardAction += SingleAttack;
                break;
            case 101:
                cardAction += SingleAttack;
                cardAction += DrawSkill;
                break;
            case 102:
                cardAction += async (card) => {
                    await DelayTask(1f);
                    ContinuousSinglettack(card, 0.3f).Forget();
                };
                break;
            case 103:
                cardAction += (card) => ContinuousMultiAttack(card, 0.3f).Forget();
                break;
            case 104: 
                cardAction += ContinuousDrawSkill;
                break;
            case 105:
                cardAction += DefenceSkill;
                break;
            default: cardAction = null; break;
        }
        return cardAction;
    }

    void SingleAttack(Card card)
    {
        card.TargetEnemy.TakeDamageEnemy(card.Data.Damage);        // 이 전 단계에서 null 검사를 하기 때문에 ?. 할 필요 없음.
        card.Target(null);
        //Enemy enemy = EnemyManager.Instance.targetEnemy;
        //enemy.TakeDamageEnemy(card.Data.Damage);
        //if (!card.Enhanced)
        //    enemy.TakeDamageEnemy(card.Data.Damage);
        //else
        //    enemy.TakeDamageEnemy(card.Data.EnhancedDamage);
        //enemy.TakeDamageEnemy(card.Data.Damage).Forget();
    }
    void MultiAttack(Card card)
    {
        for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
        {
            EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.Damage);
        }
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
    async UniTaskVoid ContinuousSinglettack(Card card, float delay)
    {
        //Enemy enemy = EnemyManager.Instance.targetEnemy;
        if (card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
            return;
        for (int i = 1; i < card.Data.Count; ++i)
        {
            await DelayTask(delay);
            if (card.TargetEnemy.TakeDamageEnemy(card.Data.Damage))
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
    async UniTaskVoid ContinuousMultiAttack(Card card, float delay)
    {
        MultiAttack(card);
        for (int j = 1; j < /*(!card.Enhanced ? card.Data.Count : card.Data.EnhancedCount)*/card.Data.Count; ++j)
        {
            await DelayTask(delay);
            MultiAttack(card);
            //DelayTask().ContinueWith(() =>    //ContinueWith() 사용시 UniTask가 종종 최대 15초까지 안 끝나는 오류 발생
            //{
            //    MultiAttack(card);
            //});
        }
    }
    void DrawSkill(Card card)
    {
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.AddCard().Forget();
    }

    void ContinuousDrawSkill(Card card)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    {
        //TurnManager.Instance.DrawTask(card.Data.Draw).Forget();
        CardManager.Instance.AddCards(card.Data.Draw).Forget();
        //if (!card.Enhanced)
        //    CardManager.Instance.AddCards(card.Data.Draw).Forget();
        //else
        //    CardManager.Instance.AddCards(card.Data.EnhancedDraw).Forget();
    }
    void DefenceSkill(Card card)
    {
        GameManager.Instance.player.Defence(card.Data.Defence);
        //if (!card.Enhanced)
        //    GameManager.Instance.player.Defence(card.Data.Defence);
        //else
        //    GameManager.Instance.player.Defence(card.Data.EnhancedDefence);
    }

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    }
}
