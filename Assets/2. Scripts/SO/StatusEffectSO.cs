using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Object/StatusEffectSO")]
public class StatusEffectSO : ScriptableObject
{
    public Sprite[] SESprites;

    public SEData[] SEDatas;
}


[Serializable]
public class SEData
{
    public StatusEffect SE;
    public StatusEffectType SET;

    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;
}
