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

    public bool Resurrection { get; private set; }
    public int AddMaxHP { get; private set; }
    public int AddAttackPower { get; private set; }
    public int AddDefensePower { get; private set; }
    public int AddHealPower { get; private set; }
    public int AddCriticalChance { get; private set; }
    public int AddCriticalDamage { get; private set; }

    public ReactiveProperty<int> Goods { get; private set; } = new();

    public int PauseInt;

    public int NowChapterLV = 1;

    bool _isESCPause = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Goods.Subscribe(goods =>
        {
            if (InGame)
                InGameUIManager.Instance.ChangeStatus(7, goods);
        });
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
    public void AddGoods(int value)
    {
        Goods.Value += value;
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
