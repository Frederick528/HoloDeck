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
            if (CardManager.Instance.HandCard.Count >= 10)
                break;
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }
        isLoading = false;
    }
    public async UniTask DrawTask() // 단일 뽑기 (수정 필요: OnAddCard에 있는 await가 작용하지 않아서, 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        if (CardManager.Instance.HandCard.Count >= 10)
            return;
        isLoading = true;

        OnAddCard?.Invoke();
        //if (CardManager.Instance.DrawDeck.Count == 0)
        //    await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

        isLoading = false;
    }
    public async UniTask DrawTask(int drawCardCount)    // 여러 개 뽑기 (수정 필요: 단일 뽑기와 똑같은 문제)
    {
        isLoading = true;
        for (int i = 0; i < drawCardCount; i++)
        {
            if (CardManager.Instance.HandCard.Count >= 10)
            {
                break;
            }
            //await UniTask.RunOnThreadPool(() => OnAddCard?.Invoke());
            OnAddCard?.Invoke();
            //if (CardManager.Instance.DrawDeck.Count == 0)
            //{
            //    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            //}
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

        }
        isLoading = false;
    }

    public async UniTask EndTurnTask()
    {
        myTurn = false;
        await CardManager.Instance.ThrowAwayCard();
        isLoading = true;
    }
    public async UniTask MyTurnTask(int drawCardValue)
    {
        GameManager.Instance.player.ChangeHoloValue(GameManager.Instance.player.maxHolo);
        await StartTurnTask();
        //isLoading = false;    // 위 코드에서 isLoading = false로 변경
        myTurn = true;
    }
}
