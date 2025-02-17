using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    public Dictionary<int, CardData> CardDatas { get; private set; } = new Dictionary<int, CardData>();
    public Dictionary<int, ItemData> ItemDatas { get; private set; } = new Dictionary<int, ItemData>();

    public EventQueue AbilityEventQueue = new();

    [SerializeField] bool fastMode;

    [SerializeField] CardSO cardSO;
    [SerializeField] ItemSO _itemSO;

    //public Arrow ArrowCursor;

    public Player player;

    public int PauseInt;

    bool _isESCPause = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Screen.SetResolution(1920, 1080, true);
        //DontDestroyOnLoad(gameObject);

        //StartCoroutine(ReadSpreadSheet.LoadData("https://docs.google.com/spreadsheets/d/1CqNR2Rh_OIVe8n0CG8vC7YVpbNUn_-0rXeBab72gXvs", "A3:D14", 0));
        
        //if (!CardDataDeserializer.TryGetData(1015, out CardData row))
        //{
        //    Debug.Log("데이터 테이블을 불러오는 과정에서 문제가 발생했습니다.");
        //}
    }

    //private void Start()
    //{
    //    ArrowCursor = FindObjectOfType<Arrow>(true);
    //    //UiManager.Instance.SetupGameUi(true);
    //    //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    //}
    public CardData FindCardData(int id)   // Id 값으로 카드데이터 가져오기
    {
        //CardData cardData;
        if (CardDatas.TryGetValue(id, out CardData cardData))
        {
            return cardData;
        }
        else
        {
            cardData = (CardData)Array.Find(cardSO.Cards, x => x.Id == id).Clone();
            CardDatas.Add(id, cardData);
            return cardData;
        }
        //return cardSO.Cards.Find(x => x.Id == Id);
        //return Array.Find(cardSO.Cards, x => x.Id == Id);
    }
    public ItemData FindItemData(int id)   // Id 값으로 카드데이터 가져오기
    {
        //ItemData itemData;
        if (ItemDatas.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }
        else
        {
            itemData = Array.Find(_itemSO.Items, x => x.Id == id);
            ItemDatas.Add(id, itemData);
            return itemData;
        }
        //return cardSO.Cards.Find(x => x.Id == Id);
        //return Array.Find(cardSO.Cards, x => x.Id == Id);
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
    //        UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.Map, false);
    //        UiManager.Instance.SetCanvasRaycast(UiManager.CanvasName.InGame, false);
    //        UiManager.Instance.SetCanvasRaycast(UiManager.CanvasName.Battle, false);
    //    }
    //    else
    //    {
    //        UiManager.Instance.SetCanvasRaycast(UiManager.CanvasName.InGame, true);
    //        UiManager.Instance.SetCanvasRaycast(UiManager.CanvasName.Battle, true);
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
            int i = 0;
            while (!EnemyManager.Instance.SpawnEnemy(100, i))
            {
                i++;
                if (i > EnemyManager.Instance.EnemySpawnPosition.Count - 1)
                    break; 
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            fastMode = !fastMode;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            UiManager.Instance.LookMap();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (MapManager.Instance.currStage.State == Map.StageState.Enemy)
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
        //    AddDeck(cardSO.Cards[1], EAddDeck.Draw);
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
            CardManager.Instance.AddCard(FindCardData(100));
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ShopManager.Instance.ChangeCardShop();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TurnManager.Instance.EndBattle().Forget();
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
            ItemManager.Instance.ItemAbility(5);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PotionManager.Instance.GetPotion(1);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            print(FindItemData(100));

        }
#endif
    }
}
