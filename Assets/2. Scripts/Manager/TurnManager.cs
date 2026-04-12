using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {get; private set;}
    public enum TurnType { Player, Enemy, Nobody }

    private ReactiveProperty<bool> _InBattle = new();
    public IReadOnlyReactiveProperty<bool> InBattle => _InBattle;
    //public bool InBattle { get; private set; }

    public TurnType CurTurnType { get; private set; }

    int startCardCount = 5;

    //public static Action OnAddCard;

    public bool IsLoading { get; private set; }
    //public bool MyTurn;

    bool _canEndTurn;

    int _turnAttackCount = 0;

    int _turnSkillCount =0;

    int _battleAttackCount = 0;
    int _battleSkillCount = 0;

    int _battleZeroCostCount = 0;

    bool _isFirstCardPlayed = false;

    public CancellationTokenSource CancelSource = new();

    public event Action OnBattleStart;
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyTurnEnd;
    public event Action OnBattleEnd;

    void Awake()
    {
        Instance = Instance != null ? Instance : this;
        CurTurnType = TurnType.Nobody;
    }
    public void AddAttackCardsPlayed()
    {
        _turnAttackCount++;
        _battleAttackCount++;
        if (!_isFirstCardPlayed)
        {
            _isFirstCardPlayed = true;
        }
        CardManager.Instance.NotifyActionProgress(SpecialTagType.UsedAttackCard, 1);
        GameEvents.NotifyPlayState();
    }
    public void AddSkillCardsPlayed()
    {
        _turnSkillCount++;
        _battleSkillCount++;
        if (!_isFirstCardPlayed)
        {
            _isFirstCardPlayed = true;
        }
        CardManager.Instance.NotifyActionProgress(SpecialTagType.UsedSkillCard, 1);
        GameEvents.NotifyPlayState();
    }
    public void AddZeroCardsPlayed()
    {
        _battleZeroCostCount++;
        GameEvents.NotifyPlayState();
    }
    public void UsedFirstCardPlayed()
    {
        _isFirstCardPlayed = true;
        GameEvents.NotifyPlayState();
    }
    public int GetTurnAttackCount()
    {

        return _turnAttackCount;
    }
    public int GetTurnSkillCount()
    {

        return _turnSkillCount;
    }
    public int GetBattleAttackCount()
    {
        return _battleAttackCount;
    }
    public int GetBattleSkillCount()
    {

        return _battleSkillCount;
    }
    public int GetBattleZeroCostCount()
    {

        return _battleZeroCostCount;
    }
    public bool GetFirstCardPlayed()
    {

        return _isFirstCardPlayed;
    }


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
        if (IsLoading && CurTurnType != TurnType.Player)       // 로딩 상태에서 내 턴이 아닌 경우
        {
            CardManager.Instance.SetCardState(0);       // Nothing
            await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, cancellationToken: CancelSource.Token); // 종료 다음에 버리는 시간동안은 확대 안 되게
            CardManager.Instance.SetCardState(1);       // Over
        }
        else if (IsLoading)             // 그냥 로딩 상태(내 턴인 상황에서)
            CardManager.Instance.SetCardState(0);       // Nothing
        else if (CurTurnType == TurnType.Player)
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
        InGameButtonManager.Instance.TurnEndBtnInvert(!isOn);
        InGameUIManager.Instance.ChangeTurnButtonText(!isOn);
        ChangeCardState().Forget();
    }

    public void AddStartCardCount(int count)
    {
        startCardCount += count;
        //if (startCardCount < 0)
        //    startCardCount = 0;
    }

    void ResetTurnStart()
    {
        _turnAttackCount = 0;
        _turnSkillCount = 0;
        _isFirstCardPlayed = false;
    }

    void ResetBattleStart()
    {
        _battleAttackCount = 0;
        _battleSkillCount = 0;
        _battleZeroCostCount = 0;
    }

    public void StartBattle()
    {
        _InBattle.Value = true;
        CancelSource = new();

        // 1. 배틀 시작 이벤트 실행
        OnBattleStart?.Invoke();
        
        ResetBattleStart();

        // 배틀 시작 시 무조건 내 턴부터 시작
        StartPlayerTurn().Forget();

    }

    public async UniTask StartPlayerTurn()        // 시작 뽑기 (수정 필요: OnAddCard가 액션이라 Invoke 사용시, await가 작용하지 않아 카드덱이 0개일 경우, 0.5초 뒤에 뽑는 것이 적용되지 않음.)
    {
        //GameSetup();
        CurTurnType = TurnType.Player;

        ResetTurnStart();
        //BattleManager.Instance.HitEntity.Item1 = null;
        //MyTurn = true;
        OnPlayerTurnStart?.Invoke();
        //InGameManager.Instance.Player.AddCurHolo(InGameManager.Instance.Player.MaxHolo);
        //InGameManager.Instance.Player.ShieldReset();
        //ResetTurnStart();

        //foreach (var enemy in EnemyManager.Instance.EnemyList)
        //{
        //    enemy.NextPattern();
        //}

        //InGameUIManager.Instance.ChangeTurnButtonText(true);

        SetLoading(true);


        //await CardManager.Instance.DrawCards(startCardCount);      // DrawCard(int count)로 대체 가능
        await CardManager.Instance.DrawCard(startCardCount);
        await InGameManager.Instance.Player.OnTurnStartTask();
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

    public async UniTask EndPlayerTurn(bool endBattle = false)
    {
        if (InGameManager.Instance.PauseInt != 0) return;     // Pause 상태면 턴종 불가능
        if (!_canEndTurn && !endBattle) return;             // 턴종 가능한지 확인, 단, 배틀 종료 상태에서는 턴종 가능한가와 상관없이 진행

        CurTurnType = TurnType.Nobody;

        OnPlayerTurnEnd?.Invoke();
        //MyTurn = false;
        //InGameButtonManager.Instance.TurnEndBtnInvert(false);
        //InGameUIManager.Instance.ChangeTurnButtonText(false);
        SetLoading(true);

        await CardManager.Instance.ThrowAwayCard();
        InGameManager.Instance.Player.TurnStatusEffect(); // 상태 이상 정산
        StartEnemyTurn().Forget();
        //if (endBattle)
        //{
        //    if (EnemyManager.Instance.EnemyList.Count > 0)
        //    {
        //        foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
        //        {
        //            Destroy(enemy.gameObject);
        //        }
        //        EnemyManager.Instance.EnemyList.Clear();
        //        EnemyManager.Instance.CanEnemySpawn(true);
        //    }
        //    InGameManager.Instance.Player.ShieldReset();
        //    InGameManager.Instance.Player.RemoveStatusEffect();
        //    await CardManager.Instance.ThrowAwayCard();
        //    CardManager.Instance.ClearCard();
        //}
        //else
        //{
        //    await CardManager.Instance.ThrowAwayCard();
        //    InGameManager.Instance.Player.TurnStatusEffect();
        //    StartEnemyTurn().Forget();
        //}
        //if (endBattle)
        //{
        //    CardManager.Instance.ClearCard();
        //    InGameManager.Instance.Player.ShieldReset();
        //    InGameManager.Instance.Player.RemoveStatusEffect();
        //    //if (EnemyManager.Instance.EnemyList.Count > 0)
        //    //{
        //    //    foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
        //    //    {
        //    //        Destroy(enemy.gameObject);
        //    //    }
        //    //    EnemyManager.Instance.EnemyList.Clear();
        //    //    EnemyManager.Instance.CanEnemySpawn(true);
        //    //}
        //    return;
        //}
        //InGameManager.Instance.Player.TurnStatusEffect();
        //StartEnemyTurn().Forget();
    }
    //public async UniTask MyTurnTask(int drawCardValue)  // 나중에 스타트턴이랑 합칠 예정
    //{
    //    InGameManager.Instance.player.ChangeHoloValue(InGameManager.Instance.player.MaxHolo);
    //    await StartPlayerTurn();
    //    //IsLoading = false;    // 위 코드에서 IsLoading = false로 변경
    //    //MyTurn = true;
    //}
    public async UniTask StartEnemyTurn()
    {
        CurTurnType = TurnType.Enemy;

        //BattleManager.Instance.HitEntity.Item2 = null;
        //EnemyManager.Instance.HitEnemy = null;

        //foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
        //{
        //    enemy.ShieldReset();
        //}
        OnEnemyTurnStart?.Invoke();

        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, cancellationToken: CancelSource.Token);  // 카드 다 버린 이후 적 행동 시작

        var startSnapshot = EnemyManager.Instance.EnemyList.ToList();
        foreach (var enemy in startSnapshot)
        {
            // 효과 적용 직전에 아직 살아있는지 체크
            if (enemy == null || !EnemyManager.Instance.EnemyList.Contains(enemy)) continue;

            await enemy.OnTurnStartTask();
        }

        await UniTask.WaitForSeconds(0.2f, cancellationToken: CancelSource.Token);


        var attackSnapshot = EnemyManager.Instance.EnemyList.ToList();
        for (int i = 0; i < attackSnapshot.Count; i++)
        {
            var enemy = attackSnapshot[i];

            // 공격 직전 다시 한번 생존 확인 (반사뎀 등으로 죽을 수도 있음)
            if (enemy == null || !EnemyManager.Instance.EnemyList.Contains(enemy)) continue;

            await enemy.PlayPattern();
            await UniTask.WaitForSeconds(0.5f, cancellationToken: CancelSource.Token);
        }

        EndEnemyTurn();

        //int enemyCount = EnemyManager.Instance.EnemyList.Count;
        //for (int i = 0; i < enemyCount; ++i)
        //{
        //    await EnemyManager.Instance.EnemyList[i - (enemyCount - EnemyManager.Instance.EnemyList.Count)].PlayPattern();
        //    await UniTask.WaitForSeconds(0.5f, cancellationToken: CancelSource.Token); // 적 코드 이후 잠시 딜레이 (적이 공격 중에는 죽을 일 없으니 토큰 안 쓰기) => 죽을 일 생겨서 토큰 써야할 듯 ㅋㅋㅋ
        //}
        // 적 턴 시작, 적 코드 작성
        // 적 턴이 끝나면 내 턴 시작.
        // 적 턴은 비동기함수 하나로 통침.

        //if (EnemyManager.Instance.NoEnemy)
        //{
        //    EndBattle().Forget();
        //    return;
        //}

        //foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
        //{
        //    enemy.TurnStatusEffect();
        //}
        //StartPlayerTurn().Forget();
        // await MyTurnTask(4);
    }

    private void EndEnemyTurn()
    {
        if (EnemyManager.Instance.NoEnemy)
        {
            EndBattle().Forget();
            return;
        }

        // 5. 적 턴 종료 이벤트 실행 (상태이상 턴 감소 등)
        OnEnemyTurnEnd?.Invoke();

        // 다시 내 턴으로
        StartPlayerTurn().Forget();
    }
        
    //public void StartBattle()       // 배틀 시작시, 덱 섞기 및 액션 추가
    //{
    //    _InBattle.Value = true;
    //    InGameManager.Instance.Player.StartOrEndBattle(_InBattle.Value);
    //    CancelSource = new();

    //    InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Battle, true);
    //    CardManager.Instance.SetupDrawDeck(true);
    //    //TurnManager.OnAddCard += async () =>
    //    //    await DrawCard();
    //    //OnAddCard += () =>
    //    //    CardManager.Instance.DrawCard().Forget();

    //    ResetBattleStart();

    //    StartPlayerTurn().Forget();
    //}

    public async UniTask EndBattle()         // 리팩토링 필요해보임.
    {
        _InBattle.Value = false;
        //EnemyManager.Instance.HitEnemy = null;
        //InGameManager.Instance.Player.StartOrEndBattle(_InBattle.Value);
        CancelSource.Cancel();

        OnBattleEnd?.Invoke();
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Battle, false);
        //OnAddCard = null;

        CurTurnType = TurnType.Nobody;
        SetLoading(true);

        await CardManager.Instance.ThrowAwayCard();
        CardManager.Instance.ClearCard();

        SetLoading(false);

        //TurnManager.OnAddCard -= () =>
        //    DrawCard().Forget();
        //TurnManager.OnAddCard = null;
        // TurnManager.OnAddCard -= async () =>
        //     await DrawCard();
        //await EndPlayerTurn(true);
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
