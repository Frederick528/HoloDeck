using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    //public Dictionary<int, CardData> CardDatas { get; private set; } = new Dictionary<int, CardData>();
    //public List<Card> Deck { get; private set; }

    public List<Card> MainDeck;   // 덱 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X)

    public List<Card> TotalDeck;    // 현재 가지고 있는 전체 덱 (배틀 중 추가된 카드 적용, 배틀 종료 후, 메인덱으로 바꿔줘야 함.)
    //public List<Card> MainCardDeck;     // 덱에 있는 카드 정보를 데이터 값으로 저장(배틀 중 추가된 카드는 적용X) (MainDeck과 같이 카드 추가)


    public List<Card> DrawDeck;   // 현재 내가 뽑을 수 있는 카드
    public List<Card> CardDummy;  // 카드 더미(사용 또는 버림)
    public List<Card> HandCard;   // 내 손에 있는 카드

    public bool isSingleTarget;         // 싱글 카드 듦
    public bool useSingleTargetCard;    // 공격 가능

    public ECardState CardState;

    public CardData GetCardData;

    [SerializeField] CardSO cardSO;

    public Transform CardSpawnPoint;
    public Transform CardDummyTr;
    public Transform WatingCardTr;
    public Transform PlayingCardTr;

    //[SerializeField] Transform Deck;    // 소환된 덱 카드들

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;

    //[SerializeField] GameObject ArrowCursor;

    //[SerializeField] GameObject CardPrefab;

    //public Transform cardRewardContent;

    //[SerializeField] Transform shopCard;
    //[SerializeField] Transform shopCardPrice;


    public Sprite[] CommonSprites;
    public Sprite[] RareSprites;
    public Sprite[] EpicSprites;
    public Sprite[] LegendarySprites;


    List<Card> _selectedCards = new();           // 배틀 중 버리기, 강화, 교환 등에 의해 카드 위치 조정이 되면 안 되는 카드들
    List<Card> _tempThrowAwayCards = new();      // 카드 버리기를 위한 공용 리스트로, 버릴 카드들을 추가하고, await 다음에 전부 버림.

    public Card NowPlayedCard;                            // 큐에서 실행하고 있는 카드
    Card _usedCard;                              // 사용되고 있는 카드(버리기 효과나 다른 효과가 진행되고 있는 카드)
    public Card SelectCard;                            // 들고 있는 카드(drag 중인 카드)
    bool draggable;
    ReactiveProperty<bool> isUseCard = new();     // 카드 사용존에 카드가 올라왔을 경우(카드를 놓으면 카드가 사용되는 위치)

    bool canPush = true;
    public enum ECardState { Nothing, CanMouseOver, CanMouseDrag, OnlyMouseClick }
                                    
    //UICard[] uICards = new UICard[4];

    //EventQueue _eventQueue = new();

    bool _discard;
    bool _remove;

    int _enQueuedCardCount;
    int _waitedCardOrder;

    int _isDrawingCount = 0;

    RectTransform[] _cachedDropZones;
    (RectTransform rect, int? index) _hoveredZone;
    (RectTransform rect, int? index) _lastHoveredZone; // 이전 프레임의 호버 상태 기억

    CancellationTokenSource _moveCts;


    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        CardSpawnPoint = InGameManager.Instance.PlayerTr.Find("CardSpawnPoint");
        CardDummyTr = InGameManager.Instance.PlayerTr.Find("CardDummy");
        WatingCardTr = InGameManager.Instance.PlayerTr.Find("WatingCard");
        PlayingCardTr = InGameManager.Instance.PlayerTr.Find("PlayingCard");
        myCardLeft = InGameManager.Instance.PlayerTr.Find("MyCardLeft");
        myCardRight = InGameManager.Instance.PlayerTr.Find("MyCardRight");
    }


    private void Start()
    {
        isUseCard.Subscribe((canUse) =>
        {
            if (SelectCard == null) return;
            SelectCard.TurnOnOutline(canUse).Forget();
            if (canUse)
            {
                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);
            }
            if (canUse && (SelectCard.Data.CardTag == CardTag.SingleAttack || SelectCard.Data.CardTag == CardTag.SkillTargetSingle) && !isSingleTarget)
            {
                BattleManager.Instance.SetActiveArrowCursor(true, 0);
                //PullCard();
                SelectCard.transform.DOKill();        // 마우스 커서가 카드를 나갈 때 카드 크기가 원래대로 돌아가는 코드를 멈춰주는 함수.
                SelectCard.MoveTransform(new PRS(new(0, CardUtils.LargeCardPosY), SelectCard.transform.localRotation, SelectCard.transform.localScale), true, CardUtils.CardFastMoveDelay);
                //SelectCard.transform.position = new Vector2(0, CardUtils.LargeCardPosY);
                isSingleTarget = true;
            }
            else if (!canUse && isSingleTarget)
            {
                CancelCardMoveTask();
                BattleManager.Instance.SetActiveArrowCursor(false, 0);
                isSingleTarget = false;
                if (SelectCard != null)
                {
                    Cursor.visible = false;
                }
            }
        }).AddTo(this);

        _cachedDropZones = ItemManager.Instance.GetPotionRects();
    }
    public void RewardedCard()
    {
        if (GetCardData == null) return;
        AddDeck(GetCardData, EAddDeck.Main);
        MapManager.Instance.GetReward();
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, true);
    }

    public void SetupStartCardDeck()   // 시작할 때, 메인덱을 설정하는 함수 (게임 시작 이후에는 사용하지 않음.)
    {
        foreach (CardData cd in InGameManager.Instance.CardSO.Cards)
        {
            if (cd.ID >= 10000) continue;   // 일단 강화 카드 제외
            AddDeck(cd, EAddDeck.Main);
        }
        //int _startDeck = 6;     // 여기 밑 코드 변경해야 함. 캐릭터별로 얻는 카드와 카드 ID가 달라지기 때문에 switch로 구별.
        //for (int i = 0; i < _startDeck; i++)
        //    AddDeck(100 + i, EAddDeck.Main);
        //AddDeck(101, EAddDeck.Main);
        //AddDeck(101, EAddDeck.Main);
        //AddDeck(101, EAddDeck.Main);
        //AddDeck(503, EAddDeck.Main);
        //AddDeck(801, EAddDeck.Main);
        //AddDeck(802, EAddDeck.Main);
        //AddDeck(1001, EAddDeck.Main);
        //AddDeck(1002, EAddDeck.Main);
        //AddDeck(105, EAddDeck.Main);
        //AddDeck(105, EAddDeck.Main);
        //AddDeck(105, EAddDeck.Main);
    }
    public void AddDeck(CardData cardData, EAddDeck eAddDeck)       // 덱에 카드를 추가할 때 사용, 핸드로 카드를 가져올 때는 AddCard 함수 사용. (주로 데이터 자체가 이동할 때 사용) => 해결함 이제 그냥 써도 됨.
    {
        Card setCard = PoolManager.Instance.GetCard(/*out Card setCard*/);
        GameObject cardObject = setCard.gameObject;
        setCard.name = cardData.Name;
        cardObject.name = cardData.Name;    // 시각화 용도

        setCard.Setup(cardData);
        switch (eAddDeck)
        {
            case EAddDeck.Main:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                MainDeck.Add(setCard);
                //MainCardDeck.Add(blockCard);
                break;

            case EAddDeck.Draw:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                DrawDeck.Add(setCard);
                InGameUIManager.Instance.SetDrawCount();
                ShuffleDeck();
                break;

            case EAddDeck.Dummy:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                CardDummy.Add(setCard);
                InGameUIManager.Instance.SetDummyCount();
                break;

            case EAddDeck.Hand:
                if (HandCard.Count < 10)
                {
                    setCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();
                    cardObject.transform.position = Vector3.zero;
                    HandCard.Add(setCard);
                    SetOriginOrder();
                    CardAlignment();
                }
                else
                {
                    cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                    CardDummy.Add(setCard);
                    InGameUIManager.Instance.SetDummyCount();
                }
                break;
        }
        TotalDeck.Add(setCard);
    }
    public void AddDeck(int id, EAddDeck eAddDeck)
    {
        CardData cardData = InGameManager.Instance.FindCardData(id);

        AddDeck(cardData, eAddDeck);
    }

    //async UniTaskVoid WaitUnblock(Card blockCard, float waitTime)
    //{
    //    blockCard.Block = true;
    //    await UniTask.WaitForSeconds(waitTime, true);       // 카드를 가져오기 위해 블락하는 거라, TimeScale은 무시함.
    //    blockCard.Block = false;
    //}

    //void BlockCard(Card blockCard)
    //{
    //    blockCard.Block = true;
    //}
    //void UnblockCard(Card blockCard)
    //{
    //    blockCard.Block = false;
    //}

    //public void FailedUseCard(Card useCard)
    //{
    //    useCard.Block = false;
    //    useCard.Used = false;
    //}

    //void CheckEnemyDead(Card card)
    //{
    //    int count = (card.Data.Count == 0) ? 1 : card.Data.Count;
    //    switch (card.Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            card.TargetEnemy.CheckIfDead(card.Data.Damage, count);
    //            break;
    //        case CardTag.AllAttack:
    //            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //            {
    //                enemy.CheckIfDead(card.Data.Damage, count);
    //            }
    //            break;
    //    }
    //}

    //async UniTask<bool> BeforeUsingCard(Card card)
    //{
    //    if (InGameManager.Instance.player.CurHolo < card.Data.Cost)
    //    {
    //        return false;
    //    }
    //    card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
    //    if (!await card.CheckUseConditions())
    //    {
    //        return false;
    //    }

    //    InGameManager.Instance.player.AddCurHolo(-card.Data.Cost);

    //    card.CheckEnemyDead();

    //    return true;
    //}

    //public async UniTask AfterCardAbility(Card playedCard, bool endBattle = false)
    //{
    //    await playedCard.TaskMoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), false, CardUtils.ThrowAwayCardDelay);

    //    if (!endBattle)
    //    {
    //        CardDummy.Add(playedCard);
    //        InGameUIManager.Instance.SetDummyCount();
    //    }
    //    playedCard.Block = false;
    //    playedCard.Used = false;
    //}

    async UniTask<bool> CheckCanUsingCard(Card card/*, bool singleAtk = false*/)
    {
        //if (card.Used) return;
        //card.Used = true;
        //card.CardOrder.SetOriginOrder(-10);

        _usedCard = card;       // 다른 카드가 사용 중이면 사용 못 하게 막을지 고민 중
        SetOriginOrder();
        CardAlignment();
        //if (singleAtk)
        //    card.Target(EnemyManager.Instance.targetEnemy);
        if (!await card.BeforeUsingCard())
        {
            //card.Used = false;
            //HandCard.Add(card);
            _usedCard = null;
            SetOriginOrder();
            CardAlignment();
            card.FailureBeforeUseCard?.Invoke();    // 카드 사용 실패 시, 즉시 실행했던 내용들 복구.
            await PutDownCard(card);

            return false;
        }

        //InGameManager.Instance.AbilityEventQueue.Enqueue(card);
        //_eventQueue.Enqueue(card);
        _usedCard = null;

        HandCard.Remove(card);


        //card.MoveTransform(new PRS(WatingCardTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.CardAlignmentDelay);
        //SetOriginOrder();
        //CardAlignment();

        return true;
    }

    public void ChangeTotalCardDesc()
    {
        foreach (Card card in TotalDeck)
        {
            card.CardDataReset();
        }
    }

    public void ChangeHandCardDesc()
    {
        foreach (Card card in HandCard)
        {
            card.CardDataReset();
        }
    }

    public void ClearCard()
    {
        DrawDeck.Clear();
        CardDummy.Clear();
        InGameUIManager.Instance.SetDrawCount();
        InGameUIManager.Instance.SetDummyCount();
        //HandCard.Clear();     클리어카드 전에 카드를 전부 버리기 때문에 HandCard.Clear()는 안 해도 됨.
        for (int i = 0; i < TotalDeck.Count; ++i)
        {
            {
                if (MainDeck.Contains(TotalDeck[i]))
                {
                    continue;
                }
                //TotalDeck[i].CardDataReset(true);
                TotalDeck[i].CardRelease();
                TotalDeck.Remove(TotalDeck[i--]);
            }
        }
    }
    public void SetupDrawDeck(bool start = false)  // 드로우덱 섞기(start가 true일 경우, 메인덱에서 가져옴. false일 경우, 카드더미에서 가져옴.)
    {
        if (!start)
        {
            //DrawDeck = CardDummy.ToList();
            foreach (Card card in CardDummy)
            {
                DrawDeck.Add(card);         // 여기선 AddDeck 안 씀.
                InGameUIManager.Instance.SetDrawCount();
                card.transform.position = CardSpawnPoint.position;
            }
            CardDummy.Clear();
            InGameUIManager.Instance.SetDummyCount();
        }
        else
        {
            //DrawDeck = MainDeck.ToList();
            foreach (Card card in MainDeck)
            {
                DrawDeck.Add(card);
                InGameUIManager.Instance.SetDrawCount();
                card.transform.position = CardSpawnPoint.position;
            }
        }
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < DrawDeck.Count; i++)
        {
            int rand = Random.Range(0, DrawDeck.Count);
            Card temp = DrawDeck[i];
            DrawDeck[i] = DrawDeck[rand];
            DrawDeck[rand] = temp;
        }
    }

    public async UniTask<Card> CardToDraw()
    {
        if (HandCard.Count + _isDrawingCount >= 10) return null;
        _isDrawingCount++;

        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기. 이 경우에는 카드 뽑기가 0.5초 후 가능 (카드 버려지는 시간인 0.3초보단 높게 잡아야 함.)
        {
            SetupDrawDeck();
            await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, cancellationToken: TurnManager.Instance.CancelSource.Token);
        }

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
        {
            _isDrawingCount--;
            return null;
        }

        Card card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        InGameUIManager.Instance.SetDrawCount();
        _isDrawingCount--;
        return card;
    }

    public Card CardToDraw(Card drawCard)
    {
        if (HandCard.Count == 10) return null;

        Card card = DrawDeck.Find(x => x == drawCard);
        DrawDeck.Remove(card);
        InGameUIManager.Instance.SetDrawCount();
        return card;
    }

    //public async UniTask<Card[]> CardsToDraw(int count)
    //{
    //    if (count <= 0)
    //        return null;
    //    if (HandCard.Count + count > 10)
    //        count = 10 - HandCard.Count;
    //    Card[] card = new Card[count];
    //    int tempDraw = DrawDeck.Count;
    //    if (DrawDeck.Count < count)    // 덱에 있는 카드가 뽑을 카드보다 적으면, 일단 덱에 있는 카드를 뽑고 덱 섞기. 이 경우에는 카드 뽑기가 LoadCardDummyDelay초 후 가능 (카드 버려지는 시간인 ThrowAwayCardDelay초보단 높게 잡아야 함.)
    //    {
    //        for (int i = 0; i < tempDraw; ++i)
    //        {
    //            card[i] = DrawDeck[0];
    //            DrawDeck.RemoveAt(0);
    //        }
    //        SetupDrawDeck();
    //        await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    //    }
    //    else if (DrawDeck.Count >= count)
    //    {
    //        for (int i = 0; i < count; ++i)
    //        {
    //            card[i] = DrawDeck[0];
    //            DrawDeck.RemoveAt(0);
    //        }
    //        return card;
    //    }


    //    if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
    //        return card;

    //    //if (DrawDeck.Count < Count)
    //    //    Count = DrawDeck.Count;

    //    //Card[] card = new Card[Count];
    //    for (int i = tempDraw; i < count; ++i)      // 위에서 리턴이 걸리지 않으면, 남은 카드를 뽑음. 남은 카드를 뽑던 중, 덱에 있는 카드가 없을 경우, 리턴
    //    {
    //        if (DrawDeck.Count == 0)
    //            return card;
    //        card[i] = DrawDeck[0];
    //        DrawDeck.RemoveAt(0);
    //    }
    //    return card;
    //}

    public async UniTask DrawCard()   // 손패로 드로우할 카드 (DrawCards와 다르게 배열 생성을 안 하기 때문에 1개 뽑을 때는 이걸 사용하는 게 맞을 듯.) => count로 바꾸면서 그냥 똑같아짐.
    {
        Card drawCard = await CardToDraw();
        if (drawCard == null)
            return;
        HandCard.Add(drawCard);

        drawCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();

        SetOriginOrder();
        CardAlignment();
        await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    }
    public async UniTask DrawCard(int count)   // Count로 뽑는 거 성공 시, 아래 있는 DrawCards는 필요없음.
    {
        for (int i = 0; i < count; ++i)
        {
            await DrawCard();
            //Card drawCard = await CardToDraw();
            //if (drawCard == null)
            //    break;

            //HandCard.Add(drawCard);

            //drawCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();
            ////drawCard.BlockCard();

            //SetOriginOrder();
            //CardAlignment();
            //await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, cancellationToken: TurnManager.Instance.CancelSource.Token);

            ////drawCard.UnblockCard();
        }
    }

    //public async UniTask DrawCards(int count)   // 손패로 드로우할 카드 (배열을 생성하지만, DrawCard와는 다르게 뽑을 수 있는 카드보다 뽑는 카드가 더 많을 경우, 더미->드로우를 한 번만 진행함.)
    //{
    //    Card[] drawCard = await CardsToDraw(count);
    //    if (drawCard == null)
    //        return;

    //    //GameObject cardObject = Instantiate(CardPrefab, cardSpawnPoint.position, Quaternion.identity);
    //    //Card card = drawCard.GetComponent<Card>();
    //    //drawCard.Setup(drawCard.Data);
    //    for (int i = 0; i < drawCard.Length; ++i)
    //    {
    //        if (drawCard[i] == null)
    //            return;
    //        HandCard.Add(drawCard[i]);

    //        SetOriginOrder();
    //        CardAlignment();
    //        //if (i != drawCard.Length - 1)
    //        //{
    //            await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    //        //}
    //    }
    //}

    public void AddCard(Card addCard)    // 덱에서 손패로 카드를 가져옴.
    {
        Card drawCard = CardToDraw(addCard);
        if (drawCard == null)
            return;

        HandCard.Add(drawCard);

        drawCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();

        SetOriginOrder();
        CardAlignment();

    }

    //public void AddCard(CardData cardData)  // 카드 생성
    //{
    //    AddDeck(cardData, EAddDeck.Hand);

    //    SetOriginOrder();
    //    CardAlignment();
    //}

    //public void AddCard(int id)  // 카드 생성
    //{
    //    AddDeck(id, EAddDeck.Hand);

    //    SetOriginOrder();
    //    CardAlignment();
    //}

    public void ResetSetting()
    {
        isSingleTarget = false;
        useSingleTargetCard = false;
        draggable = false;
        BattleManager.Instance.SetActiveArrowCursor(false, 0);
        isUseCard.Value = false;

        SelectCard = null;      // isUseCard와 순서 중요! selectCard가 밑에 있어야 함.

        canPush = true;         // PullCard랑 중복 호출이긴 함.

        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, true);
        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.ActionCard, true);
    }
    public void ReturnSelectedCard()
    {
        foreach (Card selectedCard in _selectedCards)
        {
            selectedCard.Selected = false;
            //HandCard.Add(selectedCard);
        }
        //SetOriginOrder();             // 리턴 카드를 할 경우, 사용하고 있던 카드를 다시 내 손패로 돌려보냄. 이때, 전체 카드 정렬을 하기 때문에 여기서 따로 할 필요 없음.
        //CardAlignment();
        _selectedCards.Clear();
    }
    void SelectedCards(Card card)
    {
        if (card.Selected)
        {
            card.Selected = false;
            _selectedCards.Remove(card);
            SetSelectedCardsOrder(_selectedCards);
            SelectedCardsAlignment(_selectedCards);
            //HandCard.Add(card);
            SetOriginOrder();
            CardAlignment();

        }
        else
        {
            if (_usedCard != null)
            {
                if (_usedCard.Data.Discard > 0 && _selectedCards.Count == _usedCard.Data.Discard)
                {
                    Card returnCard = _selectedCards[0];
                    returnCard.Selected = false;
                    _selectedCards.RemoveAt(0);
                }
            }
            else
            {
                if (NowPlayedCard.Data.Discard > 0 && _selectedCards.Count == NowPlayedCard.Data.Discard)
                {
                    Card returnCard = _selectedCards[0];
                    returnCard.Selected = false;
                    _selectedCards.RemoveAt(0);
                }
            }
            card.Selected = true;
            _selectedCards.Add(card);
            SetSelectedCardsOrder(_selectedCards);
            SelectedCardsAlignment(_selectedCards);
            //HandCard.Remove(card);
            SetOriginOrder();
            CardAlignment();
        }
        //if (HandCard.Count <= _usedCard.Data.Discard)
        //{
        //    // 카드 효과가 버리기일 경우, 위 상황에서는 바로 버리기 가능해야 함. 반대로 카드조건이 버리기일 경우 카드 사용될 때 실행
        //}
        if (_usedCard != null)      // 카드 조건이 버리기인 경우   (조건일 경우)
        {
            if (_usedCard.Data.Discard > 0)
            {
                if (_selectedCards.Count == _usedCard.Data.Discard)     // 몇 개일 때 사용 가능
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
            else if (_usedCard.Data.Discard == 0)                       // 버려도 되고 안 버려도 됨.
            {
                InGameButtonManager.Instance.DiscardBtnInvert(true);
            }
            else
            {
                if (_selectedCards.Count >= -_usedCard.Data.Discard)        // 몇 개 이상부터 사용 가능
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
        }
        else                        // 카드 효과가 버리기인 경우
        {
            if (NowPlayedCard.Data.Discard > 0)
            {
                if (_selectedCards.Count == NowPlayedCard.Data.Discard)     // 위와 동일
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
            else if (NowPlayedCard.Data.Discard == 0)
            {
                InGameButtonManager.Instance.DiscardBtnInvert(true);
            }
            else
            {
                if (_selectedCards.Count >= -NowPlayedCard.Data.Discard)
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    InGameButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
        }
    }
    public void ChangeDiscard(bool discard)
    {
        _discard = discard;
        //InGameManager.Instance.Pause(discard);
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.SelectedCard, discard);
        if (discard)
        {
            SetCardState(3);   // Click
            InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
            InGameButtonManager.Instance.ActItemBtnInvert(false);
        }
        else
        {
            SetCardState(2);    // Drag
            InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
            InGameButtonManager.Instance.ActItemBtnInvert(true);
        }
    }
    public void ChangeRemove(bool remove)
    {
        _remove = remove;
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.SelectedCard, remove);
        if (remove)
        {
            SetCardState(3);   // Click
            InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
            InGameButtonManager.Instance.ActItemBtnInvert(false);
        }
        else
        {
            SetCardState(2);    // Drag
            InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
            InGameButtonManager.Instance.ActItemBtnInvert(true);
        }
    }
    void DiscardCard(Card card)      // 카드 선택해서 버리기
    {
        SelectedCards(card);
    }

    void RemoveCard(Card card)       // 카드 삭제하기(배틀 안에서만 적용)
    {
        SelectedCards(card);
    }

    public async UniTask ThrowAwaySelectedCard()
    {
        foreach (Card targetCard in _selectedCards)
        {
            targetCard.BlockCard();
            targetCard.Selected = false;
            targetCard.MoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);
            HandCard.Remove(targetCard);
            _tempThrowAwayCards.Add(targetCard);
        }
        _selectedCards.Clear();
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        foreach (Card targetCard in _tempThrowAwayCards)
        {
            CardDummy.Add(targetCard);
            targetCard.UnblockCard();
        }
        InGameUIManager.Instance.SetDummyCount();
        //_selectedCards.Clear();         // 정렬에 있는 카드들을 버리는 시간동안 selectedCards의 값이 있기 때문에 정렬에 문제가 생김. => _temp에 추가하고, await 전에 클리어하는 걸로 일단 해결
        _tempThrowAwayCards.Clear();
    }

    public async UniTask ThrowAwayCard()        // 모든 카드를 카드 더미로
    {
        ResetSetting();
        foreach (Card targetCard in HandCard)
        {
            targetCard.BlockCard();
            targetCard.MoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);
            _tempThrowAwayCards.Add(targetCard);
        }
        HandCard.Clear();
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        foreach (Card targetCard in _tempThrowAwayCards)
        {
            CardDummy.Add(targetCard);
            targetCard.UnblockCard();
        }
        InGameUIManager.Instance.SetDummyCount();
        _tempThrowAwayCards.Clear();
    }
    public async UniTask ThrowAwayCard(Card throwCard)
    {
        HandCard.Remove(throwCard);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), throwCard.GetCancellationTokenOnDestroy(), CardUtils.ThrowAwayCardDelay);

        CardDummy.Add(throwCard);
        InGameUIManager.Instance.SetDummyCount();
    }

    public async UniTask PlayedCard(Card playedCard)
    {
        if (!TurnManager.Instance.InBattle.Value)
        {
            playedCard.FailedUseCard();
            return;
        }
        if (!await CheckCanUsingCard(playedCard))
        {
            playedCard.FailedUseCard();
            return;
        }

        NowPlayedCard = playedCard;       // 마지막으로 시전한 카드 정보를 받아와야 할 수도 있기 때문에 일단 초기화는 안 함.

        //InGameManager.Instance.Player.AttackAnimation().Forget();

        //HandCard.Remove(playedCard);

        //SetOriginOrder();
        //CardAlignment();
        playedCard.MoveTransform(new PRS(PlayingCardTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.CardAlignmentDelay);
        bool endBattle = await playedCard.UseTask().SuppressCancellationThrow();
        //if (endBattle)
        //{
        //    //_eventQueue.QueueClear();
        //}
        await playedCard.AfterCardAbility(endBattle);
    }


    void SetSelectedCardsOrder(List<Card> cardList)
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            Card targetCard = cardList[i];
            targetCard.CardOrder.SetOriginOrder(i);
        }
    }

    void SelectedCardsAlignment(List<Card> cardList)     // 버리기, 삭제 등에서 사용할 예정
    {
        List<PRS> CardPRSs;
        CardPRSs = SerialAlignment(new Vector2(-6, 0.5f), new Vector2(6, 0.5f), cardList.Count, CardUtils.CardScale * 0.7f);
        for (int i = 0; i < cardList.Count; i++)
        {
            Card targetCard = cardList[i];
            targetCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();
            targetCard.MoveTransform(CardPRSs[i], true, CardUtils.CardAlignmentDelay);
        }
    }

    List<PRS> SerialAlignment(Vector3 leftVector, Vector3 rightVectpr, int cardCount, Vector3 scale)
    {
        float[] cardLerps = new float[cardCount];
        List<PRS> results = new List<PRS>(cardCount);

        if (cardCount == 1)
        {
            cardLerps = new float[] { 0.5f };
        }
        else
        {
            float interval = cardCount < 7f ? 1f / 7 : 1f / cardCount;
            float cardPos = 0f;
            for (int i = 0; i < cardCount; i++)
            {
                if (i == 0)
                    cardPos += cardCount < 7f ? (interval * (4f - cardCount * 0.5f)) : interval * 0.5f;
                else
                    cardPos += interval;
                cardLerps[i] = cardPos;
            }
        }

        for (int i = 0; i < cardCount; i++)
        {
            Vector3 targetPos = Vector3.Lerp(leftVector, rightVectpr, cardLerps[i]);
            //targetPos.z = -i * 5;

            targetPos.y += 2f;

            results.Add(new PRS(targetPos, Quaternion.identity, scale));
        }
        return results;
    }

    public void HandCardSort()
    {
        SetOriginOrder();
        CardAlignment();
    }

    void SetOriginOrder()       // 카드가 보이는 순서 설정
    {
        int alignmentIdx = 0;
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];
            if (targetCard == SelectCard)       // 내가 현재 마우스를 올리고 있는 카드인 경우
                continue;
            if (targetCard == _usedCard || targetCard.Selected || targetCard.IsEnqueued)      // 카드 조건 확인 상태 || 버리기 및 소멸 등으로 선택된 상태 || 큐에 들어가 있는 상태
            {
                ++alignmentIdx;
                continue;
            }
            targetCard.CardOrder.SetOriginOrder(i - alignmentIdx);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs;
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count - _selectedCards.Count - (_usedCard ? 1 : 0) - _enQueuedCardCount, CardUtils.CardScale);
        int alignmentIdx = 0;
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];

            //if (targetCard == SelectCard)
            //{
            //    targetCard.OriginPRS = originCardPRSs[i - alignmentIdx];
            //    continue;
            //}
            if (targetCard == _usedCard || targetCard.Selected || targetCard.IsEnqueued)
            {
                ++alignmentIdx;
                continue;
            }
            //print($"{HandCard.Count} / {HandCard.Count - _selectedCards.Count - (_usedCard ? 1 : 0)} / {i - alignmentIdx}");
            targetCard.OriginPRS = originCardPRSs[i - alignmentIdx];
            targetCard.MoveTransform(targetCard.OriginPRS, true, CardUtils.CardAlignmentDelay);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int cardCount, Vector3 scale)
    {
        float[] cardLerps = new float[cardCount];
        List<PRS> results = new List<PRS>(cardCount);

        if (cardCount == 1)
        {
            cardLerps = new float[] { 0.5f };
        }
        else
        {
            float interval = cardCount < 7f ? 1f / 7 : 1f / cardCount;
            float cardPos = 0f;
            for (int i = 0; i < cardCount; i++)
            {
                if (i == 0)
                    cardPos += cardCount < 7f ? (interval * (4f - cardCount * 0.5f)) : interval * 0.5f;
                else
                    cardPos += interval;
                cardLerps[i] = cardPos;
            }
        }

        for (int i = 0; i < cardCount; i++)
        {
            Vector3 targetPos = Vector3.Lerp(leftTr.position, rightTr.position, cardLerps[i]);
            //targetPos.z = -i * 5;

            float curve = Mathf.Sqrt(Mathf.Pow(/*height*/0.5f, 2) - Mathf.Pow(cardLerps[i] - 0.5f, 2));   // 원의 방정식
            //float curve = Mathf.Sqrt(Mathf.Pow(/*height*/0.5f, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // 타원의 방정식

            targetPos.y += 3f * curve - 1.5f;
            Quaternion targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, cardLerps[i]);

            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    public void CardInQueue(Card card, bool isIn)
    {
        card.IsEnqueued = isIn;
        if (isIn)
        {
            card.ImmediatelyUseCard?.Invoke();
            card.MoveTransform(new PRS(WatingCardTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.CardAlignmentDelay);
            _enQueuedCardCount++;
            if (_enQueuedCardCount > 1)
            {
                card.CardOrder.SetOriginOrder(++_waitedCardOrder);
            }
            else
            {
                _waitedCardOrder = -10;
                card.CardOrder.SetOriginOrder(_waitedCardOrder);
            }
        }
        else
            _enQueuedCardCount--;

    }

    //public void DeQueueCard(Card card)
    //{
    //    card.IsEnqueued = false;
    //    enQueuedCardCount--;
    //}

    #region MyCard

    public void CardMouseOver(Card card)                    // Enter로 안 하는 이유는... Enter로 하면 순간순간 카드가 over 안 되는 경우의 수가 존재함.
    {
        if (CardState == ECardState.Nothing || draggable)
            return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        LargeCard(card);
        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
        if (card.Selected) return;
        PushCard(card);
    }
    public void CardMouseExit(Card card)
    {
        if (CardState == ECardState.Nothing || draggable)
            return;
        //if (EventSystem.current.IsPointerOverGameObject())
        //    return;
        if (card.Selected)
        {
            card.MoveTransform(new PRS(new(card.transform.position.x, card.transform.position.y, 0), Quaternion.identity, CardUtils.CardScale * 0.7f));
            card.CardOrder.SetMostFrontOrder(false);
            return;
        }
        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
        PutDownCard(card).Forget();
    }


    void LargeCard(Card card)
    {
        if (card.Selected)
        {
            card.transform.DOKill();
            //Vector3 selectedLargePos = new(card.transform.position.x, card.transform.position.y, 0);      // z축 변경 안 하면, MouseOver 문제 생김.
            card.MoveTransform(new PRS(new(card.transform.position.x, card.transform.position.y, -1), Quaternion.identity, CardUtils.CardScale * 0.8f));
            card.CardOrder.SetMostFrontOrder(true);
            return;
        }

        card.transform.DOKill();              // 정렬 드로우 문제 등 제거
        Vector3 largePos = new(card.OriginPRS.pos.x, CardUtils.LargeCardPosY, -1f);      // z축 변경 안 하면, MouseOver 문제 생김.
        card.MoveTransform(new PRS(largePos, Quaternion.identity, CardUtils.CardScale * 1.2f));

        card.CardOrder.SetMostFrontOrder(true);
    }
    void PushCard(Card card)
    {
        if (!canPush)
            return;

        int cardIndex = HandCard.IndexOf(card);
        if (cardIndex == -1)
            return;
        for (int i = 1; i < cardIndex + 1; ++i)
        {
            if (HandCard[cardIndex - i] == _usedCard || HandCard[cardIndex - i].Selected || HandCard[cardIndex - i].IsEnqueued)
                continue;
            HandCard[cardIndex - i].transform.DOMoveX(HandCard[cardIndex - i].OriginPRS.pos.x - 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);        // .SetUpdate(true) 추가함.
        }

        for (int i = 1; i < HandCard.Count - cardIndex; ++i)
        {
            if (HandCard[cardIndex + i] == _usedCard || HandCard[cardIndex + i].Selected || HandCard[cardIndex + i].IsEnqueued)
                continue;
            HandCard[cardIndex + i].transform.DOMoveX(HandCard[cardIndex + i].OriginPRS.pos.x + 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);        // .SetUpdate(true) 추가함.
        }

        canPush = false;
    }

    void PullCard()     // PushCard()보다 움직임 속도가 빨라야 함. 즉, dotweenTime 값은 더 작아야 함.
    {
        canPush = true;
        for (int i = HandCard.Count -1; i >= 0; --i) {
            if (HandCard[i] == _usedCard || HandCard[i].Selected || HandCard[i].IsEnqueued)
                continue;
            HandCard[i].transform.DOKill();            // 정렬 하는 코드 삭제
            HandCard[i].MoveTransform(HandCard[i].OriginPRS, true, CardUtils.CardAlignmentDelay * 0.9f);
        }
        //foreach (Card card in HandCard)
        //{
        //    if (card == _usedCard || card.Selected || card.IsEnqueued)
        //        continue;
        //    card.transform.DOKill();            // 정렬 하는 코드 삭제
        //    card.MoveTransform(card.OriginPRS, true, CardUtils.CardAlignmentDelay * 0.9f);
        //}

    }
    public void CardMouseDown(Card card)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (CardState == ECardState.OnlyMouseClick)
        {
            if (_discard)
            {
                DiscardCard(card);
            }
            else if (_remove)
            {
                RemoveCard(card);
            }
            return;
        }
        if (CardState != ECardState.CanMouseDrag)
            return;

        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, false);
        //InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.ActionCard, false);

        Cursor.visible = false;

        SelectCard = card;
        draggable = true;
        card.BlockCard();
    }

    public void CardMouseUp(Card card)
    {
        Cursor.visible = true;
        if (CardState != ECardState.CanMouseDrag || !draggable)
        {
            //card.Block = false;
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (card.Used) return;

        if (isUseCard.Value)        // 카드 사용 가능 범위에 들어왔는지 확인
        {
            if (card.Data.CardTag != CardTag.SingleAttack && card.Data.CardTag != CardTag.SkillTargetSingle)     // 지정 카드를 제외한 나머지
            {
                ResetSetting();
                InGameManager.Instance.AbilityEventQueue.Enqueue(card);
                //await CheckCanUseingCard(card);
            }
            else if (useSingleTargetCard)                      // 지정 카드 사용이 가능할 경우
            {
                card.Target(card.CheckTarget/*EnemyManager.Instance.TargetEnemy*//*.GetComponent<Enemy>()*/);
                ResetSetting();
                InGameManager.Instance.AbilityEventQueue.Enqueue(card);
                //await CheckCanUseingCard(card/*, true*/);
            }
            else                                               // 지정 카드이지만, 대상을 지정하지 않았을 경우
            {
                CancelCardMoveTask();
                ResetSetting();
                PutDownCard(card).Forget();
            }
        }
        else
        {
            ResetSetting();
            PutDownCard(card).Forget();
        }

        //ResetSetting();
    }


    public async UniTask PutDownCard(Card card)
    {
        if (card == null)
            return;
        card.CardOrder.SetMostFrontOrder(false);
        PullCard();
        await card.TaskMoveTransform(card.OriginPRS, TurnManager.Instance.CancelSource.Token, CardUtils.CardAlignmentDelay).SuppressCancellationThrow();
        card.UnblockCard();
    }

    public void CardDrag(Card card)
    {
        if (CardState != ECardState.CanMouseDrag || !draggable)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            ResetSetting();
            PutDownCard(card).Forget();
            return;
        }

        DetectCardArea();

        CheckActionCard();

        Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (_hoveredZone.rect != null)
        {
            // 1. 방금 막 포션 구역에 들어왔을 때 (딱 한 번만 실행)
            if (_lastHoveredZone != _hoveredZone)
            {
                card.DOKill();
                card.MoveTransform(new PRS(_hoveredZone.rect.position, Quaternion.identity, CardUtils.CardScale * 0.3f), true, CardUtils.CardAlignmentDelay);
                _lastHoveredZone = _hoveredZone;

                PotionItem potion = ItemManager.Instance.GetPotionItems()[_hoveredZone.index.Value];

                if (potion != null)
                {
                    potion.SetPreviewAugment("데미지를 3 증가시킵니다.");
                    potion.OnPointerEnter(null);
                }


                if (card.Data.CardTag == CardTag.SingleAttack || card.Data.CardTag == CardTag.SkillTargetSingle)
                {
                    CancelCardMoveTask();
                    BattleManager.Instance.SetActiveArrowCursor(false, 0);
                }
                else
                {
                    Cursor.visible = true;
                }

            }
            // 들어와 있는 상태라면 트윈을 더 호출하지 않고 트윈이 끝날 때까지 기다리거나 위치 고정
        }
        else
        {
            // 2. 구역에서 나갔을 때 (딱 한 번만 실행)
            if (_lastHoveredZone.rect != null)
            {
                card.DOKill();
                PotionItem potion = ItemManager.Instance.GetPotionItems()[_lastHoveredZone.index.Value];

                if (potion != null)
                {
                    potion.ClearPreviewAugment();
                    potion.OnPointerExit(null);
                }

                _lastHoveredZone.rect = null;
                _lastHoveredZone.index = null;


                if (card.Data.CardTag == CardTag.SingleAttack || card.Data.CardTag == CardTag.SkillTargetSingle)
                {
                    _moveCts = new();
                    MoveToDefaultAndShowCursor(card, _moveCts.Token).Forget();
                }
                else
                {
                    Cursor.visible = false;
                }
            }
            if (!isUseCard.Value || (card.Data.CardTag != CardTag.SingleAttack && card.Data.CardTag != CardTag.SkillTargetSingle))
            {
                card.transform.position = Vector3.Lerp(card.transform.position, tempPos, Time.deltaTime * 20f);
                card.transform.localScale = Vector3.Lerp(card.transform.localScale, CardUtils.CardScale * 1.2f, Time.deltaTime * 7.5f);
            }

        }
        //else if ((/*isUseCard.Value && */card.Data.CardTag != CardTag.SingleAttack))
        //{
        //    card.DOKill();
        //    card.MoveTransform(new PRS(tempPos, Quaternion.identity, CardUtils.CardScale * 1.2f), true, CardUtils.CardFastMoveDelay);
        //    //card.transform.position = tempPos;
        //}
        //else if (!isUseCard.Value)
        //{
        //    card.DOKill();
        //    card.MoveTransform(new PRS(tempPos, Quaternion.identity, CardUtils.CardScale * 1.2f), true, CardUtils.CardFastMoveDelay);
        //    //card.transform.position = tempPos;
        //}
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        int layer = LayerMask.NameToLayer("UsedCardArea");
        isUseCard.Value = Array.Exists(hits, x => x.collider.gameObject.layer == layer);      // 카드 사용 범위에 있을 경우, isUseCard = true

    }

    void CheckActionCard()
    {
        if (_cachedDropZones == null) return;
        _hoveredZone.rect = null;
        _hoveredZone.index = null;
        int i = 0;
        foreach (var zone in _cachedDropZones)
        {
            // Raycast Target이 꺼져있어도 수학적으로 계산됨
            if (RectTransformUtility.RectangleContainsScreenPoint(zone, Input.mousePosition, Camera.main))
            {
                _hoveredZone.rect = zone;
                _hoveredZone.index = i;
                return; // 찾았으면 루프 종료
            }
            i++;
        }
    }

    public void CancelCardMoveTask()
    {
        _moveCts?.Cancel();
        _moveCts?.Dispose();
        _moveCts = null;
    }

    // 실제 이동 대기 및 커서 처리 함수
    private async UniTaskVoid MoveToDefaultAndShowCursor(Card card, CancellationToken token)
    {
        await card.TaskMoveTransform(
                new PRS(new(0, CardUtils.LargeCardPosY), Quaternion.identity, CardUtils.CardScale * 1.2f),
                token,
                CardUtils.CardFastMoveDelay * 3f,
                Ease.InExpo
            );

        BattleManager.Instance.SetActiveArrowCursor(true, 0);
    }

    /// <summary>
    /// 0 = Nothing, 1 = CanMouseOver, 2 = CanMouseDrag, 3 = OnlyMouseClick
    /// </summary>
    /// <param name="index"></param>
    public void SetCardState(int index)
    {
        switch (index)
        {
            case 0:
                CardState = ECardState.Nothing;
                PutDownCard(SelectCard).Forget();
                ResetSetting();
                break;
            case 1:
                CardState = ECardState.CanMouseOver;
                PutDownCard(SelectCard).Forget();
                ResetSetting();
                break; 
            case 2:
                CardState = ECardState.CanMouseDrag;
                break; 
            case 3:
                CardState = ECardState.OnlyMouseClick;
                PutDownCard(SelectCard).Forget();
                ResetSetting();
                break;
        }
    }

    //void SetActiveArrowCursor(bool isOn)
    //{
    //    ArrowCursor.SetActive(isOn);
    //    Cursor.visible = !isOn;
    //}
    #endregion

    private void OnDestroy()
    {
        _moveCts?.Cancel();
        _moveCts?.Dispose();
    }
}
