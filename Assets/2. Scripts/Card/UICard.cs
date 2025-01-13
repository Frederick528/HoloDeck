using System.Collections;
using System.Collections.Generic;
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
        nameText.text = data.Name;
        costText.text = data.Cost.ToString();
        desText.text = data.Descript;
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
