//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class ItemDescWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    public Item HaveItem;
//    TMP_Text _text;  // 텍스트 컴포넌트
//    Image _backgroundImage;        // 배경 이미지
//    RectTransform _textRect;
//    RectTransform _bgRect;

//    void Start()
//    {
//        _backgroundImage = transform.Find("ItemDescWindow").GetComponent<Image>();
//        _text = _backgroundImage.transform.Find("DescText").GetComponent<TMP_Text>();
//        _textRect = _text.GetComponent<RectTransform>();
//        _bgRect = _backgroundImage.GetComponent<RectTransform>();
//    }

//    void AdjustBackgroundSize()
//    {
//        // 텍스트의 크기를 가져와서 배경 이미지 크기 설정 (_textRectWidth = 처음 정해준 width 길이, _text.preferredHeight 줄바꿈 되는만큼의 길이)
//        float width;
//        float height;
//        width = _text.preferredWidth < _textRect.rect.width ? _text.preferredWidth : _textRect.rect.width;
//        height = _text.preferredHeight > _textRect.rect.height ? _text.preferredHeight : _textRect.rect.height;
//        _bgRect.sizeDelta = new Vector2(width, height);
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (HaveItem == null) return;
//        _backgroundImage.gameObject.SetActive(true);
//        _text.text = HaveItem.Desc;
//        AdjustBackgroundSize();
//    }
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (_backgroundImage.gameObject.activeSelf)
//            _backgroundImage.gameObject.SetActive(false);
//    }
//}
