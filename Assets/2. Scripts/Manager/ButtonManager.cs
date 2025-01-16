using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }

    private void Awake() => Instance = this;

    [SerializeField] Button turnEndButton;

    public void TurnEndButton()
    {
        TurnManager.Instance.EndTurn().Forget();
    }

    public void TurnEndButtonInvert(bool state)
    {
        TurnManager.Instance.ChangeCanEnd(state);
        turnEndButton.interactable = state;
    }
}
