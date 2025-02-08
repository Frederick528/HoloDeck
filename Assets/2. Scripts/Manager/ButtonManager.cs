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

    int _nowCardState;

    private void Awake() => Instance = this;

    private void Start()
    {
        _turnEndButton = UiManager.Instance.FindChildByName(UiManager.Instance.Canvas(UiManager.CanvasName.Battle), "TurnEndButton").GetComponent<Button>();
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
        _turnEndButton.interactable = state;
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
        GameManager.Instance.Pause(isOn);
    }

    public void SetViewDeck(int deckIdx)
    {
        switch (deckIdx)
        {
            case 0:
                UiManager.Instance.SetViewDeck(CardManager.Instance.DrawDeck);
                break;
            case 1:
                UiManager.Instance.SetViewDeck(CardManager.Instance.CardDummy);
                break;
        }
    }

    public void ChangeScene(int idx)
    {
        SceneManager.LoadScene(idx);
    }
}
