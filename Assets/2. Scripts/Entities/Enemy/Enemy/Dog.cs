using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Enemy
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
                AttackPattern(_defaultEnemyData.Damage*2);
                //RemoveStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual));
                //AddStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual), enemyData.Damage);
                //_nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                //_nextActText.text = enemyData.Damage.ToString();
                //_nextPattern = (StatusEffect.Attack, () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage);
                //}));
                break;
            case 2:
                AttackPattern(_defaultEnemyData.Damage);
                //RemoveStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual));
                //AddStatusEffect((StatusEffect.Attack, StatusEffectType.Perpetual), enemyData.Damage / 2);
                //_nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                //_nextActText.text = (enemyData.Damage / 2).ToString();
                //_nextPattern = (StatusEffect.Attack, () => UniTask.Create(async () =>
                //{
                //    await Attack(enemyData.Damage / 2);
                //}));
                turn = 0;
                break;
        }
    }

    //public UniTask Pattern()
    //{
    //    //await base.Pattern();
    //    turn++;
    //    switch (turn)
    //    {
    //        case 1:
    //            _nextPattern = () => UniTask.Create(async () =>
    //            {
    //                await Attack(enemyData.Damage);
    //                _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
    //            });
    //            break;
    //        case 2:
    //            await Attack(enemyData.Damage / 2);
    //            _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
    //            //InGameManager.Instance.player.TakeDamagePlayer((int)(enemyData.damage/2)).Forget();
    //            turn = 0;
    //            break;
    //    }
    //}
}
