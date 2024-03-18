using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance { get; private set; }

    private void Awake() => instance = this;

    [SerializeField] GameObject turnEndButton;

    public void TurnEndButtonTask()
    {
        TurnEndButtonInvert(false);
        TurnManager.Instance.EndTurnTask().Forget();
    }

    public void TurnEndButtonInvert(bool state)
    {
        turnEndButton.GetComponent<Button>().interactable = state;
    }
}
