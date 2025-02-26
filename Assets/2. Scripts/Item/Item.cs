using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Item
{
    ItemData _defaultData = null;
    string _defaultDesc = null;
    public ItemData Data;
    public string Desc;
    ItemAbility _itemAbility = new();

    public Func<UniTask> ItemTask;

    public Enemy TargetEnemy { get; private set; } = null;        // 아이템 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    public int CurCharge;

    public void Setup(ItemData data)
    {
        _defaultData = data;
        Data = _defaultData;
        if (Data.MaxCharge != 0)
        {
            if (Data.CurCharge == 0) { CurCharge = Data.MaxCharge; }
            else if (Data.CurCharge == -1) { CurCharge = 0; }
        }

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
                int damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
                int shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
                if (damage < 0) { damage = 0; }
                if (shield < 0) { shield = 0; }
                sb.Replace("{Damage}", damage.ToString());
                sb.Replace("{Shield}", shield.ToString());
                sb.Replace("{Draw}", (_defaultData.Draw).ToString());
                sb.Replace("{Heal}", (_defaultData.Heal).ToString());
                sb.Replace("{Duration}", (_defaultData.Duration).ToString());
                break;
        }

        _defaultDesc = sb.ToString();
        Desc = sb.ToString();

        _itemAbility.SetItemAbility(this);

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
    }

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

    public void CheckEnemyDead()
    {
        switch (Data.AttackType)
        {
            case AttackType.Single:
                TargetEnemy.CheckIfDead(Data.Damage, 1);
                break;
            case AttackType.Multi:
                foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
                {
                    enemy.CheckIfDead(Data.Damage, 1);
                }
                break;
        }
    }

    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }

    public async UniTask UseTask()
    {
        if (Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
        {
            ItemManager.Instance.Charge(Data.MaxCharge);
            return;
        }
        //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
        //UniTask uniTask = UniTask.Create(() => CardTask);
        //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
        await ItemTask();
    }
}
