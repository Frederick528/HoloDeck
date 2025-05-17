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

    public int PauseInt;

    public int NowChapterLV;

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

    public void ChangeScene(int idx)
    {
        SceneManager.LoadScene(idx);
        switch (idx)
        {
            case 0:
                DestroyAllInGameDontDestroyObjects();
                NowChapterLV = 0;       // ·Îºñ
                break;
            case 1:
                NowChapterLV = 1;
                //MapManager.Instance.CreateMapCnt = 15;
                //MapManager.Instance.MaxDistance = (3, 3);
                //MapManager.Instance.MapScale = 1;
                break;
            case 2:
                NowChapterLV = 2;
                //MapManager.Instance.CreateMapCnt = 30;
                //MapManager.Instance.MaxDistance = (4, 3);
                //MapManager.Instance.MapScale = 0.95f;
                //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false, MapManager.Instance.currStage.rewardBox);
                break;
        }
    }
}
