using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemTag
{
    Passive,        // 1~500
    Active,         // 501~1000
    Potion          // 1001~
}
public enum AttackType
{
    None,
    Single,
    Multi
}
public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum ItemCanUse
{
    None,
    OnlyBattle,
    Anytime
}

[Serializable]
public class ItemData
{
    public string Name;
    public int Id;
    public int MaxCharge;
    public int CurCharge;

    public int Damage;
    public int Shield;
    public int Draw;
    public int Heal;

    public int Duration;

    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;
    public ItemTag ItemTag;
    public ItemRarity ItemRarity;
    public AttackType AttackType;
    public ItemCanUse ItemCanUse;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Sprite[] ItemSprites;

    public ItemData[] Items;
}
