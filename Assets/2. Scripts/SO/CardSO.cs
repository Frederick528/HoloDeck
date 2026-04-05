using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardTag
{
    SingleAttack,
    AllAttack,
    RandomAttack,
    SkillTargetSelf,
    SkillTargetSingle,
    SkillTargetAll,
    SkillTargetRandom
}
public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public struct SpecialTagData
{
    public string Tag;              // 효과 이름 (예: ATKUp)
    public string Type;   // 종류 (예: TurnDuration)
    public string Amount;            // 값 (예: 5, x)
    public string Duration;         // 지속시간 (예: 3, -1)
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
public class CardData
{
    public string Name;
    public int ID;
    public int Cost;
    //public int EnhancedCost;
    public int Damage;
    //public int EnhancedDamage;
    public int Shield;
    //public int EnhancedDefence;
    public int Count;
    //public int EnhancedCount;
    public int Draw;
    //public int EnhancedDraw;
    public int Discard;
    public int Remove;
    public int HP;
    //public float CardUseDelay;
    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    //[TextArea(1, 5)]
    //public string EnhancedDescript;
    public Sprite Sprite;
    public GameObject Effect;
    public List<SpecialTagData> SpecialTags = new();
    public bool HasCondition;
    public CardTag CardTag;
    public CardRarity CardRarity;

    public CardData Clone()
    {
        return new CardData
        {
            Name = Name,
            ID = ID,
            Cost = Cost,
            Damage = Damage,
            Shield = Shield,
            Count = Count,
            Draw = Draw,
            Discard = Discard,
            Remove = Remove,
            HP = HP,
            //CardUseDelay = CardUseDelay,
            Price = Price,
            Descript = Descript,
            Sprite = Sprite,
            Effect = Effect,
            SpecialTags = SpecialTags,
            HasCondition = HasCondition,
            CardTag = CardTag,
            CardRarity = CardRarity
        };
    }
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Sprite[] CardSprites;
    public GameObject[] CardEffects;

    public CardData[] Cards;

    public Vector2Int[] ClassifyCardRarityID = new Vector2Int[4];
    public Vector2Int[] ClassifyEnhancedCardRarityID = new Vector2Int[4];
}
