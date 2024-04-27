using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventStage : MonoBehaviour, IStage
{
    Map _map;

    public void Enter(Map map)
    {
        if (!_map)
            _map = map;
        print("Event!!");
    }
}
