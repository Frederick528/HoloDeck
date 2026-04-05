using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField]
    Renderer _outLineRenderer;
    [SerializeField]
    Renderer[] _backRenderers;
    [SerializeField]
    Renderer[] _middleRenderers;
    [SerializeField]
    Renderer _frameRenderer;
    [SerializeField]
    Renderer[] _frontRenderers;
    [SerializeField]
    Renderer[] _mostFrontRenderers;
    [SerializeField]
    //string sortingLayerName;

    int originOrder;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;
        SetOrder(originOrder);
    }

    public void SetMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 200 : originOrder);
    }

    public void SetOrder(int order)
    {
        int mulOrder = order * 5;
        _outLineRenderer.sortingOrder = mulOrder - 2;
        foreach (var renderer in _backRenderers)
        {
            //renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder - 1;
        }
        foreach (var renderer in _middleRenderers)
        {
            //renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }
        _frameRenderer.sortingOrder = mulOrder + 1;
        foreach (var renderer in _frontRenderers)
        {
            //renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 2;
        }
        foreach (var renderer in _mostFrontRenderers)
        {
            //renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 3;
        }
    }
}
