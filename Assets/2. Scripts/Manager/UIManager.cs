using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.Mathematics;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
    //public List<GameObject> CanvasList;
    List<GraphicRaycaster> _canvasRaycaster = new();
    public Dictionary<int, Transform> CanvasDict = new();

    Transform _canvas;



    [Header("UICardPrefab")]
    [SerializeField] UICard _uiCard;

    [Header("Panel")]
    Transform _cardEnlargePanel;
    Transform _itemEnlargePanel;


    Transform _shopPanel;
    Transform _shopEnlargePanel;

    [Header("Box")]
    Transform[] _rewardBoxes;

    [HideInInspector]
    public Transform PassiveTransform;
    [HideInInspector]
    public Transform ActiveTransform;
    [HideInInspector]
    public Transform PotionTransform;

    Transform _statusWindow;
    Image[] _statusImg = new Image[3];
    TMP_Text[] _statusText = new TMP_Text[7];

    Transform _cardRewardContent;
    Transform _itemRewardContent;

    int _lastViewDeckCount;
    Transform _viewDeckContent;
    List<UICard> _deckUICards = new();

    UICard[] _uiCards = new UICard[4];
    UIItem[] _uiItems = new UIItem[4];

    TMP_Text _topHealthText;     // TMP텍스트로 변경가능성있음
    TMP_Text _topCoinText;

    TMP_Text _holoValue;
    TMP_Text _turnEndButtonText;
    TMP_Text _drawCount;
    TMP_Text _dummyCount;

    bool _addRewardItemCount;


    private void Awake()
    {
        _canvas = GameObject.Find("Canvases").GetComponent<Transform>();

        if (Instance == null)
        {
            Instance = this;
            //transform.SetParent(null);
            _canvas.gameObject.name = "CanvasesDontDestroy";
            GameManager.Instance.AddDontDestroy(_canvas.gameObject);
            GameManager.Instance.AddDontDestroy(transform.root.gameObject);
            //DontDestroyOnLoad(_canvas.gameObject);
            //DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(_canvas.gameObject);
            Destroy(transform.root.gameObject);
            return;
        }

        for (int i = 0; i < /*CanvasList.Count*/_canvas.childCount; ++i)
        {
            //_canvasRaycaster.Add(CanvasList[i].GetComponent<GraphicRaycaster>());
            //CanvasDict.Add(i, CanvasList[i]);
            _canvasRaycaster.Add(_canvas.GetChild(i).GetComponent<GraphicRaycaster>());
            CanvasDict.Add(i, _canvas.GetChild(i));
        }

        _cardEnlargePanel = Canvas(CanvasName.CardReward).Find("CardEnlargePanel");
        _cardRewardContent = ContinueFindChildByName(Canvas(CanvasName.CardReward), "Content");
        _itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel");
        _itemRewardContent = ContinueFindChildByName(Canvas(CanvasName.ItemReward), "Content");

        _viewDeckContent = ContinueFindChildByName(Canvas(CanvasName.ViewDeck), "Content");
        //for (int i = 0; i < ViewDeckContent.childCount; ++i)
        //{
        //    _deckUICards.Add(ViewDeckContent.GetChild(i).GetComponent<UICard>());
        //}

        PassiveTransform = ContinueFindChildByName(Canvas(CanvasName.InGame), "PassiveItem");
        ActiveTransform = ContinueFindChildByName(Canvas(CanvasName.InGame), "ActiveItemButton");
        PotionTransform = ContinueFindChildByName(Canvas(CanvasName.InGame), "PotionItem");

        _statusWindow = ContinueFindChildByName(Canvas(CanvasName.InGame), "Status");
        _statusImg[0] = _statusWindow.Find("HPCircle").GetComponent<Image>();
        _statusText[0] = _statusWindow.Find("HP").GetComponent<TMP_Text>();

        _statusText[1] = _statusWindow.Find("ATK").GetComponent<TMP_Text>();
        _statusText[2] = _statusWindow.Find("DEF").GetComponent<TMP_Text>();
        _statusText[3] = _statusWindow.Find("HEAL").GetComponent<TMP_Text>();


        _statusImg[1] = _statusWindow.Find("CriticalBar").GetComponent<Image>();
        _statusText[4] = _statusWindow.Find("CriticalChance").GetComponent<TMP_Text>();
        _statusText[5] = _statusWindow.Find("CriticalDamage").GetComponent<TMP_Text>();
        _statusText[6] = _statusWindow.Find("Critical").GetComponent<TMP_Text>();

        _statusImg[2] = ContinueFindChildByName(_statusImg[0].transform, "PlayerImage").GetComponent<Image>();

        _shopPanel = Canvas(CanvasName.Shop).Find("ShopPanel");
        _shopEnlargePanel = Canvas(CanvasName.Shop).Find("ShopEnlargePanel");

        _topHealthText = ContinueFindChildByName(Canvas(CanvasName.InGame), "HealthText").GetComponent<TMP_Text>();
        _topCoinText = ContinueFindChildByName(Canvas(CanvasName.InGame), "CoinText").GetComponent<TMP_Text>();

        _holoValue = ContinueFindChildByName(Canvas(CanvasName.Battle), "HoloValueText").GetComponent<TMP_Text>();
        _turnEndButtonText = ContinueFindChildByName(Canvas(CanvasName.Battle), "TurnText").GetComponent<TMP_Text>();
        _drawCount = ContinueFindChildByName(Canvas(CanvasName.Battle), "DrawCountText").GetComponent<TMP_Text>();
        _dummyCount = ContinueFindChildByName(Canvas(CanvasName.Battle), "DummyCountText").GetComponent<TMP_Text>();

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
        for (int i = 0; i < _uiItems.Length; ++i)
        {
            _uiItems[i] = _itemRewardContent.GetChild(i).GetComponent<UIItem>();
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

    public Transform ContinueFindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = ContinueFindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    //public Transform FindChildByName(CanvasName canvasName, string name)
    //{
    //    return CanvasDict[(int)canvasName].Find(name);
    //}


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

            CanvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            CanvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.RewardBox:
                    _rewardBoxes[idx].gameObject.SetActive(true);
                    break;
                case CanvasName.Map:
                    if (Canvas(CanvasName.ViewDeck).gameObject.activeSelf)
                    {
                        SetActiveCanvas(CanvasName.ViewDeck, false);
                        //InGameManager.Instance.Pause(false);      // UI가 막아서 뷰덱 중에는 맵 화면 클릭 불가(그렇게 되도록 배치한 거라서 문제인 건 아니고, 그냥 나중을 위한 코멘트임.)
                    }
                    break;
                case CanvasName.ViewDeck:
                    if (Canvas(CanvasName.Map).gameObject.activeSelf)
                        SetActiveCanvas(CanvasName.Map, false);
                    break;
            }
        }

    }

    public Transform Canvas(CanvasName canvasName)
    {
        return CanvasDict[(int)canvasName];
    }

    public void SetCanvasRaycast(CanvasName canvasName, bool isOn)
    {
        _canvasRaycaster[(int)canvasName].enabled = isOn;
    }

    public void ShowStatus()
    {
        if (_statusWindow.localPosition.x == -985)
        {
            _statusWindow.localPosition = new Vector3(-1460, 0, 0);
        }
        else if (_statusWindow.localPosition.x == -1460)
        {
            _statusWindow.localPosition = new Vector3(-985, 0, 0);
        }
    }

    public void ChangeStatus(int statusIdx, int amount, float refAmount = -1)
    {
        switch (statusIdx)
        {
            case 0:
                if (refAmount == -1) return;
                _statusImg[0].fillAmount = amount / refAmount;
                _topHealthText.text = $"{amount} / {refAmount}";
                _statusText[statusIdx].text = $"HP: {amount}<size={_statusText[0].fontSize * 0.8f}>\nMax HP: {refAmount}</size>";
                break;
            case 1:
                _statusText[statusIdx].text = "ATK: " + amount.ToString();
                break;
            case 2:
                _statusText[statusIdx].text = "DEF: " + amount.ToString();
                break;
            case 3:
                _statusText[statusIdx].text = "HEAL: " + amount.ToString();
                break;
            case 4:
                _statusText[statusIdx].text = "CriticalChance: " + amount.ToString() + "%";
                break;
            case 5:
                _statusText[statusIdx].text = "CriticalDamage: " + amount.ToString() + "%";
                break;
            case 6:
                if (refAmount == -1) return;
                _statusImg[1].fillAmount = amount / refAmount;
                _statusText[statusIdx].text = $"{amount} / {refAmount}";
                break;
        }
    }

    public void MoveMap()
    {
        SetActiveCanvas(CanvasName.CardReward, false);
        SetActiveCanvas(CanvasName.ItemReward, false);
        SetActiveCanvas(CanvasName.Shop, false);
    }
    public void ShowRewardCard(CardData[] reward)        // 해당 부분들 맵, 상점으로 다 이동시켜야 함.
    {
        for (int i = 0; i < reward.Length; ++i)
        {
            if (reward[i] == null) continue;        // 카드는 나중에 중복으로 떠도 되기 때문에 카드레어도에 따른 카드풀이 3~4장 이상이면 null이 뜰 가능성이 존재하지 않음.
            _uiCards[i].Setup(/*InGameManager.Instance.FindCardData(reward[i])*/reward[i]);
        }
    }
    public void ShowRewardItem(ItemData[] reward)
    {
        SetActiveCanvas(CanvasName.ItemReward, true);
        for (int i = 0; i < reward.Length; ++i)
        {
            if (reward[i] == null)
            {
                _itemRewardContent.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            else if (!_itemRewardContent.GetChild(i).gameObject.activeSelf)
            {
                if (i == reward.Length - 1 && !_addRewardItemCount)
                    continue;
                _itemRewardContent.GetChild(i).gameObject.SetActive(true);
            }
            _uiItems[i].Setup(reward[i], i);
        }
        SetActiveCanvas(CanvasName.ItemReward, false);
    }
    public void ChangeRewardCardCount(bool isOn)
    {
        _cardRewardContent.GetChild(3).gameObject.SetActive(isOn);
    }
    public void ChangeRewardItemCount(bool isOn)
    {
        _addRewardItemCount = isOn;
        _itemRewardContent.GetChild(3).gameObject.SetActive(isOn);
    }

    public void SetViewDeck(List<Card> deck)                // 풀링이지만, Release 개념이 아닌, 활성화 비활성화로 진행됨. Release는 인덱스로 넣는데, Get은 Release된 것 중에서 마지막에 넣었던 것을 꺼내오기 때문에 생긴 문제
    {
        _viewDeckContent.localPosition = new Vector3(_viewDeckContent.localPosition.x, 0);
        if (deck.Count > _deckUICards.Count)
        {
            int deckUICardCount = _deckUICards.Count;
            for (int i = 0; i < _lastViewDeckCount; ++i)
            {
                _deckUICards[i].Setup(deck[i].Data);
            }
            for (int i = _lastViewDeckCount; i < _deckUICards.Count; ++i)
            {
                _deckUICards[i].gameObject.SetActive(true);
                _deckUICards[i].Setup(deck[i].Data);
            }
            for (int i = deckUICardCount; i < deck.Count; ++i)
            {
                _deckUICards.Add(Instantiate(_uiCard, _viewDeckContent));
                _deckUICards[i].Setup(deck[i].Data);
            }
        }
        else
        {
            if (deck.Count > _lastViewDeckCount)
            {
                for (int i = 0; i < _lastViewDeckCount; ++i)
                {
                    _deckUICards[i].Setup(deck[i].Data);
                }
                for (int i = _lastViewDeckCount; i < deck.Count; ++i)
                {
                    _deckUICards[i].gameObject.SetActive(true);
                    _deckUICards[i].Setup(deck[i].Data);
                }
            }
            else
            {
                for (int i = 0; i < deck.Count; ++i)
                {
                    _deckUICards[i].Setup(deck[i].Data);
                }
                for (int i = deck.Count; i < _lastViewDeckCount; ++i)
                {
                    _deckUICards[i].gameObject.SetActive(false);
                }
            }
        }
        _lastViewDeckCount = deck.Count;
        SetActiveCanvas(CanvasName.ViewDeck, true);
        InGameManager.Instance.Pause(true);
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
        SetActiveCanvas(CanvasName.Map, !Canvas(CanvasName.Map).gameObject.activeSelf);
    }

    public void SetHolo(int curHolo, int maxHolo)
    {
        _holoValue.text = $"{curHolo} / {maxHolo}";
    }

    //public void SetHealth(int _curHP, int _maxHP)
    //{
    //    _topHealthText.text = $"{_curHP} / {_maxHP}";
    //}

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
