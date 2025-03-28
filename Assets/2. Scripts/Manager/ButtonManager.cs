using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }

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
        if (UIManager.Instance == null) { return; }
        _turnEndButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.Battle), "TurnEndButton").GetComponent<Button>();
        DiscardButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.SelectedCard), "DiscardButton").GetComponent<Button>();
        DiscardCancelButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.SelectedCard), "CancelButton").GetComponent<Button>();

        TurnPassiveButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.PassiveTransform, "TurnButton").GetComponentsInChildren<Button>(true);
        //TurnPassiveButton[0].onClick.AddListener(() =>
        //{
        //    ItemManager.Instance.TurnPassiveItemPage(0);
        //});
        //TurnPassiveButton[1].onClick.AddListener(() =>
        //{
        //    ItemManager.Instance.TurnPassiveItemPage(1);
        //});

        ActiveItemButton = UIManager.Instance.ActiveTransform.GetComponent<Button>();

        PotionButtons = UIManager.Instance.PotionTransform.GetComponentsInChildren<Button>();
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
                UIManager.Instance.SetViewDeck(CardManager.Instance.TotalDeck);
                break;
            case 1:
                UIManager.Instance.SetViewDeck(CardManager.Instance.DrawDeck);
                break;
            case 2:
                UIManager.Instance.SetViewDeck(CardManager.Instance.CardDummy);
                break;
        }
    }

    public void ControlStatusWindow()
    {
        Transform statusWindow = UIManager.Instance.StatusWindow;
        if (statusWindow.localPosition.x == -985)
        {
            statusWindow.localPosition = new Vector3(-1460, 0, 0);
        }
        else if (statusWindow.localPosition.x == -1460)
        {
            statusWindow.localPosition = new Vector3(-985, 0, 0);
        }
    }

    public void LookMap()
    {
        UIManager.Instance.LookMap();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx">
    /// 0 = Title
    /// </param>
    public void ChangeScene(int idx)
    {
        SceneManager.LoadScene(idx);
        switch (idx)
        {
            case 0:
                GameManager.Instance.DestroyAllDontDestroyObjects();
                break;
            case 1:
                MapManager.Instance.CreateMapCnt = 15;
                MapManager.Instance.MaxDistance = (3, 3);
                MapManager.Instance.MapScale = 1;
                break;
            case 2:
                MapManager.Instance.CreateMapCnt = 30;
                MapManager.Instance.MaxDistance = (4, 3);
                MapManager.Instance.MapScale = 0.95f;
                UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, false, MapManager.Instance.currStage.rewardBox);
                break;
        }
    }
}
