using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageContext
{
    //public
    IStage CurrentStage;
    //{
    //    get; set;
    //}
    readonly Map _map;
    public StageContext(Map map)
    {
        _map = map;
    }
    public async UniTask Transition()
    {
        Action action = () =>
        {
            MapManager.Instance.PrevStage = MapManager.Instance.currStage;
            MapManager.Instance.currStage = _map;
            CurrentStage.Enter(_map);
            MapManager.Instance.ShowReward(_map);
        };
        await InGameManager.Instance.Player.ExitAndEnterStage(action);
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage.Enter(_map);
    }
    public async UniTask Transition(IStage stage)
    {
        Action action = () =>
        {
            MapManager.Instance.PrevStage = MapManager.Instance.currStage;
            MapManager.Instance.currStage = _map;
            CurrentStage = stage;
            CurrentStage.Enter(_map);
            MapManager.Instance.ShowReward(_map);
        };
        await InGameManager.Instance.Player.ExitAndEnterStage(action);
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage = stage;
        //CurrentStage.Enter(_map);
    }

    public async UniTask LoadTransition(IStage stage, bool changeScene)
    {
        Action action = () =>
        {
            MapManager.Instance.PrevStage = MapManager.Instance.currStage;
            MapManager.Instance.currStage = _map;
            CurrentStage = stage;
            CurrentStage.Enter(_map);
            MapManager.Instance.ShowReward(_map);
        };
        await InGameManager.Instance.Player.EnterChapterDoor(action, changeScene);
    }
}
