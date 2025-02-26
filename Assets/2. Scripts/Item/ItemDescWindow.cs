using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDescWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TMP_Text _text;  // 텍스트 컴포넌트
    Image _backgroundImage;        // 배경 이미지
    float _textRectWidth;
    RectTransform _bgRect;

    void Start()
    {
        _backgroundImage = transform.Find("ItemDescWindow").GetComponent<Image>();
        _text = _backgroundImage.transform.Find("DescText").GetComponent<TMP_Text>();
        _textRectWidth = _text.GetComponent<RectTransform>().rect.width;
        _bgRect = _backgroundImage.GetComponent<RectTransform>();
        // 텍스트의 크기에 맞춰 이미지 크기 조정
        AdjustBackgroundSize();
    }

    void AdjustBackgroundSize()
    {
        // 텍스트의 크기를 가져와서 배경 이미지 크기 설정 (_textRectWidth = 처음 정해준 width 길이, _text.preferredHeight 줄바꿈 되는만큼의 길이)
        _bgRect.sizeDelta = new Vector2(_textRectWidth, _text.preferredHeight);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        print(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        print(eventData);
    }
}
