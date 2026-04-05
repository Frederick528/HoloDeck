using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite1 : Enemy
{
    int turn;

    void Start()
    {
        BaseThickness = 0.003f;
        //_nextActImg.transform.localPosition = new Vector3(0, 2.2f);
    }

    public override void NextPattern()
    {
        turn++;
        _nextActImg.gameObject.SetActive(true);
        switch (turn)
        {
            case 1:
                SetRepeat(2);
                AttackPattern(_defaultEnemyData.Damage);
                break;
            case 2:
                SetRepeat();
                SpecialPattern(3);
                turn = 0;
                break;
        }
    }

    protected override void SpecialPattern(int value, bool addPattern = false, float delay = 0.3f)
    {
        _specialDesc = "해당 적은 공격력을 {n}만큼 2턴동안 얻습니다.";

        _nextPattern.Add(async () => 
        {
            await UniTask.WaitForSeconds(delay/*, cancellationToken: TurnManager.Instance.CancelSource.Token*/);
            if (this.CurHP.Value > 0)
                AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.TurnDuration), value, 2);
        });

        //_nextPattern = () => UniTask.Create(async () =>
        //{
        //    await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //    AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.TurnDuration), value, 2);
        //});
        base.SpecialPattern(value, addPattern, delay);
    }
}
