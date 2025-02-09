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
            //EnemySpawn(0);
        }
        print("Boss");
    }
}
