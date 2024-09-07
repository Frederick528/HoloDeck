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

    //public IObjectPool<GameObject> MapPool { get; set; }
    public Button btn;
    public Image img;
    public Vector3Int array_Position;
    public List<Map> aroundStage = new();

    public IStage
        stage;

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
            rewardBox = Random.Range(0, 5);
            MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(true);
            // 캐릭터별로 보상이 바뀌는 코드 넣어야 함.
            reward[0] = Random.Range(100, 105);
            reward[1] = Random.Range(100, 105);
            reward[2] = Random.Range(100, 105);
            reward[3] = Random.Range(100, 105);     // 유물 효과로 카드 선택지 +1
            CardManager.Instance.ShowRewardCard(reward);

        }

        else if (rewardBox != -1 && rewarded)
            MapManager.Instance.rewardCanvas.GetChild(rewardBox).gameObject.SetActive(false);
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