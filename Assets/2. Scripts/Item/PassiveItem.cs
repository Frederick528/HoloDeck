using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PassiveItem : Item
{
    public override void Setup(ItemData data)
    {
        base.Setup(data);
        //_defaultDesc = _defaultData.Descript;
        //Desc = _defaultDesc;
        Desc = _defaultData.Descript;

        _itemAbility.SetPassiveItemAbility(this);

        AdjustBackgroundSize();
    }
}
