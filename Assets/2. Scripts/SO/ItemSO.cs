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
public class    ItemData/* : ICloneable*/
{
    public string Name;
    public int ID;
    public ItemTag ItemTag;
    public ItemRarity ItemRarity;

    public bool StartCharge;       // bool 타입으로 변경해도 될 듯.
    public int MaxCharge;

    public int Damage;
    public int Shield;
    public int Draw;
    public int Heal;

    public int Duration;

    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;
    public AttackType AttackType;
    public ItemCanUse ItemCanUse;

    public ItemData Clone()
    {
        return new ItemData
        {
            Name = Name,
            ID = ID,
            ItemTag = ItemTag,
            ItemRarity = ItemRarity,
            StartCharge = StartCharge,
            MaxCharge = MaxCharge,
            Damage = Damage,
            Shield = Shield,
            Draw = Draw,
            Heal = Heal,
            Duration = Duration,
            Price = Price,
            Descript = Descript,
            Sprite = Sprite,
            AttackType = AttackType,
            ItemCanUse = ItemCanUse
        };
    }
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Sprite[] ItemSprites;

    public ItemData[] Items;

    public Vector2Int PassiveID = new();
    public Vector2Int ActiveID = new();
    public Vector2Int PotionID = new();
}
