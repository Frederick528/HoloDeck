using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbility
{
    public Action<Card> cardAction;
    const float attackSpeed = 0.3f;
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
            case 100:
                cardAction += SingleAttack;
                break;
            case 101:
                cardAction += SingleAttack;
                cardAction += DrawSkill;
                break;
            case 102:
                cardAction += (card) => ContinuousSinglettack(card).Forget();
                break;
            case 103:
                cardAction += (card) => ContinuousMultiAttack(card).Forget();
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
        Enemy enemy = EnemyManager.Instance.targetEnemy;
        if (!card.enhanced)
            enemy.TakeDamageEnemy(card.Data.damage);
        else
            enemy.TakeDamageEnemy(card.Data.enhancedDamage);
        //enemy.TakeDamageEnemy(card.Data.damage).Forget();
    }
    void MultiAttack(Card card)
    {
        if (!card.enhanced)
        {
            for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
            {
                EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.damage);
                //EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.damage).Forget();
            }
        }
        else
        {
            for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; i--)
            {
                EnemyManager.Instance.enemies[i].TakeDamageEnemy(card.Data.enhancedDamage);
            }
        }
    }
    async UniTaskVoid ContinuousSinglettack(Card card)
    {
        Enemy enemy = EnemyManager.Instance.targetEnemy;
        if (!card.enhanced)
        {
            if (enemy.TakeDamageEnemy(card.Data.damage))
                return;
            //enemy.TakeDamageEnemy(card.Data.damage).Forget();
            for (int i = 1; i < card.Data.count; i++)
            {
                await DelayTask();
                if (enemy.TakeDamageEnemy(card.Data.damage))
                    return;
                //if (enemy != null)
                    //enemy.TakeDamageEnemy(card.Data.damage).Forget();
                //DelayTask().ContinueWith(() =>
                //{
                //    if (enemy != null)
                //        enemy.TakeDamage(card.Data.damage);
                //});
            }
        }
        else
        {
            if (enemy.TakeDamageEnemy(card.Data.enhancedDamage))
                return;
            for (int i = 1; i < card.Data.enhancedCount; i++)
            {
                await DelayTask();
                if (enemy.TakeDamageEnemy(card.Data.enhancedDamage))
                    return;
            }
        }
    }
    async UniTaskVoid ContinuousMultiAttack(Card card)
    {
        MultiAttack(card);
        for (int j = 1; j < (!card.enhanced ? card.Data.count : card.Data.enhancedCount); j++)
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
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.AddCard().Forget();
    }

    void ContinuousDrawSkill(Card card)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.count 값이 아닌 Data.draw 값으로 얼마나 뽑을지 정함.
    {
        //TurnManager.Instance.DrawTask(card.Data.draw).Forget();
        if (!card.enhanced)
            CardManager.Instance.AddCards(card.Data.draw).Forget();
        else
            CardManager.Instance.AddCards(card.Data.enhancedDraw).Forget();
    }
    void DefenceSkill(Card card)
    {
        if (!card.enhanced)
            GameManager.Instance.player.Defence(card.Data.defence);
        else
            GameManager.Instance.player.Defence(card.Data.enhancedDefence);
    }

    async UniTask DelayTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(attackSpeed));
    }
}
