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

    [SerializeField] UICard enlargeCard;
    CardData _getCardData;

    private void Start()
    {
        TryGetComponent(out Button _cardBtn);
        if (_cardBtn != null)
        {
            _cardBtn.onClick.AddListener(() =>
            {
                CardManager.Instance.GetCardData = _getCardData;
                enlargeCard.Setup(_getCardData);
            });

        }
    }

    public void Setup(CardData data)
    {
        _getCardData = data;
        StringBuilder sb = new StringBuilder(data.Descript);
        sb.Replace("{Damage}", (data.Damage).ToString());
        sb.Replace("{Shield}", (data.Shield).ToString());
        sb.Replace("{Count}", (data.Count).ToString());
        sb.Replace("{Draw}", (data.Draw).ToString());

        _nameText.text = data.Name;
        _costText.text = data.Cost.ToString();
        _descText.text = sb.ToString();
        _character.sprite = data.Sprite;

        switch (data.CardTag)
        {
            case CardTag.SingleAttack:
            case CardTag.MultiAttack:
                _tagText.text = "Attack";
                break;
            case CardTag.Skill:
                _tagText.text = "Skill";
                break;
        }
        switch (data.CardRarity)
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
