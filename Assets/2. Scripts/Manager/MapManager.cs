using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public Transform rewardCanvas;

    public Map currStage;

    public bool canMove;

    void Awake() { Instance = this; }

    public void ClearStage()
    {
        TurnManager.Instance.EndBattle();
        currStage.ClearMap();
        canMove = true;
    }

    public void SetupStart(List<Vector3Int> direction4, List<Map> maps)
    {
        // 시작 장소 활성화 코드 5줄
        currStage = maps[0];
        currStage.gameObject.SetActive(true);
        //currStage.img.color = Color.white;
        currStage.btn.interactable = true;
        currStage.LookingStage(direction4, maps);
        ClearStage();
    }

    public void RewardStage()
    {
        currStage.RewardBox();
    }
}
