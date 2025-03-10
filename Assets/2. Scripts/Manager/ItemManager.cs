using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public Dictionary<int, bool> itemDict = new();      // 리스트로 하고, 중복 제거하는 형식으로도 가능할 듯

    [SerializeField] Item _passiveItemPrefab;
    List<Item> _passiveItem = new();
    RectTransform _passiveTransform;

    ActiveItem _activeItem;
    TMP_Text _activeItemCharge;
    public bool HaveActiveItem;

    PotionItem[] _potionItem = new PotionItem[4];
    public bool[] HavePotionItem = new bool[4];

    int _clickedPotionIdx;

    int _arrowIdx;

    //public bool arrowOn;   // 게임매니저에서 한 번에 처리하고 싶었으나, Card 사용 코드 때문에 그냥 각각의 코드에서 실행하는 방법 사용. => 성공함.
    public string ItemDesc;

    //[SerializeField]
    //Transform itemRewardContent;
    //[SerializeField]
    //Button activeItemBtn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _passiveTransform = (RectTransform)UIManager.Instance.ContinueFindChildByName(UIManager.Instance.PassiveTransform, "PassiveContent");
        _activeItemCharge = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.ActiveTransform, "Skill(NIY)").GetComponent<TMP_Text>();      // 이미지로 변경해야 함.

        _activeItem = UIManager.Instance.ActiveTransform.GetComponent<ActiveItem>();
        _potionItem = UIManager.Instance.PotionTransform.GetComponentsInChildren<PotionItem>();
        ButtonManager.Instance.TurnPassiveButton[0].onClick.AddListener(() =>
        {
            _passiveTransform.offsetMin = new Vector2(_passiveTransform.offsetMin.x + 1600 > 0 ? 0 : _passiveTransform.offsetMin.x + 1600, _passiveTransform.offsetMin.y);
        });
        ButtonManager.Instance.TurnPassiveButton[1].onClick.AddListener(() =>
        {
            _passiveTransform.offsetMin = new Vector2(_passiveTransform.offsetMin.x - 1600 < -1600 * (int)(_passiveItem.Count * 0.05f) ? -1600 * (int)(_passiveItem.Count * 0.05f) : _passiveTransform.offsetMin.x - 1600, _passiveTransform.offsetMin.y);
        });

        ButtonManager.Instance.ActiveItemButton.onClick.AddListener(() =>
        {
            if (_activeItem.Data.ItemCanUse == ItemCanUse.Anytime)
            {
                if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                {
                    UseActiveItem();
                }
            }
            else if (_activeItem.Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.MyTurn)      // Arrow Cursor 쓰는 것들을 의미함.
            {
                if (_activeItem.Data.AttackType == AttackType.None)
                {
                    if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                    {
                        UseActiveItem();
                    }
                }
                else if (_activeItem.Data.AttackType == AttackType.Multi)
                {
                    if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                    {
                        //_activeItem.CheckEnemyDead();
                        UseActiveItem();
                    }
                }
                else if (_activeItem.Data.AttackType == AttackType.Single)
                {
                    if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                    {
                        _arrowIdx = 1;
                        BattleManager.Instance.SetActiveArrowCursor(true, _arrowIdx);
                    }
                }
            }
        });

        for (int idx = 0; idx < HavePotionItem.Length; ++idx)
        {
            int i = idx;
            ButtonManager.Instance.PotionButtons[i].onClick.AddListener(() =>
            {
                _clickedPotionIdx = i;
                if (!HavePotionItem[_clickedPotionIdx]) { return; }
                if (_potionItem[i].Data.ItemCanUse == ItemCanUse.Anytime)
                {
                    UsePotionItem();
                }
                else if (_potionItem[i].Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.MyTurn)      // Arrow Cursor가 쓰이거나 전투 관련을 의미함.
                {
                    if (_potionItem[i].Data.AttackType == AttackType.Single)
                    {
                        _arrowIdx = 2;
                        BattleManager.Instance.SetActiveArrowCursor(true, _arrowIdx);
                    }
                    else
                    {
                        UsePotionItem();
                    }
                }
                _potionItem[i].DescWindowOff();
                //ButtonManager.Instance.PotionButtons[i].onClick.RemoveAllListeners();
            });
        }

        //print(_potionItem[0].Data == null);
    }

    // 여기도 SO 이용해서 Dict 만들고 정보 가져올 듯?

    //public void SettingItem(ItemData[] reward)
    //{
    //    UIManager.Instance.ShowRewardItem(reward);
    //    //for (int i = 0; i < reward.Length; ++i)
    //    //{
    //    //    int itemIdx = i;      // 값을 미리 저장하지 않으면 에러가 뜸.
    //    //    itemRewardContent.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>  // 캐싱할 거임.
    //    //    {
    //    //        GetItem(/*InGameManager.Instance.FindItemData(itemIdx)*/reward[itemIdx]);
    //    //        InGameManager.Instance.ReturnRandomItem();
    //    //    });
    //    //}
    //}
    //public void SettingPotion(int[] reward)       // 아직 포션 얻는 곳 안 정함.
    //{
    //    for (int i = 0; i < reward.Length; ++i)
    //    {
    //        int itemIdx = reward[i];      // 값을 미리 저장하지 않으면 에러가 뜸.
    //        //itemRewardContent.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
    //        //{
    //        //    print(itemIdx);
    //        //    GetPotion(itemIdx);
    //        //    InGameManager.Instance.ReturnRandomItem();
    //        //});
    //    }
    //}
    //public void GetPotion(int id)
    //{
    //    for (int i = 0; i < _boolPotion.Length; ++i)
    //    {
    //        if (!_boolPotion[i])
    //        {
    //            //potionBtns[i].onClick.RemoveAllListeners();
    //            ButtonManager.Instance.PotionButtons[i].onClick.AddListener(() =>
    //            {
    //                _clickedPotionIdx = i;
    //            });
    //            _boolPotion[i] = true;
    //            return;
    //        }
    //    }
    //}

    public (ItemData, int)? GetItem(ItemData itemData, int value = -1)
    {
        (ItemData, int)? changedItem = null;
        //Item item = new();
        //item.Setup(InGameManager.Instance.FindItemData(id));
        switch (itemData.ItemTag)
        {
            case ItemTag.Passive:
                GetPassiveItem(itemData);
                break;
            case ItemTag.Active:
                changedItem = ChanageActiveItem(itemData, value);
                break;
            case ItemTag.Potion:
                ChangePotionItem(itemData);
                break;
        }
        itemDict[itemData.ID] = true;

        return changedItem;
    }
    //public void GetItem(ItemData itemData)
    //{
    //    item.Setup
    //}

    //public void ItemAbility(int id)
    //{
    //    switch (id)
    //    {
    //        case 0:
    //            TurnManager.Instance.AddStartCardCount(1);
    //            break;
    //        case 1:
    //            InGameManager.Instance.player.AddMaxHealth(25);
    //            break;
    //        case 2:
    //            InGameManager.Instance.player.AddMaxHolo(1);
    //            break;
    //        case 3:
    //            UIManager.Instance.ChangeRewardCardCount(true);
    //            break;
    //        case 4:
    //            //_activeItem = 나중에 itemSO에서 가져올 것. 그럼 밑에 코드도 필요없음.
    //            _activeItem = new()
    //            {
    //                Damage = 0,
    //                maxCharge = 6,
    //                curCharge = 6
    //            };
    //            _activeItemCharge.text = _activeItem.Data.CurCharge.ToString();
    //            //_activeItem.curCharge = _activeItem.maxCharge;
    //            ButtonManager.Instance.ActiveItemButton.onClick.RemoveAllListeners();
    //            ButtonManager.Instance.ActiveItemButton.onClick.AddListener(() =>
    //            {
    //                if (_activeItem.Data.CurCharge >= _activeItem.Data.MaxCharge)
    //                {
    //                    InGameManager.Instance.player.Heal(10);
    //                    Charge(-_activeItem.Data.CurCharge);
    //                }
    //            });
    //            break;
    //        case 5:
    //            //_activeItem = 나중에 itemSO에서 가져올 것. 그럼 밑에 코드도 필요없음.
    //            _activeItem = new()
    //            {
    //                Damage = 2,
    //                maxCharge = 2,
    //                curCharge = 2
    //            };
    //            _activeItemCharge.text = _activeItem.Data.CurCharge.ToString();
    //            //_activeItem.curCharge = _activeItem.maxCharge;
    //            ButtonManager.Instance.ActiveItemButton.onClick.RemoveAllListeners();
    //            ButtonManager.Instance.ActiveItemButton.onClick.AddListener(() =>
    //            {
    //                if (TurnManager.Instance.MyTurn && _activeItem.curCharge >= _activeItem.maxCharge)
    //                {
    //                    BattleManager.Instance.SetActiveArrowCursor(true, 1);
    //                    //SettingSingleTarget(true, 10);
    //                }
    //            });
    //            break;
    //        default:
    //            break;
    //    }
    //}
    //public void BreakItem(int id)
    //{
    //    switch (id)
    //    {
    //        case 0:
    //            TurnManager.Instance.AddStartCardCount(-1);
    //            break;
    //        case 1:
    //            InGameManager.Instance.player.AddMaxHealth(-25);
    //            break;
    //        case 2:
    //            InGameManager.Instance.player.AddMaxHolo(-1);
    //            break;
    //        case 3:
    //            UIManager.Instance.ChangeRewardCardCount(false);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public Vector2 SettingArrowPos(int idx)
    {
        switch (idx)
        {
            case 1:
                return new Vector2(ButtonManager.Instance.ActiveItemButton.transform.position.x, ButtonManager.Instance.ActiveItemButton.transform.position.y - 0.4f);
            case 2:
                return new Vector2(ButtonManager.Instance.PotionButtons[_clickedPotionIdx].transform.position.x, ButtonManager.Instance.PotionButtons[_clickedPotionIdx].transform.position.y - 0.4f);
        }
        return Vector2.zero;
    }
    //public void TurnPassiveItemPage(int idx)
    //{
    //    switch (idx)
    //    {
    //        case 0:
    //            _passiveTransform.position = new Vector2(_passiveTransform.position.x - 1600 < 0 ? 0 : _passiveTransform.position.x - 1600, 0);
    //            break;
    //        case 1:
    //            _passiveTransform.position = new Vector2(_passiveTransform.position.x + 1600 > 1600 * (int)(_passiveItem.Count * 0.05f) ? 1600 * (int)(_passiveItem.Count * 0.05f) : _passiveTransform.position.x + 1600, 0);
    //            break;
    //    }
    //}
    public void GetPassiveItem(ItemData itemData)
    {
        Item passiveItem = Instantiate(_passiveItemPrefab, _passiveTransform);
        passiveItem.Setup(itemData);
        _passiveItem.Add(passiveItem);
        if (_passiveItem.Count > 20 && !ButtonManager.Instance.TurnPassiveButton[0].gameObject.activeSelf)
        {
            ButtonManager.Instance.SetActiveTurnPassiveBtn(true);
        }
    }
    public (ItemData, int)? ChanageActiveItem(ItemData itemData, int value)       // 아이템 체인지하는 코드 추가해야 함.
    {
        (ItemData, int)? changedItem = null;
        if (HaveActiveItem)
        {
            changedItem = (_activeItem.Data, _activeItem.CurCharge);
            _activeItem.Setup(itemData, value);
            //MapManager.Instance.ChangedUseItem(data, curCharge);
            //changed = true;
        }
        else
        {
            _activeItem.Setup(itemData, value);
        }
        //ButtonManager.Instance.ActiveItemButton.onClick.RemoveAllListeners();
        _activeItemCharge.text = _activeItem.CurCharge.ToString();
        HaveActiveItem = true;
        //ButtonManager.Instance.ActiveItemButton.onClick.AddListener(() =>
        //{
        //    if (_activeItem.Data.ItemCanUse == ItemCanUse.Anytime)
        //    {
        //        if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
        //        {
        //            UseActiveItem();
        //        }
        //    }
        //    else if (_activeItem.Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.MyTurn)      // Arrow Cursor 쓰는 것들을 의미함.
        //    {
        //        if (_activeItem.Data.AttackType == AttackType.None)
        //        {
        //            if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
        //            {
        //                UseActiveItem();
        //            }
        //        }
        //        else if (_activeItem.Data.AttackType == AttackType.Multi)
        //        {
        //            if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
        //            {
        //                //_activeItem.CheckEnemyDead();
        //                UseActiveItem();
        //            }
        //        }
        //        else if (_activeItem.Data.AttackType == AttackType.Single)
        //        {
        //            if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
        //            {
        //                _arrowIdx = 1;
        //                BattleManager.Instance.SetActiveArrowCursor(true, _arrowIdx);
        //            }
        //        }
        //    }
        //});
        return changedItem;
    }

    public void ChangePotionItem(ItemData itemData)        // 다 차있으면 바꾸는 코드 필요
    {
        for (int i = 0; i < HavePotionItem.Length; ++i)
        {
            if (!HavePotionItem[i])
            {
                HavePotionItem[i] = true;

                //ButtonManager.Instance.PotionButtons[i].onClick.RemoveAllListeners();
                _potionItem[i].Setup(itemData);
                _potionItem[i].BtnIdx = i;
                //ButtonManager.Instance.PotionButtons[i].onClick.AddListener(() =>
                //{
                //    _clickedPotionIdx = i;
                //    if (!HavePotionItem[_clickedPotionIdx]) { return; }
                //    if (_potionItem[i].Data.ItemCanUse == ItemCanUse.Anytime)
                //    {
                //        UsePotionItem();
                //    }
                //    else if (_potionItem[i].Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.MyTurn)      // Arrow Cursor가 쓰이거나 전투 관련을 의미함.
                //    {
                //        if (_potionItem[i].Data.AttackType == AttackType.Single)
                //        {
                //            _arrowIdx = 2;
                //            BattleManager.Instance.SetActiveArrowCursor(true, _arrowIdx);
                //        }
                //        else
                //        {
                //            UsePotionItem();
                //        }
                //    }
                //    _potionItem[i].DescWindowOff();
                //    //ButtonManager.Instance.PotionButtons[i].onClick.RemoveAllListeners();
                //});
                break;
            }
        }
        // 포션을 교체하는 코드
    }

    public void UseActiveItem()
    {
        InGameManager.Instance.AbilityEventQueue.Enqueue(_activeItem);
        Charge(-_activeItem.Data.MaxCharge);
    }

    public void UsePotionItem()
    {
        InGameManager.Instance.AbilityEventQueue.Enqueue(_potionItem[_clickedPotionIdx]);
        HavePotionItem[_clickedPotionIdx] = false;
    }

    public void Charge(int value)
    {
        if (!HaveActiveItem) return;

        _activeItem.CurCharge = Mathf.Clamp(_activeItem.CurCharge + value, 0, _activeItem.Data.MaxCharge);

        _activeItemCharge.text = _activeItem.CurCharge.ToString();
    }

    //public void SettingSingleTarget(bool arrow, int value = 0)
    //{
    //    arrowOn = arrow;
    //    activeValue = value;
    //    InGameManager.Instance.SetActiveArrowCursor(arrow);
    //}

    public void AttackSingleTarget(Enemy enemy)     // 아이템 비사용시, 끄는 방법이 필요함. + 다른 것들 터치 안 되도록 설정
    {
        switch (_arrowIdx)
        {
            case 1:
                if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                {
                    _activeItem.Target(enemy);
                    _activeItem.CheckEnemyDead();
                    //enemy.CheckIfDead(_activeItem.Data.Damage, 1);      // CheckEnemyDead 이걸로 바꿔야 함.
                    UseActiveItem();
                }
                break;
            case 2:
                _potionItem[_clickedPotionIdx].Target(enemy);
                _potionItem[_clickedPotionIdx].CheckEnemyDead();
                UsePotionItem();
                break;
        }
    }
}
