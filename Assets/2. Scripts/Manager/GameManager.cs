using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<GameObject> InGameDontDestroyObjects = new();

    public bool InGame = false;

    public int AddMaxHP { get; private set; }
    public int AddAttackPower { get; private set; }
    public int AddDefensePower { get; private set; }
    public int AddHealPower { get; private set; }
    public int AddCriticalChance { get; private set; }
    public int AddCriticalDamage { get; private set; }
    public int Resurrection { get; private set; }
    public int AddCoinGained { get; private set; }

    public ReactiveProperty<int> Goods { get; private set; } = new();

    public Upgrade[] Upgrades { get; private set; }

    public int PauseNum;

    public int NowChapterLV;

    public int PlayerInt;

    public GameObject OutGameRootObj = null;
    public (int, int) ScreenWH = (1920, 1080);
    public bool FullScreen = false;

    //public bool IsSceneChange;

    //public int OnUINum;

    bool _isESCPause = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            OutGameRootObj = transform.root.gameObject;
            DontDestroyOnLoad(OutGameRootObj);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
        ResolutionSetting(Camera.main);
    }

    private void Start()
    {
        Goods
            .Where(_ => InGame)
            .Subscribe(goods => InGameUIManager.Instance.ChangeStatus(8, goods)).AddTo(this);

        if (!InGame)
            NowChapterLV = 0;

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            NowChapterLV = 4;
        }

        //NowChapterLV = SceneManager.GetActiveScene().buildIndex;

        Upgrades = new Upgrade[8];
        for (int i = 0; i < Upgrades.Length; i++)
        {
            Upgrades[i] = new Upgrade(i, 0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && OutGameUIManager.Instance != null)
        {
            if (!OutGameUIManager.Instance.OutGameCanvas(OutGameUIManager.CanvasName.Fade).gameObject.activeSelf)
            {
                if (OutGameUIManager.Instance.OpenUIOrder.Count == 0)
                {
                    OutGameUIManager.Instance.SetActiveCanvas(OutGameUIManager.CanvasName.Option, /*!_isESCPause*/true, 0);
                }
                else
                {
                    OutGameUIManager.Instance.CloseUIByOrder();
                }
            }
        }
    }


    public void ResolutionSetting(Camera cam, int width = -1, int height = -1)
    {
        if (width == -1 || height == -1)
        {
            width = ScreenWH.Item1;
            height = ScreenWH.Item2;
        }
        else
        {
            ScreenWH = (width, height);
        }

        float targetAspectRatio = 16.0f / 9.0f;
        // 현재 화면의 비율
        float windowAspectRatio = (float)width / (float)height;

        // 목표 비율보다 화면이 가로로 더 넓은 경우 (Pillarbox)
        if (windowAspectRatio > targetAspectRatio)
        {
            float newWidth = targetAspectRatio / windowAspectRatio;
            cam.rect = new Rect((1f - newWidth) / 2f, 0, newWidth, 1f);
        }
        // 목표 비율보다 화면이 세로로 더 긴 경우 (Letterbox)
        else
        {
            float newHeight = windowAspectRatio / targetAspectRatio;
            cam.rect = new Rect(0, (1f - newHeight) / 2f, 1f, newHeight);
        }
        Screen.SetResolution(width, height, FullScreen);
    }

    public void ESC(bool esc)
    {
        _isESCPause = esc;
        Pause(_isESCPause);
        //OutGameUIManager.Instance.SetActiveCanvas(OutGameUIManager.CanvasName.Option, _isESCPause, 0);
        //TurnManager.Instance.DrawCardTask().Forget();
    }

    public void Pause(bool pause)
    {
        if (pause && PauseNum == 0)
            ++PauseNum;
        else if (!pause && PauseNum == 1)
            --PauseNum;
        else
        {
            PauseNum = pause ? ++PauseNum : --PauseNum;
            return;
        }
        //if (_selectAbility && _option) return;
        Time.timeScale = pause ? 0 : 1;
        //Physics2D.autoSyncTransforms = pause ? true : false;      // 정지상태에서 카드를 사용하는 경우에는 필요함. 근데, 지금은 따로 필요없음.
    }

    //public void OnUI(bool isOn)
    //{
    //    if (isOn)
    //    {
    //        ++OnUINum;
    //    }
    //    else
    //    {
    //        --OnUINum;
    //    }
    //}

    public void AddInGameDontDestroy(GameObject gameObject)
    {
        InGameDontDestroyObjects.Add(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyAllInGameDontDestroyObjects()
    {
        foreach (GameObject obj in InGameDontDestroyObjects)
        {
            Destroy(obj);
        }
        InGameDontDestroyObjects.Clear();

        InGame = false;
    }
    public void AddGoods(int amount)
    {
        Goods.Value += amount;
    }
    public void ChangeUpgrade(int idx, int amount)
    {
        switch (idx)
        {
            case 0:
                AddMaxHP = amount;
                break;
            case 1:
                AddAttackPower = amount;
                break;
            case 2:
                AddDefensePower = amount;
                break;
            case 3:
                AddHealPower = amount;
                break;
            case 4:
                AddCriticalChance = amount;
                break;
            case 5:
                AddCriticalDamage = amount;
                break;
            case 6:
                Resurrection = amount;
                break;
            case 7:
                AddCoinGained = amount;
                break;
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    public async UniTaskVoid ChangeScene(int idx)
    {
        bool lobby = NowChapterLV == 0;
        if (idx == 0 && SceneManager.GetActiveScene().buildIndex == idx) return;
        //IsSceneChange = true;
        if(lobby || idx == 0)
            await OutGameUIManager.Instance.FadeOut(0.55f);
        switch (idx)
        {
            case 0:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = idx;       // 로비
                await SceneManager.LoadSceneAsync(idx);
                //SceneManager.LoadScene(idx);
                break;
            case 1:
            case 2:
            case 3:
                NowChapterLV = idx;
                await SceneManager.LoadSceneAsync(1);
                //SceneManager.LoadScene(1);
                break;
            case 4:
                NowChapterLV = idx;
                await SceneManager.LoadSceneAsync(2);
                //SceneManager.LoadScene(2);
                break;
            default:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = 0;       // 로비
                await SceneManager.LoadSceneAsync(0);
                //SceneManager.LoadScene(0);
                break;
        }
        if (lobby && InGameUIManager.Instance)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var task1 = UniTask.WaitForSeconds(2f);
            //{
            //    await UniTask.WaitForSeconds(2f);
            //    if (!cts.IsCancellationRequested)
            //    {
            //        cts.Cancel();
            //        cts.Dispose();
            //    }
            //});
            var task2 = UniTask.WaitUntil(() => InGameUIManager.Instance.EndLoad, PlayerLoopTiming.Update, cts.Token);
            await UniTask.WhenAny(
                task1, task2
                ).SuppressCancellationThrow();
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
        if (idx == 0)
        {
            ResolutionSetting(Camera.main);
        }
        if (lobby || idx == 0)
            await OutGameUIManager.Instance.FadeIn(0.75f);
        //IsSceneChange = false;
    }

    public async UniTask ChangeScene()
    {
        switch (NowChapterLV)
        {
            case 0:
                DestroyAllInGameDontDestroyObjects();
                await SceneManager.LoadSceneAsync(0);
                break;
            case 1:
            case 2:
            case 3:
                await SceneManager.LoadSceneAsync(1);
                break;
            case 4:
                await SceneManager.LoadSceneAsync(2);
                break;
            default:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = 0;       // 로비
                await SceneManager.LoadSceneAsync(0);
                break;
        }
    }
}
