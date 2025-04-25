using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }


    public Dictionary<int, CardData> CardDatas { get; private set; } = new Dictionary<int, CardData>();     // 나중에 CardManager로 이동
    public Vector2Int[] CardRarityID { get; private set; }
    public Vector2Int[] EnhancedCardRarityID { get; private set; }

    List<CardData>[] _randomCardList = new List<CardData>[4];
    //List<CardData> _popRandomCardList = new();


    public Dictionary<int, ItemData> ItemDatas { get; private set; } = new Dictionary<int, ItemData>();     // 나중에 ItemManager로 이동
    public Vector2Int PassiveID { get; private set; }
    public Vector2Int ActiveID { get; private set; }
    public Vector2Int PotionID { get; private set; }
    List<ItemData>[] _randomItemList = new List<ItemData>[3];
    //List<ItemData> _popRandomItemList = new();

    public EventQueue AbilityEventQueue = new();                // 나중에 BattleManager로 이동

    [SerializeField] bool fastMode;

    public StatusEffectSO SESO;
    [SerializeField] CardSO _cardSO;
    [SerializeField] ItemSO _itemSO;

    //public Arrow ArrowCursor;

    public Player player;

    public int PauseInt;

    //public int NowChapterLV = 1;

    bool _isESCPause = false;

    GameObject[] _camera;
    GameObject[] _player;

    void Awake()
    {
        _camera = GameObject.FindGameObjectsWithTag("MainCamera");
        _player = GameObject.FindGameObjectsWithTag("Player");
        if (Instance == null)
        {
            Instance = this;
            //transform.SetParent(null);
            //DontDestroyOnLoad(_camera[0]);
            GameManager.Instance.AddInGameDontDestroy(_camera[0]);
            GameManager.Instance.AddInGameDontDestroy(_player[0]);
            player = _player[0].GetComponentInChildren<Player>();
            GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
            GameManager.Instance.InGame = true;
        }
        else
        {
            Destroy(_camera[1]);
            Destroy(_player[1]);
            Destroy(transform.root.gameObject);
        }

        //StartCoroutine(ReadSpreadSheet.LoadData("https://docs.google.com/spreadsheets/d/1CqNR2Rh_OIVe8n0CG8vC7YVpbNUn_-0rXeBab72gXvs", "A3:D14", 0));

        //if (!CardDataDeserializer.TryGetData(1015, out CardData row))
        //{
        //    Debug.Log("데이터 테이블을 불러오는 과정에서 문제가 발생했습니다.");
        //}
    }

    private void Start()
    {
        //Random.InitState(255);
        CardRarityID = _cardSO.ClassifyCardRarityID;
        EnhancedCardRarityID = _cardSO.ClassifyEnhancedCardRarityID;

        PassiveID = _itemSO.PassiveID; ActiveID = _itemSO.ActiveID; PotionID = _itemSO.PotionID;

        SettingRandomCardList();
        SettingRandomItemList();

        //InGameUIManager.Instance.SetupGameUi(true);
        //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }
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
            case 1:
                probability = Random.Range(1, 1001);
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
            case 2:
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
            case 3:
                probability = Random.Range(1, 1001);
                if (probability > 4)
                {
                    return CardSwapAndPop(2, Random.Range(0, _randomCardList[2].Count));
                }
                else
                {
                    return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
                }
            case 4:
                return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
            case 5:     // 보스 (일단 할 거 없어서 그냥 레전더리 카드 주는 걸로 함.)
                return CardSwapAndPop(3, Random.Range(0, _randomCardList[3].Count));
            default:
                return null;
        }
    }
    public void ReturnRandomCard(CardData[] cardDatas)
    {
        foreach (CardData cardData in cardDatas/*_popRandomCardList*/)           // _popRandomCardList로 했으나, 굳이 이렇게 해야하나? 싶어서 그냥 Map에서 받아오도록 변경함.
        {
            if (cardData == null) return;       // 카드 개수가 부족해서 null 뜰 때가 있음. 그냥 리턴해서 없애도록 함.

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
    public ItemData ItemSwapAndPop(int listIdx, int randomIdx)
    {
        ItemData randomItem = null;
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
    public ItemData RandomItem()     // Potion은 따로 만들 것.
    {
        int probability = Random.Range(1, 3);
        ItemData randomItem;
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
    public void ReturnRandomItem(ItemData[] itemDatas)
    {
        foreach (ItemData itemData in itemDatas/*_popRandomItemList*/)       // _popRandomItemList로 했으나, 카드와 다르게 아이템은 안 먹은 경우에는 다른 방에서 뜨면 안 되고, 2가지 이상의 방에서 아이템이 떴는데, 한 곳에서 먹으면, ReturnRandomItem()을 실행하기 때문에 그냥 현재 방에 있는 Item들을 리턴하는 코드로 변경.
        {
            if (itemData == null) return;
            if (ItemManager.Instance.itemDict.ContainsKey(itemData.ID) && ItemManager.Instance.itemDict[itemData.ID]) continue;
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
            cardData = (CardData)Array.Find(_cardSO.Cards, x => x.ID == id).Clone();
            CardDatas.Add(id, cardData);
            return cardData;
        }
        //return _cardSO.Cards.Find(x => x.ID == ID);
        //return Array.Find(_cardSO.Cards, x => x.ID == ID);
    }
    public ItemData FindItemData(int id)   // ID 값으로 데이터 가져오기
    {
        //ItemData itemData;
        if (ItemDatas.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }
        else
        {
            itemData = Array.Find(_itemSO.Items, x => x.ID == id);
            ItemDatas.Add(id, itemData);
            return itemData;
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

    public void ChangeCoinValue(int coin)
    {
        player.Coin.Value += coin;
    }

    public void Pause(bool pause)
    {
        if (pause && PauseInt == 0)
            ++PauseInt;
        else if (!pause && PauseInt == 1)
            --PauseInt;
        else
        {
            PauseInt = pause ? ++PauseInt : --PauseInt;
            return;
        }
        //if (_selectAbility && _option) return;
        Time.timeScale = pause ? 0 : 1;
        //Physics2D.autoSyncTransforms = pause ? true : false;      // 정지상태에서 카드를 사용하는 경우에는 필요함. 근데, 지금은 따로 필요없음.
    }

    void Update()
    {
        if (fastMode && PauseInt == 0)
        {
            Time.timeScale = 3f;
        }
        else { Time.timeScale = PauseInt != 0 ? 0 : 1; }

        //print(AbilityEventQueue._queue.Count);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isESCPause = !_isESCPause;
            Pause(_isESCPause);
            //TurnManager.Instance.DrawCardTask().Forget();
        }
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
            TurnManager.Instance.EndTurn().Forget();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnManager.Instance.StartTurnTask().Forget();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            player.AddAttackPower(1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            player.AddDefencePower(1);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            InGameUIManager.Instance.ShowStatus();
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
            fastMode = !fastMode;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            InGameUIManager.Instance.LookMap();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (MapManager.Instance.currStage.State == Map.StageState.Enemy || MapManager.Instance.currStage.State == Map.StageState.Boss)
            {
                for (int i = EnemyManager.Instance.EnemyList.Count - 1; i >= 0; --i)
                {
                    EnemyManager.Instance.EnemyList[i].CheckIfDead(9999, 1);
                    EnemyManager.Instance.EnemyList[i].TakeDamageEnemy(9999).Forget();
                    //EnemyManager.Instance.enemies[i].TakeDamageEnemy(9999).Forget();
                }
            }
            else
            {
                MapManager.Instance.ClearStage().Forget();
            }
        }



        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            CardManager.Instance.AddDeck(100, EAddDeck.Main);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CardManager.Instance.AddDeck(100, EAddDeck.Draw);
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
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ShopManager.Instance.ChangeCardShop();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            MapManager.Instance.MovePrevStage().Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TurnManager.Instance.StartBattle();
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
            ItemManager.Instance.GetItem(FindItemData(1));
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            print(FindItemData(100));

        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            player.GetStatusEffect(StatusEffect.ATKUp, 1, 1);
        }
#endif
    }
}
