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
public class CardData
{
    public string name;
    public int id;
    public int cost;
    public int enhancedCost;
    public int damage;
    public int enhancedDamage;
    public int defence;
    public int enhancedDefence;
    public int count;
    public int enhancedCount;
    public int draw;
    public int enhancedDraw;
    public int price;
    [TextArea(1,5)]
    public string descript;
    [TextArea(1, 5)]
    public string enhancedDescript;
    public Sprite sprite;
    public CardTag cardTag;
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Sprite[] cardSprites;

    public CardData[] cards;
}
