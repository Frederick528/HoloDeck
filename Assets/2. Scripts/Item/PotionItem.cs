using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionItem : UseItem
{
    public int BtnIdx = -1;

    //public PotionItem(int damage, int shield, int draw, int heal, int duration, AttackType attackType, ItemCanUse itemCanUse) : base(damage, shield, draw, heal, duration, attackType, itemCanUse)
    //{
    //}

    public override void Setup<T>(T data)
    {
        //_defaultData = data;
        //Data = _defaultData;
        base.Setup(data);

        StringBuilder sb = new StringBuilder(_defaultData.Descript);

        sb.Replace("{Damage}", (Damage).ToString());
        sb.Replace("{Shield}", (Shield).ToString());
        sb.Replace("{Draw}", (Draw).ToString());
        sb.Replace("{Heal}", (Heal).ToString());
        sb.Replace("{Duration}", (Duration).ToString());

        //_defaultDesc = sb.ToString();
        Desc = sb.ToString();

        _itemAbility.SetPotionItemAbility(this);

        AdjustBackgroundSize();
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (BtnIdx == -1 || !ItemManager.Instance.HavePotionItem[BtnIdx])
            return;
        base.OnPointerEnter(eventData);
    }
    public async override UniTask UseTask()
    {
        if (ItemCanUse == ItemCanUse.OnlyBattle && !TurnManager.Instance.InBattle.Value)        // 사용 중 배틀이 끝나는 경우
        {
            ItemManager.Instance.HavePotionItem[BtnIdx] = true;
            return;
        }
        await base.UseTask();
    }
}
