using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }


    public Dictionary<int, CardData> CardDatas { get; private set; } = new Dictionary<int, CardData>();     // 나중에 CardManager로 이동
    public Vector2Int[] CardRarityID { get; private set; }
    public Vector2Int[] EnhancedCardRarityID { get; private set; }

    List<CardData>[] _randomCardList = new List<CardData>[4];
    //List<CardData> _popRandomCardList = new();


    public Dictionary<int, ItemBase> ItemDatas { get; private set; } = new Dictionary<int, ItemBase>();     // 나중에 ItemManager로 이동
    public Vector2Int PassiveID { get; private set; }
    public Vector2Int ActiveID { get; private set; }
    public Vector2Int PotionID { get; private set; }
    List<ItemBase>[] _randomItemList = new List<ItemBase>[3];
    //List<ItemData> _popRandomItemList = new();

    public EventQueue AbilityEventQueue = new();                // 나중에 BattleManager로 이동

    public StatusEffectSO SESO;
    public CardSO CardSO;
    [SerializeField] ItemSO _itemSO;

    //public Arrow ArrowCursor;

    public Transform PlayerTr;
    public Player Player;

    public int PauseInt;
    public bool ShowStatus;

    //public int CurrentLoadAsyncCount = 0;

    AsyncOperationHandle<GameObject> _playerHandle;
    GameObject _playerObj;

    //public int NowChapterLV = 1;

    //GameObject[] _camera;
    //GameObject[] _player;

    void Awake()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        if (Instance == null)
        {
            Instance = this;
            LoadAsync().Forget();
            //transform.SetParent(null);
            //DontDestroyOnLoad(_camera[0]);
            GameManager.Instance.AddInGameDontDestroy(Camera.main.gameObject);
            GameManager.Instance.AddInGameDontDestroy(player[0]);
            PlayerTr = player[0].transform;
            //Player = player[0].GetComponentInChildren<Player>(true);
            GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
            GameManager.Instance.InGame = true;
            //SpawnPlayer(GameManager.Instance.PlayerInt);
        }
        else
        {
            Camera[] mainCamera = Camera.allCameras;
            for (int i = mainCamera.Length - 1; i >= 0; --i)
            {
                if (i == 0) break;
                Destroy(mainCamera[i].gameObject);
            }
            //Destroy(mainCamera[1]);
            Destroy(player[1]);
            Destroy(transform.root.gameObject);
        }
        GameManager.Instance.ResolutionSetting(Camera.main);
        //StartCoroutine(ReadSpreadSheet.LoadData("https://docs.google.com/spreadsheets/d/1CqNR2Rh_OIVe8n0CG8vC7YVpbNUn_-0rXeBab72gXvs", "A3:D14", 0));

        //if (!CardDataDeserializer.TryGetData(1015, out CardData row))
        //{
        //    Debug.Log("데이터 테이블을 불러오는 과정에서 문제가 발생했습니다.");
        //}
    }

    void StartBattle()
    {
        Player.StartOrEndBattle(TurnManager.Instance.InBattle.Value);
    }
    void PlayerTurnStart()
    {
        Player.AddCurHolo(Player.MaxHolo);
        Player.ShieldReset();
    }

    void EndBattle()
    {
        Player.StartOrEndBattle(TurnManager.Instance.InBattle.Value);
        Player.ShieldReset();
        Player.RemoveStatusEffect();
    }
    private void Start()
    {
        //Random.InitState(255);
        CardRarityID = CardSO.ClassifyCardRarityID;
        EnhancedCardRarityID = CardSO.ClassifyEnhancedCardRarityID;

        PassiveID = _itemSO.PassiveID; ActiveID = _itemSO.ActiveID; PotionID = _itemSO.PotionID;

        SettingRandomCardList();
        SettingRandomItemList();

        TurnManager.Instance.OnBattleStart += StartBattle;
        TurnManager.Instance.OnPlayerTurnStart += PlayerTurnStart;
        TurnManager.Instance.OnBattleEnd += EndBattle;

        //SpawnPlayer(0);

        //InGameUIManager.Instance.SetupGameUi(true);
        //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }

    //public void StartLoadAsync(bool isStart)
    //{
    //    if (isStart)
    //    {
    //        CurrentLoadAsyncCount++;
    //    }
    //    else
    //    {
    //        CurrentLoadAsyncCount--;
    //    }
    //}

    public async UniTask LoadAsync()
    {
        GameManager.Instance.StartLoadAsync(true);
        string playerName = "playerName";
        int playerIdx = GameManager.Instance.PlayerInt;
        switch (playerIdx)
        {
            case 0:
                playerName = "PicoChan";
                break;
            case 1:
                playerName = "Muryotaisu";
                break;
        }
        _playerHandle = Addressables.LoadAssetAsync<GameObject>(playerName + ".prefab");


        await _playerHandle.ToUniTask();

        // 성공 여부 확인
        if (_playerHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("인게임 매니저 모든 에셋 로드 성공!");
            GameManager.Instance.StartLoadAsync(false);
            _playerObj = _playerHandle.Result;
            //await GameManager.Instance.IsAsyncLoadComplete.Where(isAllComplete => isAllComplete).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
            await UniRxExtensions.AwaitTrueAsync(GameManager.Instance.IsAsyncLoadComplete, this.GetCancellationTokenOnDestroy());
            SpawnPlayer(_playerObj, playerIdx);
        }
        else
        {
            Debug.LogWarning("하나 이상의 에셋 로드 실패");
        }


    }

    public void SpawnPlayer(GameObject player, int playerInt)
    {
        Player = Instantiate(player, PlayerTr).GetComponentInChildren<Player>();
        CardManager.Instance.SetupStartCardDeck();
        Player.SpawnPlayer();
        Player.BaseColor = Color.blue;
        switch (playerInt)
        {
            case 0:
                Player.BaseThickness = 0.0018f;
                break;
            case 1:
                Player.BaseThickness = 0.0012f;
                break;
        }
    }

    //public async UniTask AllLoadAsync()
    //{
    //    CancellationTokenSource cts = new CancellationTokenSource();
    //    var task1 = UniTask.WaitForSeconds(10f, cancellationToken: cts.Token);
    //    //{
    //    //    await UniTask.WaitForSeconds(2f);
    //    //    if (!cts.IsCancellationRequested)
    //    //    {
    //    //        cts.Cancel();
    //    //        cts.Dispose();
    //    //    }
    //    //});
    //    var task2 = UniTask.WaitUntil(() => CurrentLoadAsyncCount == 0, PlayerLoopTiming.Update, cts.Token);
    //    await UniTask.WhenAny(
    //        task1, task2
    //        );
    //    if (!cts.IsCancellationRequested)
    //    {
    //        cts.Cancel();
    //        cts.Dispose();
    //    }
    //}
    void SettingRandomCardList()
    {
        for (int i = 0; i < 4; ++i)
        {
            _randomCardList[i] = new();
            for (int j = CardRarityID[i].x; j < CardRarityID[i].y + 1; ++j)
            {
                _randomCardList[i].Add(FindCardData(j));
            }
        }
    }

    void SettingRandomItemList()
    {
        _randomItemList[0] = new();
        for (int j = PassiveID.x; j < PassiveID.y + 1; ++j)
        {
            _randomItemList[0].Add(FindItemData(j));
        }
        _randomItemList[1] = new();
        for (int j = ActiveID.x; j < ActiveID.y + 1; ++j)
        {
            _randomItemList[1].Add(FindItemData(j));
        }
        _randomItemList[2] = new();
        for (int j = PotionID.x; j < PotionID.y + 1; ++j)
        {
            _randomItemList[2].Add(FindItemData(j));
        }
    }
    public CardData CardSwapAndPop(int listIdx, int randomIdx)
    {
        CardData randomCard = null;
        if (_randomCardList[listIdx].Count > 1)
        {
            (_randomCardList[listIdx][randomIdx], _randomCardList[listIdx][^1]) = (_randomCardList[listIdx][^1], _randomCardList[listIdx][randomIdx]);
            randomCard = _randomCardList[listIdx][^1];
            //_popRandomCardList.Add(randomCard);
            _randomCardList[listIdx].RemoveAt(_randomCardList[listIdx].Count - 1);
        }
        else if (_randomCardList[listIdx].Count == 1)       // 카드풀이 늘어나면 if문은 빼도 됨. 에픽이랑 레전더리 개수가 부족해서 카드 시각화 안 되는 오류 때문에 조건문 걸어놓은 거임.
        {
            randomCard = _randomCardList[listIdx][0];
            //_popRandomCardList.Add(randomCard);
            _randomCardList[listIdx].RemoveAt(0);
        }
        else
        {
            print("End");
        }
        return randomCard;
    }
    public CardData RandomCard(int rewardIdx)
    {
        int probability;
        switch (rewardIdx)
        {
            case 1:     // 커먼 상자
                probability = Random.Range(1, 1001);        // 카드 등급 업 이벤트
                if (probability > 10)
                {
                    return CardSwapAndPop(0, Random.Range(0, _randomCardList[0].Count));
                }
                else if (probability > 5)
                {
                    return CardSwapAndPop(1, Random.Range(0, _randomCardList[1].Count));
                }
                else if (probability > 1)
                {
                    return CardSwapAndPop(2, Random.Range(0, _randomCardList[2].Count));
                }
                else
                {
                    return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
                }
            case 2:     // 레어 상자
                probability = Random.Range(1, 1001);
                if (probability > 7)
                {
                    return CardSwapAndPop(1, Random.Range(0, _randomCardList[1].Count));
                }
                else if (probability > 2)
                {
                    return CardSwapAndPop(2, Random.Range(0, _randomCardList[2].Count));
                }
                else
                {
                    return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
                }
            case 3:     // 에픽 상자
                probability = Random.Range(1, 1001);
                if (probability > 4)
                {
                    return CardSwapAndPop(2, Random.Range(0, _randomCardList[2].Count));
                }
                else
                {
                    return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
                }
            case 4:     // 전설 상자
                return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
            case 5:     // 보스 (일단 할 거 없어서 그냥 전설 상자랑 똑같게 만듦.)
                return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
            case 10:    // 상점
                probability = Random.Range(1, 1001);
                if (probability > 300 + 0)
                {
                    return CardSwapAndPop(0, Random.Range(0, _randomCardList[0].Count));
                }
                else if (probability > 15 + 0)
                {
                    return CardSwapAndPop(1, Random.Range(0, _randomCardList[1].Count));
                }
                else if (probability > 2 + 0)
                {
                    return CardSwapAndPop(2, Random.Range(0, _randomCardList[2].Count));
                }
                else
                {
                    return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
                }
            default:
                return null;
        }
    }
    public CardData[] RandomCards(int rewardIdx, int repeatNum)
    {
        CardData[] cards = new CardData[repeatNum];
        for (int i = 0; i < repeatNum; ++i)
        {
            CardData card = RandomCard(rewardIdx);
            int j = 0;
            while (card == null && j < 5)       // 랜덤 카드를 했는데, 없을 경우 5번 더 반복함. 확률적으로 등급이 올라가는 이벤트가 있기 때문.
            {
                card = RandomCard(rewardIdx);
                ++j;
            }
            cards[i] = card;
        }
        ReturnRandomCard(cards);
        return cards;
    }
    public void ReturnRandomCard(CardData[] cardDatas)
    {
        foreach (CardData cardData in cardDatas/*_popRandomCardList*/)           // _popRandomCardList로 했으나, 굳이 이렇게 해야하나? 싶어서 그냥 Map에서 받아오도록 변경함.
        {
            if (cardData == null) continue;       // 카드 개수가 부족해서 null 뜰 때가 있음.

            if (cardData.CardRarity == CardRarity.Legendary)
            {
                _randomCardList[3].Add(cardData);
            }
            else if (cardData.CardRarity == CardRarity.Epic)
            {
                _randomCardList[2].Add(cardData);
            }
            else if (cardData.CardRarity == CardRarity.Rare)
            {
                _randomCardList[1].Add(cardData);
            }
            else
            {
                _randomCardList[0].Add(cardData);
            }
        }
        //_popRandomCardList.Clear();
    }
    public ItemBase ItemSwapAndPop(int listIdx, int randomIdx)
    {
        ItemBase randomItem = null;
        if (_randomItemList[listIdx].Count > 1)
        {
            (_randomItemList[listIdx][randomIdx], _randomItemList[listIdx][^1]) = (_randomItemList[listIdx][^1], _randomItemList[listIdx][randomIdx]);
            randomItem = _randomItemList[listIdx][^1];
            //_popRandomItemList.Add(randomItem);
            _randomItemList[listIdx].RemoveAt(_randomItemList[listIdx].Count - 1);
        }
        else if (_randomItemList[listIdx].Count == 1)       // 아이템풀이 늘어나면 if문은 빼도 됨. 개수가 부족해서 시각화 안 되는 오류 때문에 조건문 걸어놓은 거임.
        {
            randomItem = _randomItemList[listIdx][0];
            //_popRandomItemList.Add(randomItem);
            _randomItemList[listIdx].RemoveAt(0);
        }
        else
        {
            print("End");
        }
        return randomItem;
    }
    public ItemBase RandomItem()     // Potion은 따로 만들 것.
    {
        int probability = Random.Range(1, 3);
        ItemBase randomItem;
        if (probability == 1)
        {
            randomItem = ItemSwapAndPop(0, Random.Range(0, _randomItemList[0].Count));
            if (randomItem != null)
            {
                return randomItem;
            }
            else
            {
                return ItemSwapAndPop(1, Random.Range(0, _randomItemList[1].Count));
            }
        }
        else
        {
            randomItem = ItemSwapAndPop(1, Random.Range(0, _randomItemList[1].Count));
            if (randomItem != null)
            {
                return randomItem;
            }
            else
            {
                return ItemSwapAndPop(0, Random.Range(0, _randomItemList[0].Count));
            }
        }
        
    }
    public void ReturnRandomItem(ItemBase[] itemDatas)
    {
        foreach (ItemBase itemData in itemDatas/*_popRandomItemList*/)       // _popRandomItemList로 했으나, 카드와 다르게 아이템은 안 먹은 경우에는 다른 방에서 뜨면 안 되고, 2가지 이상의 방에서 아이템이 떴는데, 한 곳에서 먹으면, ReturnRandomItem()을 실행하기 때문에 그냥 현재 방에 있는 Item들을 리턴하는 코드로 변경.
        {
            if (itemData == null) return;
            if (ItemManager.Instance.itemDict.TryGetValue(itemData.ID, out bool value) && value) continue;
            if (itemData.ItemTag == ItemTag.Potion)
            {
                _randomItemList[2].Add(itemData);
            }
            else if (itemData.ItemTag == ItemTag.Active)
            {
                _randomItemList[1].Add(itemData);
            }
            else
            {
                _randomItemList[0].Add(itemData);
            }
        }
        //_popRandomItemList.Clear();
    }
    public CardData FindCardData(int id)   // ID 값으로 카드데이터 가져오기
    {
        //CardData cardData;
        if (CardDatas.TryGetValue(id, out CardData cardData))
        {
            return cardData;
        }
        else
        {
            cardData = Array.Find(CardSO.Cards, x => x.ID == id).Clone();
            CardDatas.Add(id, cardData);
            return cardData;
        }
        //return _cardSO.Cards.Find(x => x.ID == ID);
        //return Array.Find(_cardSO.Cards, x => x.ID == ID);
    }
    public ItemBase FindItemData(int id)   // ID 값으로 데이터 가져오기
    {
        if (ItemDatas.TryGetValue(id, out ItemBase itemData))
        {
            return itemData;
        }
        else
        {
            if (_itemSO.PassiveID.x <= id && id <= _itemSO.PassiveID.y)
            {
                itemData = _itemSO.PassiveItems.Find(x => x.ID == id);
            }
            else if (_itemSO.ActiveID.x <= id && id <= _itemSO.ActiveID.y)
            {
                itemData = _itemSO.ActiveItems.Find(x => x.ID == id);
            }
            else if (_itemSO.PotionID.x <= id && id <= _itemSO.PotionID.y)
            {
                itemData = _itemSO.PotionItems.Find(x => x.ID == id);
            }
            if (itemData != null)
            {
                ItemDatas.Add(id, itemData);
                return itemData;
            }
            return null;
        }
        //return _cardSO.Cards.Find(x => x.ID == ID);
        //return Array.Find(_cardSO.Cards, x => x.ID == ID);
    }

    /// <summary>
    /// Arrow커서의 활성화 여부와 위치를 설정합니다.
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="arrowIdx">Arrow의 위치를 의미(0 = Card, 1 = Active, 2 = Potion)</param>
    //public void SetActiveArrowCursor(bool isOn, int arrowIdx)
    //{
    //    if (isOn)
    //    {
    //        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);
    //        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, false);
    //        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
    //    }
    //    else
    //    {
    //        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, true);
    //        InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
    //    }
    //    ArrowCursor.ArrowIndex = arrowIdx;
    //    ArrowCursor.SetStartArrow();
    //    ArrowCursor.gameObject.SetActive(isOn);
    //    Cursor.visible = !isOn;
    //}

    public int ChangeCoinValue(int coin)
    {
        int changeCoin = coin;
        Player.Coin.Value += changeCoin;
        if (Player.Coin.Value < 0)
        {
            changeCoin -= Player.Coin.Value;
            Player.Coin.Value = 0;

        }
        return changeCoin;
    }

    //public void Pause(bool pause)
    //{
    //    if (pause && PauseInt == 0)
    //        ++PauseInt;
    //    else if (!pause && PauseInt == 1)
    //        --PauseInt;
    //    else
    //    {
    //        PauseInt = pause ? ++PauseInt : --PauseInt;
    //        return;
    //    }
    //    //if (_selectAbility && _option) return;
    //    Time.timeScale = pause ? 0 : 1;
    //    //Physics2D.autoSyncTransforms = pause ? true : false;      // 정지상태에서 카드를 사용하는 경우에는 필요함. 근데, 지금은 따로 필요없음.
    //}

    void Update()
    {
        //if (_fastMode && GameManager.Instance.PauseNum == 0)
        //{
        //    Time.timeScale = 3f;
        //}
        //else if (_slowMode && GameManager.Instance.PauseNum == 0)
        //{
        //    Time.timeScale = 0.25f;
        //}
        //else { Time.timeScale = GameManager.Instance.PauseNum != 0 ? 0 : 1; }


        if (Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 300, LayerMask.GetMask("Default"));

                EnemyManager.Instance.CheckEnemy(hit.transform);
            }
        }
        //print(AbilityEventQueue._queue.Count);

//#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TurnManager.OnAddCard?.Invoke();
        //}
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CardManager.Instance.DrawCard().Forget();
            //TurnManager.Instance.DrawCardTask().Forget();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            CardSwapAndPop(0, Random.Range(0, _randomCardList[0].Count));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            TurnManager.Instance.EndPlayerTurn().Forget();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            MapManager.Instance.MoveBossStage().Forget();
        }

        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    player.AddAttackPower(-1);
        //}

        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    player.AddDefencePower(1);
        //}

        if (Input.GetKeyDown(KeyCode.S))
        {
            InGameUIManager.Instance.ShowStatus().Forget();
            //int i = 0;
            //while (!EnemyManager.Instance.SpawnEnemy(100, i))
            //{
            //    i++;
            //    if (i > EnemyManager.Instance.EnemySpawnPosition.Length - 1)
            //        break; 
            //}
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance.FastMode();
            //_fastMode = !_fastMode;
            //if (_fastMode)
            //    _slowMode = false;
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            GameManager.Instance.SlowMode();
            //_slowMode = !_slowMode;
            //if (_slowMode)
            //    _fastMode = false;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            InGameUIManager.Instance.LookMap();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (MapManager.Instance.CurrStage.State == Map.StageState.Enemy || MapManager.Instance.CurrStage.State == Map.StageState.Boss)
            {
                for (int i = EnemyManager.Instance.EnemyList.Count - 1; i >= 0; --i)
                {
                    //EnemyManager.Instance.EnemyList[i].CheckIfDead(9999, 1);
                    EnemyManager.Instance.EnemyList[i].TakeDamage(9999).Forget();
                    //EnemyManager.Instance.enemies[i].TakeDamageEnemy(9999).Forget();
                }
            }
            else if (MapManager.Instance.CurrStage.State == Map.StageState.Start)
            {
                for (int i = EnemyManager.Instance.EnemyList.Count - 1; i >= 0; --i)
                {
                    //EnemyManager.Instance.EnemyList[i].CheckIfDead(9999, 1);
                    EnemyManager.Instance.EnemyList[i].TakeDamage(9999).Forget();
                    //EnemyManager.Instance.enemies[i].TakeDamageEnemy(9999).Forget();
                }
            }
            else
            {
                MapManager.Instance.ClearStage().Forget();
            }
        }

        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    foreach (var enemy in EnemyManager.Instance.EnemyList)
        //        enemy.BaseThickness -= 0.0001f;
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    foreach (var enemy in EnemyManager.Instance.EnemyList)
        //        enemy.BaseThickness += 0.0001f;
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    EnemyManager.Instance.TestSpawn().Forget();
        //}
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (MapManager.Instance.CurrStage.State == Map.StageState.Start)
            {
                EnemyManager.Instance.Spawndummy();
                if (!TurnManager.Instance.InBattle.Value)
                    TurnManager.Instance.StartBattle();
            }
        }
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    AddDeck(_cardSO.Cards[1], EAddDeck.Draw);
        //}
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CardManager.Instance.AddDeck(100, EAddDeck.Dummy);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))       // 카드 찾아서 뽑기 (수정 필요해보임. 덱에서 인덱스로 GameObject를 지정해서 넣어줄거면 굳이 drawCard 함수에서 카드를 확인해 볼 필요가 없음.)
        {
            //DrawCard(Deck.GetComponentsInChildren<Card>()[2].gameObject);  // 전투덱에서 가져오는 경우
            //DrawCard(DrawDeck[2]);   // 드로우덱에서 가져오는 경우
            //DrawCard(CardDummy[0]);  // 버린 카드덱에 있는 카드가 드로우덱에도 있을 경우 => 적용 안됨. 주소 문제인 듯
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))       // 카드 생성
        {
            CardManager.Instance.AddDeck(FindCardData(100), EAddDeck.Hand);
            CardManager.Instance.AddDeck(FindCardData(101), EAddDeck.Hand);
            CardManager.Instance.AddDeck(FindCardData(102), EAddDeck.Hand);
            //CardManager.Instance.AddDeck(FindCardData(103), EAddDeck.Hand);
            //CardManager.Instance.AddDeck(FindCardData(104), EAddDeck.Hand);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ShopManager.Instance.ChangeCardShop(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            MapManager.Instance.MovePrevStage().Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            MapManager.Instance.GetLootItem();
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            ChangeCoinValue(100);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            ChangeCoinValue(-100);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ItemManager.Instance.GetItem(FindItemData(501));
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ItemManager.Instance.GetItem(FindItemData(100));
            ItemManager.Instance.GetItem(FindItemData(101));
            ItemManager.Instance.GetItem(FindItemData(102));
            ItemManager.Instance.GetItem(FindItemData(1001));
            ItemManager.Instance.GetItem(FindItemData(1001));
            ItemManager.Instance.GetItem(FindItemData(1001));
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            print(FindItemData(100));
            MapManager.Instance.ShowAllMap();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Player.AddCurHolo(10);
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            Player.AddStatusEffect((StatusEffect.Weaking, StatusEffectType.NoAmountPerpetual), 1);
            Player.AddStatusEffect((StatusEffect.Vulnerable, StatusEffectType.NoAmountPerpetual), 1);
            //Player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), 1);
            //Player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.UseAmountTurnDuration), 3, 4);
            //Player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.TurnDuration), 3, 10);
            //player.AddAndApplyStatusEffect((StatusEffect.HealUp, StatusEffectType.InfiniteDuration), 1);
            //player.AddAndApplyStatusEffect((StatusEffect.DEFUp, StatusEffectType.InfiniteDuration), 1);
            //Player.AddStatusEffect((StatusEffect.Resurrection, StatusEffectType.UseAmountTurnDuration), 1, 10);
            //Player.AddStatusEffect((StatusEffect.Reflection, StatusEffectType.UseAmountTurnDuration), 5, 3);
            //Player.AddStatusEffect((StatusEffect.Protect, StatusEffectType.DurationIsAmount), 0, 3);
            //player.AddAndApplyStatusEffect((StatusEffect.Resurrection, StatusEffectType.InfiniteDuration), 1);
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            List<(StatusEffect, StatusEffectType)> curStatus = Player.CurStatusEffectList.ToList();
            for (int i = 0; i < curStatus.Count; i++)
            {
                Player.RemoveStatusEffect(curStatus[i]);
            } 
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            foreach (var enemy in EnemyManager.Instance.EnemyList)
                enemy.Critical(-10);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (var enemy in EnemyManager.Instance.EnemyList)
                enemy.Critical(+10);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var enemy in EnemyManager.Instance.EnemyList)
                enemy.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.TurnDuration), 1);
        }
//#endif
    }

    private void OnDestroy()
    {
        if (Instance != this) return; 
        _playerObj = null;
        if (_playerHandle.IsValid())
            Addressables.Release(_playerHandle);

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBattleStart -= StartBattle;
            TurnManager.Instance.OnPlayerTurnStart -= PlayerTurnStart;
            TurnManager.Instance.OnBattleEnd -= EndBattle;
        }
    }
}
