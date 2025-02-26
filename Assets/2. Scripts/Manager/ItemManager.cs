using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public Dictionary<int, bool> itemDict = new();      // 리스트로 하고, 중복 제거하는 형식으로도 가능할 듯

    List<Item> _passiveItem = new();

    Item _activeItem;
    TMP_Text _activeItemCharge;

    Item[] _potionItem = new Item[4];
    bool[] _boolPotion = new bool[4];
    int _clickedPotionIdx;


    //public bool arrowOn;   // 게임매니저에서 한 번에 처리하고 싶었으나, Card 사용 코드 때문에 그냥 각각의 코드에서 실행하는 방법 사용. => 성공함.
    public string ItemDesc;

    [SerializeField]
    Transform itemRewardContent;
    [SerializeField]
    //Button activeItemBtn;

    void Awake() { Instance = this; }

    private void Start()
    {
        _activeItemCharge = UIManager.Instance.ContinueFindChildByName(UIManager.Instance.Canvas(UIManager.CanvasName.InGame), "Skill(NIY)").GetComponent<TMP_Text>();

        _activeItem = UIManager.Instance.ActiveTransform.GetComponent<Item>();
        _potionItem = UIManager.Instance.PotionTransform.GetComponentsInChildren<Item>();
    }

    // 여기도 SO 이용해서 Dict 만들고 정보 가져올 듯?

    public void SettingItem(int[] reward)
    {
        for (int i = 0; i < reward.Length; ++i)
        {
            int itemIdx = reward[i];      // 값을 미리 저장하지 않으면 에러가 뜸.
            itemRewardContent.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>  // 캐싱할 거임.
            {
                print(itemIdx);
                GetItem(itemIdx);
                InGameManager.Instance.ReturnRandomItem();
            });
        }
    }
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

    public void GetItem(int id)
    {
        //Item item = new();
        //item.Setup(InGameManager.Instance.FindItemData(id));
        if (id > 1000)
        {
            ChangePotionItem(id);
        }
        else if (id > 500)
        {
            ChanageActiveItem(id);
        }
        else if (id > 0)
        {
            _potionItem[3].Setup(InGameManager.Instance.FindItemData(id));      // 효과 적용되는 지만 확인용
        }
        itemDict[id] = true;
        //MapManager.Instance.GetReward();
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

    public void ChanageActiveItem(int id)       // 아이템 체인지하는 코드 추가해야 함.
    {
        _activeItem.Setup(InGameManager.Instance.FindItemData(id));
        _activeItemCharge.text = _activeItem.CurCharge.ToString();
        ButtonManager.Instance.ActiveItemButton.onClick.RemoveAllListeners();
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
                        BattleManager.Instance.SetActiveArrowCursor(true, 1);
                    }
                }
            }
        });
    }

    public void ChangePotionItem(int id)        // 다 차있으면 바꾸는 코드 필요
    {
        for (int i = 0; i < _boolPotion.Length; ++i)        // 칸 다 차있으면 막는 코드 필요
        {
            if (!_boolPotion[i])
            {

                //potionBtns[i].onClick.RemoveAllListeners();
                _potionItem[i].Setup(InGameManager.Instance.FindItemData(id));
                ButtonManager.Instance.PotionButtons[i].onClick.AddListener(() =>
                {
                    _clickedPotionIdx = i;
                    if (_potionItem[i].Data.ItemCanUse == ItemCanUse.Anytime)
                    {
                        UsePotionItem();
                    }
                    else if (_potionItem[i].Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.MyTurn)      // Arrow Cursor가 쓰이거나 전투 관련을 의미함.
                    {
                        if (_potionItem[i].Data.AttackType == AttackType.Single)
                        {
                            BattleManager.Instance.SetActiveArrowCursor(true, 2);
                        }
                        else
                        {
                            UsePotionItem();
                        }
                    }
                    _potionItem[i].DescWindowOff();
                    ButtonManager.Instance.PotionButtons[i].onClick.RemoveAllListeners();
                });
                _boolPotion[i] = true;
                return;
            }
        }
    }

    public void UseActiveItem()
    {
        InGameManager.Instance.AbilityEventQueue.Enqueue(_activeItem);
        Charge(-_activeItem.CurCharge);
    }

    public void UsePotionItem()
    {
        InGameManager.Instance.AbilityEventQueue.Enqueue(_potionItem[_clickedPotionIdx]);
        ButtonManager.Instance.PotionButtons[_clickedPotionIdx].onClick.RemoveAllListeners();
        _boolPotion[_clickedPotionIdx] = false;
    }

    public void Charge(int value)
    {
        if (_activeItem == null) return;

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
        if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
        {
            _activeItem.Target(enemy);
            _activeItem.CheckEnemyDead();
            //enemy.CheckIfDead(_activeItem.Data.Damage, 1);      // CheckEnemyDead 이걸로 바꿔야 함.
            UseActiveItem();
        }
        //if (!arrowOn) return false;
        //_activeItem.Target(EnemyManager.Instance.TargetEnemy);
        //enemy.CheckIfDead(_activeItem.Data.Damage, 1);      // CheckEnemyDead 이걸로 바꿔야 함.
        //UseActiveItem();
        //InGameManager.Instance.AbilityEventQueue.Enqueue(_activeItem);
        //enemy.TakeDamageEnemy(_activeItem.Damage).Forget();            // 일단 forget했는데, 상황에 따라 달라짐

        //Charge(-_activeItem.maxCharge);

        //SettingSingleTarget(false);
        //BattleManager.Instance.SetActiveArrowCursor(false, 1);

        //return true;
    }
}
