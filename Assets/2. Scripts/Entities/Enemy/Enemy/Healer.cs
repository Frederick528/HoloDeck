using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : Enemy
{
    int turn;
    //int _resistance = 1;

    void Start()
    {
        BaseThickness = 0.01f;
        //EnemySubScribe();
        AddStatusEffect((StatusEffect.Reflection, StatusEffectType.Perpetual), 3);
        AddStatusEffect((StatusEffect.Protect, StatusEffectType.Perpetual), 1);
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
                DefensePattern(_defaultEnemyData.Damage);
                //RemoveStatusEffect((StatusEffect.Heal, StatusEffectType.Perpetual));
                //AddStatusEffect((StatusEffect.Defense, StatusEffectType.Perpetual), enemyData.Damage);
                //_nextActImg.sprite = EnemyManager.Instance.NextActImg(1);
                //_nextActText.text = enemyData.Damage.ToString();
                //_nextPattern = (StatusEffect.Defense, () => UniTask.Create(async () =>
                //{
                //    await Shield(enemyData.Damage);
                //}));
                break;
            case 2:
                HealPattern(_defaultEnemyData.Damage);
                //RemoveStatusEffect((StatusEffect.Defense, StatusEffectType.Perpetual));
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

    //protected override async UniTask BeforeTakeDamage()
    //{
    //    await base.Shield(_resistance);
    //    await base.BeforeTakeDamage();
    //}

    //protected override async UniTask AfterTakeDamage()
    //{
    //    player.TakeDamagePlayer(enemyData.Damage).Forget();
    //    await base.AfterTakeDamage();
    //}

    //protected override int ResistDamage(int damage)
    //{
    //    int resistDamage;
    //    if (damage > _resistance)
    //    {
    //        resistDamage = damage - _resistance;      // BeforeTakeDamage로 얻을 _shield 양만큼 빼서 계산.
    //        print(resistDamage);
    //    }
    //    else
    //    {
    //        resistDamage = 0;
    //    }
    //    return resistDamage;
    //}

    //public override void CheckIfDead(int damage, int count)
    //{
    //    int resistDamage;
    //    if (damage > _resistance)
    //    {
    //        resistDamage = damage - _resistance;      // BeforeTakeDamage로 얻을 _shield 양만큼 빼서 계산.
    //    }
    //    else
    //    {
    //        resistDamage = 0;
    //    }
    //    base.CheckIfDead(resistDamage, count);
    //}


    //public override async UniTask PlayPattern()
    //{
    //    turn++;
    //    switch (turn)
    //    {
    //        case 1:
    //            await Shield(enemyData.Damage);
    //            _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
    //            break;
    //        case 2:
    //            await Heal(enemyData.Damage);
    //            _nextActImg.sprite = EnemyManager.Instance.NextActImg(1);
    //            turn = 0;
    //            break;
    //    }
    //    //await base.Pattern();
    //}
}
