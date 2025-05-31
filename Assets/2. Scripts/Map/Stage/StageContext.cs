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
    public void Transition()
    {
        Action action = () =>
        {
            CurrentStage.Enter(_map);
        };
        InGameManager.Instance.Player.ExitAndEnterStage(action).Forget();
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage.Enter(_map);
    }
    public void Transition(IStage stage)
    {
        Action action = () =>
        {
            CurrentStage = stage;
            CurrentStage.Enter(_map);
        };
        InGameManager.Instance.Player.ExitAndEnterStage(action).Forget();
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage = stage;
        //CurrentStage.Enter(_map);
    }
}
