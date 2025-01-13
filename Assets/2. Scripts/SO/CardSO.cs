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
//public struct CardData
//{
//    public string KR; //= "한국어 텍스트";
//    public string EN; //= "English";
//    public int Date; //= 0;
//    public int Hunger; //= 0;
//    public int Thirst; //= 0;
//    public string Descript; //= "카드 종류에 대한 설명";
//    public string Effect; //= "실제로 작동할 효과에 대한 설명";
//    public string Info; // = "카드에 대한 설명";
//    public string Craft; //= "조합 식";
//    public string CraftResult; //= "조합 결과 아이템";
//    public string CraftEffect; // "조합한 아이템 사용 효과";
//}
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
