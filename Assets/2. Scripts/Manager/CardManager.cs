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

    public ECardState cardState;

    public CardData rewardCardData;

    [SerializeField] CardSO cardSO;

    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardDummyTr;

    //[SerializeField] Transform Deck;    // 소환된 덱 카드들

    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;

    //[SerializeField] GameObject arrow;

    [SerializeField] GameObject cardPrefab;

    public Transform cardRewardContent;

    [SerializeField] Transform shopCard;
    [SerializeField] Transform shopCardPrice;

     List<Card> _selectedCards = new();           // 배틀 중 버리기, 강화, 교환 등에서 선택한 카드 리스트

    Card selectCard;                            // 들고 있는 카드(drag 중인 카드)
    bool draggable;
    ReactiveProperty<bool> isUseCard = new();     // 카드 사용존에 카드가 올라왔을 경우(카드를 놓으면 카드가 사용되는 위치)

    bool canPush = true;
    public enum ECardState { Nothing, CanMouseOver, CanMouseDrag, OnlyMouseClick }

    int shopCardIdx;

    //UICard[] uICards = new UICard[4];

    EventQueue _eventQueue = new();

    bool _discard;
    bool _remove;


    private void Awake() => Instance = this;


    private void Start()
    {
        isUseCard.Subscribe((canUse) =>
        {
            selectCard?.TurnOnOutline(canUse);
            if (canUse && selectCard.Data.CardTag == CardTag.SingleAttack && !isSingleTarget)
            {
                GameManager.Instance.ArrowCursor(true);
                //PullCard();
                selectCard.transform.DOKill();        // 마우스 커서가 카드를 나갈 때 카드 크기가 원래대로 돌아가는 코드를 멈춰주는 함수.
                selectCard.transform.position = new Vector2(0, -3.32f);
                isSingleTarget = true;
            }
            //else if (isUseCard.Value && selectCard.Data.CardTag != CardTag.SingleAttackAb)
            //{
            //    Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //    selectCard.transform.position = tempPos;
            //}
            else if (!canUse && isSingleTarget)
            {
                GameManager.Instance.ArrowCursor(false);
                isSingleTarget = false;
            }
            //else if (!isUseCard.Value)
            //{
            //    Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //    selectCard.transform.position = tempPos;
            //}
        });

        SetupStartCardDeck();

        //for (int i = 0; i < uICards.Length; ++i)
        //{
        //    uICards[i] = cardRewardContent.GetChild(i).GetComponent<UICard>();
        //}
        //StartBattle();    // 현재 Battle.cs에서 진행중
    }
    //private void Update()
    //{
    //    SetCardState();     // UniRx 이용해서 따로 처리할 것

    //}

    //public void ShowRewardCard(int[] reward)        // 해당 부분들 맵, 상점으로 다 이동시켜야 함.
    //{
    //    for (int i = 0; i < reward.Length; ++i)
    //        uICards[i].Setup(FindCardData(reward[i]));
    //}

    public void RewardedCard()
    {
        AddDeck(rewardCardData, EAddDeck.Main);
        MapManager.Instance.GetReward();
        UiManager.Instance.LookMap();
    }

    //public void SettingCardShop()
    //{
    //    for (int i = 0; i < shopCard.childCount; ++i)
    //    {
    //        CardData _cardData = FindCardData(Random.Range(100, 106));
    //        shopCard.GetChild(i).GetComponent<UICard>().Setup(_cardData);       // 나중에 다 캐싱할 것
    //        shopCardPrice.GetChild(i).GetComponent<TMP_Text>().text = _cardData.Price.ToString();
    //    }
    //}
    //public void BuyCard()
    //{
    //    if (GameManager.Instance.player.Coin.Value >= rewardCardData.Price)
    //    {
    //        GameManager.Instance.player.Coin.Value -= rewardCardData.Price;
    //        AddDeck(rewardCardData, EAddDeck.Main);
    //        shopCard.GetChild(shopCardIdx).GetComponent<UICard>().gameObject.SetActive(false);
    //        shopCardPrice.GetChild(shopCardIdx).GetComponent<TMP_Text>().text = "";
    //    }
    //    else
    //        print("돈부족");
    //}

    //public void BuyCardIdx(int idx)
    //{
    //    shopCardIdx = idx;
    //}

    //public CardData FindCardData(int id)   // Id 값으로 카드데이터 가져오기
    //{
    //    CardData _cardData;
    //    if (CardDatas.TryGetValue(id, out _cardData))
    //    {
    //        return _cardData;
    //    }
    //    else
    //    {
    //        _cardData = (CardData)Array.Find(cardSO.Cards, x => x.Id == id).Clone();
    //        CardDatas.Add(id, _cardData);
    //        return _cardData;
    //    }
    //    //return cardSO.Cards.Find(x => x.Id == Id);
    //    //return Array.Find(cardSO.Cards, x => x.Id == Id);
    //}

    void SetupStartCardDeck()   // 시작할 때, 메인덱을 설정하는 함수 (게임 시작 이후에는 사용하지 않음.)
    {
        int _startDeck = 6;     // 여기 밑 코드 변경해야 함. 캐릭터별로 얻는 카드와 카드 ID가 달라지기 때문에 switch로 구별.
        for (int i = 0; i < _startDeck; i++)
            AddDeck(100 + i, EAddDeck.Main);

        //AddDeck(FindCardInCardSO(1000), EAddDeck.Main);     // 이 부분은 제거할 것
        //AddDeck(FindCardInCardSO(1001), EAddDeck.Main);     // 이 부분은 제거할 것
    }
    public void AddDeck(CardData cardData, EAddDeck eAddDeck)       // 덱에 카드를 추가할 때 사용, 핸드로 카드를 가져올 때는 DrawCard 함수 사용. (주로 데이터 자체가 이동할 때 사용)
    {
        //GameObject cardObject = PoolManager.Instance.CardPool.Get();
        //    /*Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, Deck);*/
        //Card blockCard = cardObject.GetComponent<Card>();
        PoolManager.Instance.GetCard(out GameObject cardObject, out Card setCard);

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
                ShuffleDeck();
                break;

            case EAddDeck.Dummy:
                cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                CardDummy.Add(setCard);
                break;

            case EAddDeck.Hand:                 // 핸드로 가져오는 건 카드 정렬 때문에 DrawCard 함수를 이용해서만 접근할 것.
                if (HandCard.Count < 10)
                {
                    WaitUnblock(setCard, CardUtils.CardAlignmentDelay).Forget();
                    cardObject.transform.position = Vector3.zero;
                    HandCard.Add(setCard);
                }
                else
                {
                    cardObject.transform.localScale = CardUtils.CardScale * 0.5f;
                    CardDummy.Add(setCard);
                }
                break;
        }
        TotalDeck.Add(setCard);
    }
    public void AddDeck(int id, EAddDeck eAddDeck)     // 덱에 카드를 아이디로 추가할 때 사용, 핸드로 카드를 가져올 때는 DrawCard 함수 사용.
    {
        CardData cardData = GameManager.Instance.FindCardData(id);

        AddDeck(cardData, eAddDeck);
    }

    async UniTaskVoid WaitUnblock(Card blockCard, float waitTime)
    {
        blockCard.Block = true;
        await UniTask.WaitForSeconds(waitTime);
        blockCard.Block = false;
    }

    void BlockCard(Card blockCard)
    {
        blockCard.Block = true;
    }
    void UnblockCard(Card blockCard)
    {
        blockCard.Block = false;
    }

    public void FailedUseCard(Card useCard)
    {
        useCard.Block = false;
        useCard.Used = false;
    }

    bool BeforeUsingCard(Card card)
    {
        if (GameManager.Instance.player.CurHolo < card.Data.Cost)
        {
            PutDownCard(card).Forget();
            return false;
        }

        //if (card.CheckUseConditions)
        //{

        //}
        
        GameManager.Instance.player.ChangeHoloValue(-card.Data.Cost);

        return true;
    }

    async UniTask AfterCardAbility(Card usedCard, bool endBattle = false)
    {
        await usedCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), false, CardUtils.ThrowAwayCardDelay);

        if (!endBattle)
        {
            CardDummy.Add(usedCard);
        }
        //usedCard.transform.position = cardSpawnPoint.position;
        usedCard.Block = false;
        usedCard.Used = false;
    }

    //IEnumerator WaitUnblock(Card blockCard, float waitTime)       //  .Forget() 추가하기 귀찮아서 그냥 코루틴으로 작성함.
    //{
    //    yield return new WaitForSeconds(waitTime);
    //    blockCard.Block = false;
    //}

    public void ChangeTotalCardDesc()
    {
        foreach (Card card in TotalDeck)
        {
            card.ChangeCardDesc();
        }
    }

    public void ClearCard()
    {
        DrawDeck.Clear();
        CardDummy.Clear();
        //HandCard.Clear();     클리어카드 전에 카드를 전부 버리기 때문에 HandCard.Clear()는 안 해도 됨.
        for (int i = 0; i < TotalDeck.Count; ++i)
        {
            {
                if (MainDeck.Contains(TotalDeck[i]))
                {
                    //card.DOKill();
                    //card.MoveTransform(new PRS(cardSpawnPoint.position, Quaternion.identity, CardUtils.CardScale), false);
                    //TotalDeck[i].CardDataReset();
                    continue;
                }
                //TotalDeck[i].CardDataReset(true);
                TotalDeck[i].CardRelease();
                TotalDeck.Remove(TotalDeck[i--]);
            }

            //for (int i = 0; i < Deck.childCount; i++)
            //{
            //    if (!Deck.GetChild(i).gameObject.activeSelf)
            //        continue;
            //    Card card = Deck.GetComponentsInChildren<Card>(true)[i];
            //    //GameObject cardObject = Deck.GetChild(i).gameObject;
            //    if (MainDeck.Contains(card))
            //    {
            //        //card.DOKill();
            //        //card.MoveTransform(new PRS(cardSpawnPoint.position, Quaternion.identity, CardUtils.CardScale), false);
            //        continue;
            //    }
            //    card.CardRelease();
            //    //MainCardDeck.Add(card);
            //    //cardObject.GetComponent<Card>().CardRelease();

            //}
        }
    }
    public void SetupDrawDeck(bool start = false)  // 드로우덱 섞기(start가 true일 경우, 메인덱에서 가져옴. false일 경우, 카드더미에서 가져옴.)
    {
        if (!start)
        {
            DrawDeck = CardDummy.ToList();
            foreach (Card card in DrawDeck)
            {
                card.transform.position = cardSpawnPoint.position;
            }
            //foreach (Card card in CardDummy)
            //{
            //    DrawDeck.Add(card);
            //}
            CardDummy.Clear();
        }
        else
        {
            DrawDeck = MainDeck.ToList();
            foreach (Card card in DrawDeck)
            {
                card.transform.position = cardSpawnPoint.position;
            }
            //foreach (Card card in MainDeck)
            //{
            //    DrawDeck.Add(card);
            //}
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
        return card;
    }

    public Card CardToDraw(Card drawCard)
    {
        //if (DrawDeck.Count == 0)    // 뽑을 카드가 없으면 버려진 카드를 다시 불러오고, 덱 섞기. 이 경우에는 카드 뽑기가 LoadCardDummyDelay초 후 가능 (카드 버려지는 시간인 ThrowAwayCardDelay초보단 높게 잡아야 함.)
        //{
        //    SetupDrawDeck();
        //    await UniTask.WaitForSeconds(CardUtils.LoadCardDummyDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //}

        //if (DrawDeck.Count == 0)    // 덱을 섞은 후에도 뽑을 카드가 없으면 리턴
        //    return null;
        if (HandCard.Count == 10) return null;

        Card card = DrawDeck.Find(x => x == drawCard);
        DrawDeck.Remove(card);
        return card;

        //for (int i = 0; i < DrawDeck.Count; i++)
        //{
        //    if (DrawDeck[i] == drawCard)
        //    {
        //        Card card = DrawDeck[i];
        //        DrawDeck.RemoveAt(i);
        //        return card;
        //    }
        //}
        //return null;
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

    public async UniTask DrawCard()   // 손패로 드로우할 카드 (DrawCards와 다르게 배열 생성을 안 하기 때문에 1개 뽑을 때는 이걸 사용하는 게 맞을 듯.)
    {
        Card drawCard = await CardToDraw();
        if (drawCard == null)
            return;
        //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        //Card card = drawCard.GetComponent<Card>();
        //drawCard.Setup(drawCard.Data);
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

    //    //GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
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

        //Card card = drawCard.GetComponent<Card>();
        //drawCard.Setup(drawCard.Data);
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
        GameManager.Instance.ArrowCursor(false);
        isUseCard.Value = false;

        selectCard = null;      // isUseCard와 순서 중요! selectCard가 밑에 있어야 함.

        canPush = true;         // PullCard랑 중복 호출이긴 함.

        //_discard = false;
        //_remove = false;
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
    }
    public void ChangeDiscard(bool discard)
    {
        _discard = discard;
        UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.SelectedCard, discard);
    }
    public void ChangeRemove(bool remove)
    {
        _discard = remove;
    }
    void DiscardCard(Card card)      // 카드 선택해서 버리기
    {
        SelectedCards(card);
    }

    void RemoveCard(Card card)       // 카드 삭제하기(배틀 안에서만 적용)
    {
        SelectedCards(card);
    }

    public async UniTask ThrowAwayCard()        // 모든 카드를 카드 더미로
    {
        ResetSetting();
        foreach (Card targetCard in HandCard)
        {
            BlockCard(targetCard);
            targetCard.MoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), true, CardUtils.ThrowAwayCardDelay);
        }
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        foreach (Card targetCard in HandCard)
        {
            CardDummy.Add(targetCard);
            UnblockCard(targetCard);
            //targetCard.transform.position = cardSpawnPoint.position;
        }
        HandCard.Clear();


        //foreach (Card dummyCard in CardDummy)
        //{
        //    dummyCard.Block = false;
        //    dummyCard.CardPool.Release(dummyCard.gameObject);
        //}
    }
    public async UniTask ThrowAwayCard(Card throwCard)
    {
        HandCard.Remove(throwCard);

        SetOriginOrder();
        CardAlignment();

        await throwCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), false, CardUtils.ThrowAwayCardDelay);

        //throwCard.Block = false;
        //throwCard.transform.position = cardSpawnPoint.position;
        CardDummy.Add(throwCard);
    }

    public async UniTask UsedCard(Card usedCard)
    {
        if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
        {
            FailedUseCard(usedCard);
            _eventQueue.QueueClear();
            return;
        }

        HandCard.Remove(usedCard);


        //await UniTask.Delay(TimeSpan.FromSeconds(usedCard.Data.cardUseDelay));

        //if (MapManager.Instance.currStage.cleared)
        //    return;

        SetOriginOrder();
        CardAlignment();

        //usedCard.CardTask?.Invoke(usedCard);
        //bool endBattle = await usedCard.CardTask.SuppressCancellationThrow();

        //usedCard.UseAction();
        //bool endBattle = await usedCard.UseLazy().SuppressCancellationThrow();

        bool endBattle = await usedCard.UseTask().SuppressCancellationThrow();
        if (endBattle)
        {
            _eventQueue.QueueClear();
        }
        await AfterCardAbility(usedCard, endBattle);

        //bool endBattle = await UniTask.WaitForSeconds(usedCard.Data.CardUseDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();   // Action<Card>였을 때 사용. 일단 지금은 사용 해제
        //if (endBattle)
        //{
        //    _eventQueue.QueueClear();
        //}

        /*bool shutdown = */
        //await usedCard.TaskMoveTransform(new PRS(cardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), false, CardUtils.ThrowAwayCardDelay).SuppressCancellationThrow();

        //if (!endBattle)
        //{
        //    CardDummy.Add(usedCard);
        //}

        ////if (!shutdown)
        ////{
        //usedCard.transform.position = cardSpawnPoint.position;
        ////}
        ////else
        ////{
        ////    usedCard.MoveTransform(new PRS(cardSpawnPoint.position, Quaternion.identity, CardUtils.CardScale));
        ////}
        ////throwCard.Block = false;
        ////usedCard.transform.position = cardSpawnPoint.position;
        //usedCard.Block = false;
        //usedCard.Used = false;

        ////await UniTask.Delay(TimeSpan.FromSeconds(usedCard.Data.cardUseDelay));
        ////CardDummy.Add(usedCard);
    }


    void SetSelectedCardsOrder(List<Card> cardList)
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            Card targetCard = cardList[i];
            //if (targetCard == selectCard)
            //    continue;
            targetCard.CardOrder.SetOriginOrder(i);
        }
    }

    void SelectedCardsAlignment(List<Card> cardList)     // 버리기, 삭제 등에서 사용할 예정
    {
        List<PRS> CardPRSs/* = new List<PRS>()*/;
        CardPRSs = SerialAlignment(new Vector2(-6, 1), new Vector2(6, 1), cardList.Count/*, 0.5f*/, CardUtils.CardScale * 0.7f);
        for (int i = 0; i < cardList.Count; i++)
        {
            Card targetCard = cardList[i];
            WaitUnblock(targetCard, CardUtils.CardAlignmentDelay).Forget();

            //targetCard.OriginPRS = CardPRSs[i];
            //if (targetCard == selectCard)
            //    continue;
            targetCard.MoveTransform(CardPRSs[i], true, CardUtils.CardAlignmentDelay);
        }
    }

    List<PRS> SerialAlignment(Vector3 leftVector, Vector3 rightVectpr, int cardCount/*, float height*/, Vector3 scale)
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

            //float curve = Mathf.Sqrt(Mathf.Pow(/*height*/0.5f, 2) * (1 - (Mathf.Pow(cardLerps[i] - 0.5f, 2) / Mathf.Pow(leftTr.position.x, 2))));   // 타원의 방정식

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
            if (targetCard == selectCard)
                continue;
            targetCard.CardOrder.SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs/* = new List<PRS>()*/;
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, HandCard.Count/*, 0.5f*/, CardUtils.CardScale);
        for (int i = 0; i < HandCard.Count; i++)
        {
            Card targetCard = HandCard[i];

            targetCard.OriginPRS = originCardPRSs[i];
            if (targetCard == selectCard)
                continue;
            targetCard.MoveTransform(targetCard.OriginPRS, true, CardUtils.CardAlignmentDelay);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int cardCount/*, float height*/, Vector3 scale)
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
        if (cardState == ECardState.Nothing || draggable)
            return;
        //selectCard = card;
        LargeCard(/*true, */card);
        if (card.Selected) return;
        PushCard(card);
    }
    public void CardMouseExit(Card card)
    {
        if (cardState == ECardState.Nothing || draggable)
            return;
        //selectCard = null;
        //LargeCard(false, card);
        //PullCard();
        if (card.Selected)
        {
            card.MoveTransform(new PRS(card.transform.position, Quaternion.identity, CardUtils.CardScale * 0.7f));
            card.CardOrder.SetMostFrontOrder(false);
            return;
        }
        PutDownCard(card).Forget();
    }


    void LargeCard(/*bool isLarge, */Card card/*, bool selectedCards = false*/)
    {
        if (card.Selected)
        {
            card.transform.DOKill();
            card.MoveTransform(new PRS(card.transform.position, Quaternion.identity, CardUtils.CardScale * 0.8f));
            card.CardOrder.SetMostFrontOrder(true);
            return;
        }
        //if (selectedCards)
        //{
        //    return;
        //}
        //if (isLarge)
        //{
            card.transform.DOKill();              // 정렬 드로우 문제 등 제거
            Vector3 largePos = new Vector3(card.OriginPRS.pos.x, -3.32f, /*-100f*/0);
            card.MoveTransform(new PRS(largePos, Quaternion.identity, CardUtils.CardScale * 1.2f));
        //}
        //else
        //    card.MoveTransform(card.OriginPRS, true, CardUtils.CardAlignmentDelay);

        //card.GetComponent<Order>().SetMostFrontOrder(isLarge);
        card.CardOrder.SetMostFrontOrder(true);
    }
    void PushCard(Card card)
    {
        if (!canPush)
            return;
        //int cardIndex = -1;
        //for (int i = 0; i < HandCard.Count; i++)
        //{
        //    if (HandCard[i] == card)
        //    {
        //        cardIndex = i;
        //        break;
        //    }
        //}
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

        //for (int i = 1; i < HandCard.Count; ++i)    // 나중에 수정 필요해보임.
        //{
        //    if (cardIndex - i >= 0)
        //    {
        //        //HandCard[cardIndex - i].transform.DOKill();
        //        HandCard[cardIndex - i].transform.DOMoveX(HandCard[cardIndex - i].OriginPRS.pos.x - 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);        // .SetUpdate(true) 추가함.
        //    }
        //    if (cardIndex + i < HandCard.Count)
        //    {
        //        //HandCard[cardIndex - i].transform.DOKill();
        //        HandCard[cardIndex + i].transform.DOMoveX(HandCard[cardIndex + i].OriginPRS.pos.x + 0.5f / i, CardUtils.CardAlignmentDelay).SetUpdate(true);
        //    }
        //}
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
        if (cardState == ECardState.OnlyMouseClick)
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
        if (cardState != ECardState.CanMouseDrag)
            return;
        selectCard = card;
        draggable = true;
        BlockCard(card);
        //card.Used = false;
    }

    public void CardMouseUp(Card card)
    {
        //draggable = false;
        //arrow.SetActive(false); => ArrowCursor(false);
        if (cardState != ECardState.CanMouseDrag)
        {
            //card.Block = false;
            return;
        }
        //draggable = false;
        //GameManager.Instance.ArrowCursor(false);
        //selectCard = null;

        if (GameManager.Instance.player.CurHolo < card.Data.Cost)
        {
            PutDownCard(card).Forget();
            ResetSetting();

            return;
        }

        if (isUseCard.Value && card.Data.CardTag != CardTag.SingleAttack)     // 단일타격을 제외한 나머지
        {
            _eventQueue.Enqueue(card);
            //CheckCanUseCard(card);
        }
        else if (isUseCard.Value /*&& card.Data.CardTag == CardTag.SingleAttackAb */&& useSingleTargetCard)     // 단일타격이 가능할 경우
        {
            card.Target(EnemyManager.Instance.targetEnemy);
            _eventQueue.Enqueue(card);
            //CheckCanUseCard(card);
        }
        else        // 사용되지 않은 경우
        {
            PutDownCard(card).Forget();
        }

        ResetSetting();
    }


    public async UniTask CheckCanUseCard(Card card)
    {
        //card.CardTask?.Invoke(card);
        if (GameManager.Instance.player.CurHolo < card.Data.Cost)
        {
            PutDownCard(card).Forget();
            return;
        }
        GameManager.Instance.player.ChangeHoloValue(-card.Data.Cost);
        await UsedCard(card);
        //await UniTask.WaitForSeconds(card.Data.CardUseDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //await UsedCard(card);
        //await UsedCard(card);
        //card.Block = false;
        //GameManager.Instance.blockClick = false;
    }

    //public void CheckCanUseCard(Card card)
    //{
    //    //card.CardTask?.Invoke(card);
    //    if (GameManager.Instance.player.CurHolo < card.Data.Cost)       // 두 번 체크해야 함.
    //    {
    //        PutDownCard(card).Forget();
    //        return;
    //    }
    //    GameManager.Instance.player.ChangeHoloValue(-card.Data.Cost);
    //    UsedCard(card).Forget();
    //    //await UsedCard(card);
    //    //card.Block = false;
    //    //GameManager.Instance.blockClick = false;
    //}

    //public bool CanUseHolo(Card card)                     // 나중에 쓸 수도 있으나, 일단 BeforeUsingCard로 사용.
    //{
    //    if (GameManager.Instance.player.CurHolo < card.Data.Cost)
    //    {
    //        PutDownCard(card).Forget();
    //        return false;
    //    }
    //    GameManager.Instance.player.ChangeHoloValue(-card.Data.Cost);
    //    return true;
    //}



    public async UniTaskVoid PutDownCard(Card card)
    {
        //comeBackCard = true;
        card.CardOrder.SetMostFrontOrder(false);
        PullCard();
        await card.TaskMoveTransform(card.OriginPRS, true, CardUtils.CardAlignmentDelay).SuppressCancellationThrow();
        UnblockCard(card);
        //draggable = false;
        //GameManager.Instance.blockClick = false;
    }

    public void CardDrag(Card card)
    {
        if (cardState != ECardState.CanMouseDrag || !draggable)
            return;

        DetectCardArea();

        //if (isUseCard.Value && card.Data.CardTag == CardTag.SingleAttackAb && !isSingleTarget)
        //{
        //    GameManager.Instance.ArrowCursor(true);
        //    //PullCard();
        //    card.transform.DOKill();        // 마우스 커서가 카드를 나갈 때 카드 크기가 원래대로 돌아가는 코드를 멈춰주는 함수.
        //    card.transform.position = new Vector2(0, -3.32f);
        //    isSingleTarget = true;
        //}
        /*else */if (isUseCard.Value && card.Data.CardTag != CardTag.SingleAttack)
        {
            Vector2 tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            card.transform.position = tempPos;
        }
        //else if (!isUseCard.Value && isSingleTarget)
        //{
        //    GameManager.Instance.ArrowCursor(false);
        //    isSingleTarget = false;
        //}
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

    public void SetCardState(int index)
    {
        switch (index)
        {
            case 0:
                cardState = ECardState.Nothing;
                break;
            case 1:
                cardState = ECardState.CanMouseOver;
                break; 
            case 2:
                cardState = ECardState.CanMouseDrag;
                break; 
            case 3:
                cardState = ECardState.OnlyMouseClick;
                break;
        }
    }

    //void ArrowCursor(bool isOn)
    //{
    //    arrow.SetActive(isOn);
    //    Cursor.visible = !isOn;
    //}
    #endregion
}
