using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PRS
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
    }
}

public class CardUtils
{
    public static Vector3 CardScale => new Vector3(2.1f, 2.4f, 1f);
    public static float LargeCardPosY => -3.2f;
    public static float ThrowAwayCardDelay => 0.3f;
    public static float LoadCardDummyDelay => 0.4f;
    public static float CardAlignmentDelay => 0.3f;
}

public enum EAddDeck
{
    Main, Draw, Dummy, Hand/*, Total*/
    //MainNDraw, MainNDummy, MainNHand, DrawNHand, DummyNHand,
    //MainNDrawNDummy, MainNDrawNHand, DrawNDummyNHand,
    //MainNDrawNDummyNHand
}

public enum StatusEffect
{
    Attack, Defense, Heal, ATKUp, DEFUp, HealUp, Vulnerable, Weaking, Reflection, Protect, Resurrection, CoinGained, CriticalChanceUp, CriticalDamageUp, HPUp
}

public enum StatusEffectType
{
    InfiniteDuration,                   // 무한 지속 (duration = -1), 버프 제거로만 삭제 가능
    TurnDuration,                       // 턴마다 지속시간 1 감소
    DurationIsAmount,                   // 턴마다 지속시간이 1 감소되며, 이는 값을 의미하기도 함
    UseAmountInfiniteDuration,          // Amount 값이 사용되며, 값이 0이 되지 않는다면, 버프 제거로만 삭제 가능
    UseAmountTurnDuration,              // Amount 값이 사용되며, 턴마다 지속시간 1 감소
    Perpetual,                          // 영구적으로 적용. 버프제거로도 안 사라짐.
    UseAmountPerpetual                  // Amount 값이 사용되며, 값이 0이 되지 않는다면, 영구적으로 적용.
}

public class FindTransform
{
    public static Transform ContinueFindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = ContinueFindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
