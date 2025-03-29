using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStage : MonoBehaviour, IStage
{
    Map _map;

    public void Enter(Map map)
    {
        if (!_map)
            _map = map;
        if (!map.cleared)
        {
            TurnManager.Instance.StartBattle();
            if (_map.RandEnemyPattern == -1)
            {
                EnemySpawn(0);
            }
            else
            {
                EnemySpawnPattern(_map.RandEnemyPattern);
            }
        }
            print("Enemy");
    }

    public void EnemySpawn(int mapLV)
    {
        switch (mapLV)
        {
            case 0:
                EnemySpawnPattern(Random.Range(0, 3));
                break;
            case 1:
                EnemySpawnPattern(Random.Range(100, 110));
                break;
            case 2:
                EnemySpawnPattern(Random.Range(200, 210));
                break;
        }
        
    }

    public void EnemySpawnPattern(int rand)
    {
        _map.RandEnemyPattern = rand;
        switch (rand)
        {
            case 0:
                EnemyManager.Instance.SpawnEnemy(100, 0);
                EnemyManager.Instance.SpawnEnemy(101, 1);
                break;
            case 1:
                EnemyManager.Instance.SpawnEnemy(100, 0);
                EnemyManager.Instance.SpawnEnemy(100, 1);
                EnemyManager.Instance.SpawnEnemy(100, 2);
                break;
            case 2:
                EnemyManager.Instance.SpawnEnemy(102);
                EnemyManager.Instance.SpawnEnemy(102);
                EnemyManager.Instance.SpawnEnemy(102);
                break;
        }
    }

}
