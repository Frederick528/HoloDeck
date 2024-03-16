using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public List<Enemy> enemies;
    [SerializeField] GameObject enemyPrefab;
    public List<Transform> enemySpawnPosition;
    public Enemy targetEnemy;
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
        
    }

    public bool SpawnEnemy(int enemyMaxHp, int spawnPosIndex = 0)
    {
        if (!enemySpawnPosition[spawnPosIndex].gameObject.activeSelf)
            return false;
        GameObject enemyObject = Instantiate(enemyPrefab, enemySpawnPosition[spawnPosIndex].position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemies.Add(enemy);
        enemy.SetupEnemy(enemyMaxHp, spawnPosIndex);
        enemySpawnPosition[spawnPosIndex].gameObject.SetActive(false);
        return true;
    }
}
