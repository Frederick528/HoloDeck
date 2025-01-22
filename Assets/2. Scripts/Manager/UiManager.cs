using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UiManager;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    public enum CanvasName
    {
        InGame,
        Battle,
        Map,
        RewardBox,
        CardReward,
        ItemReward,
        Shop,
        SelectedCard
    }
    public List<GameObject> CanvasList;
    public Dictionary<int, GameObject> CanvasDict = new();

    [Header("Panel")]
    Transform _cardEnlargePanel;
    Transform _itemEnlargePanel;

    
    Transform _shopPanel;
    Transform _shopEnlargePanel;

    [Header("Box")]
    Transform[] _rewardBoxes;

    public Transform _cardRewardContent;
    Transform _itemRewardContent;

    UICard[] _uICards = new UICard[4];

    [Header("Um..")]
    [SerializeField] TextMeshProUGUI topHealthText;     // TMP텍스트로 변경가능성있음
    [SerializeField] TextMeshProUGUI topCoinText;
    [SerializeField] TextMeshProUGUI turnEndButtonText;


    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < CanvasList.Count; ++i)
        {
            CanvasDict.Add(i, CanvasList[i]);
        }
    }

    private void Start()
    {
        _cardEnlargePanel = Canvas(CanvasName.CardReward).Find("CardEnlargePanel");
        //_cardRewardContent = Canvas(CanvasName.CardReward).Find("Content");
        _itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel");
        _itemRewardContent = Canvas(CanvasName.ItemReward).Find("Content");
        _shopPanel = Canvas(CanvasName.Shop).Find("ShopPanel");
        _shopEnlargePanel = Canvas(CanvasName.Shop).Find("ShopEnlargePanel");

        Transform rewardBoxCanvas = Canvas(CanvasName.RewardBox);
        _rewardBoxes = new Transform[rewardBoxCanvas.childCount];
        for (int i = 0;i < _rewardBoxes.Length; ++i)
        {
            _rewardBoxes[i] = rewardBoxCanvas.GetChild(i);
        }

        for (int i = 0; i < _uICards.Length; ++i)
        {
            _uICards[i] = _cardRewardContent.GetChild(i).GetComponent<UICard>();
        }

        SetActiveCanvas(CanvasName.Map, true);
    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)
        {
            switch (canvasName)
            {
                case CanvasName.RewardBox:
                    if (idx == -1) break;
                    _rewardBoxes[idx].gameObject.SetActive(false);
                    break;
                case CanvasName.CardReward:
                    _cardEnlargePanel.gameObject.SetActive(false);
                    break;
                case CanvasName.ItemReward:
                    _itemEnlargePanel.gameObject.SetActive(false);
                    break;
                case CanvasName.Shop:
                    _shopPanel.gameObject.SetActive(false);
                    _shopEnlargePanel.gameObject.SetActive(false);
                    break;
            }

            CanvasDict[(int)canvasName].SetActive(false);
        }
        else
        {
            CanvasDict[(int)canvasName].SetActive(true);

            switch (canvasName)
            {
                case CanvasName.RewardBox:
                    _rewardBoxes[idx].gameObject.SetActive(true);
                    break;
            }
        }

    }

    public Transform Canvas(CanvasName canvasName)
    {
        return CanvasDict[(int)canvasName].transform;
    }

    public void MoveMap()
    {
        SetActiveCanvas(CanvasName.CardReward, false);
        SetActiveCanvas(CanvasName.ItemReward, false);
        SetActiveCanvas(CanvasName.Shop, false);
    }
    public void ShowRewardCard(int[] reward)        // 해당 부분들 맵, 상점으로 다 이동시켜야 함.
    {
        for (int i = 0; i < reward.Length; ++i)
            _uICards[i].Setup(GameManager.Instance.FindCardData(reward[i]));
    }

    //public void SetupGameUi(bool state)
    //{
    //    SetActiveCanvas(CanvasName.INGAME);
    //}
    //public void SetupBattleUi(bool state)
    //{
    //    SetActiveCanvas(CanvasName.BATTLE);
    //}

    //public void SetupSelectCardInBattleUi(bool state)
    //{
    //    foreach (GameObject gameObject in UI)
    //    {
    //        gameObject.SetActive(state);
    //    }
    //}

    public void ChangeTurnButtonText(bool turn)
    {
        if (turn)
            turnEndButtonText.text = "Turn End";
        else
            turnEndButtonText.text = "Enemy's Turn";
    }
    public void LookMap()
    {
        GameObject map = CanvasDict[(int)CanvasName.Map];
        map.SetActive(!map.activeSelf);
    }

    public void SetHealth(int curHp, int maxHp)
    {
        topHealthText.text = $"{curHp} / {maxHp}";
    }

    public void SetCoin(int coin)
    {
        topCoinText.text = coin.ToString();
    }
}
