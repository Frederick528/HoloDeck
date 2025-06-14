using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

public class ActiveItem : UseItem
{
    public int CurCharge;
    public void ActiveItemDataReset()
    {
        if (_defaultData.Damage != 0)
            Data.Damage = _defaultData.Damage + InGameManager.Instance.Player.AttackPower.Value;
        if (_defaultData.Shield != 0)
            Data.Shield = _defaultData.Shield + InGameManager.Instance.Player.DefensePower.Value;
        if (Data.Damage < 0) { Data.Damage = 0; }
        if (Data.Shield < 0) { Data.Shield = 0; }
        Data.Draw = _defaultData.Draw;
        Data.Heal = _defaultData.Heal;
        Data.Duration = _defaultData.Duration;

        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", Data.Damage.ToString());
        sb.Replace("{Shield}", Data.Shield.ToString());
        sb.Replace("{Draw}", Data.Draw.ToString());
        sb.Replace("{Heal}", Data.Heal.ToString());
        sb.Replace("{Duration}", Data.Duration.ToString());
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

    public void Setup(ItemData data, int value)
    {
        //_defaultData = data;
        //Data = _defaultData;
        base.Setup(data);

        if (value != -1) { CurCharge = value; }
        else if (Data.StartCharge) { CurCharge = Data.MaxCharge; }
        else if (!Data.StartCharge) { CurCharge = 0; }

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
        if (!ItemManager.Instance.HaveActiveItem)
            return;
        base.OnPointerEnter(eventData);
    }

    public async override UniTask UseTask()
    {
        if (Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.CancelSource.Token.IsCancellationRequested)        // 사용 중 배틀이 끝나는 경우
        {
            ItemManager.Instance.Charge(Data.MaxCharge);
            return;
        }
        await base.UseTask();
        if (Data.Damage != 0)           // 액티브 아이템은 공격력 증가 효과를 받기 때문에 사용 후, 공격력 값이 감소하는 상태 효과들을 감소시켜줘야 함.
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.ATKUp, out _);
        }
    }
}
