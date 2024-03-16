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

    public async UniTask StartTurnTask()        // 시작 뽑기 (수정 필요: OnAddCard가 액션이라 Invoke 사용시, await가 작용하지 않아 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        //GameSetup();
        GameManager.Instance.player.ChangeHoloValue(GameManager.Instance.player.maxHolo);
        isLoading = true;
        for (int i = 0; i < startCardCount; i++)
        {
            if (CardManager.Instance.HandCard.Count >= 10)
                break;
            if (CardManager.Instance.DrawDeck.Count == 0)
            {
                OnAddCard?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));    // OnAddCard에서 진행되는 await 따로 실행
            }
            else
            {
                OnAddCard?.Invoke();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.CardAlignmentDelay));        // OnAddCard에서 진행되는 await 따로 실행
        }
        isLoading = false;
        myTurn = true;
        UiManager.instance.ChangeTurnButtonText(myTurn);
    }
    public async UniTask DrawTask() // 단일 뽑기 (수정 필요: OnAddCard에 있는 await가 작용하지 않아서, 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        if (CardManager.Instance.HandCard.Count >= 10)
            return;
        isLoading = true;

        if (CardManager.Instance.DrawDeck.Count == 0)
        {
            OnAddCard?.Invoke();
            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));    // OnAddCard에서 진행되는 await 따로 실행
        }
        else
        {
            OnAddCard?.Invoke();
        }

        await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.CardAlignmentDelay));        // OnAddCard에서 진행되는 await 따로 실행
        isLoading = false;
    }
    public async UniTask DrawTask(int drawCardCount)    // 여러 개 뽑기 (수정 필요: 단일 뽑기와 똑같은 문제)
    {
        isLoading = true;
        for (int i = 0; i < drawCardCount; i++)
        {
            if (CardManager.Instance.HandCard.Count >= 10)
                break;
            if (CardManager.Instance.DrawDeck.Count == 0)
            {
                OnAddCard?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.LoadCardDummyDelay));    // OnAddCard에서 진행되는 await 따로 실행
            }
            else
            {
                OnAddCard?.Invoke();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(CardUtils.CardAlignmentDelay));        // OnAddCard에서 진행되는 await 따로 실행
        }
        isLoading = false;
    }

    public async UniTask EndTurnTask()
    {
        myTurn = false;
        UiManager.instance.ChangeTurnButtonText(myTurn);
        await CardManager.Instance.ThrowAwayCard();
        isLoading = true;
        EnemyTurnTask().Forget();
    }
    //public async UniTask MyTurnTask(int drawCardValue)  // 나중에 스타트턴이랑 합칠 예정
    //{
    //    GameManager.Instance.player.ChangeHoloValue(GameManager.Instance.player.maxHolo);
    //    await StartTurnTask();
    //    //isLoading = false;    // 위 코드에서 isLoading = false로 변경
    //    //myTurn = true;
    //}
    public async UniTask EnemyTurnTask()
    {
        await StartTurnTask();
        // await MyTurnTask(4);
    }
}
