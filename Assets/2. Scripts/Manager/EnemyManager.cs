using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public Dictionary<int, EnemyData> EnemyDatas { get; private set; } = new Dictionary<int, EnemyData>();
    public List<Enemy> EnemyList;
    public Dictionary<int, Enemy> EnemyDict = new();     // OnTriggerEnter에서 gameObject로 받기 때문에 List에서 확인이 안 됨. 그렇다고 GetComponent 하면 비용적으로 별로라서 그냥 Dict 하나 만듦.
    public Vector3[] EnemySpawnPosition = new Vector3[5];
    bool[] _enemySpawn;
    public Enemy TargetEnemy;
    //public Arrow ArrowCursor;

    public bool MapClear = false;

    [SerializeField] EnemySO enemySO;
    //[SerializeField] Enemy enemy;


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            EnemySpawnPosition[0] = (new Vector3(3f, 0.5f));      // 0
            EnemySpawnPosition[1] = (new Vector3(5f, 0.5f));      // 1
            EnemySpawnPosition[2] = (new Vector3(7f, 0.5f));      // 2
            EnemySpawnPosition[3] = (new Vector3(4f, 1.5f));      // 3
            EnemySpawnPosition[4] = (new Vector3(6f, 1.5f));      // 4
        }
    }

    private void Start()
    {
        _enemySpawn = new bool[EnemySpawnPosition.Length];
        CanEnemySpawn(true);
    }

    public void CanEnemySpawn(bool canSpawn)
    {
        Array.Fill(_enemySpawn, canSpawn);
    }

    public bool SpawnEnemy(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (!_enemySpawn[spawnPosIndex]/*enemySpawnPosition[spawnPosIndex].gameObject.activeSelf*/)       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.EnemyPrefab, EnemySpawnPosition[spawnPosIndex], Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        EnemyList.Add(enemy);
        EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        _enemySpawn[spawnPosIndex] = false;
        //enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public bool SpawnEnemy(int enemyId)
    {
        EnemyData enemyData;
        GameObject enemyObject;
        Enemy enemy;
        for (int i = 0; i < EnemySpawnPosition.Length; ++i)
        {
            if (!_enemySpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.EnemyPrefab, EnemySpawnPosition[i], Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();
            EnemyList.Add(enemy);
            EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
            enemy.SetupEnemy(enemyData, i);
            _enemySpawn[i] = false;
            //enemySpawnPosition[i].gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public async UniTask<bool> KillEnemyCheck(Enemy enemy, UniTask task)
    {
        EnemyList.Remove(enemy);
        EnemyDict.Remove(enemy.gameObject.GetInstanceID());
        await task;
        _enemySpawn[enemy.spawnPosIdx] = true;
        return EnemyList.Count == 0;
    }

    public EnemyData FindEnemyData(int id)   // ID 값으로 적 데이터 가져오기
    {
        EnemyData enemyData;
        if (EnemyDatas.TryGetValue(id, out enemyData))
        {
            return enemyData;
        }
        else
        {
            enemyData = Array.Find(enemySO.EnemyDatas, x => x.ID == id);
            EnemyDatas.Add(id, enemyData);
            return enemyData;
        }
        //return Array.Find(enemySO.enemyDatas, x => x.ID == ID);
    }
}
