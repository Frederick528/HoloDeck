using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine.EventSystems;

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

    public bool isSingleTarget;
    public bool useSingleTargetCard;

    public ECardState CardState;

    public CardData GetCardData;

    [SerializeField] CardSO cardSO;

    public Transform CardSpawnPoint;
    public Transform CardDummyTr;

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


    List<Card> _selectedCards = new();           // 배틀 중 버리기, 강화, 교환 등에서 선택한 카드 리스트

    Card _playedCard;                            // 큐에서 실행한 카드
    Card _usedCard;                              // 사용되고 있는 카드(버리기 효과나 다른 효과가 진행되고 있는 카드)
    Card _selectCard;                            // 들고 있는 카드(drag 중인 카드)
    bool draggable;
    ReactiveProperty<bool> isUseCard = new();     // 카드 사용존에 카드가 올라왔을 경우(카드를 놓으면 카드가 사용되는 위치)

    bool canPush = true;
    public enum ECardState { Nothing, CanMouseOver, CanMouseDrag, OnlyMouseClick }

    int shopCardIdx;
                                    
    //UICard[] uICards = new UICard[4];

    //EventQueue _eventQueue = new();

    bool _discard;
    bool _remove;


    private void Awake() => Instance = this;


    private void Start()
    {
        CardSpawnPoint = UIManager.Instance.Player.Find("CardSpawnPoint");
        CardDummyTr = UIManager.Instance.Player.Find("CardDummy");
        myCardLeft = UIManager.Instance.Player.Find("MyCardLeft");
        myCardRight = UIManager.Instance.Player.Find("MyCardRight");
        isUseCard.Subscribe((canUse) =>
        {
            _selectCard?.TurnOnOutline(canUse);
            if (canUse && _selectCard.Data.CardTag == CardTag.SingleAttack && !isSingleTarget)
            {
                BattleManager.Instance.SetActiveArrowCursor(true, 0);
                //PullCard();
                _selectCard.transform.DOKill();        // 마우스 커서가 카드를 나갈 때 카드 크기가 원래대로 돌아가는 코드를 멈춰주는 함수.
                _selectCard.transform.position = new Vector2(0, CardUtils.LargeCardPosY);
                isSingleTarget = true;
            }
            else if (!canUse && isSingleTarget)
            {
                BattleManager.Instance.SetActiveArrowCursor(false, 0);
                isSingleTarget = false;
            }
        });

        SetupStartCardDeck();
    }
    public void RewardedCard()
    {
        AddDeck(GetCardData, EAddDeck.Main);
        MapManager.Instance.GetReward();
        UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.Map, true);
    }

    void SetupStartCardDeck()   // 시작할 때, 메인덱을 설정하는 함수 (게임 시작 이후에는 사용하지 않음.)
    {
        int _startDeck = 6;     // 여기 밑 코드 변경해야 함. 캐릭터별로 얻는 카드와 카드 ID가 달라지기 때문에 switch로 구별.
        for (int i = 0; i < _startDeck; i++)
            AddDeck(100 + i, EAddDeck.Main);
    }
    public void AddDeck(CardData cardData, EAddDeck eAddDeck)       // 덱에 카드를 추가할 때 사용, 핸드로 카드를 가져올 때는 DrawCard 함수 사용. (주로 데이터 자체가 이동할 때 사용)
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
                UIManager.Instance.SetDrawCount();
                ShuffleDeck();
                break;

            case EAddDeck.Dummy:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                CardDummy.Add(setCard);
                UIManager.Instance.SetDummyCount();
                break;

            case EAddDeck.Hand:                 // 핸드로 가져오는 건 카드 정렬 때문에 DrawCard 함수를 이용해서만 접근할 것.
                if (HandCard.Count < 10)
                {
                    setCard.WaitUnblock(CardUtils.CardAlignmentDelay).Forget();
                    cardObject.transform.position = Vector3.zero;
                    HandCard.Add(setCard);
                }
                else
                {
                    cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                    CardDummy.Add(setCard);
                    UIManager.Instance.SetDummyCount();
                }
                break;
        }
        TotalDeck.Add(setCard);
    }
    public void AddDeck(int id, EAddDeck eAddDeck)     // 덱에 카드를 아이디로 추가할 때 사용, 핸드로 카드를 가져올 때는 DrawCard 함수 사용.
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
    //        case CardTag.MultiAttack:
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
    //        UIManager.Instance.SetDummyCount();
    //    }
    //    playedCard.Block = false;
    //    playedCard.Used = false;
    //}

    async UniTask CheckCanUseingCard(Card card/*, bool singleAtk = false*/)
    {
        //if (card.Used) return;
        card.Used = true;
        card.CardOrder.SetOriginOrder(-10);

        HandCard.Remove(card);
        SetOriginOrder();
        CardAlignment();
        _usedCard = card;       // 다른 카드가 사용 중이면 사용 못 하게 막을지 고민 중
        //if (singleAtk)
        //    card.Target(EnemyManager.Instance.targetEnemy);
        if (!await card.BeforeUsingCard())
        {
            card.Used = false;
            HandCard.Add(card);
            SetOriginOrder();
            CardAlignment();
            _usedCard = null;
            PutDownCard(card).Forget();

            return;
        }


        InGameManager.Instance.AbilityEventQueue.Enqueue(card);
        //_eventQueue.Enqueue(card);
        _usedCard = null;
    }

    public void ChangeTotalCardDesc()
    {
        foreach (Card card in TotalDeck)
        {
            card.CardDataReset();
        }
    }

    public void ClearCard()
    {
        DrawDeck.Clear();
        CardDummy.Clear();
        UIManager.Instance.SetDrawCount();
        UIManager.Instance.SetDummyCount();
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
                UIManager.Instance.SetDrawCount();
                card.transform.position = CardSpawnPoint.position;
            }
            CardDummy.Clear();
            UIManager.Instance.SetDummyCount();
        }
        else
        {
            //DrawDeck = MainDeck.ToList();
            foreach (Card card in MainDeck)
            {
                DrawDeck.Add(card);
                UIManager.Instance.SetDrawCount();
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
        if (HandCard.Count == 10) return null;

        if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기. 이 경우에는 카드 뽑기가 0.5초 후 가능 (카드 버려지는 시간인 0.3초보단 높게 잡아야 함.)
        {
            SetupDrawDeck();
            await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        }

        if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
            return null;

        Card card = DrawDeck[0];
        DrawDeck.RemoveAt(0);
        UIManager.Instance.SetDrawCount();
        return card;
    }

    public Card CardToDraw(Card drawCard)
    {
        if (HandCard.Count == 10) return null;

        Card card = DrawDeck.Find(x => x == drawCard);
        DrawDeck.Remove(card);
        UIManager.Instance.SetDrawCount();
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

        SetOriginOrder();
        CardAlignment();
        await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    }
    public async UniTask DrawCard(int count)   // Count로 뽑는 거 성공 시, 아래 있는 DrawCards는 필요없음.
    {
        for (int i = 0; i < count; ++i)
        {
            Card drawCard = await CardToDraw();
            if (drawCard == null)
                break;

            HandCard.Add(drawCard);

            SetOriginOrder();
            CardAlignment();
            await UniTask.WaitForSeconds(CardUtils.CardAlignmentDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
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

        SetOriginOrder();
        CardAlignment();

    }

    public void AddCard(CardData cardData)  // 카드 생성
    {
        AddDeck(cardData, EAddDeck.Hand);

        SetOriginOrder();
        CardAlignment();
    }

    public void AddCard(int id)  // 카드 생성
    {
        AddDeck(id, EAddDeck.Hand);

        SetOriginOrder();
        CardAlignment();
    }

    public void ResetSetting()
    {
        isSingleTarget = false;
        useSingleTargetCard = false;
        draggable = false;
        BattleManager.Instance.SetActiveArrowCursor(false, 0);
        isUseCard.Value = false;

        _selectCard = null;      // isUseCard와 순서 중요! selectCard가 밑에 있어야 함.

        canPush = true;         // PullCard랑 중복 호출이긴 함.

        UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, true);
    }
    public void ReturnSelectedCard()
    {
        foreach (Card selectedCard in _selectedCards)
        {
            selectedCard.Selected = false;
            HandCard.Add(selectedCard);
        }
        //SetOriginOrder();             // 리턴 카드를 할 경우, 사용하고 있던 카드를 다시 내 손패로 돌려보냄. 이때, 전체 카드 정렬을 하기 때문에 여기서 따로 할 필요 없음.
        //CardAlignment();
        _selectedCards.Clear();
    }
    void SelectedCards(Card card)
    {
        if (_selectedCards.Contains(card))
        {
            card.Selected = false;
            _selectedCards.Remove(card);
            SetSelectedCardsOrder(_selectedCards);
            SelectedCardsAlignment(_selectedCards);
            HandCard.Add(card);
            SetOriginOrder();
            CardAlignment();

        }
        else
        {
            card.Selected = true;
            _selectedCards.Add(card);
            SetSelectedCardsOrder(_selectedCards);
            SelectedCardsAlignment(_selectedCards);
            HandCard.Remove(card);
            SetOriginOrder();
            CardAlignment();
        }
        //if (HandCard.Count <= _usedCard.Data.Discard)
        //{
        //    // 카드 효과가 버리기일 경우, 위 상황에서는 바로 버리기 가능해야 함. 반대로 카드조건이 버리기일 경우, 불가능.
        //}
        if (_usedCard != null)      // 카드 조건이 버리기인 경우
        {
            if (_usedCard.Data.Discard > 0)
            {
                if (_selectedCards.Count == _usedCard.Data.Discard)
                {
                    ButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    ButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
            else if (_usedCard.Data.Discard == 0)
            {
                ButtonManager.Instance.DiscardBtnInvert(true);
            }
            else
            {
                if (_selectedCards.Count >= -_usedCard.Data.Discard)
                {
                    ButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    ButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
        }
        else                        // 카드 효과가 버리기인 경우
        {
            if (_playedCard.Data.Discard > 0)
            {
                if (_selectedCards.Count == _playedCard.Data.Discard)
                {
                    ButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    ButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
            else if (_playedCard.Data.Discard == 0)
            {
                ButtonManager.Instance.DiscardBtnInvert(true);
            }
            else
            {
                if (_selectedCards.Count >= -_playedCard.Data.Discard)
                {
                    ButtonManager.Instance.DiscardBtnInvert(true);
                }
                else
                {
                    ButtonManager.Instance.DiscardBtnInvert(false);
                }
            }
        }
    }
    public void ChangeDiscard(bool discard)
    {
        _discard = discard;
        //InGameManager.Instance.Pause(discard);
        UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.SelectedCard, discard);
        if (discard)
        {
            SetCardState(3);   // Click
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, false);
            ButtonManager.Instance.ActItemBtnInvert(false);
        }
        else
        {
            SetCardState(2);    // Drag
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, true);
            ButtonManager.Instance.ActItemBtnInvert(true);
        }
    }
    public void ChangeRemove(bool remove)
    {
        _discard = remove;
        UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.SelectedCard, remove);
        if (remove)
        {
            SetCardState(3);   // Click
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, false);
            ButtonManager.Instance.ActItemBtnInvert(false);
        }
        else
        {
            SetCardState(2);    // Drag
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, true);
            ButtonManager.Instance.ActItemBtnInvert(true);
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
        }
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        foreach (Card targetCard in _selectedCards)
        {
            CardDummy.Add(targetCard);
            targetCard.UnblockCard();
        }
        UIManager.Instance.SetDummyCount();
        _selectedCards.Clear();
    }

    public async UniTask ThrowAwayCard()        // 모든 카드를 카드 더미로
    {
        ResetSetting();
        foreach (Card targetCard in HandCard)
        {
            targetCard.BlockCard();
            targetCard.MoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);
        }
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        foreach (Card targetCard in HandCard)
        {
            CardDummy.Add(targetCard);
            targetCard.UnblockCard();
        }
        UIManager.Instance.SetDummyCount();
        HandCard.Clear();
    }
    public async UniTask ThrowAwayCard(Card throwCard)
    {
        HandCard.Remove(throwCard);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), false, CardUtils.ThrowAwayCardDelay);

        CardDummy.Add(throwCard);
        UIManager.Instance.SetDummyCount();
    }

    public async UniTask PlayedCard(Card playedCard)
    {
        if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
        {
            playedCard.FailedUseCard();
            return;
        }
        _playedCard = playedCard;       // 마지막으로 시전한 카드 정보를 받아와야 할 수도 있기 때문에 일단 초기화는 안 함.

        HandCard.Remove(playedCard);

        SetOriginOrder();
        CardAlignment();

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

    void SetOriginOrder()       // 카드가 보이는 순서 설정
    {
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];
            if (targetCard == _selectCard)
                continue;
            targetCard.CardOrder.SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs;
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count, CardUtils.CardScale);
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];

            targetCard.OriginPRS = originCardPRSs[i];
            if (targetCard == _selectCard)
                continue;
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

    #region MyCard

    public void CardMouseOver(Card card)                    // Enter로 안 하는 이유는... Enter로 하면 순간순간 카드가 over 안 되는 경우의 수가 존재함.
    {
        if (CardState == ECardState.Nothing || draggable)
            return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        LargeCard(card);
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
            HandCard[cardIndex - i].transform.DOMoveX(HandCard[cardIndex - i].OriginPRS.pos.x - 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);        // .SetUpdate(true) 추가함.
        }

        for (int i = 1; i < HandCard.Count - cardIndex; ++i)
        {
            HandCard[cardIndex + i].transform.DOMoveX(HandCard[cardIndex + i].OriginPRS.pos.x + 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);        // .SetUpdate(true) 추가함.
        }

        canPush = false;
    }

    void PullCard()     // PushCard()보다 움직임 속도가 빨라야 함. 즉, dotweenTime 값은 더 작아야 함.
    {
        canPush = true;
        foreach (Card card in HandCard)
        {
            card.transform.DOKill();            // 정렬 하는 코드 삭제
            card.MoveTransform(card.OriginPRS, true, CardUtils.CardAlignmentDelay * 0.5f);
        }

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

        UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, false);

        _selectCard = card;
        draggable = true;
        card.BlockCard();
    }

    public async UniTask CardMouseUp(Card card)
    {
        if (CardState != ECardState.CanMouseDrag)
        {
            //card.Block = false;
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (card.Used) return;

        if (isUseCard.Value)        // 카드 사용 가능 범위에 들어왔는지 확인
        {
            if (card.Data.CardTag != CardTag.SingleAttack)     // 단일타격을 제외한 나머지
            {
                ResetSetting();
                await CheckCanUseingCard(card);
            }
            else if (useSingleTargetCard)                      // 단일타격이 가능할 경우
            {
                card.Target(EnemyManager.Instance.TargetEnemy/*.GetComponent<Enemy>()*/);
                ResetSetting();
                await CheckCanUseingCard(card/*, true*/);
            }
            else
            {
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

    public async UniTaskVoid PutDownCard(Card card)
    {
        card.CardOrder.SetMostFrontOrder(false);
        PullCard();
        await card.TaskMoveTransform(card.OriginPRS, true, CardUtils.CardAlignmentDelay).SuppressCancellationThrow();
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

        if (isUseCard.Value && card.Data.CardTag != CardTag.SingleAttack)
        {
            Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            card.transform.position = tempPos;
        }
        else if (!isUseCard.Value)
        {
            Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            card.transform.position = tempPos;
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        int layer = LayerMask.NameToLayer("UsedCardArea");
        isUseCard.Value = Array.Exists(hits, x => x.collider.gameObject.layer == layer);      // 카드 사용 범위에 있을 경우, isUseCard = true

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
                break;
            case 1:
                CardState = ECardState.CanMouseOver;
                break; 
            case 2:
                CardState = ECardState.CanMouseDrag;
                break; 
            case 3:
                CardState = ECardState.OnlyMouseClick;
                break;
        }
    }

    //void SetActiveArrowCursor(bool isOn)
    //{
    //    ArrowCursor.SetActive(isOn);
    //    Cursor.visible = !isOn;
    //}
    #endregion
}
