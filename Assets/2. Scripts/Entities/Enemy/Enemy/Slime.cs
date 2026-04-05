using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    int turn;

    void Start()
    {
        BaseThickness = 0.007f;
        //EnemySubScribe();
        //_nextActImg.transform.localPosition = new Vector3(0, 1.8f);

        //NextPattern();
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                AttackPattern(_defaultEnemyData.Damage);
                //RemoveStatusEffect((StatusEffect.Heal, StatusEffectType.Perpetual));
                //AddStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual), enemyData.Damage);
                //_nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                //_nextActText.text = enemyData.Damage.ToString();
                //_nextPattern = (StatusEffect.Attack, () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage);
                //}));
                break;
            case 2:
                HealPattern(_defaultEnemyData.Damage);
                //RemoveStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual));
                //AddStatusEffect((StatusEffect.Heal, StatusEffectType.Perpetual), enemyData.Damage);
                //_nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
                //_nextActText.text = enemyData.Damage.ToString();
                //_nextPattern = (StatusEffect.Heal, () => UniTask.Create(async () =>
                //{
                //    await Heal(enemyData.Damage);
                //}));
                turn = 0;
                break;
        }
    }

    //public override async UniTask PlayPattern()
    //{
    //    turn++;
    //    switch (turn)
    //    {
    //        case 1:
    //            await Attack(enemyData.Damage);
    //            _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
    //            break;
    //        case 2:
    //            await Heal(enemyData.Damage);
    //            _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
    //            turn = 0;
    //            break;
    //    }
    //}
}
