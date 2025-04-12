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

    readonly int _battleAnimBool = Animator.StringToHash("Battle");

    int _attackPower;
    int _defensePower;
    //[SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        GameManager.Instance.AddDontDestroy(transform.root.gameObject);
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
        SetupPlayer(80, 30);
        UIManager.Instance.ChangeStatus(5, _criticalDamage.Value);      // 값이 변해야 UI에 적용되는데, 치뎀은 처음에 기본값을 그대로 사용하기 때문에 값이 변하지 않아 UI에 적용이 되지 않음. 따라서 따로 적용
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

        _criticalChance.Subscribe(criChance => UIManager.Instance.ChangeStatus(4, criChance));

        _criticalDamage.Subscribe(criDamage => UIManager.Instance.ChangeStatus(5, criDamage));

        _useCritical.Subscribe(useCri => UIManager.Instance.ChangeStatus(6, _curCritical.Value, useCri));

        _curCritical.Subscribe(curCri => UIManager.Instance.ChangeStatus(6, curCri, _useCritical.Value));

    }

    void SetupPlayer(int hp, int criticalChance = 10)
    {
        SetupEntity(hp, criticalChance);
        MaxHolo = 3;
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

    public void StartOrEndBattle(bool isBattleStart)
    {
        animator.SetBool(_battleAnimBool, isBattleStart);
    }
}
