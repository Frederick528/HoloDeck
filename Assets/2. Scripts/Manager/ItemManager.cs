using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public Dictionary<int, bool> itemDict = new();      // 리스트로 하고, 중복 제거하는 형식으로도 가능할 듯

    Item _activeItem;
    TMP_Text _activeItemCharge;

    //public bool arrowOn;   // 게임매니저에서 한 번에 처리하고 싶었으나, Card 사용 코드 때문에 그냥 각각의 코드에서 실행하는 방법 사용. => 성공함.
    public string ItemDesc;

    [SerializeField]
    Transform itemRewardContent;
    [SerializeField]
    //Button activeItemBtn;

    void Awake() { Instance = this; }

    private void Start()
    {
        _activeItemCharge = UiManager.Instance.ContinueFindChildByName(UiManager.Instance.Canvas(UiManager.CanvasName.InGame), "Skill(NIY)").GetComponent<TMP_Text>();
    }

    // 여기도 SO 이용해서 Dict 만들고 정보 가져올 듯?

    public void SettingItem(int[] reward)
    {
        for (int i = 0; i < reward.Length; ++i)
        {
            int _item = reward[i];      // 값을 미리 저장하지 않으면 에러가 뜸.
            itemRewardContent.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
            {
                print(_item);
                GetItem(_item);
                InGameManager.Instance.ReturnRandomItem();
            });
        }
    }

    public void GetItem(int id)
    {
        Item item = new();
        item.Setup(InGameManager.Instance.FindItemData(id));
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
    //            UiManager.Instance.ChangeRewardCardCount(true);
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
    //            UiManager.Instance.ChangeRewardCardCount(false);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public void ChanageActiveItem(Item item)
    {
        _activeItem = item;
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
                else if (_activeItem.CurCharge >= _activeItem.Data.MaxCharge)
                {
                    BattleManager.Instance.SetActiveArrowCursor(true, 1);
                }
            }
        });
    }

    public void UseActiveItem()
    {
        InGameManager.Instance.AbilityEventQueue.Enqueue(_activeItem);
        Charge(-_activeItem.CurCharge);
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
            _activeItem.Target(EnemyManager.Instance.TargetEnemy);
            enemy.CheckIfDead(_activeItem.Data.Damage, 1);      // CheckEnemyDead 이걸로 바꿔야 함.
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
