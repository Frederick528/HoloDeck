using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance { get; private set; }

    private void Awake() => instance = this;

    [SerializeField] Button turnEndButton;

    public void TurnEndButton()
    {
        TurnEndButtonInvert(false);
        TurnManager.Instance.EndTurn().Forget();
    }

    public void TurnEndButtonInvert(bool state)
    {
        turnEndButton.interactable = state;
    }
}
