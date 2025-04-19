using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class InGameButtonManager : MonoBehaviour
{
    public static InGameButtonManager Instance { get; private set; }

    Button _turnEndButton;
    [HideInInspector]
    public Button DiscardButton;
    [HideInInspector]
    public Button DiscardCancelButton;
    [HideInInspector]
    public Button[] TurnPassiveButton = new Button[2];
    [HideInInspector]
    public Button ActiveItemButton;
    [HideInInspector]
    public Button[] PotionButtons = new Button[4];

    int _nowCardState;

    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
    }

    private void Start()
    {
        if (InGameUIManager.Instance == null) { return; }
        _turnEndButton = FindTransform.ContinueFindChildByName(InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.Battle), "TurnEndButton").GetComponent<Button>();
        DiscardButton = FindTransform.ContinueFindChildByName(InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard), "DiscardButton").GetComponent<Button>();
        DiscardCancelButton = FindTransform.ContinueFindChildByName(InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard), "CancelButton").GetComponent<Button>();

        TurnPassiveButton = FindTransform.ContinueFindChildByName(InGameUIManager.Instance.PassiveTransform, "TurnButton").GetComponentsInChildren<Button>(true);
        //TurnPassiveButton[0].onClick.AddListener(() =>
        //{
        //    ItemManager.Instance.TurnPassiveItemPage(0);
        //});
        //TurnPassiveButton[1].onClick.AddListener(() =>
        //{
        //    ItemManager.Instance.TurnPassiveItemPage(1);
        //});

        ActiveItemButton = InGameUIManager.Instance.ActiveTransform.GetComponent<Button>();

        PotionButtons = InGameUIManager.Instance.PotionTransform.GetComponentsInChildren<Button>();
    }
    [VisibleEnum(typeof(InGameUIManager.CanvasName))]
    public void OnCanvas(int canvasNameIdx)
    {
        InGameUIManager.Instance.SetActiveCanvas((InGameUIManager.CanvasName)canvasNameIdx, true);
    }
    [VisibleEnum(typeof(InGameUIManager.CanvasName))]
    public void OffCanvas(int canvasNameIdx)
    {
        InGameUIManager.Instance.SetActiveCanvas((InGameUIManager.CanvasName)canvasNameIdx, false);
    }
    public void TurnEndBtn()
    {
        TurnManager.Instance.EndTurn().Forget();
    }

    public void TurnEndBtnInvert(bool state)
    {
        TurnManager.Instance.ChangeCanEnd(state);
        _turnEndButton.interactable = state;
    }

    public void DiscardBtnInvert(bool state)
    {
        DiscardButton.interactable = state;
    }
    public void SetActiveDiscardCancelBtn(bool state)
    {
        DiscardCancelButton.gameObject.SetActive(state);
    }

    public void SetActiveTurnPassiveBtn(bool state)
    {
        TurnPassiveButton[0].gameObject.SetActive(state);
        TurnPassiveButton[1].gameObject.SetActive(state);
    }
    public void ActItemBtnInvert(bool state)
    {
        ActiveItemButton.interactable = state;
    }

    public void RewardedCardBtn()
    {
        CardManager.Instance.RewardedCard();
    }

    public void BuyCardBtn()
    {
        ShopManager.Instance.BuyCard();
    }
    public void SetBuyCardIdx(int idx)
    {
        ShopManager.Instance.BuyCardIdx(idx);
    }

    public void Pause(bool isOn)
    {
        InGameManager.Instance.Pause(isOn);
    }

    public void SetViewDeck(int deckIdx)
    {
        switch (deckIdx)
        {
            case 0:
                InGameUIManager.Instance.SetViewDeck(CardManager.Instance.TotalDeck);
                break;
            case 1:
                InGameUIManager.Instance.SetViewDeck(CardManager.Instance.DrawDeck);
                break;
            case 2:
                InGameUIManager.Instance.SetViewDeck(CardManager.Instance.CardDummy);
                break;
        }
    }

    public void ControlStatusWindow()
    {
        InGameUIManager.Instance.ShowStatus();
    }

    public void EventReward(bool agree)
    {
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Event, false);
        MapManager.Instance.ClearStage().Forget();
    }

    public void LookMap()
    {
        InGameUIManager.Instance.LookMap();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx">
    /// 0 = Title
    /// </param>
    public void ChangeScene(int idx)
    {
        GameManager.Instance.ChangeScene(idx);
        //switch (idx)
        //{
        //    case 0:
        //        GameManager.Instance.DestroyAllInGameDontDestroyObjects();
        //        break;
        //    case 1:
        //        InGameManager.Instance.NowChapterLV = 1;
        //        MapManager.Instance.CreateMapCnt = 15;
        //        MapManager.Instance.MaxDistance = (3, 3);
        //        MapManager.Instance.MapScale = 1;
        //        break;
        //    case 2:
        //        InGameManager.Instance.NowChapterLV = 2;
        //        MapManager.Instance.CreateMapCnt = 30;
        //        MapManager.Instance.MaxDistance = (4, 3);
        //        MapManager.Instance.MapScale = 0.95f;
        //        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, MapManager.Instance.currStage.rewardBox);
        //        break;
        //}
    }

    public void NextChapter()
    {
        ChangeScene(++GameManager.Instance.NowChapterLV);
    }
}
