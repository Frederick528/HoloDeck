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
        CurrentStage.Enter(_map);
    }
    public void Transition(IStage stage)
    {
        CurrentStage = stage;
        CurrentStage.Enter(_map);
    }
}
