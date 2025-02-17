using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemTag
{
    Passive,
    Active,
    Potion
}
public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
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
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Sprite[] ItemSprites;

    public ItemData[] Items;
}
