using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance { get; private set; }

    private void Awake() => instance = this;

    public void TurnEndButtonTask()
    {
        TurnManager.Instance.EndTurnTask().Forget();
    }
}
