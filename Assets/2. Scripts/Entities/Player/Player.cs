using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;

public class Player : Entity
{
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
    void Start()
    {
        PlayerSubScribe();
        SetupPlayer(10, 3);
    }

    void PlayerSubScribe()
    {
        EntitySubScribe();
        maxHp.Subscribe(hp => UiManager.Instance.SetHealth(curHp.Value, maxHp.Value));
        curHp.Subscribe(hp => UiManager.Instance.SetHealth(curHp.Value, maxHp.Value));

        Coin.Subscribe(coin =>
        {
            UiManager.Instance.SetCoin(coin);
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
        UiManager.Instance.SetHolo(CurHolo, MaxHolo);
    }
    public async UniTask<bool> TakeDamagePlayer(int dmg)
    {
        if (!TakeDamage(dmg))
            return false;
        TurnManager.Instance.EndBattle().Forget();
        await base.DieAnimation();
        print("플레이어가 죽었습니다.");
        UiManager.Instance.SetActiveCanvas(UiManager.CanvasName.GameOver, true);
        return true;
    }

    public void ChangeHoloValue(int chargeOrUse)
    {
        CurHolo = Mathf.Clamp(CurHolo + chargeOrUse, 0, MaxHolo);
        UiManager.Instance.SetHolo(CurHolo, MaxHolo);
    }

    public void ChangeHealth(int value)
    {
        maxHp.Value += value;
        Heal(value);
    }

    public void ChangeHolo(int value)
    {
        MaxHolo += value;
        if (MaxHolo < 0)
            MaxHolo = 0;
    }
    public void ChangeAttackPower(int value)
    {
        AttackPower.Value += value;
    }
    public void ChangeDefencePower(int value)
    {
        DefencePower.Value += value;
    }

    public void StartOrEndBattle(bool isBattleStart)
    {
        animator.SetBool(_battleAnimBool, isBattleStart);
    }
}
