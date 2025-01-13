using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public Dictionary<int, bool> itemDict = new();      // 리스트로 하고, 중복 제거하는 형식으로도 가능할 듯

    public ActiveItem activeItem;
    public TMP_Text activeItemCharge;

    public bool arrowOn;   // 게임매니저에서 한 번에 처리하고 싶었으나, Card 사용 코드 때문에 그냥 각각의 코드에서 실행하는 방법 사용.
    int activeValue;

    [SerializeField]
    Transform itemRewardContent;
    [SerializeField]
    Button activeItemBtn;

    void Awake() { Instance = this; }

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
            });
        }
    }

    public void GetItem(int id)
    {
        itemDict[id] = true;
        ItemAbility(id);
        MapManager.Instance.currStage.rewarded = true;
        MapManager.Instance.currStage.TreasureBox();
    }

    public void ItemAbility(int id)
    {
        switch (id)
        {
            case 0:
                TurnManager.Instance.AddStartCardCount(1);
                break;
            case 1:
                GameManager.Instance.player.ChangeHealth(25);
                break;
            case 2:
                GameManager.Instance.player.ChangeHolo(1);
                break;
            case 3:
                CardManager.Instance.cardRewardContent.Find("Card4").gameObject.SetActive(true);
                break;
            case 4:
                //activeItem = 나중에 itemSO에서 가져올 것. 그럼 밑에 코드도 필요없음.
                activeItem = new()
                {
                    maxCharge = 6,
                    curCharge = 6
                };
                activeItemCharge.text = activeItem.curCharge.ToString();
                //activeItem.curCharge = activeItem.maxCharge;
                activeItemBtn.onClick.RemoveAllListeners();
                activeItemBtn.onClick.AddListener(() =>
                {
                    if (activeItem.curCharge >= activeItem.maxCharge)
                    {
                        GameManager.Instance.player.Heal(10);
                        Charge(-activeItem.maxCharge);
                    }
                });
                break;
            case 5:
                //activeItem = 나중에 itemSO에서 가져올 것. 그럼 밑에 코드도 필요없음.
                activeItem = new()
                {
                    maxCharge = 2,
                    curCharge = 2
                };
                activeItemCharge.text = activeItem.curCharge.ToString();
                //activeItem.curCharge = activeItem.maxCharge;
                activeItemBtn.onClick.RemoveAllListeners();
                activeItemBtn.onClick.AddListener(() =>
                {
                    if (TurnManager.Instance.myTurn && activeItem.curCharge >= activeItem.maxCharge)
                    {
                        SettingSingleTarget(true, 10);
                    }
                });
                break;
            default:
                break;
        }
    }
    public void BreakItem(int id)
    {
        switch (id)
        {
            case 0:
                TurnManager.Instance.AddStartCardCount(-1);
                break;
            case 1:
                GameManager.Instance.player.ChangeHealth(-25);
                break;
            case 2:
                GameManager.Instance.player.ChangeHolo(-1);
                break;
            case 3:
                CardManager.Instance.cardRewardContent.Find("Card4").gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void Charge(int value)
    {
        if (activeItem == null) return;

        activeItem.curCharge = Mathf.Clamp(activeItem.curCharge + value, 0, activeItem.maxCharge);

        activeItemCharge.text = activeItem.curCharge.ToString();
    }

    public void SettingSingleTarget(bool arrow, int value = 0)
    {
        arrowOn = arrow;
        activeValue = value;
        GameManager.Instance.ArrowCursor(arrow);
    }

    public bool AttackSingleTarget(Enemy enemy)     // 아이템 비사용시, 끄는 방법이 필요함. + 다른 것들 터치 안 되도록 설정
    {
        if (!arrowOn) return false;

        enemy.TakeDamageEnemy(activeValue);

        Charge(-activeItem.maxCharge);

        SettingSingleTarget(false);

        return true;
    }
}
