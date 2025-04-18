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
        if (!map.cleared)
        {
            if (_map.RandomPattern == -1)
            {
                Event();
            }
            else
            {
                EventPattern(_map.RandomPattern);
            }
        }
        print("Event!!");
    }

    public void Event()
    {
        switch (GameManager.Instance.NowChapterLV)
        {
            case 1:
            case 2:
                EventPattern(Random.Range(0, 3));
                break;
            case 3:
                EventPattern(Random.Range(100, 110));
                break;
            case 4:
                EventPattern(Random.Range(200, 210));
                break;
        }

    }

    public void EventPattern(int rand)
    {
        _map.RandomPattern = rand;
        switch (rand)
        {
            case 0:
            case 1:
            case 2:
                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Event, true);
                break;
        }
    }
}
