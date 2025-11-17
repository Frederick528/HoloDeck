using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    //public Transform rewardCanvas;

    public int CreateMapCnt;
    public (int, int) MaxDistance = (3,3);

    public float MapScale = 1;

    public Map PrevStage { get; private set; }
    public Map CurrStage { get; private set; }

    public bool canMove;
    public bool StopMove;

    public bool[] IsSaveChapter = new bool[5];

    public Sprite[] MapIcon = new Sprite[6];

    public int? CurShowBoxIdx;
    private Box[] _boxes;
    private Transform[] _boxesTop;
    bool _isBoxOpen;
    bool _isDropBoxOpen;
    //public GameObject LootBox;
    //public GameObject TreasureBox;
    //public GameObject LowTierBox;
    //public GameObject HighTierBox;

    GameObject _redPortal;
    GameObject _greenPortal;

    AsyncOperationHandle<GameObject> _redHandle;
    AsyncOperationHandle<GameObject> _greenHandle;

    SettingMap _settingMap;

    [SerializeField] GameObject _mapPrefab;
    [SerializeField] GameObject _markPrefab;

    //bool _onLoaded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAsync().Forget();
            _settingMap = new(this);
            GameObject boxObj = GameObject.Find("Boxes");
            if (boxObj != null)
            {
                _boxes = new Box[boxObj.transform.childCount];
                _boxesTop = new Transform[_boxes.Length];
                for (int i = 0; i < _boxes.Length; ++i)
                {
                    int idx = i;
                    _boxes[idx] = boxObj.transform.GetChild(i).GetComponent<Box>();
                    _boxes[idx].OpenBox = () =>
                    {
                        switch (idx)
                        {
                            case (int)Map.BoxType.Treasure:
                                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.ItemReward, true);
                                BoxOpen(true)/*.Forget()*/;
                                break;
                            case (int)Map.BoxType.Drop:
                                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Inventory, true, 1);
                                DropBoxOpen(true)/*.Forget()*/;
                                break;
                            default:
                                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.CardReward, true);
                                BoxOpen(true)/*.Forget()*/;
                                break;
                        }
                    };
                    _boxesTop[idx] = FindTransform.ContinueFindChildObjByName(_boxes[idx].transform, "top");
                }
                GameManager.Instance.AddInGameDontDestroy(boxObj);
                //LootBox = boxObj.transform.Find(nameof(LootBox)).gameObject;
                //TreasureBox = boxObj.transform.Find(nameof(TreasureBox)).gameObject;
                //LowTierBox = boxObj.transform.Find(nameof(LowTierBox)).gameObject;
                //HighTierBox = boxObj.transform.Find(nameof(HighTierBox)).gameObject;

            }
            else
            {
                print("Addressable이랑 instantiate 이용해서 오브젝트 만들어야 함.");
            }
        }
    }

    private void Start()
    {
        //if (_onLoaded) return;
        //PrevStage = null;
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        CurrStage.stageContext.ImmediateTransition(CurrStage.stage);
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


    public async UniTask LoadAsync()
    {
        GameManager.Instance.StartLoadAsync(true);
        _redHandle = Addressables.LoadAssetAsync<GameObject>("RedPortal.prefab");
        _greenHandle = Addressables.LoadAssetAsync<GameObject>("GreenPortal.prefab");


        await UniTask.WhenAll(
            _redHandle.ToUniTask(),
            _greenHandle.ToUniTask()
            ); 

        // 성공 여부 확인
        if (_redHandle.Status == AsyncOperationStatus.Succeeded &&
            _greenHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("맵매니저 모든 에셋 로드 성공!");
            _redPortal = Instantiate(_redHandle.Result);
            _redPortal.SetActive(false);
            _greenPortal = Instantiate(_greenHandle.Result);
            _greenPortal.SetActive(false);
            GameManager.Instance.StartLoadAsync(false);
        }
        else
        {
            Debug.LogWarning("하나 이상의 에셋 로드 실패");
        }

        GameManager.Instance.AddInGameDontDestroy(_redPortal);
        GameManager.Instance.AddInGameDontDestroy(_greenPortal);


    }

    public void ShowBox(int idx, bool show)
    {
        if (!show)
        {
            if (idx == (int)Map.BoxType.Treasure)
            {
                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.ItemReward, false);
                CurShowBoxIdx = null;
            }
            else if (idx == (int)Map.BoxType.Drop)
            {
                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Inventory, false);
            }
            else
            {
                InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.CardReward, false);
                CurShowBoxIdx = null;
            }
        }
        else
        {
            _boxesTop[idx].localRotation = Quaternion.identity;
            if (idx != (int)Map.BoxType.Drop)
                CurShowBoxIdx = idx;
        }
        _boxes[idx].gameObject.SetActive(show);
    }
    public void DropBoxOpen(bool open)
    {
        if (_isDropBoxOpen == open)
        {
            return;
        }
        int idx = (int)Map.BoxType.Drop;
        if (!_boxes[idx].gameObject.activeSelf)
            return;
        _isDropBoxOpen = open;
        Quaternion targetAngle = open ? Quaternion.Euler(-60f, 0, 0) : Quaternion.identity;
        float duration = 1f;

        _boxesTop[idx].transform
            .DOLocalRotateQuaternion(targetAngle, duration)
            .SetEase(Ease.Linear);
        //.AsyncWaitForCompletion();

        //Quaternion targetAngle = open ? Quaternion.Euler(-60f, 0, 0) : Quaternion.identity;
        //Quaternion startAngle = _boxesTop[idx].localRotation;

        //float elapsed = 0f;
        //float duration = 1f;
        //while (elapsed < duration)
        //{
        //    await UniTask.Yield();
        //    if (_isDropBoxOpen == open)
        //    {
        //        Quaternion currentRotation = Quaternion.Slerp(startAngle, targetAngle, elapsed / duration);
        //        elapsed += Time.deltaTime;
        //        //float t = Mathf.Clamp01(elapsed / duration);
        //        //_boxesTop[idx].localEulerAngles = new Vector3(currentRotation, 0, 0);
        //        _boxesTop[idx].localRotation = currentRotation;
        //    }
        //    else
        //    {
        //        return;
        //    }

        //}
        ////_boxesTop[idx].localEulerAngles = new Vector3(targetAngle, 0, 0);
        //_boxesTop[idx].localRotation = targetAngle;
    }
    public void BoxOpen(bool open)
    {
        if (_isBoxOpen == open)
        {
            return;
        }
        int idx = CurrStage.rewardBox;
        if (!_boxes[idx].gameObject.activeSelf)
            return;
        _isBoxOpen = open;
        Quaternion targetAngle = open ? Quaternion.Euler(-130f, 0, 0) : Quaternion.identity;
        float duration = 1f;

        _boxesTop[idx].transform
            .DOLocalRotateQuaternion(targetAngle, duration)
            .SetEase(Ease.Linear);
            //.AsyncWaitForCompletion();

        //Quaternion targetAngle = open ? Quaternion.Euler(-130f, 0, 0) : Quaternion.identity;
        //Quaternion startAngle = _boxesTop[idx].localRotation;

        //float elapsed = 0f;
        //float duration = 1f;
        //while (elapsed < duration)
        //{
        //    await UniTask.Yield();
        //    if (_isBoxOpen == open)
        //    {
        //        //float t = Mathf.Clamp01(elapsed / duration);
        //        Quaternion currentRotation = Quaternion.Slerp(startAngle, targetAngle, elapsed / duration);
        //        elapsed += Time.deltaTime;
        //        //_boxesTop[idx].localEulerAngles = new Vector3(currentRotation, 0, 0);
        //        _boxesTop[idx].localRotation = currentRotation;
        //    }
        //    else
        //    {
        //        return;
        //    }

        //}
        //_boxesTop[idx].localRotation = targetAngle;

    }

    public void CloseUIBeforeEnterStage()
    {
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);
        if (CurrStage.State == Map.StageState.Shop)
        {
            ShopManager.Instance.CloseShop();
        }
    }

    public void SaveChapter(int chapterLV)
    {
        _settingMap.SaveChapter(chapterLV);
    }

    public async UniTaskVoid LoadChapter(int chapterLV, bool isNext = true, bool changeScene = false)
    {
        //if (OutGameUIManager.Instance && !previous)
        //{
        //    await OutGameUIManager.Instance.FadeOut(0.55f);
        //}
        if (TurnManager.Instance.InBattle.Value)
            await TurnManager.Instance.EndBattle();
        //PrevStage = null;

        CloseUIBeforeEnterStage();
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);
        //if (CurrStage.State == Map.StageState.Shop)
        //{
        //    ShopManager.Instance.CloseShop();
        //}

        SetMapSize();
        _settingMap.LoadChapter(chapterLV);
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false);          // 방 로드 후, 몇몇 UI 비활성화 (상자)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);               // 방 로드 후, 몇몇 UI 비활성화 (상점 보상)
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);                // 방 로드 후, 몇몇 UI 비활성화 (맵)
        await LoadStage(isNext, changeScene);
        ShopManager.Instance.ChangeCardShop();
        //if (previous)
        //{
        //    await MoveBossStage();
        //}
        //else
        //{
        //    await MoveStage(_settingMap.Maps[0]);
        //}
        //if (OutGameUIManager.Instance)
        //{
        //    print("AA");
        //    await OutGameUIManager.Instance.FadeIn(0.75f);
        //}
    }

    public async UniTaskVoid ResetChapter(bool changeScene = false)
    {
        //if (OutGameUIManager.Instance)
        //    await OutGameUIManager.Instance.FadeOut(0.55f);
        if (TurnManager.Instance.InBattle.Value)
            await TurnManager.Instance.EndBattle();

        CloseUIBeforeEnterStage();
        //if (CurrStage.State == Map.StageState.Shop)
        //{
        //    ShopManager.Instance.CloseShop();
        //}
        //PrevStage = null;
        SetMapSize();
        bool isEndBoss = GameManager.Instance.NowChapterLV == 4;
        _settingMap.Start(isEndBoss);
        LoadStage(true, changeScene).Forget();
        //await _settingMap.EnterChapter(currStage, changeScene, true);
        //ClearStage().Forget();


        //InGameManager.Instance.Player.EnterChapterDoor(null, changeScene).Forget();

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
        if (CurrStage.State == Map.StageState.Enemy)
        {
            await TurnManager.Instance.EndBattle();
        }
        else if (CurrStage.State == Map.StageState.Boss)
        {
            ShowNextDoor(true);
            ShowPreviousDoor(true);
            await TurnManager.Instance.EndBattle();
        }
        else if (CurrStage.State == Map.StageState.Start)
        {
            ShowPreviousDoor(false);
            ShowPreviousDoor(true);     // 껐다가 켜주는 이유는 1스테이지일 경우, true가 리턴되어 false만 되고, 그 외에는 켜져야하기 때문
            ShowNextDoor(false);
        }
        CurrStage.ClearMap();
        canMove = true;
    }

    public async UniTaskVoid ClearStage(Map stage)
    {
        _settingMap.ShowMapBtn.SetActive(true);
        if (CurrStage.State == Map.StageState.Enemy)
        {
            await TurnManager.Instance.EndBattle();
        }
        else if (CurrStage.State == Map.StageState.Boss)
        {
            ShowNextDoor(true);
            ShowPreviousDoor(true);
            await TurnManager.Instance.EndBattle();
        }
        else if (CurrStage.State == Map.StageState.Start)
        {
            ShowPreviousDoor(false);
            ShowPreviousDoor(true);
            ShowNextDoor(false);
        }
        stage.ClearMap();
        canMove = true;
    }

    public void ShowNextDoor(bool isShow)
    {
        _settingMap.NextChapterBtn.SetActive(isShow);
        if (_redPortal == null) return;
        _redPortal.SetActive(isShow);
    }

    public void ShowPreviousDoor(bool isShow)
    {
        if (isShow && GameManager.Instance.NowChapterLV <= 1) return;
        _settingMap.PreviousChapterBtn.SetActive(isShow);
        if (_greenPortal == null) return;
        _greenPortal.SetActive(isShow);
    }

    public async UniTask MovePrevStage()
    {
        if (PrevStage == null) return;

        if (/*!StopMove*/TurnManager.Instance.InBattle.Value)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(PrevStage);

        if (CurrStage.cleared)
            canMove = true;
    }

    public async UniTask MoveStage(Map stage)
    {
        if (stage == null) return;
        if (/*!StopMove*/TurnManager.Instance.InBattle.Value)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(stage);

        if (CurrStage.cleared)
            canMove = true;
    }

    public async UniTask MoveBossStage()
    {
        if (TurnManager.Instance.InBattle.Value)
            await TurnManager.Instance.EndBattle();
        await _settingMap.MoveStage(_settingMap.Maps[CreateMapCnt - 1]);

        if (CurrStage.cleared)
            canMove = true;
    }

    public async UniTask LoadStage(bool isNext, bool changeScene)
    {
        if (isNext)
        {
            print("isNext");
            await _settingMap.LoadStage(_settingMap.Maps[0], changeScene, isNext);
        }
        else
        {
            print("isPrevious");
            await _settingMap.LoadStage(_settingMap.Maps[CreateMapCnt - 1], changeScene, isNext);
        }
        if (CurrStage.cleared)
            canMove = true;
    }

    public void HideLoadStage()
    {
        // 방 로드 후, 몇몇 UI 비활성화
        if (CurShowBoxIdx != null)
        {
            ShowBox(CurShowBoxIdx.Value, false);
            CurShowBoxIdx = null;

        }
        ShowBox((int)Map.BoxType.Drop, false);
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);
    }

    public void HideReward(Map map)
    {
        // 떠나려는 방에 보상이 떴는데, 그 보상을 받지 않고 떠난다면, 잠시 해당 스테이지 보상을 숨김. 
        if (map.rewardBox != -1 && (map.ChangedItem || !map.rewarded))
        {
            //_mapManager.rewardCanvas.GetChild(_mapManager.currStage.rewardBox).gameObject.SetActive(false);
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, map.rewardBox);
            //BoxOpen(false).Forget();
            ShowBox(map.rewardBox, false);
        }
        if (map.LootBox)
        {
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, (int)Map.BoxType.Drop);
            //BoxOpen(false, true).Forget();
            ShowBox((int)Map.BoxType.Drop, false);
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
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, map.rewardBox);
            ShowBox(map.rewardBox, true);
            if (map.rewardBox == 0)
            {
                InGameUIManager.Instance.ShowRewardItem(map.ItemReward);
            }
            else
            {
                InGameUIManager.Instance.ShowRewardCard(map.CardReward);
            }
        }
        if (map.LootBox)
        {
            //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, true, (int)Map.BoxType.Drop);
            ShowBox((int)Map.BoxType.Drop, true);
        }
    }

    public void EnterChappter(Map map)
    {
        // 새로운 층으로 이동하기 때문에 PrevStage = null
        PrevStage = null;
        CurrStage = map;
        ShowReward(map);

        if (!map.cleared)
        {
            _settingMap.ShowMapBtn.SetActive(false);

        }
        else
        {
            _settingMap.ShowMapBtn.SetActive(true);
        }
        ShowPreviousDoor(false);
        ShowNextDoor(false);
    }
    public void EnterStage(Map map)
    {
        PrevStage = CurrStage;
        CurrStage = map;
        ShowReward(map);

        if (!map.cleared)
        {
            _settingMap.ShowMapBtn.SetActive(false);

        }
        else
        {
            _settingMap.ShowMapBtn.SetActive(true);
        }
        ShowPreviousDoor(false);
        ShowNextDoor(false);
    }
    public void SetupStart(List<Vector3Int> direction4, List<Map> maps)
    {
        // 시작 장소 활성화 코드 5줄
        CurrStage = maps[0];
        CurrStage.gameObject.SetActive(true);
        //currStage.img.color = Color.white;
        CurrStage.LightMap(true);
        CurrStage.LookingStage(direction4, maps);
        //Debug.Log(currStage.stageContext.ImmediateTransition(currStage.stage));
        //currStage.stageContext
    }

    // Enemy와 Boss에서 사용되며, 사용시 방 보상 획득 가능
    public void RewardStage()
    {
        if (CurrStage.State == Map.StageState.Enemy)
            CurrStage.DropRewardBox();
        else if (CurrStage.State == Map.StageState.Boss)
            CurrStage.DropBossBox();
    }
    public void GetReward(bool changed = false) // 메인박스 전용
    {
        CurrStage.rewarded = true;
        switch (CurrStage.State)
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

    public void GetLootItem()
    {
        CurrStage.DropLootBox();
    }

    public void Treasure(bool changed)
    {
        CurrStage.DropTreasureBox(changed);
    }

    public void ChangedUseItem(ChargeItemBase itemData, int idx)
    {
        CurrStage.ChangedUseItem(itemData, idx);
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

    private void OnDestroy()
    {
        if (Instance != this) return;
        _redPortal = null;
        _greenPortal = null;
        if (_redHandle.IsValid())
            Addressables.Release(_redHandle);
        if (_greenHandle.IsValid())
            Addressables.Release(_greenHandle);
    }
}
