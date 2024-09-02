using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {get; private set;}
    void Awake() => Instance = this;

    [SerializeField] TurnType turnType;
    [SerializeField] int startCardCount;

    public static Action OnAddCard;

    public ReactiveProperty<bool> isLoading = new();
    public bool myTurn;

    enum TurnType { My, Enemy }

    //void GameSetup()
    //{
    //    turnType = TurnType.My;
    //}

    private void Start()
    {
        isLoading.Subscribe(isOn =>
        {
            ButtonManager.instance.TurnEndButtonInvert(!isOn);
            if (isLoading.Value)
                CardManager.Instance.cardState = CardManager.ECardState.Nothing;
            else if (!myTurn)
                CardManager.Instance.cardState = CardManager.ECardState.CanMouseOver;

            else if (myTurn)
                CardManager.Instance.cardState = CardManager.ECardState.CanMouseDrag;
        });
    }

    public void SetBool(bool isOn)
    {
        isLoading.Value = isOn;
    }

    public async UniTask StartTurnTask()        // 시작 뽑기 (수정 필요: OnAddCard가 액션이라 Invoke 사용시, await가 작용하지 않아 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        //GameSetup();
        GameManager.Instance.player.ChangeHoloValue(GameManager.Instance.player.MaxHolo);
        myTurn = true;
        UiManager.instance.ChangeTurnButtonText(myTurn);
        SetBool(true);
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
        SetBool(false);
    }
    public async UniTask DrawTask() // 단일 뽑기 (수정 필요: OnAddCard에 있는 await가 작용하지 않아서, 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        if (CardManager.Instance.HandCard.Count >= 10)
            return;
        SetBool(true);

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
        SetBool(false);
    }
    public async UniTask DrawTask(int drawCardCount)    // 여러 개 뽑기 (수정 필요: 단일 뽑기와 똑같은 문제)
    {
        SetBool(true);
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
        SetBool(false);
    }

    public async UniTask EndTurnTask(bool endBattle = false)
    {
        myTurn = false;
        UiManager.instance.ChangeTurnButtonText(myTurn);
        await CardManager.Instance.ThrowAwayCard();
        SetBool(true);

        if (endBattle)
        {
            CardManager.Instance.ClearCard();
            return;
        }

        EnemyTurnTask().Forget();
    }
    //public async UniTask MyTurnTask(int drawCardValue)  // 나중에 스타트턴이랑 합칠 예정
    //{
    //    GameManager.Instance.player.ChangeHoloValue(GameManager.Instance.player.MaxHolo);
    //    await StartTurnTask();
    //    //isLoading = false;    // 위 코드에서 isLoading = false로 변경
    //    //myTurn = true;
    //}
    public async UniTask EnemyTurnTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));  // 지금은 적 코드가 없으므로 대신 딜레이 코드 추가
        for (int i = 0; i < EnemyManager.Instance.enemies.Count; ++i)
        {
            EnemyManager.Instance.enemies[i].Pattern();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }
        // 적 턴 시작, 적 코드 작성
        // 적 턴이 끝나면 내 턴 시작.
        // 적 턴은 비동기함수 하나로 통침.
        StartTurnTask().Forget();
        // await MyTurnTask(4);
    }

    public void StartBattle()       // 배틀 시작시, 덱 섞기 및 액션 추가
    {
        UiManager.instance.SetupBattleUi(true);
        CardManager.Instance.SetupDrawDeck(true);
        //TurnManager.OnAddCard += async () =>
        //    await AddCard();
        OnAddCard += () =>
            CardManager.Instance.AddCard().Forget();
        StartTurnTask().Forget();
    }

    public void EndBattle()         // 리팩토링 필요해보임.
    {
        UiManager.instance.SetupBattleUi(false);
        OnAddCard = null;
        //TurnManager.OnAddCard -= () =>
        //    AddCard().Forget();
        //TurnManager.OnAddCard = null;
        // TurnManager.OnAddCard -= async () =>
        //     await AddCard();
        EndTurnTask(true).Forget();
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
}
