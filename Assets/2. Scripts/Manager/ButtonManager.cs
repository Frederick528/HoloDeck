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
    public Button ActiveItemButton;
    [HideInInspector]
    public Button[] PotionButtons = new Button[4];

    int _nowCardState;

    private void Awake() => Instance = this;

    private void Start()
    {
        _turnEndButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.Battle), "TurnEndButton").GetComponent<Button>();
        DiscardButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.SelectedCard), "DiscardButton").GetComponent<Button>();
        DiscardCancelButton = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.SelectedCard), "CancelButton").GetComponent<Button>();
        ActiveItemButton = UIManager.Instance.ActiveTransform.GetComponent<Button>();

        PotionButtons = UIManager.Instance.PotionTransform.GetComponentsInChildren<Button>();
    }

    public void DiscardBtnInvert(bool state)
    {
        DiscardButton.interactable = state;
    }
    public void DiscardCancelBtnInvert(bool state)
    {
        DiscardCancelButton.interactable = state;
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

    public void LookMap()
    {
        UIManager.Instance.LookMap();
    }
    public void ChangeScene(int idx)
    {
        SceneManager.LoadScene(idx);
    }
}
