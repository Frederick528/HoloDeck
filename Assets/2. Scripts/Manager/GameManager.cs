using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] bool fastMode;

    [SerializeField] CardSO cardSO;

    [SerializeField] GameObject arrow;

    public Player player;

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

    private void Start()
    {
        UiManager.instance.SetupGameUi(true);
        //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }

    public void ArrowCursor(bool isOn)
    {
        arrow.SetActive(isOn);
        Cursor.visible = !isOn;
    }

    public void ChangeCoinValue(int coin)
    {
        player.Coin.Value += coin;
    }

    void Update()
    {
        if (fastMode)
        {
            Time.timeScale = 3f;
        }
        else { Time.timeScale = 1; }

#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TurnManager.OnAddCard?.Invoke();
        //}
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnManager.Instance.DrawTask().Forget();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            TurnManager.Instance.EndTurnTask().Forget();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnManager.Instance.StartTurnTask().Forget();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            player.ChangeAttackPower(1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            player.ChangeDefencePower(1);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            int i = 0;
            while (!EnemyManager.Instance.SpawnEnemy(100, i))
            {
                i++;
                if (i > EnemyManager.Instance.enemySpawnPosition.Count - 1)
                    break; 
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            UiManager.instance.LookMap();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (MapManager.Instance.currStage.State == Map.StageState.ENEMY)
            {
                for (int i = EnemyManager.Instance.enemies.Count - 1; i >= 0; --i)
                {
                    EnemyManager.Instance.enemies[i].TakeDamageEnemy(9999);
                    //EnemyManager.Instance.enemies[i].TakeDamageEnemy(9999).Forget();
                }
            }
            else
            {
                MapManager.Instance.ClearStage();
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
            //AddCard(Deck.GetComponentsInChildren<Card>()[2].gameObject);  // 전투덱에서 가져오는 경우
            //AddCard(DrawDeck[2]);   // 드로우덱에서 가져오는 경우
            //AddCard(CardDummy[0]);  // 버린 카드덱에 있는 카드가 드로우덱에도 있을 경우 => 적용 안됨. 주소 문제인 듯
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))       // 카드 생성
        {
            CardManager.Instance.AddCard(cardSO.Cards[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TurnManager.Instance.EndBattle();
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
            PotionManager.Instance.GetPotion(2);
        }
#endif
    }
}
