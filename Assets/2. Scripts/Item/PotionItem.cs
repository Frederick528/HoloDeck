using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionItem : UseItem
{
    public int BtnIdx = -1;
    public override void Setup(ItemData data)
    {
        //_defaultData = data;
        //Data = _defaultData;
        base.Setup(data);

        StringBuilder sb = new StringBuilder(_defaultData.Descript);

        sb.Replace("{Damage}", (_defaultData.Damage).ToString());
        sb.Replace("{Shield}", (_defaultData.Shield).ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        sb.Replace("{Heal}", (_defaultData.Heal).ToString());
        sb.Replace("{Duration}", (_defaultData.Duration).ToString());

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
        if (Data.ItemCanUse == ItemCanUse.OnlyBattle && !TurnManager.Instance.InBattle)        // 사용 중 배틀이 끝나는 경우
        {
            ItemManager.Instance.HavePotionItem[BtnIdx] = true;
            return;
        }
        await base.UseTask();
    }
}
