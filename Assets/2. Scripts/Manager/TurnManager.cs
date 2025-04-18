using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {get; private set;}
    void Awake()
    {
        Instance = Instance != null ? Instance : this;
    }

    public bool InBattle { get; private set; }

    [SerializeField] TurnType turnType;

    int startCardCount = 5;

    //public static Action OnAddCard;

    public bool IsLoading;
    public bool MyTurn;

    bool _canEndTurn;

    enum TurnType { My, Enemy }

    public CancellationTokenSource CancelSource = new CancellationTokenSource();

    //void GameSetup()
    //{
    //    turnType = TurnType.My;
    //}

    //private void Start()
    //{
    //    IsLoading.Subscribe(isOn =>
    //    {
    //        ButtonManager.Instance.TurnEndButtonInvert(!isOn);
    //        ChangeCardState().Forget();
    //    });
    //}
    public void ChangeCanEnd(bool canEnd)
    {
        _canEndTurn = canEnd;
    }
    async UniTask ChangeCardState()
    {
        if (IsLoading && !MyTurn)       // 로딩 상태에서 내 턴이 아닌 경우
        {
            CardManager.Instance.SetCardState(0);       // Nothing
            await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, false, PlayerLoopTiming.Update, CancelSource.Token); // 종료 다음에 버리는 시간동안은 확대 안 되게
            CardManager.Instance.SetCardState(1);       // Over
        }
        else if (IsLoading)             // 그냥 로딩 상태(내 턴인 상황에서)
            CardManager.Instance.SetCardState(0);       // Nothing
        else if (MyTurn)
            CardManager.Instance.SetCardState(2);       // Drag
        //if (IsLoading)
        //    CardManager.Instance.CardState = CardManager.ECardState.Nothing;
        //else if (!MyTurn)
        //{
        //    CardManager.Instance.CardState = CardManager.ECardState.CanMouseOver;
        //}
        //else if (MyTurn)
        //    CardManager.Instance.CardState = CardManager.ECardState.CanMouseDrag;
    }

    public void SetLoading(bool isOn)
    {
        IsLoading = isOn;
        ButtonManager.Instance.TurnEndBtnInvert(!isOn);
        ChangeCardState().Forget();
    }

    public void AddStartCardCount(int count)
    {
        startCardCount += count;
        //if (startCardCount < 0)
        //    startCardCount = 0;
    }

    public async UniTask StartTurnTask()        // 시작 뽑기 (수정 필요: OnAddCard가 액션이라 Invoke 사용시, await가 작용하지 않아 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        //GameSetup();
        MyTurn = true;

        InGameManager.Instance.player.AddCurHolo(InGameManager.Instance.player.MaxHolo);
        InGameManager.Instance.player.ShieldReset();

        InGameUIManager.Instance.ChangeTurnButtonText(MyTurn);

        SetLoading(true);
        //await CardManager.Instance.DrawCards(startCardCount);      // DrawCard(int count)로 대체 가능
        await CardManager.Instance.DrawCard(startCardCount);
        //for (int i = 0; i < startCardCount; i++)
        //{
        //    if (CardManager.Instance.HandCard.Count >= 10)
        //        break;
        //    if (CardManager.Instance.DrawDeck.Count == 0)
        //    {
        //        OnAddCard?.Invoke();
        //        await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, CancelSource.Token);    // OnAddCard에서 진행되는 await 따로 실행
        //    }
        //    else
        //    {
        //        OnAddCard?.Invoke();
        //    }

        //    await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, CancelSource.Token);        // OnAddCard에서 진행되는 await 따로 실행
        //}
        SetLoading(false);
    }
    //public async UniTask DrawTask() // 단일 뽑기 (수정 필요: OnAddCard에 있는 await가 작용하지 않아서, 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    //{
    //    if (CardManager.Instance.HandCard.Count >= 10)
    //        return;
    //    SetLoading(true);

    //    if (CardManager.Instance.DrawDeck.Count == 0)
    //    {
    //        OnAddCard?.Invoke();
    //        await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, CancelSource.Token);    // OnAddCard에서 진행되는 await 따로 실행
    //    }
    //    else
    //    {
    //        OnAddCard?.Invoke();
    //    }

    //    await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, CancelSource.Token);        // OnAddCard에서 진행되는 await 따로 실행
    //    SetLoading(false);
    //}
    //public async UniTask DrawTask(int drawCardCount)    // 여러 개 뽑기 (수정 필요: 단일 뽑기와 똑같은 문제)
    //{
    //    SetLoading(true);
    //    for (int i = 0; i < drawCardCount; i++)
    //    {
    //        if (CardManager.Instance.HandCard.Count >= 10)
    //            break;
    //        if (CardManager.Instance.DrawDeck.Count == 0)
    //        {
    //            OnAddCard?.Invoke();
    //            await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, CancelSource.Token);    // OnAddCard에서 진행되는 await 따로 실행
    //        }
    //        else
    //        {
    //            OnAddCard?.Invoke();
    //        }

    //        await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, CancelSource.Token);        // OnAddCard에서 진행되는 await 따로 실행
    //    }
    //    SetLoading(false);
    //}

    //public async UniTask DrawCardTask(int drawCardCount = 1)
    //{
    //    SetLoading(true);
    //    if (drawCardCount == 1)
    //    {
    //        await CardManager.Instance.DrawCard();
    //    }
    //    else
    //    {
    //        await CardManager.Instance.DrawCards(drawCardCount);
    //    }
    //    SetLoading(false);
    //}

    public async UniTask EndTurn(bool endBattle = false)
    {
        if (InGameManager.Instance.PauseInt != 0) return;     // Pause 상태면 턴종 불가능
        if (!_canEndTurn && !endBattle) return;             // 턴종 가능한지 확인, 단, 배틀 종료 상태에서는 턴종 가능한가와 상관없이 진행
        MyTurn = false;
        ButtonManager.Instance.TurnEndBtnInvert(MyTurn);
        InGameUIManager.Instance.ChangeTurnButtonText(MyTurn);
        SetLoading(true);
        await CardManager.Instance.ThrowAwayCard();
        if (endBattle)
        {
            CardManager.Instance.ClearCard();
            InGameManager.Instance.player.ShieldReset();
            if (EnemyManager.Instance.EnemyList.Count > 0)
            {
                foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
                {
                    Destroy(enemy.gameObject);
                }
                EnemyManager.Instance.EnemyList.Clear();
                EnemyManager.Instance.CanEnemySpawn(true);
            }
            return;
        }

        EnemyTurnTask().Forget();
    }
    //public async UniTask MyTurnTask(int drawCardValue)  // 나중에 스타트턴이랑 합칠 예정
    //{
    //    InGameManager.Instance.player.ChangeHoloValue(InGameManager.Instance.player.MaxHolo);
    //    await StartTurnTask();
    //    //IsLoading = false;    // 위 코드에서 IsLoading = false로 변경
    //    //MyTurn = true;
    //}
    public async UniTask EnemyTurnTask()
    {
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, false, PlayerLoopTiming.Update, CancelSource.Token);  // 카드 다 버린 이후 적 행동 시작
        for (int i = 0; i < EnemyManager.Instance.EnemyList.Count; ++i)
        {
            await EnemyManager.Instance.EnemyList[i].Pattern();
            await UniTask.WaitForSeconds(0.5f/*, false, PlayerLoopTiming.Update, CancelSource.Token*/); // 적 코드 이후 잠시 딜레이 (적이 공격 중에는 죽을 일 없으니 토큰 안 쓰기)
        }
        // 적 턴 시작, 적 코드 작성
        // 적 턴이 끝나면 내 턴 시작.
        // 적 턴은 비동기함수 하나로 통침.
        StartTurnTask().Forget();
        // await MyTurnTask(4);
    }

    public void StartBattle()       // 배틀 시작시, 덱 섞기 및 액션 추가
    {
        InBattle = true;
        InGameManager.Instance.player.StartOrEndBattle(InBattle);
        CancelSource = new();

        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Battle, true);
        CardManager.Instance.SetupDrawDeck(true);
        //TurnManager.OnAddCard += async () =>
        //    await DrawCard();
        //OnAddCard += () =>
        //    CardManager.Instance.DrawCard().Forget();


        StartTurnTask().Forget();
    }

    public async UniTask EndBattle()         // 리팩토링 필요해보임.
    {
        InBattle = false;
        InGameManager.Instance.player.StartOrEndBattle(InBattle);
        CancelSource.Cancel();
        InGameManager.Instance.AbilityEventQueue.QueueClear();

        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Battle, false);
        //OnAddCard = null;

        //TurnManager.OnAddCard -= () =>
        //    DrawCard().Forget();
        //TurnManager.OnAddCard = null;
        // TurnManager.OnAddCard -= async () =>
        //     await DrawCard();
        await EndTurn(true);
        //DrawDeck.Clear();
        //CardDummy.Clear();
        ////HandCard.Clear();
        //for (int i = 0; i < Deck.childCount; i++)
        //{
        //    if (!Deck.GetChild(i).gameObject.activeSelf)
        //        continue;
        //    Card card = Deck.GetComponentsInChildren<Card>(true)[i];
        //    //GameObject cardObject = Deck.GetChild(i).gameObject;
        //    if (MainDeck.Contains(card))
        //        continue;
        //    card.CardRelease();
        //    //MainCardDeck.Add(card);
        //    //cardObject.GetComponent<Card>().CardRelease();

        //}
        //foreach (Card card in MainCardDeck)
        //{
        //    card.CardPool.Release(card.gameObject);
        //}
        //MainCardDeck.Clear();

        //foreach (Card card in MainCardDeck)
        //{
        //    AddDeck(card.Data, EAddDeck.Main);
        //}

    }

    private void OnDestroy()
    {
        CancelSource.Cancel();
        CancelSource.Dispose();
    }
}
