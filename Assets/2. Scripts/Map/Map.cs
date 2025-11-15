using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    //public List<GameObject> minimapMapWall;
    //public GameObject floorMap;
    //public bool visitedMap = false;

    //public GameObject currStage;

    public class ChapterMapInfo
    {
        public bool HasInfo = false;

        bool _isActive = false;
        bool _isInteractable = false;

        bool _visited = false;
        bool _cleared = false;
        bool _rewarded = false;
        bool _notUsed = false;

        bool _changedItem = false;

        int _randomPattern = -1;

        int _rewardBox = -1;
        bool _lootBox = false;

        CardData[] _cardReward = new CardData[4];
        ItemBase[] _itemReward = new ItemBase[4];

        Vector3Int _array_Position;
        List<Map> _aroundStage = new();

        StageState _state;

        public void SaveInfo(Map map)
        {
            _visited = map.visited;
            _cleared = map.cleared;
            _rewarded = map.rewarded;
            _notUsed = map.NotUsed;

            _changedItem = map.ChangedItem;

            _randomPattern = map.RandomPattern;

            _rewardBox = map.rewardBox;
            _lootBox = map.LootBox;

            _cardReward = map.CardReward;
            _itemReward = map.ItemReward;

            _array_Position = map.array_Position;
            _aroundStage = map.aroundStage;

            _state = map.State;

            _isActive = map.gameObject.activeSelf;
            _isInteractable = map.btn.interactable;
        }

        public void LoadInfo(Map map)
        {
            if (!HasInfo)
            {
                map.NotUsed = true;
                map.gameObject.SetActive(false);
                return;
            }

            map.visited = _visited;
            map.cleared = _cleared;
            map.rewarded = _rewarded;
            map.NotUsed = _notUsed;

            map.ChangedItem = _changedItem;

            map.RandomPattern = _randomPattern;

            map.rewardBox = _rewardBox;
            map.LootBox = _lootBox;

            map.CardReward = _cardReward;
            map.ItemReward = _itemReward;

            map.array_Position = _array_Position;
            map.aroundStage = _aroundStage;

            map.State = _state;

            map.SettingMap((int)_state);

            map.gameObject.SetActive(_isActive);
            map.ClearMark.SetActive(_cleared);
            map.LightMap(_isInteractable);
        }
    }

    public ChapterMapInfo[] ChapterMapInfos;        // 챕터 개수만큼
    public bool visited = false;
    public bool cleared = false;
    public bool rewarded = false;
    public bool NotUsed = false;

    public bool ChangedItem = false;

    public int RandomPattern = -1;

    public int rewardBox { get; private set; } = -1;
    public bool LootBox { get; private set; } = false;
    public enum BoxType
    {
        Treasure, Common, Rare, Epic, Legendary, Boss, Drop
    }

    public CardData[] CardReward { get; private set; } = new CardData[4];
    public ItemBase[] ItemReward { get; private set; } = new ItemBase[4];

    //public IObjectPool<GameObject> MapPool { get; set; }
    public Button btn;
    //public TMP_Text TMP_Text;       // 나중에 맵 이미지 생기면, 삭제할 예정.
    public Image Icon;
    public GameObject ClearMark;
    public Vector3Int array_Position;
    public List<Map> aroundStage = new();

    public IStage stage;
    public IStage[] Stages = new IStage[6];
    public enum StageState
    {
        Start,
        Treasure,
        Shop,
        Event,
        Enemy,
        Boss
    }

    public StageState State;

    public StageContext stageContext;

    readonly Color _start = new(0.049f, 0.487f, 0.057f);
    readonly Color _treasure = new(1.0f, 0.901f, 0.051f);
    readonly Color _shop = new(0.235f, 0.314f, 0.688f);
    readonly Color _event = new(0.688f, 0.431f, 0.784f);
    readonly Color _enemy = new(0.884f, 0.207f, 0.207f);
    readonly Color _boss = new(0.501f, 0.048f, 0.068f);

    public void SettingMap(int idx)
    {
        if (Stages[0] == null)
        {
            Stages[0] = GetComponent<StartStage>();
            Stages[1] = GetComponent<TreasureStage>();
            Stages[2] = GetComponent<ShopStage>();
            Stages[3] = GetComponent<EventStage>();
            Stages[4] = GetComponent<EnemyStage>();
            Stages[5] = GetComponent<BossStage>();

            ChapterMapInfos = new ChapterMapInfo[MapManager.Instance.IsSaveChapter.Length];
            for (int i = 0; i < ChapterMapInfos.Length; ++i)
            {
                ChapterMapInfos[i] = new ChapterMapInfo();
            }
        }

        ChapterMapInfos[GameManager.Instance.NowChapterLV].HasInfo = true;
        Icon.sprite = MapManager.Instance.MapIcon[idx];
        //TMP_Text.text = MapManager.Instance.MapString[idx];
        stage = Stages[idx];
        switch (idx)
        {
            case 0:
                State = StageState.Start;
                Icon.color = _start;
                break;
            case 1:
                State = StageState.Treasure;
                Icon.color = _treasure;
                break;
            case 2:
                State = StageState.Shop;
                Icon.color = _shop;
                break;
            case 3:
                State = StageState.Event;
                Icon.color = _event;
                break;
            case 4:
                State = StageState.Enemy;
                Icon.color = _enemy;
                break;
            case 5:
                State = StageState.Boss;
                Icon.color = _boss;
                break;
        }
        LightMap(false);
    }
    public void ResetMap()
    {
        //if (ItemReward[0] != null)
        //{
        //    print("S");
        //    if (!rewarded)
        //    {
        //        print("AAS");
        //        InGameManager.Instance.ReturnRandomItem(ItemReward);
        //    }
        //    ItemReward = new ItemBase[4];
        //}
        ItemReward = new ItemBase[4];
        visited = false;
        cleared = false;
        rewarded = false;
        NotUsed = false;
        ChangedItem = false;
        RandomPattern = -1;
        rewardBox = -1;
        LootBox = false;
        ClearMark.SetActive(cleared);
        aroundStage.Clear();
    }
    public void LookingStage(List<Vector3Int> direction4, List<Map> maps)
    {
        foreach (Vector3Int direction in direction4)
        {
            Map connectMap = maps.Find(x => x.array_Position == array_Position + direction);
            if (connectMap != null)
            {
                if (connectMap.NotUsed)
                {
                    continue;
                }
                aroundStage.Add(connectMap);
                connectMap.gameObject.SetActive(true);
                //connectMap.img.color = Color.white;
                //connectMap.btn.interactable = true;
            }
        }
    }
    public void ClearMap()
    {
        cleared = true;
        ClearMark.SetActive(/*cleared*/true);
        foreach (Map map in aroundStage)
        {
            if (map.NotUsed)
                { continue; }
            //map.img.color = Color.white;
            map.LightMap(true);
        }
    }

    public void LightMap(bool on)
    {
        btn.interactable = on;
        Color color = Icon.color;
        if (on)
        {
            color.a = 1;
        }
        else
        {

            color.a = 0.35f;
        }
        Icon.color = color;
    }

    public void DropBossBox()
    {
        if (rewardBox == -1)
        {
            //int probability = Random.Range(1, 101);
            rewardBox = (int)BoxType.Boss;

            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, true);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.

            CardReward = InGameManager.Instance.RandomCards(rewardBox, CardReward.Length);
            //CardReward[0] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 나중에 중복은 제외하는 코드로 변경해야 함.
            //CardReward[1] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            //CardReward[2] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            //CardReward[3] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 유물 효과로 카드 선택지 +1
            //InGameManager.Instance.ReturnRandomCard(CardReward);
            InGameUIManager.Instance.ShowRewardCard(CardReward);

        }

        else if (rewarded)
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, false);
    }

    public void DropRewardBox()
    {
        if (rewardBox == -1)
        {
            int probability = Random.Range(1, 101);
            if (probability > 30 + 0)       // 현재 스테이지 레벨에 따른 가중치 필요
            {
                rewardBox = (int)BoxType.Common;
            }
            else if (probability > 5 + 0)
            {
                rewardBox = (int)BoxType.Rare;
            }
            else if (probability > 1 + 0)
            {
                rewardBox = (int)BoxType.Epic;
            }
            else if (probability > 0)
            {
                rewardBox = (int)BoxType.Legendary;
            }
            //rewardBox = Random.Range(1, 5);
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, true);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            //int rangeMin = -1;
            //int rangeMax = -1;
            //switch (rewardBox)
            //{
            //    case 1:
            //        rangeMin = InGameManager.Instance.CardRarityID[0].x;
            //        rangeMax = InGameManager.Instance.CardRarityID[0].y + 1;
            //        break;
            //    case 2:
            //        rangeMin = InGameManager.Instance.CardRarityID[1].x;
            //        rangeMax = InGameManager.Instance.CardRarityID[1].y + 1;
            //        break;
            //    case 3:
            //        rangeMin = InGameManager.Instance.CardRarityID[2].x;
            //        rangeMax = InGameManager.Instance.CardRarityID[2].y + 1;
            //        break;
            //    case 4:
            //        rangeMin = InGameManager.Instance.CardRarityID[3].x;
            //        rangeMax = InGameManager.Instance.CardRarityID[3].y + 1;
            //        break;
            //}
            //if (rangeMin == rangeMax)
            //    return;
            CardReward = InGameManager.Instance.RandomCards(rewardBox, CardReward.Length);
            //CardReward[0] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 나중에 중복은 제외하는 코드로 변경해야 함.
            //CardReward[1] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            //CardReward[2] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            //CardReward[3] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 유물 효과로 카드 선택지 +1
            //InGameManager.Instance.ReturnRandomCard(CardReward);
            InGameUIManager.Instance.ShowRewardCard(CardReward);

        }

        else if (rewarded)
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, false);
    }

    public void DropLootBox()       // rewardBox 변수를 건드리지는 않음.
    {
        //if (대충 드랍템이 1개라도 있으면)
        if (!LootBox && true)
        {
            LootBox = true;
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, (int)Map.BoxType.Drop);
            MapManager.Instance.ShowBox((int)BoxType.Drop, true);
        }
        //else if (true)
        //{

        //}
        // 드랍템이 하나도 없으면
        else if (LootBox && false)
        {
            LootBox = false;
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, (int)Map.BoxType.Drop);

            MapManager.Instance.ShowBox((int)BoxType.Drop, false);
        }
    }

    public void DropTreasureBox(bool changed)
    {
        if (rewardBox == -1)
        {
            int probability = Random.Range(1, 101);     // 아이템도 등급이 생길 경우, 확률 개념 도입해야 함.
            rewardBox = (int)BoxType.Treasure;
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, true);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            ItemReward[0] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;     // 나중에 중복은 제외하는 코드로 변경해야 함.
            ItemReward[1] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;
            ItemReward[2] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;
            if (false)              // 유물 체크해야 함.
                ItemReward[3] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;     // 유물 효과로 아이템 선택지 +1
            //InGameManager.Instance.ReturnRandomItem();            // 카드와 다르게, 아이템은 먹을 경우, 더 이상 뜨지 않도록 바꿔야 함.
            //ItemManager.Instance.SettingItem(ItemReward);
            InGameUIManager.Instance.ShowRewardItem(ItemReward);

        }
        else if (changed)
        {
            ChangedItem = true;
            InGameUIManager.Instance.ShowRewardItem(ItemReward);      // 앞에서 ChangedUseItem 진행돼야 함.
        }
        else if (rewarded)
        {
            ChangedItem = false;
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, rewardBox);
            MapManager.Instance.ShowBox(rewardBox, false);
        }
    }

    public void ChangedUseItem(ChargeItemBase itemData, int idx)
    {
        //ActiveItemCharge.Item1 = itemData;
        for (int i = 0; i < ItemReward.Length; i++)
        {
            if (i == idx)
            {
                ItemReward[i] = itemData;
            }
            else
            {
                ItemReward[i] = null;
            }
        }
    }

    //public void EnterStage()
    //{

    //}

    //public void VisitiedMap(bool boolean, bool currBool)
    //{
    //    transform.gameObject.SetActive(boolean);
    //    if (currBool)
    //    {
    //        visited = true;
    //    }
    //}

    //public void VisitiedCurrMap()
    //{
    //    GetComponent<Image>().color = Color.white;
    //    visited = true;
    //}

    //public void MapRelease()
    //{
    //    MapPool.Release(this.gameObject);
    //}
}