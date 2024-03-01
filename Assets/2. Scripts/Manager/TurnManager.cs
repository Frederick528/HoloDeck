using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {get; private set;}
    void Awake() => Instance = this;

    [SerializeField] TurnType turnType;
    [SerializeField] int startCardCount;

    public static Action OnAddCard;

    public bool isLoading;
    public bool myTurn;

    enum TurnType { My, Enemy }

    //void GameSetup()
    //{
    //    turnType = TurnType.My;
    //}

    public async UniTask StartTurnTask()
    {
        //GameSetup();
        isLoading = true;
        for (int i = 0; i < startCardCount; i++)
        {
            OnAddCard?.Invoke();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        }
        isLoading = false;
    }
    public async UniTask DrawTask()
    {
        isLoading = true;

        OnAddCard?.Invoke();
        await UniTask.Delay(TimeSpan.FromSeconds(0.7f));

        isLoading = false;
    }

    public void EndTurn()
    {
        myTurn = false;
        isLoading = true;
        CardManager.Instance.ThrowAwayCard();
    }
}
