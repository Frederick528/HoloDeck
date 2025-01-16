using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public Transform rewardCanvas;
    public Transform shopCanvas;

    public Map currStage;

    public bool canMove;

    void Awake() { Instance = this; }

    public void ClearStage()
    {
        if (currStage.State == Map.StageState.ENEMY || currStage.State == Map.StageState.BOSS)
        {
            TurnManager.Instance.EndBattle();
        }
        currStage.ClearMap();
        canMove = true;
    }

    public void ClearStage(Map stage)
    {
        if (stage.State == Map.StageState.ENEMY || stage.State == Map.StageState.BOSS)
        {
            TurnManager.Instance.EndBattle();
        }
        stage.ClearMap();
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

    // Enemy와 Boss에서 사용되며, 사용시 방 보상 획득 가능
    public void RewardStage()
    {
        currStage.RewardBox();
    }

    public void Treasure()
    {
        currStage.TreasureBox();
    }
}
