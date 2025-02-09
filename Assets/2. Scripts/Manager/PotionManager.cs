using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionManager : MonoBehaviour
{
    [SerializeField]
    Transform potion;
    Button[] potionBtns = new Button[3];
    bool[] boolPotion = new bool[3];        // 나중에 Potion 스크립트로 변경될 수 있음.
    public static PotionManager Instance { get; private set; }
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < potionBtns.Length; ++i)
        {
            potionBtns[i] = potion.GetChild(i).GetComponent<Button>();
        }
    }

    public void GetPotion(int id)
    {
        for (int i = 0;i < potionBtns.Length; ++i)
        {
            if (!boolPotion[i])
            {
                //potionBtns[i].onClick.RemoveAllListeners();
                potionBtns[i].onClick.AddListener(() =>
                {
                    PotionAbility(i, id);
                });
                boolPotion[i] = true;
                return;
            }
        }
    }

    public void PotionAbility(int idx, int id)
    {
        switch (id)
        {
            case 0:
                InGameManager.Instance.player.Heal(10);
                potionBtns[idx].onClick.RemoveAllListeners();
                boolPotion[idx] = false;
                break;
            case 1:
                InGameManager.Instance.ChangeCoinValue(20);
                potionBtns[idx].onClick.RemoveAllListeners();
                boolPotion[idx] = false;
                break;
            case 2:
                if (TurnManager.Instance.MyTurn)
                {
                    InGameManager.Instance.player.Shield(15).Forget();
                    potionBtns[idx].onClick.RemoveAllListeners();
                    boolPotion[idx] = false;
                }
                break;
        }
    }
}
