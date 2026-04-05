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
        if (!map.cleared)
        {
            ShopManager.Instance.ChangeCardShop();
            MapManager.Instance.ClearStage(_map).Forget();
        }
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, true);
    }
}
