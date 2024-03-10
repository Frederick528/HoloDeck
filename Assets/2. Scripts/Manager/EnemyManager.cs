using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    [SerializeField] List<Enemy> enemies;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] List<Vector3> enemySpawnPosition;
    //[SerializeField] Enemy enemy;


    // Start is called before the first frame update
    void Awake()
    {
        enemySpawnPosition.Add(new Vector3(7, 1.5f));
    }

    private void Start()
    {
        SpawnEnemy(80);
    }

    public void SpawnEnemy(int enemyMaxHp, int spawnPosIndex = 0)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, enemySpawnPosition[spawnPosIndex], Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemies.Add(enemy);
        enemy.SetupEntity(enemyMaxHp);
    }
}
