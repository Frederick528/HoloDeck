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
        nameText.text = data.name;
        costText.text = data.cost.ToString();
        desText.text = data.descript;
        character.sprite = data.sprite;

        if (cardBtn != null)
        {
            cardBtn.onClick.AddListener(() =>
            {
                MapManager.Instance.rewardCardData = data;
                enlargeCard.Setup(data);
            });

        }
    }
}
