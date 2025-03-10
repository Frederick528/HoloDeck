using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveItem : UseItem
{
    public int CurCharge;
    public void ActiveItemDataReset()
    {
        if (Data.ItemTag == ItemTag.Active)
        {
            Data.Damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
            Data.Shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
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
        }
    }

    public void Setup(ItemData data, int value)
    {
        _defaultData = data;
        Data = _defaultData;

        StringBuilder sb = new StringBuilder(_defaultData.Descript);

        if (value != -1) { CurCharge = value; } 
        else if (Data.CurCharge == 0) { CurCharge = Data.MaxCharge; }
        else if (Data.CurCharge == -1) { CurCharge = 0; }

        int damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
        int shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
        if (damage < 0) { damage = 0; }
        if (shield < 0) { shield = 0; }
        sb.Replace("{Damage}", damage.ToString());
        sb.Replace("{Shield}", shield.ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        sb.Replace("{Heal}", (_defaultData.Heal).ToString());
        sb.Replace("{Duration}", (_defaultData.Duration).ToString());

        _defaultDesc = sb.ToString();
        Desc = sb.ToString();

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
    }
}
