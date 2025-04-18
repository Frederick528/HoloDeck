using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Image _image;

    TMP_Text _text;                 // 텍스트 컴포넌트
    Image _backgroundImage;         // 배경 이미지
    RectTransform _textRect;
    RectTransform _bgRect;

    ItemData _itemData;

    int _curCharge = -1;

    //private void Start()
    //{
    //    _image = GetComponent<Image>();
    //    _backgroundImage = transform.Find("ItemDescWindow").GetComponent<Image>();
    //    _text = _backgroundImage.transform.Find("DescText").GetComponent<TMP_Text>();
    //    _textRect = _text.GetComponent<RectTransform>();
    //    _bgRect = _backgroundImage.GetComponent<RectTransform>();
    //    if (TryGetComponent(out Button _itemBtn))
    //    {
    //        _itemBtn.onClick.AddListener(() =>
    //        {
    //            ItemManager.Instance.GetItem(_itemData);
    //            InGameManager.Instance.ReturnRandomItem();
    //            InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.ItemReward, false);
    //        });

    //    }
    //}

    public void Setup(ItemData itemData, int idx)
    {
        if (!_image)
        {
            _image = GetComponent<Image>();
            _backgroundImage = transform.Find("ItemDescWindow").GetComponent<Image>();
            _text = _backgroundImage.transform.Find("DescText").GetComponent<TMP_Text>();
            _textRect = _text.GetComponent<RectTransform>();
            _bgRect = _backgroundImage.GetComponent<RectTransform>();
            if (TryGetComponent(out Button _itemBtn))
            {
                _itemBtn.onClick.AddListener(() =>
                {
                    (ItemData, int)? changeditem = ItemManager.Instance.GetItem(_itemData, _curCharge);
                    InGameManager.Instance.ReturnRandomItem(MapManager.Instance.currStage.ItemReward);
                    if (changeditem != null)
                    {
                        MapManager.Instance.ChangedUseItem(changeditem.Value.Item1, idx);       // 여기 SetUp 들어가 있음.
                        _curCharge = changeditem.Value.Item2;
                        MapManager.Instance.GetReward(true);
                    }
                    else
                    {
                        _curCharge = -1;
                        MapManager.Instance.GetReward(false);
                    }
                    if (_curCharge == -1)
                    {
                        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.ItemReward, false);
                        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, true);
                    }
                });

            }
        }
        _itemData = itemData;
        if (_itemData.Sprite)
            _image.sprite = _itemData.Sprite;
        AdjustBackgroundSize();
    }

    public void AdjustBackgroundSize()
    {
        //if (_itemData.Descript == "")
        //{
        //    _bgRect.sizeDelta = Vector2.zero;
        //    return;
        //}
        switch (_itemData.ItemTag)
        {
            case ItemTag.Passive:
                _text.text = "PassiveItem\n" + _itemData.Descript;
                break;
            case ItemTag.Active:
                StringBuilder sb = new StringBuilder(_itemData.Descript);
                sb.Replace("{Damage}", (_itemData.Damage).ToString());
                sb.Replace("{Shield}", (_itemData.Shield).ToString());
                sb.Replace("{Draw}", (_itemData.Draw).ToString());
                sb.Replace("{Heal}", (_itemData.Heal).ToString());
                _text.text = $"ActiveItem\n[{_itemData.MaxCharge}Charge]\n{sb}";
                break;
        }
        // 텍스트의 크기를 가져와서 배경 이미지 크기 설정 (_textRectWidth = 처음 정해준 width 길이, _text.preferredHeight 줄바꿈 되는만큼의 길이)
        _backgroundImage.gameObject.SetActive(true);        // 텍스트 자동줄바꿈 계산을 위해 활성화해야함. 그런데, UIItem은 캔버스도 켜야되기 때문에 설명배경창뿐만 아니라 UIManager에서 추가로 캔버스를 껐다킴.
        _backgroundImage.gameObject.SetActive(false);
        float width;
        float height;
        width = _text.preferredWidth < _textRect.rect.width ? _text.preferredWidth : _textRect.rect.width;
        height = _text.preferredHeight > _textRect.rect.height ? _text.preferredHeight : _textRect.rect.height;
        _bgRect.sizeDelta = new Vector2(width, height);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        _backgroundImage.gameObject.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage.gameObject.activeSelf)
            _backgroundImage.gameObject.SetActive(false);
    }
}
