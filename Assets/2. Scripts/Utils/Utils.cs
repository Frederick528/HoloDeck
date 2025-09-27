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
    public static float LargeCardPosY => -2.55f;
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
    Attack, Defense, Heal, ATKUp, DEFUp, HealUp, Vulnerable, Weaking, Reflection, Protect, Resurrection, CoinGained, CriticalChanceUp,
    CriticalDamageUp, HPUp, Thievery, Special, UseCritical, Immunity, Vampire, Berserker, GetCritical, Bleed
    // 공격 알림, 방어 알림, 회복 알림, 공격력 업, 방어력 업, 회복력 업, 취약, 약화, 반사, 보호, 부활, 골드 획득량, 치명타 확률 업,
    // 치명타 데미지 업, 체력 업, 도둑, 특별(설명을 직접 작성), 치명타 사용, 면역, 흡혈, 피해 시 공격력 업, 치명타 얻음 알림, 출혈
}

public enum StatusEffectType
{
    InfiniteDuration,                   // 무한 지속 (duration = -1), 버프 제거로만 삭제 가능
    TurnDuration,                       // 턴마다 지속시간 1 감소
    DurationIsAmount,                   // 턴마다 지속시간이 1 감소되며, 이는 값을 의미하기도 함
    UseAmountInfiniteDuration,          // Amount 값이 사용되며, 값이 0이 되지 않는다면, 버프 제거로만 삭제 가능
    UseAmountTurnDuration,              // Amount 값이 사용되며, 턴마다 지속시간 1 감소
    Perpetual,                          // 영구적으로 적용. 버프제거로도 안 사라짐.
    UseAmountPerpetual,                 // Amount 값이 사용되며, 값이 0이 되지 않는다면, 영구적으로 적용.
    Information                         // 정보(~~을 준비 중) 표시용이며, 체력바 아래에 이미지가 표시 안 됨.
}

public class FindTransform
{
    public static RectTransform ContinueFindChildUIByName(Transform parent, string name)
    {
        foreach (RectTransform child in parent)
        {
            if (child.name == name)
                return child;

            RectTransform found = ContinueFindChildUIByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    public static Transform ContinueFindChildObjByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = ContinueFindChildObjByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
