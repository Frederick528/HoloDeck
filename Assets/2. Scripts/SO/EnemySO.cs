using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTag
{
    Nomal,
    Elite,
    Boss
}
[System.Serializable]
public class EnemyData
{
    public string Name;
    public int ID;
    public int Level;
    public int Damage;
    public int CriticalChance;
    public int HP;
    public int DropCoin;
    [TextArea(1, 5)]
    public string Descript;
    public Sprite Sprite;
    public EnemyTag EnemyTag;
    public GameObject EnemyPrefab;
}



[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Object/EnemySO")]
public class EnemySO : ScriptableObject
{
    public Sprite[] EnemySprites;
    public GameObject[] EnemyPrefabs;


    public EnemyData[] EnemyDatas;
}
