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

    [SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update
    void Start()
    {
        PlayerSubScribe();
        SetupPlayer(80, 3);
    }

    void PlayerSubScribe()
    {
        EntitySubScribe();
        maxHp.Subscribe(hp => UiManager.instance.SetHealth(curHp.Value, maxHp.Value));
        curHp.Subscribe(hp => UiManager.instance.SetHealth(curHp.Value, maxHp.Value));

        Coin.Subscribe(coin =>
        {
            UiManager.instance.SetCoin(coin);
        });
    }

    void SetupPlayer(int hp, int startHoloValue)
    {
        SetupEntity(hp);
        MaxHolo = startHoloValue;
        CurHolo = MaxHolo;
        holoValue.text = $"{CurHolo} / {MaxHolo}";
    }
    public async UniTask<bool> TakeDamagePlayer(int dmg)
    {
        if (!TakeDamage(dmg))
            return false;
        print("플레이어가 죽었습니다.");
        await base.DieAnimation();
        return true;
    }

    public void ChangeHoloValue(int chargeOrUse)
    {
        CurHolo = Mathf.Clamp(CurHolo + chargeOrUse, 0, MaxHolo);
        holoValue.text = $"{CurHolo} / {MaxHolo}";
    }
}
