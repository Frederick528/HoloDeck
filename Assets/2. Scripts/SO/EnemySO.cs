using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTag
{
    Nomal,
    Elite,
    Boss,
    EndBoss
}
[System.Serializable]
public class EnemyData
{
    public string Name;
    public int ID;
    public int Level;
    public int Damage;
    public int CriticalChance;
    public int CriticalDamage;
    public int HP;
    public int DropCoin;
    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;
    public EnemyTag EnemyTag;
    public GameObject EnemyPrefab;
    
    public EnemyData Clone()
    {
        return new EnemyData
        {
            Name = Name,
            ID = ID,
            Level = Level,
            Damage = Damage,
            CriticalChance = CriticalChance,
            CriticalDamage = CriticalDamage,
            HP = HP,
            DropCoin = DropCoin,
            Descript = Descript,
            Sprite = Sprite,
            EnemyTag = EnemyTag,
            EnemyPrefab = EnemyPrefab
        };
    }
}



[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Object/EnemySO")]
public class EnemySO : ScriptableObject
{
    public Sprite[] EnemySprites;
    public GameObject[] EnemyPrefabs;

    public Sprite[] EnemyNextAct;

    public EnemyData[] EnemyDatas;
}
