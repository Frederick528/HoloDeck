using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected ItemData _defaultData = null;
    protected string _defaultDesc = null;
    public ItemData Data { get; protected set; }
    public string Desc;
    protected ItemAbility _itemAbility = new();

    //public Func<UniTask> ItemTask;

    //public Enemy TargetEnemy { get; private set; } = null;        // 아이템 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    //public int CurCharge;

    TMP_Text _text;                 // 텍스트 컴포넌트
    Image _backgroundImage;         // 배경 이미지
    RectTransform _textRect;
    RectTransform _bgRect;

    void Awake()
    {
        _backgroundImage = transform.Find("ItemDescWindow").GetComponent<Image>();
        _text = _backgroundImage.transform.Find("DescText").GetComponent<TMP_Text>();
        _textRect = _text.GetComponent<RectTransform>();
        _bgRect = _backgroundImage.GetComponent<RectTransform>();
        //AdjustBackgroundSize();
    }

    public void AdjustBackgroundSize()
    {
        _text.text = Desc;
        if (Desc == "")
        {
            _bgRect.sizeDelta = Vector2.zero;
            return;
        }
        // 텍스트의 크기를 가져와서 배경 이미지 크기 설정 (_textRectWidth = 처음 정해준 width 길이, _text.preferredHeight 줄바꿈 되는만큼의 길이)
        float width;
        float height;
        width = _text.preferredWidth < _textRect.rect.width ? _text.preferredWidth : _textRect.rect.width;
        height = _text.preferredHeight > _textRect.rect.height ? _text.preferredHeight : _textRect.rect.height;
        _bgRect.sizeDelta = new Vector2(width, height);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //if (Data.ID == 0) return;
        if (Desc != "")
        {
            _backgroundImage.gameObject.SetActive(true);
        }
        //_text.text = Desc;
        //AdjustBackgroundSize();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage.gameObject.activeSelf)
            _backgroundImage.gameObject.SetActive(false);
    }

    public void DescWindowOff()
    {
        _backgroundImage.gameObject.SetActive(false);
    }

    public virtual void Setup(ItemData data)
    {
        _defaultData = data;
        Data = _defaultData;

        StringBuilder sb = new StringBuilder(_defaultData.Descript);

        //_nameText.text = Data.Name;
        //_character.sprite = Data.Sprite;
        //_character.size = new Vector2(7.8f, 4.6f);

        //switch (Data.ItemTag)
        //{
        //    case ItemTag.Passive:
        //    case ItemTag.Potion:
        //        sb.Replace("{Damage}", (_defaultData.Damage).ToString());
        //        sb.Replace("{Shield}", (_defaultData.Shield).ToString());
        //        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        //        sb.Replace("{Heal}", (_defaultData.Heal).ToString());
        //        sb.Replace("{Duration}", (_defaultData.Duration).ToString());
        //        break;
        //    case ItemTag.Active:
        //        if (Data.CurCharge == 0) { CurCharge = Data.MaxCharge; }
        //        else if (Data.CurCharge == -1) { CurCharge = 0; }

        //        int damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
        //        int shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
        //        if (damage < 0) { damage = 0; }
        //        if (shield < 0) { shield = 0; }
        //        sb.Replace("{Damage}", damage.ToString());
        //        sb.Replace("{Shield}", shield.ToString());
        //        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        //        sb.Replace("{Heal}", (_defaultData.Heal).ToString());
        //        sb.Replace("{Duration}", (_defaultData.Duration).ToString());
        //        break;
        //}

        //_defaultDesc = sb.ToString();
        //Desc = sb.ToString();

        _defaultDesc = _defaultData.Descript;
        Desc = _defaultDesc;

        _itemAbility.SetPassiveItemAbility(this);

        AdjustBackgroundSize();

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

    //public void CheckEnemyDead()
    //{
    //    switch (Data.AttackType)
    //    {
    //        case AttackType.Single:
    //            TargetEnemy.CheckIfDead(Data.Damage, 1);
    //            break;
    //        case AttackType.Multi:
    //            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //            {
    //                enemy.CheckIfDead(Data.Damage, 1);
    //            }
    //            break;
    //        default: break;
    //    }
    //}

    //public void Target(Enemy enemy)
    //{
    //    TargetEnemy = enemy;
    //}

    //public async UniTask UseTask()
    //{
    //    if (Data.ItemCanUse == ItemCanUse.OnlyBattle && TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
    //    {
    //        switch (Data.ItemTag)
    //        {
    //            case ItemTag.Potion:

    //                break;
    //            case ItemTag.Active:
    //                ItemManager.Instance.Charge(Data.MaxCharge);
    //                break;
    //        }
    //        return;
    //    }
    //    //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
    //    //UniTask uniTask = UniTask.Create(() => CardTask);
    //    //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
    //    await ItemTask();
    //}
}
