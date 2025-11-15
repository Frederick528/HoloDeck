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
public class ItemBase
{
    // 기본 아이템 클래스(패시브 역할을 함)
    public string Name;
    public int ID;
    public ItemTag ItemTag;
    public ItemRarity ItemRarity;
    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;

    public ItemBase() { }

    protected ItemBase(ItemBase other)
    {
        this.Name = other.Name;
        this.ID = other.ID;
        this.ItemTag = other.ItemTag;
        this.ItemRarity = other.ItemRarity;
        this.Price = other.Price;
        this.Descript = other.Descript;
        this.Sprite = other.Sprite;

    }

    public virtual ItemBase Clone()
    {
        return new ItemBase(this);
    }
}
[Serializable]
public class UseItemBase : ItemBase
{
    // 사용 아이템 클래스(포션 역할을 함)
    public int Damage;
    public int Shield;
    public int Draw;
    public int Heal;
    public int Duration;
    public AttackType AttackType;
    public ItemCanUse ItemCanUse;

    public UseItemBase() { }
    protected UseItemBase(UseItemBase other) : base(other)
    {
        this.Damage = other.Damage;
        this.Shield = other.Shield;
        this.Draw = other.Draw;
        this.Heal = other.Heal;
        this.Duration = other.Duration;
        this.AttackType = other.AttackType;
        this.ItemCanUse = other.ItemCanUse;
    }
    public override ItemBase Clone()
    {
        return new UseItemBase(this);
    }
}
[Serializable]
public class ChargeItemBase : UseItemBase
{
    // 액티브 아이템 클래스(액티브 역할을 함)
    public int MaxCharge;
    public bool StartCharge;
    public ChargeItemBase() { }
    protected ChargeItemBase(ChargeItemBase other) : base(other)
    {
        this.MaxCharge = other.MaxCharge;
        this.StartCharge = other.StartCharge;
    }
    public override ItemBase Clone()
    {
        return new ChargeItemBase(this);
    }
}

//[Serializable]
//public class  ItemData/* : ICloneable*/
//{
//    public string Name;
//    public int ID;
//    public ItemTag ItemTag;
//    public ItemRarity ItemRarity;

//    //public bool StartCharge;       // bool 타입으로 변경해도 될 듯.
//    //public int MaxCharge;

//    //public int Damage;
//    //public int Shield;
//    //public int Draw;
//    //public int Heal;

//    //public int Duration;

//    public int Price;
//    [TextArea(1, 5)]
//    public string Descript;
//    public Sprite Sprite;
//    //public AttackType AttackType;
//    //public ItemCanUse ItemCanUse;

//    public ItemData Clone()
//    {
//        return new ItemData
//        {
//            Name = Name,
//            ID = ID,
//            ItemTag = ItemTag,
//            ItemRarity = ItemRarity,
//            //StartCharge = StartCharge,
//            //MaxCharge = MaxCharge,
//            //Damage = Damage,
//            //Shield = Shield,
//            //Draw = Draw,
//            //Heal = Heal,
//            //Duration = Duration,
//            Price = Price,
//            Descript = Descript,
//            Sprite = Sprite,
//            //AttackType = AttackType,
//            //ItemCanUse = ItemCanUse
//        };
//    }
//}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Sprite[] ItemSprites;

    public List<ItemBase> PassiveItems;
    public List<UseItemBase> PotionItems;
    public List<ChargeItemBase> ActiveItems;

    public Vector2Int PassiveID = new();
    public Vector2Int ActiveID = new();
    public Vector2Int PotionID = new();
}
