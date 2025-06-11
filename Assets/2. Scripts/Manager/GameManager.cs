using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
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

    public int PlayedInt;

    public bool IsSceneChange;

    bool _isESCPause = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        Goods.Subscribe(goods =>
        {
            if (InGame)
                InGameUIManager.Instance.ChangeStatus(7, goods);
        });
        NowChapterLV = SceneManager.GetActiveScene().buildIndex;

        Upgrades = new Upgrade[8];
        for (int i = 0; i < Upgrades.Length; ++i)
        {
            Upgrades[i] = new Upgrade(i, 0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!IsSceneChange)
                OutGameUIManager.Instance.SetActiveCanvas(OutGameUIManager.CanvasName.Option, !_isESCPause, 0);
        }
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
        if (SceneManager.GetActiveScene().buildIndex == idx) return;
        IsSceneChange = true;
        await OutGameUIManager.Instance.FadeOut(0.55f);
        switch (idx)
        {
            case 0:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = idx;       // 로비
                SceneManager.LoadScene(idx);
                break;
            case 1:
            case 2:
            case 3:
                NowChapterLV = idx;
                SceneManager.LoadScene(1);
                break;
            case 4:
                NowChapterLV = idx;
                SceneManager.LoadScene(2);
                break;
            default:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = 0;       // 로비
                SceneManager.LoadScene(0);
                break;
        }
        await OutGameUIManager.Instance.FadeIn(0.75f);
        IsSceneChange = false;
    }
}
