using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    //public Transform rewardCanvas;

    public int CreateMapCnt;
    public int MaxDistance;

    public Map currStage;

    public bool canMove;

    SettingMap _settingMap = new();

    [SerializeField] GameObject[] _mapPrefab;
    [SerializeField] GameObject _markPrefab;

    void Awake() { Instance = this; }

    private void Start()
    {
        _settingMap.Start();
        ShowAllMap();
    }

    public async UniTaskVoid ClearStage()
    {
        if (currStage.State == Map.StageState.Enemy || currStage.State == Map.StageState.Boss)
        {
            await TurnManager.Instance.EndBattle();
        }
        currStage.ClearMap();
        canMove = true;
    }

    public async UniTaskVoid ClearStage(Map stage)
    {
        if (stage.State == Map.StageState.Enemy || stage.State == Map.StageState.Boss)
        {
            await TurnManager.Instance.EndBattle();
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
        ClearStage().Forget();
    }

    // Enemy와 Boss에서 사용되며, 사용시 방 보상 획득 가능
    public void RewardStage()
    {
        currStage.RewardBox();
    }
    public void GetReward(bool changed = false)
    {
        currStage.rewarded = true;
        switch (currStage.State)
        {
            case Map.StageState.Enemy:
            case Map.StageState.Boss:
                RewardStage();
                break;
            case Map.StageState.Treasure:
                Treasure(changed);
                break;
        }
    }

    public void Treasure(bool changed)
    {
        currStage.TreasureBox(changed);
    }

    public void ChangedUseItem(ItemData itemData, int curCharge)
    {
        currStage.ChangedUseItem(itemData, curCharge);
    }

    public void ShowAllMap()
    {
        foreach (Map map in _settingMap.Maps)
        {
            if (!map.gameObject.activeSelf)
                map.gameObject.SetActive(true);
        }
    }

    public GameObject MapInstantiate(int idx, Transform parent)
    {
        return Instantiate(_mapPrefab[idx], parent);
    }
    public GameObject MarkInstantiate(Transform parent)
    {
        return Instantiate(_markPrefab, parent);
    }
}
