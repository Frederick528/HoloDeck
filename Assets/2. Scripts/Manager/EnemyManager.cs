using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public Dictionary<int, EnemyData> enemyDatas { get; private set; } = new Dictionary<int, EnemyData>();
    public List<Enemy> enemies;
    public List<Transform> enemySpawnPosition;
    public GameObject targetEnemy;
    //public Arrow ArrowCursor;

    public bool MapClear = false;

    [SerializeField] EnemySO enemySO;
    //[SerializeField] Enemy enemy;


    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        //enemySpawnPosition.Add(new Vector3(6f, 1.5f));      // 0
        //enemySpawnPosition.Add(new Vector3(4.5f, 1.5f));    // 1
        //enemySpawnPosition.Add(new Vector3(7.5f, 1.5f));    // 2
        //enemySpawnPosition.Add(new Vector3(5f, 1.5f));      // 3
        //enemySpawnPosition.Add(new Vector3(7f, 1.5f));      // 4
    }

    //private void Start()
    //{
    //    ArrowCursor = FindObjectOfType<Arrow>(true);
    //}

    public bool SpawnEnemy(int enemyId, int spawnPosIndex = 0)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (!enemySpawnPosition[spawnPosIndex].gameObject.activeSelf)
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.enemyPrefab, enemySpawnPosition[spawnPosIndex].position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemies.Add(enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public EnemyData FindEnemyData(int id)   // Id 값으로 적 데이터 가져오기
    {
        EnemyData enemyData;
        if (enemyDatas.TryGetValue(id, out enemyData))
        {
            return enemyData;
        }
        else
        {
            enemyData = Array.Find(enemySO.enemyDatas, x => x.id == id);
            enemyDatas.Add(id, enemyData);
            return enemyData;
        }
        //return Array.Find(enemySO.enemyDatas, x => x.Id == Id);
    }
}
