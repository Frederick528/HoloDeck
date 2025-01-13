using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour
{
    [SerializeField] Image card;
    [SerializeField] Image character;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text desText;
    [SerializeField] Button cardBtn;

    [SerializeField] UICard enlargeCard;

    public void Setup(CardData data)
    {
        StringBuilder sb = new StringBuilder(data.Descript);
        sb.Replace("{Damage}", (data.Damage).ToString());
        sb.Replace("{Defence}", (data.Defence).ToString());
        sb.Replace("{Count}", (data.Count).ToString());
        sb.Replace("{Draw}", (data.Draw).ToString());

        nameText.text = data.Name;
        costText.text = data.Cost.ToString();
        desText.text = sb.ToString();
        character.sprite = data.Sprite;

        if (cardBtn != null)
        {
            cardBtn.onClick.AddListener(() =>
            {
                CardManager.Instance.rewardCardData = data;
                enlargeCard.Setup(data);
            });

        }
    }
}
