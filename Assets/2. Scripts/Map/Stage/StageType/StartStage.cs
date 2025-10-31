using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartStage : MonoBehaviour, IStage
{
    Map _map;

    public void Enter(Map map)
    {
        if (!_map)
            _map = map;
        print("Start");
        if (!map.cleared)
        {
            MapManager.Instance.ClearStage(_map).Forget();
        }
        else
        {
            MapManager.Instance.ShowPreviousDoor(true);
        }
    }
}
