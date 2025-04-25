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
    Attack, Defense, Heal, ATKUp, DEFUp, HealUp, Vulnerable, Weaking, Reflection, Protect
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
