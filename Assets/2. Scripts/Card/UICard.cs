using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour
{
    [SerializeField] Image _card;
    [SerializeField] Image _character;
    [SerializeField] Image _descBG;

    [SerializeField] Image[] _rararityBG;

    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _costText;
    [SerializeField] TMP_Text _tagText;
    [SerializeField] TMP_Text _descText;
    //[SerializeField] Button _cardBtn;

    public UICard EnlargeCard;
    CardData _getCardData;

    Transform _enlargeTr;

    private void Start()
    {
        TryGetComponent(out Button _cardBtn);
        if (_cardBtn != null)
        {
            _enlargeTr = EnlargeCard.transform.parent;
            _cardBtn.onClick.AddListener(() =>
            {
                CardManager.Instance.GetCardData = _getCardData;
                EnlargeCard.Setup(_getCardData);
                _enlargeTr.gameObject.SetActive(true);
                OutGameUIManager.Instance.AddOpenUIOreder("EnlargeCard", () =>
                {
                    OutGameUIManager.Instance.RemoveOpenUIOrder("EnlargeCard");
                    _enlargeTr.gameObject.SetActive(false);
                });
            });

        }
    }

    public void Setup(CardData defaultData, CardData currData = null)
    {
        _getCardData = defaultData;
        if (defaultData == null)
        {
            _nameText.text = "Null";
            _costText.text = "0";
            _descText.text = "This is Null\nYou Can't Get This";
            _character.sprite = null;
            _tagText.text = "Null";
            for (int i = 0; i < _rararityBG.Length; ++i)
                _rararityBG[i].sprite = CardManager.Instance.CommonSprites[i];
            return;
        }

        string GetColorValue(int? current, int original)
        {
            if (current == null)
                return $"{original}";
            if (current > original)
                // 차분한 딥 그린 (성장/버프 느낌)
                return $"<color=#4CAF50>{current}</color>";
            else if (current < original)
                // 묵직한 다크 레드 (상처/디버프 느낌)
                return $"<color=#B71C1C>{current}</color>";
            else                         // 동일: 검정색 (기본 색상이 검정이라면 태그를 빼도 됩니다)
                return $"{current}";
        }

        StringBuilder sb = new StringBuilder(defaultData.Descript);
        sb.Replace("{Damage}", GetColorValue(currData?.Damage, defaultData.Damage));
        sb.Replace("{Shield}", GetColorValue(currData?.Shield, defaultData.Shield));
        sb.Replace("{Count}", GetColorValue(currData?.Count, defaultData.Count));
        sb.Replace("{Draw}", GetColorValue(currData?.Draw, defaultData.Draw));
        sb.Replace("{Discard}", GetColorValue(currData?.Discard, defaultData.Discard));
        sb.Replace("{Remove}", GetColorValue(currData?.Remove, defaultData.Remove));
        sb.Replace("{HP}", GetColorValue(currData?.HP, defaultData.HP));

        _nameText.text = defaultData.Name;
        if (defaultData.Cost == -1)
        {
            _costText.text = "X";
        }
        else
        {
            _costText.text = defaultData.Cost.ToString();
        }
        _descText.text = sb.ToString();
        _character.sprite = defaultData.Sprite;

        switch (defaultData.CardTag)
        {
            case CardTag.SingleAttack:
            case CardTag.AllAttack:
            case CardTag.RandomAttack:
                _tagText.text = "Attack";
                break;
            case CardTag.SkillTargetSelf:
            case CardTag.SkillTargetSingle:
            case CardTag.SkillTargetAll:
            case CardTag.SkillTargetRandom:
                _tagText.text = "Skill";
                break;
        }
        switch (defaultData.CardRarity)
        {
            case CardRarity.Common:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.CommonSprites[i];
                break;
            case CardRarity.Rare:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.RareSprites[i];
                break;
            case CardRarity.Epic:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.EpicSprites[i];
                break;
            case CardRarity.Legendary:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.LegendarySprites[i];
                break;
        }
    }
}
