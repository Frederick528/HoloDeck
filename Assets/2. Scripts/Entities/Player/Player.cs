using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;

public class Player : Entity
{
    public static Player Instance { get; private set; }
    public int MaxHolo { get; private set; }
    public int CurHolo { get; private set; }

    public ReactiveProperty<int> Coin { get; private set; } = new();
    public ReactiveProperty<int> AttackPower { get; private set; } = new();
    public ReactiveProperty<int> DefencePower { get; private set; } = new();

    readonly int _battleAnimBool = Animator.StringToHash("Battle");

    int _attackPower;
    int _defencePower;
    //[SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }
    void Start()
    {
        PlayerSubScribe();
        SetupPlayer(80, 3);
    }

    void PlayerSubScribe()
    {
        EntitySubScribe();
        maxHp.Subscribe(hp => UIManager.Instance.SetHealth(curHp.Value, maxHp.Value));
        curHp.Subscribe(hp => UIManager.Instance.SetHealth(curHp.Value, maxHp.Value));

        Coin.Subscribe(coin =>
        {
            UIManager.Instance.SetCoin(coin);
        });

        AttackPower.Subscribe(attackPower =>
        {
            CardManager.Instance.ChangeTotalCardDesc();
        });

        DefencePower.Subscribe(defencePower =>
        {
            CardManager.Instance.ChangeTotalCardDesc();
        });
    }

    void SetupPlayer(int hp, int startHoloValue)
    {
        SetupEntity(hp);
        MaxHolo = startHoloValue;
        CurHolo = MaxHolo;
        UIManager.Instance.SetHolo(CurHolo, MaxHolo);
    }
    public async UniTaskVoid TakeDamagePlayer(int dmg)
    {
        if (!TakeDamage(dmg))
            return;
        TurnManager.Instance.EndBattle().Forget();
        await base.DieAnimation();
        print("플레이어가 죽었습니다.");
        UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.GameOver, true);
        //return true;
    }

    public void AddCurHolo(int chargeOrUse)
    {
        CurHolo = Mathf.Clamp(CurHolo + chargeOrUse, 0, MaxHolo);
        UIManager.Instance.SetHolo(CurHolo, MaxHolo);
    }

    public void AddMaxHealth(int value)
    {
        maxHp.Value += value;
        Heal(value).Forget();
    }

    public void AddMaxHolo(int value)
    {
        MaxHolo += value;
        if (MaxHolo < 0)
            MaxHolo = 0;
        //UIManager.Instance.SetHolo(CurHolo, MaxHolo);
    }
    public void AddAttackPower(int value)
    {
        AttackPower.Value += value;
    }
    public void AddDefencePower(int value)
    {
        DefencePower.Value += value;
    }

    public void StartOrEndBattle(bool isBattleStart)
    {
        animator.SetBool(_battleAnimBool, isBattleStart);
    }
}
