using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public enum CanvasName      // 순서가 Canvas 순서랑 일치해야 함.
    {
        Main,
        PowerUP,
        Character
    }

    Dictionary<int, Transform> _canvasDict = new();

    Transform _canvas;

    [Header("Lobby")]
    TMP_Text _goodsText;

    private void Start()
    {
        _canvas = GameObject.Find("LobbyCanvases").GetComponent<Transform>();
        for (int i = 0; i < /*CanvasList.Count*/_canvas.childCount; ++i)
        {
            //_canvasRaycaster.Add(_canvas.GetChild(i).GetComponent<GraphicRaycaster>());
            _canvasDict.Add(i, _canvas.GetChild(i));
        }
        _goodsText = FindTransform.ContinueFindChildByName(LobbyCanvas(CanvasName.PowerUP), "GoodsText").GetComponent<TMP_Text>();
        _goodsText.text = "Goods: " + GameManager.Instance.Goods.Value.ToString();
    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // 꺼질 때
        {
            switch (canvasName)
            {
                case CanvasName.Main:
                    break;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            _canvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.Main:
                    break;
                case CanvasName.PowerUP:
                    break;
            }
        }

    }
    public Transform LobbyCanvas(CanvasName canvasName)
    {
        return _canvasDict[(int)canvasName];
    }

    public void ChangeScene(int idx)
    {
        GameManager.Instance.ChangeScene(idx);
    }

    [VisibleEnum(typeof(CanvasName))]
    public void OnCanvas(int canvasNameIdx)
    {
        SetActiveCanvas((CanvasName)canvasNameIdx, true);
    }
    [VisibleEnum(typeof(CanvasName))]
    public void OffCanvas(int canvasNameIdx)
    {
        SetActiveCanvas((CanvasName)canvasNameIdx, false);
    }

    public void BuyUpgrade(int idx)
    {

    }
}
