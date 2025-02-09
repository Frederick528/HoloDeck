using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    public enum CanvasName
    {
        InGame,
        GameOver,
        Battle,
        Map,
        RewardBox,
        CardReward,
        ItemReward,
        Shop,
        SelectedCard,
        ViewDeck
    }
    public List<GameObject> CanvasList;
    List<GraphicRaycaster> _canvasRaycaster = new();
    public Dictionary<int, GameObject> CanvasDict = new();

    [Header("UICardPrefab")]
    [SerializeField] GameObject _uiCard;

    [Header("Panel")]
    Transform _cardEnlargePanel;
    Transform _itemEnlargePanel;

    
    Transform _shopPanel;
    Transform _shopEnlargePanel;

    [Header("Box")]
    Transform[] _rewardBoxes;

    Transform _cardRewardContent;
    Transform _itemRewardContent;

    Transform _viewDeckContent;
    List<UICard> _deckUICards = new();

    UICard[] _uiCards = new UICard[4];

    TMP_Text _topHealthText;     // TMP텍스트로 변경가능성있음
    TMP_Text _topCoinText;

    TMP_Text _holoValue;
    TMP_Text _turnEndButtonText;
    TMP_Text _drawCount;
    TMP_Text _dummyCount;


    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < CanvasList.Count; ++i)
        {
            _canvasRaycaster.Add(CanvasList[i].GetComponent<GraphicRaycaster>());
            CanvasDict.Add(i, CanvasList[i]);
        }

        _cardEnlargePanel = Canvas(CanvasName.CardReward).Find("CardEnlargePanel");
        _cardRewardContent = FindChildByName(Canvas(CanvasName.CardReward), "Content");
        _itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel");
        _itemRewardContent = FindChildByName(Canvas(CanvasName.ItemReward), "Content");

        _viewDeckContent = FindChildByName(Canvas(CanvasName.ViewDeck), "Content");
        for (int i = 0; i < _viewDeckContent.childCount; ++i)
        {
            _deckUICards.Add(_viewDeckContent.GetChild(i).GetComponent<UICard>());
        }

        _shopPanel = Canvas(CanvasName.Shop).Find("ShopPanel");
        _shopEnlargePanel = Canvas(CanvasName.Shop).Find("ShopEnlargePanel");

        _topHealthText = FindChildByName(Canvas(CanvasName.InGame), "HealthText").GetComponent<TMP_Text>();
        _topCoinText = FindChildByName(Canvas(CanvasName.InGame), "CoinText").GetComponent<TMP_Text>();

        _holoValue = FindChildByName(Canvas(CanvasName.Battle), "HoloValueText").GetComponent<TMP_Text>();
        _turnEndButtonText = FindChildByName(Canvas(CanvasName.Battle), "TurnText").GetComponent<TMP_Text>();
        _drawCount = FindChildByName(Canvas(CanvasName.Battle), "DrawCountText").GetComponent<TMP_Text>();
        _dummyCount = FindChildByName(Canvas(CanvasName.Battle), "DummyCountText").GetComponent<TMP_Text>();

        Transform rewardBoxCanvas = Canvas(CanvasName.RewardBox);
        _rewardBoxes = new Transform[rewardBoxCanvas.childCount];
        for (int i = 0; i < _rewardBoxes.Length; ++i)
        {
            _rewardBoxes[i] = rewardBoxCanvas.GetChild(i);
        }

        for (int i = 0; i < _uiCards.Length; ++i)
        {
            _uiCards[i] = _cardRewardContent.GetChild(i).GetComponent<UICard>();
        }
    }

    private void Start()
    {
        //_cardEnlargePanel = Canvas(CanvasName.CardReward).Find("CardEnlargePanel");
        //_cardRewardContent = FindChildByName(Canvas(CanvasName.CardReward), "Content");
        //_itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel");
        //_itemRewardContent = FindChildByName(Canvas(CanvasName.ItemReward), "Content");
        //_shopPanel = Canvas(CanvasName.Shop).Find("ShopPanel");
        //_shopEnlargePanel = Canvas(CanvasName.Shop).Find("ShopEnlargePanel");

        //_topHealthText = FindChildByName(Canvas(CanvasName.InGame), "HealthText").GetComponent<TMP_Text>();
        //_topCoinText = FindChildByName(Canvas(CanvasName.InGame), "CoinText").GetComponent<TMP_Text>();

        //_holoValue = FindChildByName(Canvas(CanvasName.Battle), "HoloValueText").GetComponent<TMP_Text>();
        //_turnEndButtonText = FindChildByName(Canvas(CanvasName.Battle), "TurnEndButton").GetComponent<TMP_Text>();

        //Transform rewardBoxCanvas = Canvas(CanvasName.RewardBox);
        //_rewardBoxes = new Transform[rewardBoxCanvas.childCount];
        //for (int i = 0; i < _rewardBoxes.Length; ++i)
        //{
        //    _rewardBoxes[i] = rewardBoxCanvas.GetChild(i);
        //}

        //for (int i = 0; i < _uiCards.Length; ++i)
        //{
        //    _uiCards[i] = _cardRewardContent.GetChild(i).GetComponent<UICard>();
        //}

        SetActiveCanvas(CanvasName.Map, true);
    }

    public Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
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

    public void SetCanvasRaycast(CanvasName canvasName, bool isOn)
    {
        _canvasRaycaster[(int)canvasName].enabled = isOn;
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
            _uiCards[i].Setup(InGameManager.Instance.FindCardData(reward[i]));
    }
    public void ChangeRewardCardCount(bool isOn)
    {
        _cardRewardContent.GetChild(3).gameObject.SetActive(isOn);
    }

    public void SetViewDeck(List<Card> deck)
    {
        _viewDeckContent.transform.localPosition = new Vector3(_viewDeckContent.transform.localPosition.x, 0);
        if (deck.Count > _deckUICards.Count)
        {
            for (int i = 0; i < deck.Count - _deckUICards.Count; ++i)
            {
                _deckUICards.Add(Instantiate(_uiCard, _viewDeckContent).GetComponent<UICard>());
            }
        }
        for (int j = 0; j < _deckUICards.Count; ++j)
        {
            if (j > deck.Count - 1)
            {
                _deckUICards[j].gameObject.SetActive(false);
                continue;
            }
            _deckUICards[j].gameObject.SetActive(true);
            _deckUICards[j].Setup(deck[j].Data);
        }
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
            _turnEndButtonText.text = "Turn End";
        else
            _turnEndButtonText.text = "Enemy's Turn";
    }
    public void LookMap()
    {
        GameObject map = CanvasDict[(int)CanvasName.Map];
        map.SetActive(!map.activeSelf);
    }

    public void SetHolo(int curHolo, int maxHolo)
    {
        _holoValue.text = $"{curHolo} / {maxHolo}";
    }

    public void SetHealth(int curHp, int maxHp)
    {
        _topHealthText.text = $"{curHp} / {maxHp}";
    }

    public void SetCoin(int coin)
    {
        _topCoinText.text = coin.ToString();
    }

    public void SetDrawCount()
    {
        _drawCount.text = CardManager.Instance.DrawDeck.Count.ToString();
    }

    public void SetDummyCount()
    {
        _dummyCount.text = CardManager.Instance.CardDummy.Count.ToString();
    }
}
