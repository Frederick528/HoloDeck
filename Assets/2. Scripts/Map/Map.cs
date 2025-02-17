using System.Collections;
using System.Collections.Generic;
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

    public int rewardBox { get; private set; } = -1;
    public int[] reward { get; private set; } = new int[4];
    public int[] item { get; private set; } = new int[4];

    //public IObjectPool<GameObject> MapPool { get; set; }
    public Button btn;
    public Image img;
    public Vector3Int array_Position;
    public List<Map> aroundStage = new();

    public IStage stage;
    public enum StageState
    {
        Start,
        Boss,
        Treasure,
        Shop,
        Enemy,
        Event
    }

    public StageState State;

    public StageContext stageContext;


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
            rewardBox = Random.Range(1, 5);
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.RewardBox, true, rewardBox);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            reward[0] = Random.Range(100, 106);     // 나중에 중복은 제외하는 코드로 변경해야 함.
            reward[1] = Random.Range(100, 106);
            reward[2] = Random.Range(100, 106);
            reward[3] = Random.Range(100, 106);     // 유물 효과로 카드 선택지 +1
            UiManager.Instance.ShowRewardCard(reward);

        }

        else if (rewardBox != -1 && rewarded)
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.RewardBox, false, rewardBox);
    }

    public void TreasureBox()
    {
        if (rewardBox == -1)
        {
            rewardBox = 0;
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.RewardBox, true, rewardBox);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            item[0] = Random.Range(0, 6);     // 나중에 중복은 제외하는 코드로 변경해야 함.
            item[1] = Random.Range(0, 6);
            item[2] = Random.Range(0, 6);
            item[3] = Random.Range(0, 6);     // 유물 효과로 아이템 선택지 +1
            ItemManager.Instance.SettingItem(item);

        }

        else if (rewardBox != -1 && rewarded)
            //MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
            UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.RewardBox, false, rewardBox);
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