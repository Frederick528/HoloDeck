using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardTag
{
    SingleAttack,
    MultiAttack,
    Skill
}
[System.Serializable]
public class CardData : ICloneable
{
    public string Name;
    public int Id;
    public int Cost;
    //public int EnhancedCost;
    public int Damage;
    //public int EnhancedDamage;
    public int Defence;
    //public int EnhancedDefence;
    public int Count;
    //public int EnhancedCount;
    public int Draw;
    //public int EnhancedDraw;
    public float CardUseDelay;
    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    //[TextArea(1, 5)]
    //public string EnhancedDescript;
    public Sprite Sprite;
    public CardTag CardTag;

    public object Clone()
    {
        return new CardData
        {
            Name = Name,
            Id = Id,
            Cost = Cost,
            Damage = Damage,
            Defence = Defence,
            Count = Count,
            Draw = Draw,
            CardUseDelay = CardUseDelay,
            Price = Price,
            Descript = Descript,
            Sprite = Sprite,
            CardTag = CardTag
        };
    }
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Sprite[] CardSprites;

    public CardData[] Cards;
}
