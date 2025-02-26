using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopStage : MonoBehaviour, IStage
{
    Map _map;

    public void Enter(Map map)
    {
        if (!_map)
            _map = map;
        print("Shop");

        UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.Shop, true);
    }
}
