using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class ActiveItem : UseItem
{
    readonly int _maxCharge;
    readonly bool _startCharge;

    public int MaxCharge { get; private set;  }
    public int CurCharge { get; set; }

    //public int CurCharge;

    //public ActiveItem(int maxCharge, bool startCharge, int damage, int shield, int draw, int heal, int duration, AttackType attackType, ItemCanUse itemCanUse)
    //    : base(damage, shield, draw, heal, duration, attackType, itemCanUse)
    //{
    //    this._maxCharge = maxCharge;
    //    this._startCharge = startCharge;
    //}
    public void ActiveItemDataReset()
    {
        if (_damage != 0)
            Damage = _damage + InGameManager.Instance.Player.AttackPower.Value;
        if (_shield != 0)
            Shield = _shield + InGameManager.Instance.Player.DefensePower.Value;
        if (Damage < 0) { Damage = 0; }
        if (Shield < 0) { Shield = 0; }
        Draw = _draw;
        Heal = _heal;
        Duration = _duration;

        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", Damage.ToString());
        sb.Replace("{Shield}", Shield.ToString());
        sb.Replace("{Draw}", Draw.ToString());
        sb.Replace("{Heal}", Heal.ToString());
        sb.Replace("{Duration}", Duration.ToString());
        Desc = sb.ToString();
        AdjustBackgroundSize();
        //if (Data.ItemTag == ItemTag.Active)
        //{
        //    Data.Damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
        //    Data.Shield = _defaultData.Shield + InGameManager.Instance.player.DefensePower.Value;
        //    if (Data.Damage < 0) { Data.Damage = 0; }
        //    if (Data.Shield < 0) { Data.Shield = 0; }
        //    Data.Draw = _defaultData.Draw;
        //    Data.Heal = _defaultData.Heal;
        //    Data.Duration = _defaultData.Duration;

        //    StringBuilder sb = new StringBuilder(_defaultData.Descript);
        //    sb.Replace("{Damage}", Data.Damage.ToString());
        //    sb.Replace("{Shield}", Data.Shield.ToString());
        //    sb.Replace("{Draw}", Data.Draw.ToString());
        //    sb.Replace("{Heal}", Data.Heal.ToString());
        //    sb.Replace("{Duration}", Data.Duration.ToString());
        //    Desc = sb.ToString();
        //}
    }

    public void LoadCharge(int chargeValue)
    {
        if (chargeValue != -1)
        {
            CurCharge = Mathf.Clamp(chargeValue, 0, MaxCharge);
        }
    }

    public override void Setup<T>(T data)
    {
        //_defaultData = data;
        //Data = _defaultData;
        base.Setup(data);
        // 이거 체크하는 것도 따로 함수로 만들면 좋을 듯
        if (_defaultData is ChargeItemBase chargeItemData) {
            MaxCharge = chargeItemData.MaxCharge;
            if (chargeItemData.StartCharge) { CurCharge = MaxCharge; }
            else { CurCharge = 0; }
        }
        //if (value != -1) { CurCharge = value; }
        //else if (data.StartCharge) { CurCharge = MaxCharge; }
        //else if (!data.StartCharge) { CurCharge = 0; }

        //if (value != -1) { CurCharge = value; }
        //else if (Data.StartCharge) { CurCharge = Data.MaxCharge; }
        //else if (!Data.StartCharge) { CurCharge = 0; }

        ActiveItemDataReset();
        //StringBuilder sb = new StringBuilder(_defaultData.Descript);

        //int damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
        //int shield = _defaultData.Shield + InGameManager.Instance.player.DefensePower.Value;
        //if (damage < 0) { damage = 0; }
        //if (shield < 0) { shield = 0; }
        //sb.Replace("{Damage}", damage.ToString());
        //sb.Replace("{Shield}", shield.ToString());
        //sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        //sb.Replace("{Heal}", (_defaultData.Heal).ToString());
        //sb.Replace("{Duration}", (_defaultData.Duration).ToString());

        ////_defaultDesc = sb.ToString();
        //Desc = sb.ToString();

        _itemAbility.SetActiveItemAbility(this);

        AdjustBackgroundSize();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (_defaultData == null)
            return;
        base.OnPointerEnter(eventData);
    }

    public async override UniTask UseTask()
    {
        if (ItemCanUse == ItemCanUse.OnlyBattle && !TurnManager.Instance.InBattle.Value)        // 사용 중 배틀이 끝나는 경우
        {
            ItemManager.Instance.Charge(MaxCharge);
            return;
        }
        await base.UseTask();
        if (Damage != 0)           // 액티브 아이템은 공격력 증가 효과를 받기 때문에 사용 후, 공격력 값이 감소하는 상태 효과들을 감소시켜줘야 함.
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.ATKUp, out _);
        }
    }
}
