using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;
public struct Upgrade
{
    public int Index;
    public int CurLV;
    public int MaxLV;
    public int[] Value;
    public int[] Cost;
    string _title;
    //public TMP_Text TitleText;
    //public Image ValueBar;
    //public TMP_Text ValueText;
    //public TMP_Text CostText;
    public Upgrade(int idx, int LV/*, TMP_Text[] texts, Image valueBar*/)
    {
        Index = idx;
        CurLV = LV;
        //TitleText = texts[0];
        //ValueBar = valueBar;
        //ValueText = texts[1];
        //CostText = texts[2];

        switch (Index)
        {
            case 0:
                MaxLV = 5;
                Value = new int[6] { 0, 10, 20, 30, 40, 50 };
                Cost = new int[5] { 0, 200, 300, 400, 500 };
                _title = $"Buy HP Upgrade";
                break;
            case 1:
                MaxLV = 5;
                Value = new int[6] { 0, 1, 2, 3, 4, 5 };
                Cost = new int[5] { 0, 200, 300, 400, 500 };
                _title = $"Buy ATK Upgrade";
                break;
            case 2:
                MaxLV = 5;
                Value = new int[6] { 0, 1, 2, 3, 4, 5 };
                Cost = new int[5] { 100, 200, 300, 400, 500 };
                _title = $"Buy DEF Upgrade";
                break;
            case 3:
                MaxLV = 5;
                Value = new int[6] { 0, 1, 2, 3, 4, 5 };
                Cost = new int[5] { 100, 200, 300, 400, 500 };
                _title = $"Buy HEAL Upgrade";
                break;
            case 4:
                MaxLV = 4;
                Value = new int[5] { 0, 5, 10, 15, 20 };
                Cost = new int[4] { 0, 200, 300, 400 };
                _title = $"Buy Critical Chance Upgrade";
                break;
            case 5:
                MaxLV = 4;
                Value = new int[5] { 0, 10, 20, 30, 40 };
                Cost = new int[4] { 0, 200, 300, 400 };
                _title = $"Buy Critical Damage Upgrade";
                break;
            case 6:
                MaxLV = 2;
                Value = new int[3] { 0, 1, 2 };
                Cost = new int[2] { 0, 600 };
                _title = $"Buy Resurrection Upgrade";
                break;
            case 7:
                MaxLV = 2;
                Value = new int[3] { 0, 25, 50 };
                Cost = new int[2] { 300, 600 };
                _title = $"Buy Coin Gained Upgrade";
                break;
            default:
                MaxLV = 0;
                Value = new int[0];
                Cost = new int[0];
                _title = "";
                break;
        }

        //TitleText.text = $"{_title} [{CurLV}/{MaxLV}]";
        //ValueText.text = $"+ {Value[CurLV]}";
        //CostText.text = $"Cost: {Cost[CurLV]}";
        //ValueBar.fillAmount = CurLV / MaxLV;
    }

    public void Setting(ref TMP_Text title, ref TMP_Text value, ref TMP_Text cost, ref Image valueBar)
    {
        title.text = $"{_title} [{CurLV}/{MaxLV}]";
        switch (Index)
        {
            case 4:
            case 5:
            case 7:
                value.text = $"+ {Value[CurLV]} %p";
                break;
            default:
                value.text = $"+ {Value[CurLV]}";
                break;
        }
        cost.text = $"Cost: {Cost[CurLV]}";
        valueBar.fillAmount = (float)CurLV / MaxLV;
    }

    public bool PowerUP(int LV)
    {
        if (CurLV == MaxLV) return false;
        if (Cost[CurLV] > GameManager.Instance.Goods.Value) return false;

        GameManager.Instance.AddGoods(-Cost[CurLV]);

        CurLV = LV;

        GameManager.Instance.ChangeUpgrade(Index, Value[CurLV]);
        //TitleText.text = $"{_title} [{CurLV}/{MaxLV}]";
        //ValueText.text = $"+ {Value[CurLV]}";
        //CostText.text = $"Cost: {Cost[CurLV]}";
        //ValueBar.fillAmount = (float)CurLV / MaxLV;

        return true;
    }
}
public class LobbyManager : MonoBehaviour
{
    public enum CanvasName      // 순서가 Canvas 순서랑 일치해야 함.
    {
        Main,
        PowerUP,
        Character
    }

    Dictionary<int, Transform> _canvasDict = new();

    //Transform _canvas;

    [Header("Lobby")]
    TMP_Text _goodsText;

    TMP_Text[] _titleText = new TMP_Text[8];
    TMP_Text[] _valueText = new TMP_Text[8];
    TMP_Text[] _costText = new TMP_Text[8];
    Image[] _valueBar = new Image[8];

    //Upgrade[] _upgrade;

    private void Start()
    {
        Transform canvas = GameObject.Find("LobbyCanvases").GetComponent<Transform>();
        for (int i = 0; i < /*CanvasList.Count*/canvas.childCount; ++i)
        {
            //_canvasRaycaster.Add(_canvas.GetChild(i).GetComponent<GraphicRaycaster>());
            _canvasDict.Add(i, canvas.GetChild(i));
        }
        _goodsText = FindTransform.ContinueFindChildByName(LobbyCanvas(CanvasName.PowerUP), "GoodsText").GetComponent<TMP_Text>();
        _goodsText.text = "Goods: " + GameManager.Instance.Goods.Value.ToString();

        Transform upgradeTr = FindTransform.ContinueFindChildByName(LobbyCanvas(CanvasName.PowerUP), "Upgrade");

        for (int i = 0; i < upgradeTr.childCount; ++i)
        {
            TMP_Text[] tMP_Texts = upgradeTr.GetChild(i).GetComponentsInChildren<TMP_Text>();
            _titleText[i] = tMP_Texts[0];
            _valueText[i] = tMP_Texts[1];
            _costText[i] = tMP_Texts[2];
            _valueBar[i] = FindTransform.ContinueFindChildByName(upgradeTr.GetChild(i), "ValueBar").GetComponent<Image>();

            GameManager.Instance.Upgrades[i].Setting(ref _titleText[i], ref _valueText[i], ref _costText[i], ref _valueBar[i]);
        }
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
        GameManager.Instance.ChangeScene(idx).Forget();
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
        if (GameManager.Instance.Upgrades[idx].PowerUP(GameManager.Instance.Upgrades[idx].CurLV + 1))
        {
            GameManager.Instance.Upgrades[idx].Setting(ref _titleText[idx], ref _valueText[idx], ref _costText[idx], ref _valueBar[idx]);
            _goodsText.text = "Goods: " + GameManager.Instance.Goods.Value.ToString();
        }
    }
}
