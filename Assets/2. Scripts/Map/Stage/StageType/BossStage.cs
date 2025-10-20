using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStage : MonoBehaviour, IStage
{
    Map _map;

    public void Enter(Map map)
    {
        if (!_map)
            _map = map;
        if (!_map.cleared)
        {
            print("Boss1");
            if (_map.RandomPattern == -1)
            {
                BossSpawn();
                print("Boss2"); 
            }
            else
            {
                BossSpawnPattern(_map.RandomPattern);
                print("Boss3"); 
            }
            TurnManager.Instance.StartBattle();
        }
        else
        {
            print("Boss4");  
            MapManager.Instance.ShowNextDoor(true);
            MapManager.Instance.ShowPreviousDoor(true);
        }
        print("Boss5");
    }

    public void BossSpawn()
    {
        switch (GameManager.Instance.NowChapterLV)
        {
            case 1:
                BossSpawnPattern(Random.Range(0, 0));
                break;
            case 2:
                BossSpawnPattern(Random.Range(100, 100));
                break;
            case 3:
                BossSpawnPattern(Random.Range(200, 202));
                break;
            case 4:
                BossSpawnPattern(Random.Range(300, 300));
                break;
        }

    }

    public void BossSpawnPattern(int rand)
    {
        _map.RandomPattern = rand;
        switch (rand)
        {
            case 0:
                EnemyManager.Instance.SpawnEnemy(100, 0);       // 잠시 SpawnEnemy 사용. 나중에 Boss로 바꿀거임.
                EnemyManager.Instance.SpawnEnemy(101, 1);
                EnemyManager.Instance.SpawnEnemy(102, 2);
                EnemyManager.Instance.SpawnEnemy(102, 3);
                break;
            case 100:
                EnemyManager.Instance.SpawnEnemy(500, 2);
                EnemyManager.Instance.SpawnEnemy(501, 3);
                break;
            case 200:
                EnemyManager.Instance.SpawnEnemy(900, 2);
                break;
            case 201:
                EnemyManager.Instance.SpawnEnemy(901, 2);
                break;
            case 300:
                EnemyManager.Instance.SpawnEnemy(1000, 2);
                break;
        }
    }
}
