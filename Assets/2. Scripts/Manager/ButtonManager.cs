using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }

    [SerializeField] Button turnEndButton;
    public Button DiscardButton;
    public Button DiscardCancelButton;

    private void Awake() => Instance = this;

    private void Start()
    {
        DiscardButton = UiManager.Instance.FindChildByName(UiManager.Instance.Canvas(UiManager.CanvasName.SelectedCard), "DiscardButton").GetComponent<Button>();
        DiscardCancelButton = UiManager.Instance.FindChildByName(UiManager.Instance.Canvas(UiManager.CanvasName.SelectedCard), "CancelButton").GetComponent<Button>();
    }

    public void DiscardButtonInvert(bool state)
    {
        DiscardButton.interactable = state;
    }
    public void TurnEndButton()
    {
        TurnManager.Instance.EndTurn().Forget();
    }

    public void TurnEndButtonInvert(bool state)
    {
        TurnManager.Instance.ChangeCanEnd(state);
        turnEndButton.interactable = state;
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
}
