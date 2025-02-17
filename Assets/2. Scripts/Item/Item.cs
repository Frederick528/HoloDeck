using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Item : MonoBehaviour
{
    ItemData _defaultData = null;
    string _defaultDesc = null;
    public ItemData Data;
    public string Desc;

    public ItemAbility ItemAbility = new();

    public UniTask ItemTask;

    public Enemy TargetEnemy { get; private set; } = null;        // 아이템 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    public int CurCharge;

    public void Setup(ItemData data)
    {
        _defaultData = data;
        Data = _defaultData;
        if (Data.CurCharge == 0) { CurCharge = Data.MaxCharge; }
        else if (Data.CurCharge == -1) { CurCharge = 0; }

        StringBuilder sb = new StringBuilder(_defaultData.Descript);

        //_nameText.text = Data.Name;
        //_character.sprite = Data.Sprite;
        //_character.size = new Vector2(7.8f, 4.6f);
        switch (Data.ItemTag)
        {
            case ItemTag.Passive:
            case ItemTag.Potion:
                sb.Replace("{Damage}", (_defaultData.Damage).ToString());
                sb.Replace("{Shield}", (_defaultData.Shield).ToString());
                sb.Replace("{Draw}", (_defaultData.Draw).ToString());
                sb.Replace("{Heal}", (_defaultData.Heal).ToString());
                sb.Replace("{Duration}", (_defaultData.Duration).ToString());
                break;
            case ItemTag.Active:
                sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
                sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
                sb.Replace("{Draw}", (_defaultData.Draw).ToString());
                sb.Replace("{Heal}", (_defaultData.Heal).ToString());
                sb.Replace("{Duration}", (_defaultData.Duration).ToString());
                break;
        }

        _defaultDesc = sb.ToString();
        Desc = sb.ToString();


        //switch (Data.CardRarity)
        //{
        //    case CardRarity.Common:
        //        for (int i = 0; i < _rararityBG.Length; ++i)
        //            _rararityBG[i].sprite = CardManager.Instance.CommonSprites[i];
        //        break;
        //    case CardRarity.Rare:
        //        for (int i = 0; i < _rararityBG.Length; ++i)
        //            _rararityBG[i].sprite = CardManager.Instance.RareSprites[i];
        //        break;
        //    case CardRarity.Epic:
        //        for (int i = 0; i < _rararityBG.Length; ++i)
        //            _rararityBG[i].sprite = CardManager.Instance.EpicSprites[i];
        //        break;
        //    case CardRarity.Legendary:
        //        for (int i = 0; i < _rararityBG.Length; ++i)
        //            _rararityBG[i].sprite = CardManager.Instance.LegendarySprites[i];
        //        break;
        //}

        ItemDataReset();
    }

    public void ItemDataReset()
    {
        if (Data.ItemTag == ItemTag.Active)
        {
            Data.Damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
            Data.Shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
            Data.Draw = _defaultData.Draw;
            Data.Heal = _defaultData.Heal;
            Data.Duration = _defaultData.Duration;
        }
    }

    public void ChangeCardDesc()
    {
        if (Data.ItemTag == ItemTag.Active)
        {
            StringBuilder sb = new StringBuilder(_defaultData.Descript);
            sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
            sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
            sb.Replace("{Draw}", (_defaultData.Draw).ToString());
            sb.Replace("{Heal}", (_defaultData.Heal).ToString());
            sb.Replace("{Duration}", (_defaultData.Duration).ToString());
            Desc = sb.ToString();
            ItemDataReset();
        }
    }

    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }

    public async UniTask UseTask()
    {
        //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
        //UniTask uniTask = UniTask.Create(() => CardTask);
        //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
        await ItemTask;
    }
}
