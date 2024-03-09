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
    public int cost;
    public string descript;
    public Sprite sprite;
    public CardTag cardTag;
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public CardData[] cards;
}
