using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    public enum CanvasName      // 순서가 Canvas 순서랑 일치해야 함. 번호를 지정하는 이유는 버튼에서 Enum으로 받을 때, 번호로 받아야 여기 순서를 안 따라가기 때문. (가운데 추가하는 경우 문제가 발생.)
    {
        InGame = 0,
        Battle = 1,
        RewardBox = 10,
        CardReward = 11,
        ItemReward = 12,
        SelectedCard = 15,
        ViewDeck = 20,
        Inventory = 25,
        Event = 30,
        Shop = 40,
        Map = 50,
        Enemy = 60,
        GameOver = 99,
    }

    //public List<GameObject> CanvasList;
    List<GraphicRaycaster> _canvasRaycaster = new();
    public Dictionary<int, RectTransform> _canvasDict = new();

    Transform _canvasTr;



    //[Header("UICardPrefab")]
    UICard _uiCard;

    public GameObject StatusEffectPrefab;
    public GameObject StatusEffectDescPrefab;

    //[Header("Panel")]
    RectTransform _cardEnlargePanel;
    RectTransform _itemEnlargePanel;


    RectTransform _shopPanel;
    RectTransform _shopEnlargePanel;

    //[Header("Box")]
    RectTransform[] _rewardBoxes;

    [HideInInspector]
    public RectTransform PassiveTransform;
    [HideInInspector]
    public RectTransform ActiveTransform;
    [HideInInspector]
    public RectTransform PotionTransform;

    RectTransform _statusWindow;
    Image[] _statusImg = new Image[3];
    TMP_Text[] _statusText = new TMP_Text[9];

    RectTransform _cardRewardContent;
    RectTransform _itemRewardContent;

    int _lastViewDeckCount;
    RectTransform _viewDeckContent;
    List<UICard> _deckUICards = new();

    RectTransform _inventoryContent;
    RectTransform _dropReward;
    RectTransform _dropRewardContent;
    RectTransform _inventoryInfo;

    UICard[] _uiCards = new UICard[4];
    UIItem[] _uiItems = new UIItem[4];

    TMP_Text _topHealthText;     // TMP텍스트로 변경가능성있음
    TMP_Text _topCoinText;

    TMP_Text _holoValue;
    TMP_Text _turnEndButtonText;
    TMP_Text _drawCount;
    TMP_Text _dummyCount;

    bool _addRewardItemCount;

    public Image EventImage;
    public TMP_Text EventText;

    public bool EndLoad;

    private void Awake()
    {
        _canvasTr = GameObject.Find("InGameCanvases").transform;
        if (Instance != null)
        {
            GameObject.Find("BackgroundCanvas").GetComponent<Canvas>().worldCamera = Camera.main;
            Destroy(_canvasTr.gameObject);
            //Destroy(transform.root.gameObject);
            return;
        }
        //if (Instance == null)
        //{
        //    Instance = this;
        //    //transform.SetParent(null);
        //    _canvas.gameObject.name = "InGameCanvasesDontDestroy";
        //    GameManager.Instance.AddInGameDontDestroy(_canvas.gameObject);
        //    GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
        //    //DontDestroyOnLoad(_canvas.gameObject);
        //    //DontDestroyOnLoad(transform.root.gameObject);
        //}
        //else
        //{
        //    Destroy(_canvas.gameObject);
        //    Destroy(transform.root.gameObject);
        //    return;
        //}

        Instance = this;
        _canvasTr.gameObject.name = "InGameCanvasesDontDestroy";
        GameManager.Instance.AddInGameDontDestroy(_canvasTr.gameObject);
        //GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
        CanvasName[] canvasNamesArray = (CanvasName[])Enum.GetValues(typeof(CanvasName));
        for (int i = 0; i < /*CanvasList.Count*/_canvasTr.childCount; ++i)
        {
            //_canvasRaycaster.Add(CanvasList[i].GetComponent<GraphicRaycaster>());
            //CanvasDict.Add(i, CanvasList[i]);
            _canvasRaycaster.Add(_canvasTr.GetChild(i).GetComponent<GraphicRaycaster>());
            _canvasDict.Add((int)canvasNamesArray[i], _canvasTr.GetChild(i) as RectTransform);
        }

        //Addressables.LoadAssetAsync<GameObject>("UICardImg.prefab").Completed += (op) =>
        //{
        //    if (op.Status != AsyncOperationStatus.Succeeded)
        //    {
        //        Debug.LogError("UICardImg null");
        //    }
        //    else
        //    {
        //        _uiCard = op.Result.GetComponent<UICard>();
        //    }
        //    Addressables.Release(op);

        //};
        //Addressables.LoadAssetAsync<GameObject>("StatusEffect.prefab").Completed += (op) =>
        //{
        //    if (op.Status != AsyncOperationStatus.Succeeded)
        //    {
        //        Debug.LogError("StatusEffect null");
        //    }
        //    else
        //    {
        //        StatusEffectPrefab = op.Result;
        //    }
        //    Addressables.Release(op);

        //};
        //Addressables.LoadAssetAsync<GameObject>("StatusEffectDesc.prefab").Completed += (op) =>
        //{
        //    if (op.Status != AsyncOperationStatus.Succeeded)
        //    {
        //        Debug.LogError("StatusEffectDesc null");
        //    }
        //    else
        //    {
        //        StatusEffectDescPrefab = op.Result;
        //    }
        //    Addressables.Release(op);

        //};
        LoadAsync().Forget();

        _cardEnlargePanel = Canvas(CanvasName.CardReward).Find("CardEnlargePanel") as RectTransform;
        _cardRewardContent = FindTransform.ContinueFindChildByName(Canvas(CanvasName.CardReward), "Content");
        _itemEnlargePanel = Canvas(CanvasName.ItemReward).Find("ItemEnlargePanel") as RectTransform;
        _itemRewardContent = FindTransform.ContinueFindChildByName(Canvas(CanvasName.ItemReward), "Content");

        _viewDeckContent = FindTransform.ContinueFindChildByName(Canvas(CanvasName.ViewDeck), "Content");

        _inventoryContent = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Inventory), "InventoryContent");
        _dropReward = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Inventory), "DropReward");
        _dropRewardContent = FindTransform.ContinueFindChildByName(_dropReward, "DropRewardContent");
        _inventoryInfo = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Inventory), "Information");
        //for (int i = 0; i < ViewDeckContent.childCount; ++i)
        //{
        //    _deckUICards.Add(ViewDeckContent.GetChild(i).GetComponent<UICard>());
        //}

        PassiveTransform = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "PassiveItem");
        ActiveTransform = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "ActiveItemButton");
        PotionTransform = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "PotionItem");

        _statusWindow = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "Status");
        _statusImg[0] = _statusWindow.Find("HPCircle").GetComponent<Image>();
        _statusText[0] = _statusWindow.Find("HP").GetComponent<TMP_Text>();

        _statusText[1] = _statusWindow.Find("ATK").GetComponent<TMP_Text>();
        _statusText[2] = _statusWindow.Find("DEF").GetComponent<TMP_Text>();
        _statusText[3] = _statusWindow.Find("HEAL").GetComponent<TMP_Text>();


        _statusImg[1] = _statusWindow.Find("CriticalBar").GetComponent<Image>();
        _statusText[4] = _statusWindow.Find("CriticalChance").GetComponent<TMP_Text>();
        _statusText[5] = _statusWindow.Find("CriticalDamage").GetComponent<TMP_Text>();
        _statusText[6] = _statusWindow.Find("Critical").GetComponent<TMP_Text>();
        _statusText[7] = _statusWindow.Find("Coin").GetComponent<TMP_Text>();
        _statusText[8] = _statusWindow.Find("Goods").GetComponent<TMP_Text>();

        _statusImg[2] = FindTransform.ContinueFindChildByName(_statusImg[0].transform, "PlayerImage").GetComponent<Image>();

        ChangeStatus(8, GameManager.Instance.Goods.Value);

        _shopPanel = Canvas(CanvasName.Shop).Find("ShopPanel") as RectTransform;
        _shopEnlargePanel = Canvas(CanvasName.Shop).Find("ShopEnlargePanel") as RectTransform;

        _topHealthText = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "HealthText").GetComponent<TMP_Text>();
        _topCoinText = FindTransform.ContinueFindChildByName(Canvas(CanvasName.InGame), "CoinText").GetComponent<TMP_Text>();

        _holoValue = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Battle), "HoloValueText").GetComponent<TMP_Text>();
        _turnEndButtonText = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Battle), "TurnText").GetComponent<TMP_Text>();
        _drawCount = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Battle), "DrawCountText").GetComponent<TMP_Text>();
        _dummyCount = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Battle), "DummyCountText").GetComponent<TMP_Text>();

        Transform rewardBoxCanvas = Canvas(CanvasName.RewardBox);
        _rewardBoxes = new RectTransform[rewardBoxCanvas.childCount];
        for (int i = 0; i < _rewardBoxes.Length; ++i)
        {
            _rewardBoxes[i] = rewardBoxCanvas.GetChild(i) as RectTransform;
            switch (_rewardBoxes[i].name)
            {
                case "TreasureBox":
                    _rewardBoxes[i].GetComponent<Button>().onClick.AddListener(() => SetActiveCanvas(CanvasName.ItemReward, true));
                    break;
                case "DropBox":
                    _rewardBoxes[i].GetComponent<Button>().onClick.AddListener(() => SetActiveCanvas(CanvasName.Inventory, true, 1));
                    break;
                default:
                    _rewardBoxes[i].GetComponent<Button>().onClick.AddListener(() => SetActiveCanvas(CanvasName.CardReward, true));
                    break;
            }
        }
        for (int i = 0; i < _uiCards.Length; ++i)
        {
            _uiCards[i] = _cardRewardContent.GetChild(i).GetComponent<UICard>();
        }
        for (int i = 0; i < _uiItems.Length; ++i)
        {
            _uiItems[i] = _itemRewardContent.GetChild(i).GetComponent<UIItem>();
        }

        EventImage = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Event), "EventImage").GetComponent<Image>();
        EventText = FindTransform.ContinueFindChildByName(Canvas(CanvasName.Event), "EventText").GetComponent<TMP_Text>();

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

        //SetActiveCanvas(CanvasName.Map, true);
    }

    //public Transform ContinueFindChildByName(Transform parent, string name)
    //{
    //    foreach (Transform child in parent)
    //    {
    //        if (child.name == name)
    //            return child;

    //        Transform found = ContinueFindChildByName(child, name);
    //        if (found != null)
    //            return found;
    //    }
    //    return null;
    //}

    //public Transform FindChildByName(CanvasName canvasName, string name)
    //{
    //    return CanvasDict[(int)canvasName].Find(name);
    //}
    public async UniTask LoadAsync()
    {
        var handle1 = Addressables.LoadAssetAsync<GameObject>("UICardImg.prefab");
        var handle2 = Addressables.LoadAssetAsync<GameObject>("StatusEffect.prefab");
        var handle3 = Addressables.LoadAssetAsync<GameObject>("StatusEffectDesc.prefab");

        var playerHandle = InGameManager.Instance.LoadAsync();

        // 전부 기다림
        await UniTask.WhenAll(
            handle1.ToUniTask(),
            handle2.ToUniTask(),
            handle3.ToUniTask(),
            playerHandle.ToUniTask()
        );

        // 성공 여부 확인
        if (handle1.Status == AsyncOperationStatus.Succeeded &&
            handle2.Status == AsyncOperationStatus.Succeeded &&
            handle3.Status == AsyncOperationStatus.Succeeded &&
            playerHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("모든 에셋 로드 성공!");
            _uiCard = handle1.Result.GetComponent<UICard>();
            StatusEffectPrefab = handle2.Result;
            StatusEffectDescPrefab = handle3.Result;
            InGameManager.Instance.SpawnPlayer(playerHandle.Result);
            EndLoad = true;
        }
        else
        {
            Debug.LogWarning("하나 이상의 에셋 로드 실패");
        }

        Addressables.Release(handle1);
        Addressables.Release(handle2);
        Addressables.Release(handle3);
        //InGameManager.Instance.ReleaseAddressable(playerHandle);
        //Addressables.Release(playerHandle);


    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // 꺼질 때
        {
            switch (canvasName)
            {
                case CanvasName.RewardBox:
                    if (idx == -1)          // 그냥 쓰면 모든 박스를 비활성화함. 근데 박스는 보통 1개만 활성화하니, 해당 박스가 무엇인지 특정되지 않는 상황이 아니라면, 그냥 쓰지는 말도록 하장.
                    {
                        for (int i = 0; i < _rewardBoxes.Length; ++i)
                        {
                            _rewardBoxes[i].gameObject.SetActive(false);
                            MapManager.Instance.ShowBox(i, false);
                        }
                        SetActiveCanvas(CanvasName.ItemReward, false);
                        SetActiveCanvas(CanvasName.CardReward, false);
                        break;
                    }
                    _rewardBoxes[idx].gameObject.SetActive(false);
                    MapManager.Instance.ShowBox(idx, false);
                    if (idx == 0)
                    {
                        SetActiveCanvas(CanvasName.ItemReward, false);
                    }
                    else if (idx == 6)
                    {
                        SetActiveCanvas(CanvasName.Inventory, false);
                    }
                    else
                    {
                        SetActiveCanvas(CanvasName.CardReward, false);
                    }
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
                case CanvasName.ViewDeck:
                    GameManager.Instance.Pause(false);
                    break;
                case CanvasName.Inventory:
                    _dropReward.gameObject.SetActive(false);
                    break ;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            _canvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.RewardBox:
                    if (idx == -1) break;
                    MapManager.Instance.ShowBox(idx, true);
                    _rewardBoxes[idx].gameObject.SetActive(true);
                    break;
                case CanvasName.Map:
                    //if (Canvas(CanvasName.ViewDeck).gameObject.activeSelf)
                    //{
                    //    SetActiveCanvas(CanvasName.ViewDeck, false);
                        //InGameManager.Instance.Pause(false);      // UI가 막아서 뷰덱 중에는 맵 화면 클릭 불가(그렇게 되도록 배치한 거라서 문제인 건 아니고, 그냥 나중을 위한 코멘트임.)
                    //}
                    break;
                case CanvasName.ViewDeck:
                    if (Canvas(CanvasName.Map).gameObject.activeSelf)
                        SetActiveCanvas(CanvasName.Map, false);
                    GameManager.Instance.Pause(true);
                    break;
                case CanvasName.Inventory:
                    if (idx == -1) break;
                    _dropReward.gameObject.SetActive(true);
                    break;
            }
        }

    }

    public RectTransform Canvas(CanvasName canvasName)
    {
        return _canvasDict[(int)canvasName];
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
            case 7:
                _statusText[statusIdx].text = "Coin: " + amount.ToString();
                break;
            case 8:
                _statusText[statusIdx].text = "Goods: " + amount.ToString();
                break;
        }
    }

    //public void MoveMap()
    //{
    //    SetActiveCanvas(CanvasName.CardReward, false);
    //    SetActiveCanvas(CanvasName.ItemReward, false);
    //    SetActiveCanvas(CanvasName.Shop, false);
    //}
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
        _cardRewardContent.GetChild(_cardRewardContent.childCount - 1).gameObject.SetActive(isOn);
    }
    public void ChangeRewardItemCount(bool isOn)
    {
        _addRewardItemCount = isOn;
        _itemRewardContent.GetChild(_itemRewardContent.childCount - 1).gameObject.SetActive(isOn);
    }

    public void SetViewDeck(List<Card> deck)                // 풀링이지만, Release 개념이 아닌, 활성화 비활성화로 진행됨. Release는 인덱스로 넣는데, Get은 Release된 것 중에서 마지막에 넣었던 것을 꺼내오기 때문에 생긴 문제
    {
        _viewDeckContent.anchoredPosition = new Vector3(_viewDeckContent.anchoredPosition.x, 0);
        if (deck.Count > _deckUICards.Count)
        {
            int deckUICardCount = _deckUICards.Count;
            for (int i = 0; i < _lastViewDeckCount; ++i)
            {
                _deckUICards[i].Setup(deck[i].DefaultData);
            }
            for (int i = _lastViewDeckCount; i < _deckUICards.Count; ++i)
            {
                _deckUICards[i].gameObject.SetActive(true);
                _deckUICards[i].Setup(deck[i].DefaultData);
            }
            for (int i = deckUICardCount; i < deck.Count; ++i)
            {
                _deckUICards.Add(Instantiate(_uiCard, _viewDeckContent));
                _deckUICards[i].Setup(deck[i].DefaultData);
            }
        }
        else
        {
            if (deck.Count > _lastViewDeckCount)
            {
                for (int i = 0; i < _lastViewDeckCount; ++i)
                {
                    _deckUICards[i].Setup(deck[i].DefaultData);
                }
                for (int i = _lastViewDeckCount; i < deck.Count; ++i)
                {
                    _deckUICards[i].gameObject.SetActive(true);
                    _deckUICards[i].Setup(deck[i].DefaultData);
                }
            }
            else
            {
                for (int i = 0; i < deck.Count; ++i)
                {
                    _deckUICards[i].Setup(deck[i].DefaultData);
                }
                for (int i = deck.Count; i < _lastViewDeckCount; ++i)
                {
                    _deckUICards[i].gameObject.SetActive(false);
                }
            }
        }
        _lastViewDeckCount = deck.Count;
        SetActiveCanvas(CanvasName.ViewDeck, true);
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
        if (MapManager.Instance.StopMove)
            return;
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
