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
    Vector3[] enemySpawnPosition = new Vector3[4];
    Vector3[] bossSpawnPosition = new Vector3[3];
    bool[] enemySpawn;
    bool[] bossSpawn;
    public Enemy TargetEnemy;
    public bool NoEnemy;    // TurnManager에 있는 InBattle은 전투가 끝났는가?(적이 죽는 모션까지 포함) NoEnemy는 현재 선택할 수 있는 적이 없는가?(죽는 모션 포함 X)

    //public Enemy HitEnemy;

    public Enemy EnemyInfo;
    //public Arrow ArrowCursor;

    public bool MapClear = false;

    public Vector3 EnemyCenterSpawnPos;

    [SerializeField] EnemySO enemySO;
    //[SerializeField] Enemy enemy;


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            enemySpawnPosition[0] = (new Vector3(-1.5f, -0.1f));      // 0
            enemySpawnPosition[1] = (new Vector3(1.25f, -0.1f));      // 1
            enemySpawnPosition[2] = (new Vector3(4f, -0.1f));      // 2
            enemySpawnPosition[3] = (new Vector3(6.75f, -0.1f));      // 3
            //EnemySpawnPosition[4] = (new Vector3(8.25f, 0f));      // 4

            int index = enemySpawnPosition.Length / 2;
            if (enemySpawnPosition.Length % 2 != 0)
                EnemyCenterSpawnPos = enemySpawnPosition[index];
            else
                EnemyCenterSpawnPos = (enemySpawnPosition[index] + enemySpawnPosition[index - 1]) / 2;

            bossSpawnPosition[0] = (new Vector3(-4, 0));
            bossSpawnPosition[1] = (new Vector3(0, 0));
            bossSpawnPosition[2] = (new Vector3(4, 0));
        }
    }

    private void Start()
    {
        enemySpawn = new bool[enemySpawnPosition.Length];
        bossSpawn = new bool[bossSpawnPosition.Length];
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
        Array.Fill(enemySpawn, canSpawn);
    }

    public void CanBossSpawn(bool canSpawn)
    {
        Array.Fill(bossSpawn, canSpawn);
    }

    public bool SpawnEnemy(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (spawnPosIndex >= enemySpawn.Length || !enemySpawn[spawnPosIndex])       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.EnemyPrefab, enemySpawnPosition[spawnPosIndex], Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        EnemyList.Add(enemy);
        EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        enemySpawn[spawnPosIndex] = false;
        NoEnemy = EnemyList.Count == 0;
        //enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public bool SpawnEnemy(int enemyId)
    {
        EnemyData enemyData;
        GameObject enemyObject;
        Enemy enemy;
        for (int i = 0; i < enemySpawnPosition.Length; ++i)
        {
            if (!enemySpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.EnemyPrefab, enemySpawnPosition[i], Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();
            EnemyList.Add(enemy);
            EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
            enemy.SetupEnemy(enemyData, i);
            enemySpawn[i] = false;
            NoEnemy = EnemyList.Count == 0;
            //enemySpawnPosition[i].gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public bool SpawnBoss(int enemyId, int spawnPosIndex)       // 체력 설정이 아니라 ID를 통해 몬스터 종류와 체력, 공격력을 가져오는 형식으로 변경함.
    {
        if (!bossSpawn[spawnPosIndex]/*enemySpawnPosition[spawnPosIndex].gameObject.activeSelf*/)       // 나중에 배열 만들어서 gameObject가 아니라 bool값으로 바로 받아올 것.
            return false;
        EnemyData enemyData = FindEnemyData(enemyId);
        GameObject enemyObject = Instantiate(enemyData.EnemyPrefab, bossSpawnPosition[spawnPosIndex], Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        EnemyList.Add(enemy);
        EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
        enemy.SetupEnemy(enemyData, spawnPosIndex);
        bossSpawn[spawnPosIndex] = false;
        NoEnemy = EnemyList.Count == 0;
        //enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }

    public bool SpawnBoss(int enemyId)
    {
        EnemyData enemyData;
        GameObject enemyObject;
        Enemy enemy;
        for (int i = 0; i < bossSpawnPosition.Length; ++i)
        {
            if (!bossSpawn[i]/*enemySpawnPosition[i].gameObject.activeSelf*/)
                continue;
            enemyData = FindEnemyData(enemyId);
            enemyObject = Instantiate(enemyData.EnemyPrefab, bossSpawnPosition[i], Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();
            EnemyList.Add(enemy);
            EnemyDict.Add(enemyObject.GetInstanceID(), enemy);
            enemy.SetupEnemy(enemyData, i);
            bossSpawn[i] = false;
            NoEnemy = EnemyList.Count == 0;
            //enemySpawnPosition[i].gameObject.SetActive(false);
            return true;
        }
        return false;
    }


    public async UniTask KillEnemyCheck(Enemy enemy, UniTask task)
    {
        EnemyList.Remove(enemy);
        EnemyDict.Remove(enemy.gameObject.GetInstanceID());
        /*bool noEnemy */NoEnemy = EnemyList.Count == 0;
        await task;
        enemySpawn[enemy.spawnPosIdx] = true;
        //return noEnemy;
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
