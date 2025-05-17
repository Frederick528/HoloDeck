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
        if (!map.cleared)
        {
            TurnManager.Instance.StartBattle();
            if (_map.RandomPattern == -1)
            {
                BossSpawn();
            }
            else
            {
                BossSpawnPattern(_map.RandomPattern);
            }
        }
        print("Boss");
    }

    public void BossSpawn()
    {
        switch (GameManager.Instance.NowChapterLV)
        {
            case 1:
            case 2:
                BossSpawnPattern(Random.Range(0, 1));
                break;
            case 3:
                BossSpawnPattern(Random.Range(100, 110));
                break;
            case 4:
                BossSpawnPattern(Random.Range(200, 210));
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
                print(EnemyManager.Instance.SpawnEnemy(102, 4));
                break;
            case 1:
                EnemyManager.Instance.SpawnEnemy(101, 1);
                break;
            case 2:
                EnemyManager.Instance.SpawnEnemy(102, 1);
                break;
        }
    }
}
