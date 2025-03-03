using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItem : Item
{
    public Func<UniTask> ItemTask;
    public Enemy TargetEnemy { get; private set; } = null;        // 아이템 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    public void CheckEnemyDead()
    {
        switch (Data.AttackType)
        {
            case AttackType.Single:
                TargetEnemy.CheckIfDead(Data.Damage, 1);
                break;
            case AttackType.Multi:
                foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
                {
                    enemy.CheckIfDead(Data.Damage, 1);
                }
                break;
            default: break;
        }
    }
    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }
    public async virtual UniTask UseTask()
    {
        await ItemTask();
    }
}
