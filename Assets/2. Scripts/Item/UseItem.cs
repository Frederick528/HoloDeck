using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItem : Item
{
    protected int _damage;
    protected int _shield;
    protected int _draw;
    protected int _heal;
    protected int _duration;
    protected AttackType _attackType;
    protected ItemCanUse _itemCanUse;

    public Func<UniTask> ItemTask;

    public int Damage { get; protected set; }
    public int Shield { get; protected set; }
    public int Draw { get; protected set; }
    public int Heal { get; protected set; }
    public int Duration { get; protected set; }
    public AttackType AttackType { get; protected set; }
    public ItemCanUse ItemCanUse { get; protected set; }

    public Enemy TargetEnemy { get; protected set; } = null;        // 아이템 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    //public void CheckEnemyDead()
    //{
    //    switch (Data.AttackType)
    //    {
    //        case AttackType.Single:
    //            TargetEnemy.CheckIfDead(Data.Damage, 1);
    //            break;
    //        case AttackType.Multi:
    //            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //            {
    //                enemy.CheckIfDead(Data.Damage, 1);
    //            }
    //            break;
    //        default: break;
    //    }
    //}

    public override void Setup<T>(T data)
    {
        base.Setup(data);
        if (_defaultData is UseItemBase useItemData)
        {
            _damage = useItemData.Damage;
            _shield = useItemData.Shield;
            _draw = useItemData.Draw;
            _heal = useItemData.Heal;
            _duration = useItemData.Duration;
            _attackType = useItemData.AttackType;
            _itemCanUse = useItemData.ItemCanUse;
        }
        Damage = _damage;
        Shield = _shield;
        Draw = _draw;
        Heal = _heal;
        Duration = _duration;
        AttackType = _attackType;
        ItemCanUse = _itemCanUse;
    }
    //public UseItem(int damage, int shield, int draw, int heal, int duration, AttackType attackType, ItemCanUse itemCanUse)
    //{
    //    this._damage = damage;
    //    this._shield = shield;
    //    this._draw = draw;
    //    this._heal = heal;
    //    this._duration = duration;
    //    this._attackType = attackType;
    //    this._itemCanUse = itemCanUse;
    //}
    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }
    public async virtual UniTask UseTask()
    {
        await ItemTask();
    }
}
