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
            EnemyManager.Instance.SpawnEnemy(100, 0);
            EnemyManager.Instance.SpawnEnemy(101, 1);
        }
            print("Enemy");
    }

}
