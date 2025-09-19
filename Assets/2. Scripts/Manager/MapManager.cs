using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    //public Transform rewardCanvas;

    public int CreateMapCnt;
    public (int, int) MaxDistance = (3,3);

    public float MapScale = 1;

    public Map PrevStage;
    public Map currStage;

    public bool canMove;
    public bool StopMove;

    public bool[] IsSaveChapter = new bool[5];

    public string[] MapString = new string[6] { "Start", "Treasure", "Shop", "Event", "Enemy", "Boss"};

    SettingMap _settingMap;

    [SerializeField] GameObject _mapPrefab;
    [SerializeField] GameObject _markPrefab;

    bool _onLoaded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _settingMap = new(this);
        }
    }

    private void Start()
    {
        if (_onLoaded) return;
        PrevStage = null;
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        //ShowAllMap();
    }

    void SetMapSize()
    {
        switch (GameManager.Instance.NowChapterLV)
        {
            case 1:
                CreateMapCnt = 10;
                MaxDistance = (3, 3);
                MapScale = 1;
                break;
            case 2:
                CreateMapCnt = 15;
                MaxDistance = (4, 3);
                MapScale = 1;
                //CreateMapCnt = 30;
                //MaxDistance = (4, 3);
                //MapScale = 0.95f;
                break;
            case 3:
                CreateMapCnt = 20;
                MaxDistance = (5, 3);
                MapScale = 1;
                break;
            case 4:
                CreateMapCnt = 5;
                MaxDistance = (2, 0);
                MapScale = 1.5f;
                break;
        }
        CreateMapCnt = (int)Mathf.Clamp(CreateMapCnt, 1, (MaxDistance.Item1 * 2 + 1) * (MaxDistance.Item2 * 2 + 1));
    }

    //void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //void OnDisable()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    _onLoaded = true;
    //    if (IsSaveChapter[GameManager.Instance.NowChapterLV])
    //        return;
    //    PrevStage = null;
    //    //for (int i = _settingMap.MapTr.childCount - 1; i >= 0; --i)
    //    //{
    //    //    Destroy(_settingMap.MapTr.GetChild(i).gameObject);
    //    //}
    //    SetMapSize();
    //    bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
    //    _settingMap.Start(isEndBoss);
    //    //ShowAllMap();
    //}

    public void SaveChapter(int chapterLV)
    {
        _settingMap.SaveChapter(chapterLV);
    }

    public async UniTaskVoid LoadChapter(int chapterLV, bool previous = false, bool changeScene = false)
    {
        //if (OutGameUIManager.Instance && !previous)
        //{
        //    await OutGameUIManager.Instance.FadeOut(0.55f);
        //}
        if (TurnManager.Instance.InBattle)
            await TurnManager.Instance.EndBattle();
        //PrevStage = null;
        
        
        
        SetMapSize();
        _settingMap.LoadChapter(chapterLV);
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false);          // 방 로드 후, 몇몇 UI 비활성화 (상자)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);               // 방 로드 후, 몇몇 UI 비활성화 (상점 보상)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);                // 방 로드 후, 몇몇 UI 비활성화 (맵)
        ShopManager.Instance.ChangeCardShop();
        //if (previous)
        //{
        //    await MoveBossStage();
        //}
        //else
        //{
        //    await MoveStage(_settingMap.Maps[0]);
        //}
        LoadStage(previous, changeScene).Forget();
        //if (OutGameUIManager.Instance)
        //{
        //    print("AA");
        //    await OutGameUIManager.Instance.FadeIn(0.75f);
        //}
    }

    public async UniTask ResetChapter(bool changeScene = false)
    {
        //if (OutGameUIManager.Instance)
        //    await OutGameUIManager.Instance.FadeOut(0.55f);
        if (TurnManager.Instance.InBattle)
            await TurnManager.Instance.EndBattle();

        PrevStage = null;
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        
        InGameManager.Instance.Player.EnterChapterDoor(null, changeScene).Forget();
        
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false);          // 방 생성 후, 몇몇 UI 비활성화 (상자)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);               // 방 생성 후, 몇몇 UI 비활성화 (상점 보상)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);                // 방 생성 후, 몇몇 UI 비활성화 (맵)
        //await LoadStage(false);
        //_onLoaded = true;
        //if (OutGameUIManager.Instance)
        //    await OutGameUIManager.Instance.FadeIn(0.75f);
        //ShowAllMap();
    }

    public async UniTaskVoid ClearStage()
    {
        _settingMap.ShowMapBtn.SetActive(true);
        if (currStage.State == Map.StageState.Enemy)
        {
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Boss)
        {
            ShowNextDoor(true);
            ShowPreviousDoor(true);
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Start)
        {
            ShowPreviousDoor(true);
            ShowNextDoor(false);
        }
        currStage.ClearMap();
        canMove = true;
    }

    public async UniTaskVoid ClearStage(Map stage)
    {
        _settingMap.ShowMapBtn.SetActive(true);
        if (currStage.State == Map.StageState.Enemy)
        {
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Boss)
        {
            ShowNextDoor(true);
            ShowPreviousDoor(true);
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Start)
        {
            ShowPreviousDoor(true);
            ShowNextDoor(false);
        }
        stage.ClearMap();
        canMove = true;
    }

    public void ShowNextDoor(bool isShow)
    {
        _settingMap.NextChapterBtn.SetActive(isShow);
    }

    public void ShowPreviousDoor(bool isShow)
    {
        _settingMap.PreviousChapterBtn.SetActive(isShow);
    }

    public async UniTask MovePrevStage()
    {
        if (PrevStage == null) return;

        if (/*!StopMove*/TurnManager.Instance.InBattle)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(PrevStage);

        if (currStage.cleared)
            canMove = true;
    }

    public async UniTask MoveStage(Map stage)
    {
        if (stage == null) return;
        if (/*!StopMove*/TurnManager.Instance.InBattle)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(stage);

        if (currStage.cleared)
            canMove = true;
    }

    public async UniTask MoveBossStage()
    {
        if (TurnManager.Instance.InBattle)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(_settingMap.Maps[CreateMapCnt - 1]);

        if (currStage.cleared)
            canMove = true;
    }

    public async UniTask LoadStage(bool isPrevious, bool changeScene)
    {
        if (isPrevious)
        {
            print("isPrevious");
            await _settingMap.LoadStage(_settingMap.Maps[CreateMapCnt - 1], changeScene);
        }
        else
        {
            print(isPrevious);
            await _settingMap.LoadStage(_settingMap.Maps[0], changeScene);
        }
        if (currStage.cleared)
            canMove = true;
    }

    public void HideReward(Map map)
    {
        // 떠나려는 방에 보상이 떴는데, 그 보상을 받지 않고 떠난다면, 잠시 해당 스테이지 보상을 숨김. 
        if (map.rewardBox != -1 && (map.ChangedItem || !map.rewarded))
        {
            //_mapManager.rewardCanvas.GetChild(_mapManager.currStage.rewardBox).gameObject.SetActive(false);
            InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, map.rewardBox);
        }
        if (map.State == Map.StageState.Shop)
        {
            InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);
        }
    }
    public void ShowReward(Map map)
    {
        // 들어간 방에 보상이 떴었는데, 예전에 보상을 받지 않았다면, 그 보상을 다시 시각화함.
        if (map.rewardBox != -1 && (map.ChangedItem || !map.rewarded))
        {
            InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, map.rewardBox);
            if (map.rewardBox != 0)       // 보물은 한 스테이지에 한 개이기 때문에 UI를 변경할 필요 없음.
            {
                InGameUIManager.Instance.ShowRewardCard(map.CardReward);
            }
        }
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
        if (currStage.State == Map.StageState.Enemy)
            currStage.RewardBox();
        else if (currStage.State == Map.StageState.Boss)
            currStage.BossBox();
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

    public void ChangedUseItem(ItemData itemData, int idx)
    {
        currStage.ChangedUseItem(itemData, idx);
    }

    public void ShowAllMap()
    {
        foreach (Map map in _settingMap.Maps)
        {
            if (map.NotUsed)
                continue;
            if (!map.gameObject.activeSelf)
                map.gameObject.SetActive(true);
        }
    }

    public GameObject MapInstantiate(Transform parent)
    {
        return Instantiate(_mapPrefab, parent);
    }
    //public GameObject MapInstantiate(int idx, Transform parent)
    //{
    //    return Instantiate(_mapPrefab[idx], parent);
    //}
    public GameObject MarkInstantiate(Transform parent)
    {
        return Instantiate(_markPrefab, parent);
    }
}
