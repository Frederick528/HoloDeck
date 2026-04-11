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

[System.Serializable]
public struct MasterTag
{
    public bool IsSpecial; // SpecialTag면 true, StatusEffect면 false
    public SpecialTag Special;
    public StatusEffect Status;

    public MasterTag(SpecialTag special) { IsSpecial = true; Special = special; Status = StatusEffect.None; }
    public MasterTag(StatusEffect status) { IsSpecial = false; Special = SpecialTag.None; Status = status; }

    // 문자열을 받아 우선순위에 따라 자료형으로 변환 (Special 우선)
    public static MasterTag Parse(string rawText)
    {
        if (Enum.TryParse<SpecialTag>(rawText, true, out var special))
            return new MasterTag(special);
        if (Enum.TryParse<StatusEffect>(rawText, true, out var status))
            return new MasterTag(status);

        string errorMsg = $"[데이터 오류] '{rawText}'는 유효한 태그가 아닙니다. 시트 오타를 확인하세요.";
        Debug.LogError(errorMsg); // 유니티 콘솔에 빨간 줄
        throw new ArgumentException(errorMsg); // 코드 실행 즉시 중단
    }

    public bool Is(SpecialTag tag) => IsSpecial && Special == tag;
    public bool Is(StatusEffect effect) => !IsSpecial && Status == effect;

    // [핵심] StatusEffect -> MasterTag로 자동 변환 (들어오는 방향)
    public static implicit operator MasterTag(StatusEffect status) => new MasterTag(status);

    // [핵심] SpecialTag -> MasterTag로 자동 변환 (들어오는 방향)
    public static implicit operator MasterTag(SpecialTag special) => new MasterTag(special);

    //// [핵심] MasterTag를 StatusEffect로 자동 변환해주는 마법
    //public static implicit operator StatusEffect(MasterTag master) => master.Status;

    //// [핵심] MasterTag를 SpecialTag로 자동 변환해주는 마법
    //public static implicit operator SpecialTag(MasterTag master) => master.Special;
}
[Serializable]
public struct MasterType
{
    public bool IsSpecial; // SpecialTagType이면 true, StatusEffectType이면 false
    public SpecialTagType Special;
    public StatusEffectType Status;

    public MasterType(SpecialTagType special) { IsSpecial = true; Special = special; Status = StatusEffectType.None; }
    public MasterType(StatusEffectType status) { IsSpecial = false; Special = SpecialTagType.None; Status = status; }

    public static MasterType Parse(string rawText)
    {
        if (Enum.TryParse<SpecialTagType>(rawText, true, out var special))
            return new MasterType(special);
        if (Enum.TryParse<StatusEffectType>(rawText, true, out var status))
            return new MasterType(status);

        string errorMsg = $"[데이터 오류] '{rawText}'는 유효한 타입이 아닙니다. 시트 오타를 확인하세요.";
        Debug.LogError(errorMsg); // 유니티 콘솔에 빨간 줄
        throw new ArgumentException(errorMsg); // 코드 실행 즉시 중단
    }

    public bool Is(SpecialTagType tagType) => IsSpecial && Special == tagType;
    public bool Is(StatusEffectType effectType) => !IsSpecial && Status == effectType;

    // [핵심] SpecialTagType -> MasterType로 자동 변환 (들어오는 방향)
    public static implicit operator MasterType(StatusEffectType special) => new MasterType(special);

    // [핵심] StatusEffectType -> MasterType로 자동 변환 (들어오는 방향)
    public static implicit operator MasterType(SpecialTagType status) => new MasterType(status);

    //// [핵심] MasterType를 SpecialTagType로 자동 변환해주는 마법
    //public static implicit operator SpecialTagType(MasterType master) => master.Special;

    //// [핵심] MasterType를 StatusEffectType로 자동 변환해주는 마법
    //public static implicit operator StatusEffectType(MasterType master) => master.Status;
}
[Serializable]
public struct MasterTagData
{
    public MasterTag Tag;           // 효과 이름 (예: ATKUp)
    public MasterType Type;         // 종류 (예: TurnDuration)
    public bool XAmount;
    public float Amount;           // 값 (예: 5, x)
    public bool XDuration;
    public float Duration;         // 지속시간 (예: 3, -1)
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
    public int Count;
    //public int EnhancedCost;
    public int Damage;
    public float DamageOrder;
    //public int EnhancedDamage;
    public int Shield;
    public float ShieldOrder;
    //public int EnhancedDefence;
    //public int EnhancedCount;
    public int Draw;
    public float DrawOrder;
    //public int EnhancedDraw;
    public int HP;
    public float HPOrder;

    public int Discard;
    public float DiscardOrder;
    public int Remove;
    public float RemoveOrder;
    //public float CardUseDelay;
    public int Price;
    [TextArea(1, 5)]
    public string Descript;
    //[TextArea(1, 5)]
    //public string EnhancedDescript;
    public Sprite Sprite;
    public GameObject Effect;
    public List<MasterTagData> MasterTags = new();
    public bool HasCondition;
    public CardTag CardTag;
    public CardRarity CardRarity;

    public CardData Clone()
    {
        return new CardData
        {
            Name = this.Name,
            ID = this.ID,
            Cost = this.Cost,
            Count = this.Count,
            Damage = this.Damage,
            DamageOrder = this.DamageOrder,
            Shield = this.Shield,
            ShieldOrder = this.ShieldOrder,
            Draw = this.Draw,
            DrawOrder = this.DrawOrder,
            HP = this.HP,
            HPOrder = this.HPOrder,

            Discard = this.Discard,
            DiscardOrder = this.DiscardOrder,
            Remove = this.Remove,
            RemoveOrder = this.RemoveOrder,
            //CardUseDelay = CardUseDelay,
            Price = this.Price,
            Descript = this.Descript,
            Sprite = this.Sprite,
            Effect = this.Effect,
            MasterTags = this.MasterTags,
            HasCondition = this.HasCondition,
            CardTag = this.CardTag,
            CardRarity = this.CardRarity
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
