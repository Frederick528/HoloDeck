using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;

public class Player : Entity
{
    //public static Player Instance { get; private set; }
    public int MaxHolo { get; private set; }
    public int CurHolo { get; private set; }

    public ReactiveProperty<int> Coin { get; private set; } = new();
    public ReactiveProperty<int> AttackPower { get; private set; } = new();
    public ReactiveProperty<int> DefensePower { get; private set; } = new();
    public ReactiveProperty<int> HealPower { get; private set; } = new();

    readonly int _battleAnimBool = Animator.StringToHash("Battle");

    bool _resurrection = false;
    //public int _attackPower;
    //public int _defensePower;
    //public int _healPower;

    //[SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
    //        //DontDestroyOnLoad(transform.root.gameObject);
    //    }
    //    else
    //    {
    //        Destroy(transform.root.gameObject);
    //    }
    //}
    void Start()
    {
        PlayerSubScribe();
        SetupPlayer(10, 10, 150);
    }

    void PlayerSubScribe()
    {
        EntitySubScribe();
        _maxHP.Subscribe(maxHP => UIManager.Instance.ChangeStatus(0, _curHP.Value, maxHP));
        _curHP.Subscribe(curHP => UIManager.Instance.ChangeStatus(0, curHP, _maxHP.Value));

        Coin.Subscribe(coin =>
        {
            UIManager.Instance.SetCoin(coin);
        });

        AttackPower.Subscribe(atk =>
        {
            UIManager.Instance.ChangeStatus(1, atk);
            CardManager.Instance.ChangeTotalCardDesc();
        });

        DefensePower.Subscribe(def =>
        {
            UIManager.Instance.ChangeStatus(2, def);
            CardManager.Instance.ChangeTotalCardDesc();
        });

        HealPower.Subscribe(heal =>
        {
            UIManager.Instance.ChangeStatus(3, heal);
            CardManager.Instance.ChangeTotalCardDesc();
        });

        _criticalChance.Subscribe(criChance => UIManager.Instance.ChangeStatus(4, criChance));

        _criticalDamage.Subscribe(criDamage => UIManager.Instance.ChangeStatus(5, criDamage));

        _useCritical.Subscribe(useCri => UIManager.Instance.ChangeStatus(6, _curCritical.Value, useCri));

        _curCritical.Subscribe(curCri => UIManager.Instance.ChangeStatus(6, curCri, _useCritical.Value));

    }

    void SetupPlayer(int hp, int criticalChance = 10, int criticalDamage = 150)
    {
        SetupEntity(hp + GameManager.Instance.AddMaxHP, criticalChance + GameManager.Instance.AddCriticalChance, criticalDamage + GameManager.Instance.AddCriticalDamage);
        MaxHolo = 3;
        CurHolo = MaxHolo;
        UIManager.Instance.SetHolo(CurHolo, MaxHolo);

        AddAttackPower(GameManager.Instance.AddAttackPower);
        AddDefencePower(GameManager.Instance.AddDefensePower);
        AddHealPower(GameManager.Instance.AddHealPower);
    }
    public async UniTaskVoid TakeDamagePlayer(int dmg)
    {
        if (!TakeDamage(dmg))
            return;
        if (GameManager.Instance.Resurrection && !_resurrection)
        {
            _curHP.Value = (int)(_maxHP.Value * 0.5f);
            _resurrection = true;
            return;
        }
        canvas.gameObject.SetActive(false);
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
        _maxHP.Value += value;
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
        DefensePower.Value += value;
    }
    public void AddHealPower(int value)
    {
        HealPower.Value += value;
    }

    public void StartOrEndBattle(bool isBattleStart)
    {
        animator.SetBool(_battleAnimBool, isBattleStart);
    }
}
