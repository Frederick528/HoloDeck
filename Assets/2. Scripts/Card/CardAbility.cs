using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbility
{
    public Action<Card> cardAction;
    //public Dictionary<CardTag, Action<int>> cardAction = new()
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
            case 1000:
                cardAction = SingleAttack;
                break;
            case 2000:
                cardAction = (card) => ContinuousMultiAttack(card).Forget();
                break;
            case 3000:
                cardAction = ContinuousDrawSkill;
                break;
            case 1001:
                cardAction = (card) =>
                {
                    SingleAttack(card);
                    DrawSkill(card);
                };
                break;
            case 1002:
                cardAction = (card) => ContinuousSinglettack(card).Forget();
                break;
            default: cardAction = null; break;
        }
        return cardAction;
    }

    void SingleAttack(Card card)
    {
        Enemy enemy = EnemyManager.Instance.targetEnemy;
        enemy.TakeDamage(card.Data.damage);
    }
    void MultiAttack(Card card)
    {
        for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
        {
            EnemyManager.Instance.enemies[i].TakeDamage(card.Data.damage);
        }
    }
    async UniTaskVoid ContinuousSinglettack(Card card)
    {
        Enemy enemy = EnemyManager.Instance.targetEnemy;
        enemy.TakeDamage(card.Data.damage);
        for (int i = 1; i < card.Data.count; i++)
        {
            await DelayTask();
            if (enemy != null)
                enemy.TakeDamage(card.Data.damage);
            //DelayTask().ContinueWith(() =>
            //{
            //    if (enemy != null)
            //        enemy.TakeDamage(card.Data.damage);
            //});
        }
    }
    async UniTaskVoid ContinuousMultiAttack(Card card)
    {
        MultiAttack(card);
        for (int j = 1; j < card.Data.count; j++)
        {
            await DelayTask();
            MultiAttack(card);
            //DelayTask().ContinueWith(() =>    //ContinueWith() 사용시 UniTask가 종종 최대 15초까지 안 끄나는 오류 발생
            //{
            //    MultiAttack(card);
            //});
        }
    }
    void DrawSkill(Card card)
    {
        TurnManager.Instance.DrawTask().Forget();
    }

    void ContinuousDrawSkill(Card card)
    {
        TurnManager.Instance.DrawTask(card.Data.draw).Forget();
    }
    void DefenceSkill(Card card)
    {

    }

    async UniTask DelayTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
    }
}
