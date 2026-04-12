using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class ActionCard : Card, IPointerEnterHandler, IPointerExitHandler // Card를 상속받음
{
    [Header("Action Card Settings")]
    public int SlotIdx = -1;

    private string _conditionText = "조건 없음";
    private int _currentProgress = 0;
    private int _maxProgress = 0;

    private int _upgradeCount = 0;
    private int _upgradeMaxCount = 3;
    public bool MaxUpgrade = false;

    private List<string> _appliedAugments = new();
    private List<string> _nextPreviewAugments = new();

    [Header("UI Components for Tooltip")]
    [SerializeField] private Image _backgroundImage;
    private RectTransform _textRect;
    private RectTransform _bgRect;

    [SerializeField] private GameObject _queueVisual;

    [SerializeField] Image _actionCardImage;
    [SerializeField] List<Image> _upgradeImage;

    SpriteRenderer _visualRender;
    private Vector3 _originLocalPos;
    Vector3? _originLocalScale = null;

    public SpecialTagType TargetCondition;

    protected void Awake()
    {
        // 프리팹 구조에 맞춰 경로 수정 가능
        if (_backgroundImage == null)
            _backgroundImage = transform.Find("ItemDescWindow")?.GetComponent<Image>();

        if (_backgroundImage != null)
        {
            _bgRect = _backgroundImage.GetComponent<RectTransform>();
            _descText = _backgroundImage.transform.Find("DescText")?.GetComponent<TMP_Text>();
            if (_descText != null)
                _textRect = _descText.GetComponent<RectTransform>();
        }

        //_actionCardImage = transform.Cast<Transform>()
        //                      .Select(t => t.GetComponent<Image>())
        //                      .FirstOrDefault(i => i != null);

    }

    public void ShowQueueVisual(Vector3 watingPos, int order)
    {
        if (_queueVisual == null) return;

        // 1. 이미지 등을 현재 카드 데이터와 맞춤
        _visualRender.sprite = Data.Sprite;

        _visualRender.sortingOrder = order;

        if (_originLocalScale == null)
            SetSpriteSize(200,300);

        // 2. 활성화
        _queueVisual.SetActive(true);

        // 3. 부드럽게 이동 (DOTween)
        MoveVisual(watingPos);
    }

    public void MoveVisual(Vector3 watingPos)
    {
        if (_queueVisual == null) return;
        _queueVisual.transform.DOMove(watingPos, CardUtils.CardAlignmentDelay).SetUpdate(true).SetEase(Ease.OutQuad);
        _queueVisual.transform.DORotateQuaternion(Quaternion.identity, CardUtils.CardAlignmentDelay).SetUpdate(true).SetEase(Ease.OutQuad);
        _queueVisual.transform.DOScale(_originLocalScale.Value * 0.5f, CardUtils.CardAlignmentDelay).SetUpdate(true).SetEase(Ease.OutQuad);
    }

    public override async UniTask AfterCardAbility(bool endBattle = false)
    {
        if (IsForce)
        {
            await CardManager.Instance.RemoveCopyCard(this);
            CardManager.Instance.NowPlayedCard = null;
            return;
        }

        await HideQueueVisual();
    }

    // 실제 능력이 실행되어 큐에서 빠질 때 호출
    public async UniTask HideQueueVisual()
    {
        if (_queueVisual == null) return;

        // 1. 연출: 카드가 작아지거나 투명해지는 애니메이션
        await _queueVisual.transform.DOScale(Vector3.zero, CardUtils.ThrowAwayCardDelay).SetUpdate(true).SetEase(Ease.OutQuad);

        // 1. 비활성화
        _queueVisual.SetActive(false);

        // 2. 다시 원래 카드의 위치로 복귀
        _queueVisual.transform.localPosition = _originLocalPos;
        _queueVisual.transform.localScale = _originLocalScale.Value;

        Block = false;
        Used = false;
    }

    protected override void OnEnable()
    {

    }

    protected override void OnDisable()
    {

    }

    // Card의 Setup을 오버라이드하거나 확장
    public override void Setup(CardData data)
    {

        _actionCardImage.sprite = data.Sprite;

        // 초기 조건 설정 (데이터 시트나 SO에서 가져오게 확장 가능)
        if (data.UpgradeTags.Count > 0 && data.UpgradeTags[0].Type.Special != SpecialTagType.None && data.UpgradeTags[0].Amount != 0)
        {
            CardManager.Instance.UnregisterListener(this.TargetCondition, this);
            TargetCondition = data.UpgradeTags[0].Type.Special;
            _maxProgress = (int)data.UpgradeTags[0].Amount;
            _conditionText = data.UpgradeDescriptions[0];

            CardManager.Instance.RegisterListener(this.TargetCondition, this);
        }

        if (_queueVisual == null)
        {
            _queueVisual = new GameObject($"{data.Name}_Visual");
            _queueVisual.transform.SetParent(this.transform);
            _visualRender = _queueVisual.AddComponent<SpriteRenderer>();
            _visualRender.sortingLayerName = "Card";
            _visualRender.sortingOrder = -1;
        }
        else
        {
            _visualRender = _queueVisual.GetComponent<SpriteRenderer>();
        }

        _originLocalPos = _queueVisual.transform.localPosition;
        _queueVisual.SetActive(false); // 처음엔 꺼둠

        //UpdateActionDescription();
        base.Setup(data); // 기존 카드 세팅(Data 클론, 능력 조립 등) 호출
    }
    public void SetSpriteSize(float targetWidth, float targetHeight)
    {
        if (_visualRender.sprite == null)
        {
            _originLocalScale = Vector3.one;
            return;
        }
        // 1. 스프라이트의 원래 크기(World Unit 단위)를 가져옵니다.
        float unitWidth = _visualRender.sprite.bounds.size.x;
        float unitHeight = _visualRender.sprite.bounds.size.y;

        // 2. 목표 크기를 현재 크기로 나눠서 필요한 스케일(Scale) 값을 구합니다.
        float xPlay = targetWidth / unitWidth;
        float yPlay = targetHeight / unitHeight;

        // 3. 계산된 스케일을 적용합니다.
        _originLocalScale = new Vector3(xPlay, yPlay, 1f);
        _queueVisual.transform.localScale = _originLocalScale.Value;
    }
    protected override void UpdateVisuals()
    {
        // 명미 님이 말씀하신 딱 필요한 것들만 세팅
        if (_nameText != null) _nameText.text = Data.Name;
        if (_character != null) _character.sprite = Data.Sprite;

        UpdateActionDescription();
    }

    public void UpdateActionDescription()
    {
        if (Desc == "") return;
        // [데이터 조립 로직 생략 - 이전 답변의 StringBuilder 사용]
        string finalDesc = GetActionDescriptionString(); // 조립된 문자열 가져오기

        _descText.text = finalDesc;
        AdjustBackgroundSize();
    }

    public void AdjustBackgroundSize()
    {
        if (_bgRect == null || string.IsNullOrEmpty(_descText.text))
        {
            if (_bgRect != null) _bgRect.sizeDelta = Vector2.zero;
            return;
        }

        // 텍스트의 크기를 계산하여 배경 크기 설정
        //float width = _descText.preferredWidth < _textRect.rect.width ? _descText.preferredWidth : _textRect.rect.width;
        float height = _descText.preferredHeight > _textRect.rect.height ? _descText.preferredHeight : _textRect.rect.height;

        _bgRect.sizeDelta = new Vector2(_bgRect.sizeDelta.x, height);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (!string.IsNullOrEmpty(_descText.text))
        {
            _backgroundImage.gameObject.SetActive(true);
            AdjustBackgroundSize(); // 나타날 때 다시 한번 계산
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DescWindowOff();
    }

    public void DescWindowOff()
    {
        if (_backgroundImage != null)
            _backgroundImage.gameObject.SetActive(false);
    }

    public override void RefreshCardStats()
    {
        CardDataValue addValue = _upgradeState + _permanentState + _addAbilityState;
        if (Data == null || _descText == null) return;
        // 1. 순수하게 [기본값 + 강화값]만 계산합니다. (Player 스탯 합산 제거)
        Data.Damage = DefaultData.Damage + addValue.Damage;
        Data.Shield = DefaultData.Shield + addValue.Shield;

        Data.Count = DefaultData.Count + addValue.Count;
        Data.Draw = DefaultData.Draw + addValue.Draw;
        Data.Discard = DefaultData.Discard + addValue.Discard;
        Data.Remove = DefaultData.Remove + addValue.Remove;
        Data.HP = DefaultData.HP + addValue.HP;

        if (DefaultData.Cost != -1)
            Data.Cost = DefaultData.Cost + addValue.Cost;

        _sb.Clear();
        _sb.Append(DefaultData.Descript);

        // [최적화 핵심] Data가 아니라 보정값이 다 들어있는 _displayState를 사용합니다!
        _sb.Replace("{Damage}", GetColorValue(Data.Damage, DefaultData.Damage));
        _sb.Replace("{Shield}", GetColorValue(Data.Shield, DefaultData.Shield));
        _sb.Replace("{Count}", GetColorValue(Data.Count, DefaultData.Count));
        _sb.Replace("{Draw}", GetColorValue(Data.Draw, DefaultData.Draw));
        _sb.Replace("{Discard}", GetColorValue(Data.Discard, DefaultData.Discard));
        _sb.Replace("{Remove}", GetColorValue(Data.Remove, DefaultData.Remove));
        _sb.Replace("{HP}", GetColorValue(Data.HP, DefaultData.HP));

        Desc = _sb.ToString();
        _descText.text = Desc;

        // 2. 특수 조건(첫 카드 등)이나 상태 이상(취약, 약화) 계산을 싹 건너뜁니다.
        // RefreshSpecialCondition()을 호출하지 않음으로써 순수함을 유지!

        // 3. UI 갱신을 위해 최종 출력용 변수로 넘겨줍니다.
        UpdateActionDescription();
    }

    /// <summary>
    /// 카드 설명창을 액션 카드 형식(조건 + 진행도 + 효과)으로 갱신
    /// </summary>
    public string GetActionDescriptionString()
    {
        _sb.Clear();

        // 1. [조건] 출력
        if (!_conditionText.Contains("조건 없음"))
        {
            _sb.Append("<align=center>");
            _sb.Append("<color=#FFD700><b>[조건]</b> ").Append(_conditionText).Append("</color>");
            _sb.Append(" [").Append(_currentProgress).Append("/").Append(_maxProgress).Append("]\n");
            _sb.Append("<size=80%>\n</size>");
            _sb.Append("</align>");
        }

        // 2. 기본 효과
        _sb.Append("<b>[효과]</b> ").Append(Desc).Append("\n");

        // 3. 강화 내역 (리스트 합산 체크)
        if (_appliedAugments.Count > 0 || _nextPreviewAugments.Count > 0)
        {
            _sb.Append("<size=80%>\n</size>");
            _sb.Append("<b>[강화 내역]</b>\n");

            // 현재 적용된 강화들 (흰색)
            foreach (var aug in _appliedAugments)
            {
                _sb.Append("- ").Append(aug).Append("\n");
            }

            // [수정] 다음 강화 예정 효과들 (초록색 루프)
            foreach (var preview in _nextPreviewAugments)
            {
                _sb.Append("- <color=#00FF00>").Append(preview).Append(" (예정)</color>\n");
            }
        }

        return _sb.ToString();
    }

    /// <summary>
    /// [단순 확인용] 강화를 하면 어떤 효과가 추가될지 초록색 예고 문구만 설정합니다.
    /// </summary>
    public void SetPreviewAugments(List<string> previewTexts)
    {
        _nextPreviewAugments.AddRange(previewTexts);
        UpdateActionDescription();
    }

    /// <summary>
    /// [단순 확인용] 초록색 예고 문구를 지웁니다.
    /// </summary>
    public void ClearPreviewAugment()
    {
        _nextPreviewAugments.Clear();
        UpdateActionDescription();
    }

    /// <summary>
    /// [실제 적용용] 현재 프리뷰 중인 강화 내용을 '진짜 내역'으로 옮기고, 
    /// 실제 카드 능력(Logic)도 추가합니다.
    /// </summary>
    /// <param name="newAbilityTag">강화로 추가될 실제 게임 로직 데이터</param>
    public void ConfirmAugment(List<MasterTagData> newAbilityTags = null, List<string> descTexts = null, Sprite sprite = null)
    {
        _upgradeImage[_upgradeCount].sprite = sprite;
        if (++_upgradeCount >= _upgradeMaxCount)
        {
            MaxUpgrade = true;
        }
        // 리스트가 비어있지 않다면 진짜 내역에 합침
        if (descTexts != null && descTexts.Count > 0)
        {
            _appliedAugments.AddRange(descTexts);
        }

        if (newAbilityTags != null && newAbilityTags.Count > 0)
        {
            AddEnhancement(newAbilityTags);
        }

        _nextPreviewAugments.Clear();
        RefreshCardStats();
    }

    public void AddProgress(int value)
    {
        if (CardManager.Instance.CanUseActionCard[SlotIdx]) return;

        _currentProgress = Mathf.Clamp(_currentProgress + value, 0, _maxProgress);

        if (_currentProgress >= _maxProgress)
        {
            // 조건 달성! 활성화 상태를 매니저에 보고
            CardManager.Instance.SetActionCardReady(SlotIdx, true);
        }

        UpdateActionDescription();
    }

    // 사용 후 초기화가 필요하다면 호출
    public void ResetActionCard()
    {
        _currentProgress = 0;
        CardManager.Instance.SetActionCardReady(SlotIdx, false);
        UpdateActionDescription();
    }

    public void SetHighlight(bool isOn)
    {
        if (isOn)
        {
            _actionCardImage.color = Color.white;
        }
        else
        {
            _actionCardImage.color = Color.gray;
        }
    }

    private void OnDestroy()
    {
        if (CardManager.Instance != null)
            CardManager.Instance.UnregisterListener(this.TargetCondition, this);
    }
}