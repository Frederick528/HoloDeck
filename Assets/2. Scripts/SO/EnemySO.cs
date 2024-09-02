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
    public string name;
    public int id;
    public int level;
    public int damage;
    public int hp;
    public int dropCoin;
    [TextArea(1, 5)]
    public string descript;
    public Sprite sprite;
    public EnemyTag enemyTag;
    public GameObject enemyPrefab;
}



[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Object/EnemySO")]
public class EnemySO : ScriptableObject
{
    public Sprite[] enemySprites;


    public EnemyData[] enemyDatas;
}
