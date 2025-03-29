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
    public bool visited = false;
    public bool cleared = false;
    public bool rewarded = false;

    public bool ChangedItem = false;

    public int RandEnemyPattern = -1;

    public int rewardBox { get; private set; } = -1;
    public CardData[] CardReward { get; private set; } = new CardData[4];
    public ItemData[] ItemReward { get; private set; } = new ItemData[4];

    //public IObjectPool<GameObject> MapPool { get; set; }
    public Button btn;
    public TMP_Text TMP_Text;       // 나중에 맵 이미지 생기면, 삭제할 예정.
    public Image img;
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
        }
        TMP_Text.text = MapManager.Instance.MapString[idx];
        btn.interactable = false;
        stage = Stages[idx];
        switch (idx)
        {
            case 0:
                State = StageState.Start;
                break;
            case 1:
                State = StageState.Treasure;
                break;
            case 2:
                State = StageState.Shop;
                break;
            case 3:
                State = StageState.Event;
                break;
            case 4:
                State = StageState.Enemy;
                break;
            case 5:
                State = StageState.Boss;
                break;
        }
    }
    public void ResetMap(int idx)
    {
        if (ItemReward[0] != null)
        {
            print("S");
            if (!rewarded)
            {
                print("AAS");
                InGameManager.Instance.ReturnRandomItem(ItemReward);
            }
            ItemReward = new ItemData[4];
        }
        visited = false;
        cleared = false;
        rewarded = false;
        ChangedItem = false;
        RandEnemyPattern = -1;
        rewardBox = -1;
        img.color = Color.white;
        aroundStage.Clear();
    }
    public void LookingStage(List<Vector3Int> direction4, List<Map> maps)
    {
        foreach (Vector3Int direction in direction4)
        {
            Map connectMap = maps.Find(x => x.array_Position == array_Position + direction);
            if (connectMap != null)
            {
                aroundStage.Add(connectMap);
                connectMap.gameObject.SetActive(true);
                //connectMap.img.color = Color.white;
                //connectMap.btn.interactable = true;
            }
        }
    }
    public void ClearMap()
    {
        img.color = Color.green;
        cleared = true;
        foreach (Map map in aroundStage)
        {
            //map.img.color = Color.white;
            map.btn.interactable = true;
        }
    }

    public void RewardBox()
    {
        if (rewardBox == -1)
        {
            int probability = Random.Range(1, 101);
            if (probability > 30 + 0)       // 현재 스테이지 레벨에 따른 가중치 필요
            {
                rewardBox = 1;
            }
            else if (probability > 10 + 0)
            {
                rewardBox = 2;
            }
            else if (probability > 1 + 0)
            {
                rewardBox = 3;
            }
            else if (probability > 0)
            {
                rewardBox = 4;
            }
            //rewardBox = Random.Range(1, 5);
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, true, rewardBox);
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
            CardReward[0] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 나중에 중복은 제외하는 코드로 변경해야 함.
            CardReward[1] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            CardReward[2] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;
            CardReward[3] = InGameManager.Instance.RandomCard(rewardBox);/*Random.Range(rangeMin, rangeMax)*/;     // 유물 효과로 카드 선택지 +1
            InGameManager.Instance.ReturnRandomCard(CardReward);
            UIManager.Instance.ShowRewardCard(CardReward);

        }

        else if (rewarded)
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, false, rewardBox);
    }

    public void TreasureBox(bool changed)
    {
        if (rewardBox == -1)
        {
            int probability = Random.Range(1, 101);     // 아이템도 등급이 생길 경우, 확률 개념 도입해야 함.
            rewardBox = 0;
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, true, rewardBox);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            ItemReward[0] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;     // 나중에 중복은 제외하는 코드로 변경해야 함.
            ItemReward[1] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;
            ItemReward[2] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;
            ItemReward[3] = InGameManager.Instance.RandomItem()/*Random.Range(0, 6)*/;     // 유물 효과로 아이템 선택지 +1
            //InGameManager.Instance.ReturnRandomItem();            // 카드와 다르게, 아이템은 먹을 경우, 더 이상 뜨지 않도록 바꿔야 함.
            //ItemManager.Instance.SettingItem(ItemReward);
            UIManager.Instance.ShowRewardItem(ItemReward);

        }
        else if (changed)
        {
            ChangedItem = true;
            UIManager.Instance.ShowRewardItem(ItemReward);      // 앞에서 ChangedUseItem 진행돼야 함.
        }
        else if (rewarded)
        {
            ChangedItem = false;
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, false, rewardBox);
        }
    }

    public void ChangedUseItem(ItemData itemData, int idx)
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