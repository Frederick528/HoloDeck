using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditorInternal.ReorderableList;

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
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _onLoaded = true;
        PrevStage = null;
        //for (int i = _settingMap.MapTr.childCount - 1; i >= 0; --i)
        //{
        //    Destroy(_settingMap.MapTr.GetChild(i).gameObject);
        //}
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        //ShowAllMap();
    }

    public async UniTaskVoid ResetChapter()
    {
        if (OutGameUIManager.Instance)
            await OutGameUIManager.Instance.FadeOut(0.55f);
        //_onLoaded = true;
        PrevStage = null;
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        if (OutGameUIManager.Instance)
            await OutGameUIManager.Instance.FadeIn(0.75f);
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
            _settingMap.NextChapterBtn.SetActive(true);
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Start)
        {
            _settingMap.NextChapterBtn.SetActive(false);
        }
        currStage.ClearMap();
        canMove = true;
    }

    public async UniTaskVoid ClearStage(Map stage)
    {
        if (currStage.State == Map.StageState.Enemy)
        {
            await TurnManager.Instance.EndBattle();
        }
        else if (currStage.State == Map.StageState.Boss)
        {
            _settingMap.NextChapterBtn.SetActive(true);
            await TurnManager.Instance.EndBattle();
        }
        stage.ClearMap();
        canMove = true;
    }

    public async UniTaskVoid MovePrevStage()
    {
        if (PrevStage == null) return;

        if (!StopMove)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(PrevStage);

        if (currStage.cleared)
            canMove = true;
    }

    public async UniTaskVoid MoveStage(Map stage)
    {
        if (stage == null) return;
        if (!StopMove)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(stage);

        if (currStage.cleared)
            canMove = true;
    }

    public async UniTaskVoid MoveBossStage()
    {
        if (!StopMove)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(_settingMap.Maps[_settingMap.Maps.Count - 1]);

        if (currStage.cleared)
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
