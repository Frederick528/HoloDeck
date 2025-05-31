using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EventStage : MonoBehaviour, IStage
{
    Map _map;
    //Button _eventAgreeButton;
    //Button _eventDisagreeButton;

    public void Enter(Map map)
    {
        if (!_map)
        {
            _map = map;
        }
        //if (!_eventAgreeButton)
        //{
        //    Button[] buttons = InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.Event).GetComponentsInChildren<Button>();
        //    _eventAgreeButton = buttons[0];
        //    _eventDisagreeButton = buttons[1];
        //}
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
                InGameUIManager.Instance.EventText.text = "수락 시, 맵을 밝힙니다.\n거절 시, 체력을 10 회복합니다.";
                SetEventButton(() =>
                {
                    MapManager.Instance.ShowAllMap();
                },
                () =>
                {
                    InGameManager.Instance.Player.Heal(10).Forget();
                });
                break;
            case 1:
                InGameUIManager.Instance.EventText.text = "수락 시, 맵을 밝힙니다.\n거절 시, 체력을 10 회복합니다.";
                SetEventButton(() =>
                {
                    MapManager.Instance.ShowAllMap();
                },
                () =>
                {
                    InGameManager.Instance.Player.Heal(10).Forget();
                });
                break;
            case 2:
                InGameUIManager.Instance.EventText.text = "수락 시, 맵을 밝힙니다.\n거절 시, 체력을 10 회복합니다.";
                SetEventButton(() =>
                {
                    MapManager.Instance.ShowAllMap();
                },
                () =>
                {
                    InGameManager.Instance.Player.Heal(10).Forget();
                });
                break;
        }
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Event, true);
    }
    public void SetEventButton(Action agree, Action disagree)
    {
        InGameButtonManager.Instance.EventAgreeAction = agree;
        InGameButtonManager.Instance.EventDisagreeAction = disagree;
    }
}
