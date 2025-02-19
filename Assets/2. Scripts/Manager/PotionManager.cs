using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionManager : MonoBehaviour
{
    [SerializeField]
    Transform potion;
    Button[] _potionBtns = new Button[4];
    Image[] _potionImgs = new Image[4];
    Color[] _colors = new Color[5] { Color.white, Color.red, Color.blue, Color.yellow, Color.green };
    bool[] _boolPotion = new bool[4];        // 나중에 Potion 스크립트로 변경될 수 있음.
    public int ClickedPotion;
    public static PotionManager Instance { get; private set; }
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < _potionBtns.Length; ++i)
        {
            _potionBtns[i] = potion.GetChild(i).GetComponent<Button>();
            _potionImgs[i] = potion.GetChild(i).GetComponent<Image>();
        }
    }

    public void GetPotion(int id)
    {
        for (int i = 0;i < _potionBtns.Length; ++i)
        {
            if (!_boolPotion[i])
            {
                _potionImgs[i].color = _colors[id + 1];
                //potionBtns[i].onClick.RemoveAllListeners();
                _potionBtns[i].onClick.AddListener(() =>
                {
                    ClickedPotion = i;
                    PotionAbility(i, id);
                });
                _boolPotion[i] = true;
                return;
            }
        }
    }

    public Vector2 ArrowPotionPos()
    {
        return new Vector2(_potionBtns[ClickedPotion].transform.position.x, _potionBtns[ClickedPotion].transform.position.y - 0.4f);
    }

    public void PotionAbility(int idx, int id)
    {
        switch (id)
        {
            case 0:
                InGameManager.Instance.player.Heal(10).Forget();
                _potionBtns[idx].onClick.RemoveAllListeners();
                _boolPotion[idx] = false;
                _potionImgs[idx].color = _colors[0];
                break;
            case 1:
                InGameManager.Instance.ChangeCoinValue(20);
                _potionBtns[idx].onClick.RemoveAllListeners();
                _boolPotion[idx] = false;
                _potionImgs[idx].color = _colors[0];
                break;
            case 2:
                if (TurnManager.Instance.MyTurn)
                {
                    InGameManager.Instance.player.Shield(15).Forget();
                    _potionBtns[idx].onClick.RemoveAllListeners();
                    _boolPotion[idx] = false;
                    _potionImgs[idx].color = _colors[0];
                }
                break;
            case 3:
                if (TurnManager.Instance.MyTurn)
                {
                    BattleManager.Instance.SetActiveArrowCursor(true, 2);
                    //PotionBtns[idx].onClick.RemoveAllListeners();
                    //boolPotion[idx] = false;
                }
                break;
        }
    }
}
