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
    bool[] enemySpawn;
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

    private void Start()
    {
        enemySpawn = new bool[enemySpawnPosition.Count];
        Array.Fill(enemySpawn, true);
    }

    public bool SpawnEnemy(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (!enemySpawn[spawnPosIndex]/*enemySpawnPosition[spawnPosIndex].gameObject.activeSelf*/)       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.enemyPrefab, enemySpawnPosition[spawnPosIndex].position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemies.Add(enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        enemySpawn[spawnPosIndex] = false;
        //enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public bool SpawnEnemy(int enemyId)
    {
        EnemyData enemyData;
        GameObject enemyObject;
        Enemy enemy;
        for (int i = 0; i < enemySpawnPosition.Count; ++i)
        {
            if (!enemySpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.enemyPrefab, enemySpawnPosition[i].position, Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();
            enemies.Add(enemy);
            enemy.SetupEnemy(enemyData, i);
            enemySpawn[i] = false;
            //enemySpawnPosition[i].gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public async UniTask<bool> KillEnemyCheck(Enemy enemy, UniTask task)
    {
        enemies.Remove(enemy);
        await task;
        enemySpawn[enemy.spawnPosIdx] = true;
        return enemies.Count == 0;
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
