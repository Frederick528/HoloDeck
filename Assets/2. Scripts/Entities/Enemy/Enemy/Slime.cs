using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    int turn;

    void Start()
    {
        //EnemySubScribe();
        _nextActImg.transform.localPosition = new Vector3(0, 1.8f);

        //NextPattern();
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                _nextActText.text = enemyData.Damage.ToString();
                _nextPattern = () => UniTask.Create(async () =>
                {
                    await Attack(enemyData.Damage);
                });
                break;
            case 2:
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
                _nextActText.text = enemyData.Damage.ToString();
                _nextPattern = () => UniTask.Create(async () =>
                {
                    await Heal(enemyData.Damage);
                });
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
