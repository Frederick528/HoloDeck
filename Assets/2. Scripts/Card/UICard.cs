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

    bool _isXCost;

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
        sb.Replace("{Discard}", (data.Discard).ToString());
        sb.Replace("{Remove}", (data.Remove).ToString());

        _nameText.text = data.Name;
        if (data.Cost == -1)        // 맨 처음 받을 때는 -1로 받으나, 사용 후, 코스트 값이 변함. UI카드는 기본 데이터가 아닌 변한 데이터 값을 받기 때문에 생기는 문제. 이를 해결하고자 처음에 X 코스트인지 확인하고, 이후에는 코스트 값을 변경하지 않도록 코딩함.
        {
            _costText.text = "X";
            _isXCost = true;
        }
        else if (!_isXCost)
        {
            _costText.text = data.Cost.ToString();
        }
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
