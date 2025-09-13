using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public Dictionary<int, EnemyData> EnemyDatas { get; private set; } = new Dictionary<int, EnemyData>();
    public List<Enemy> EnemyList;
    public Dictionary<int, Enemy> EnemyDict = new();     // OnTriggerEnter에서 gameObject로 받기 때문에 List에서 확인이 안 됨. 그렇다고 GetComponent 하면 비용적으로 별로라서 그냥 Dict 하나 만듦.
    Vector3[] _enemySpawnPosition = new Vector3[4];
    Vector3[] _bossSpawnPosition = new Vector3[3];
    bool[] _enemySpawn;
    bool[] _bossSpawn;
    public Enemy TargetEnemy;

    public Enemy HitEnemy;

    public Enemy EnemyInfo;
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
            _enemySpawnPosition[0] = (new Vector3(-1.5f, 0f));      // 0
            _enemySpawnPosition[1] = (new Vector3(1.25f, 0f));      // 1
            _enemySpawnPosition[2] = (new Vector3(4f, 0f));      // 2
            _enemySpawnPosition[3] = (new Vector3(6.75f, 0f));      // 3
            //EnemySpawnPosition[4] = (new Vector3(8.25f, 0f));      // 4

            _bossSpawnPosition[0] = (new Vector3(-4, 0));
            _bossSpawnPosition[1] = (new Vector3(0, 0));
            _bossSpawnPosition[2] = (new Vector3(4, 0));
        }
    }

    private void Start()
    {
        _enemySpawn = new bool[_enemySpawnPosition.Length];
        _bossSpawn = new bool[_bossSpawnPosition.Length];
        CanEnemySpawn(true);
        CanBossSpawn(true);
    }

    public void CheckEnemy(Transform targetEnemy)
    {
        if (EnemyInfo == null || targetEnemy == null)
            return;
        if (targetEnemy != EnemyInfo.transform)
            return;
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Enemy, true);

        // 이후 스테이터스 창을 닫고 싶은데, 순서 문제로 안 닫힘. 나중에 좀 더 보충.
    }

    public void CanEnemySpawn(bool canSpawn)
    {
        Array.Fill(_enemySpawn, canSpawn);
    }

    public void CanBossSpawn(bool canSpawn)
    {
        Array.Fill(_bossSpawn, canSpawn);
    }

    public bool SpawnEnemy(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (spawnPosIndex >= _enemySpawn.Length || !_enemySpawn[spawnPosIndex])       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.EnemyPrefab, _enemySpawnPosition[spawnPosIndex], Quaternion.identity);
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
        for (int i = 0; i < _enemySpawnPosition.Length; ++i)
        {
            if (!_enemySpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.EnemyPrefab, _enemySpawnPosition[i], Quaternion.identity);
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

    public bool SpawnBoss(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (!_bossSpawn[spawnPosIndex]/*enemySpawnPosition[spawnPosIndex].gameObject.activeSelf*/)       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.EnemyPrefab, _bossSpawnPosition[spawnPosIndex], Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        EnemyList.Add(enemy);
        EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        _bossSpawn[spawnPosIndex] = false;
        //enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public bool SpawnBoss(int enemyId)
    {
        EnemyData enemyData;
        GameObject enemyObject;
        Enemy enemy;
        for (int i = 0; i < _bossSpawnPosition.Length; ++i)
        {
            if (!_bossSpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.EnemyPrefab, _bossSpawnPosition[i], Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();
            EnemyList.Add(enemy);
            EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
            enemy.SetupEnemy(enemyData, i);
            _bossSpawn[i] = false;
            //enemySpawnPosition[i].gameObject.SetActive(false);
            return true;
        }
        return false;
    }


    public async UniTask<bool> KillEnemyCheck(Enemy enemy, UniTask task)
    {
        EnemyList.Remove(enemy);
        EnemyDict.Remove(enemy.gameObject.GetInstanceID());
        bool noEnemy = EnemyList.Count == 0;
        await task;
        _enemySpawn[enemy.spawnPosIdx] = true;
        return noEnemy;
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

    public Sprite NextActImg(int idx)
    {
        return enemySO.EnemyNextAct[idx];
    }
}
