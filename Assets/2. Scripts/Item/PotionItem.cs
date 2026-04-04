using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionItem : UseItem
{
    public int BtnIdx = -1;

    [Header("Condition Settings")]
    private string _conditionText = "조건 없음";
    private int _currentProgress = 0;
    private int _maxProgress = 0;

    [Header("Enhancement Settings")]
    private List<string> _appliedAugments = new List<string>();
    private string _nextPreviewAugment = ""; // 강화를 하게 되면 얻을 효과 (초록색 표시)

    private static readonly StringBuilder _sb = new StringBuilder(256);


    //public PotionItem(int damage, int shield, int draw, int heal, int duration, AttackType attackType, ItemCanUse itemCanUse) : base(damage, shield, draw, heal, duration, attackType, itemCanUse)
    //{
    //}

    public override void Setup<T>(T data)
    {
        //_defaultData = data;
        //Data = _defaultData;
        base.Setup(data);

        UpdateDescription();

        _itemAbility.SetPotionItemAbility(this);

        //StringBuilder sb = new StringBuilder(_defaultData.Descript);

        //sb.Replace("{Damage}", (Damage).ToString());
        //sb.Replace("{Shield}", (Shield).ToString());
        //sb.Replace("{Draw}", (Draw).ToString());
        //sb.Replace("{Heal}", (Heal).ToString());
        //sb.Replace("{Duration}", (Duration).ToString());

        ////_defaultDesc = sb.ToString();
        //Desc = sb.ToString();

        //_itemAbility.SetPotionItemAbility(this);

        //AdjustBackgroundSize();
    }

    /// <summary>
    /// 실시간으로 진행도나 강화 내용이 바뀔 때 호출하여 Desc를 갱신합니다.
    /// </summary>
    public void UpdateDescription()
    {
        if (_defaultData == null) return;
        if (string.IsNullOrWhiteSpace(_defaultData.Descript)) return;
        _sb.Clear();

        // 1. 조건 및 진행도 [1/4]
        _sb.Append("<b>[조건]</b> ").Append(_conditionText);
        if (!_conditionText.Contains("조건 없음"))
        {
            _sb.Append(" <color=#FFD700>[").Append(_currentProgress).Append("/").Append(_maxProgress).Append("]</color>\n");
        }

        _sb.Append("<size=80%>\n</size>"); // 줄간격 살짝 띄우기

        // 2. 기본 효과 (기존 데이터 기반)
        string baseDesc = _defaultData.Descript;
        baseDesc = baseDesc.Replace("{Damage}", Damage.ToString())
                           .Replace("{Shield}", Shield.ToString())
                           .Replace("{Draw}", Draw.ToString())
                           .Replace("{Heal}", Heal.ToString())
                           .Replace("{Duration}", Duration.ToString());

        _sb.Append("<b>[효과]</b> ").Append(baseDesc).Append("\n");

        // 3. 강화 내용 나열
        if (_appliedAugments.Count > 0 || !string.IsNullOrEmpty(_nextPreviewAugment))
        {
            _sb.Append("<size=80%>\n</size>");
            _sb.Append("<b>[강화 내역]</b>\n");

            // 현재 적용된 강화들
            foreach (var aug in _appliedAugments)
            {
                _sb.Append("- ").Append(aug).Append("\n");
            }

            // 다음 강화 예정 효과 (초록색 강조)
            if (!string.IsNullOrEmpty(_nextPreviewAugment))
            {
                _sb.Append("- <color=#00FF00>").Append(_nextPreviewAugment).Append(" (예정)</color>");
            }
        }

        Desc = _sb.ToString();

        // 설명창이 열려있다면 실시간으로 크기 조정
        AdjustBackgroundSize();
    }

    /// <summary>
    /// 외부에서 포션의 조건 진행도를 올릴 때 호출합니다.
    /// </summary>
    public void AddProgress(int value)
    {
        _currentProgress = Mathf.Clamp(_currentProgress + value, 0, _maxProgress);

        // 조건 달성 여부 체크
        if (_currentProgress >= _maxProgress)
        {
            ItemManager.Instance.HavePotionItem[BtnIdx] = true;
        }
        else
        {
            ItemManager.Instance.HavePotionItem[BtnIdx] = false;
        }

        UpdateDescription();
    }

    /// <summary>
    /// [단순 확인용] 강화를 하면 어떤 효과가 추가될지 초록색 예고 문구만 설정합니다.
    /// 리스트에 추가되지 않으므로 안심하고 호출하세요.
    /// </summary>
    public void SetPreviewAugment(string previewText)
    {
        _nextPreviewAugment = previewText;
        UpdateDescription();
    }

    /// <summary>
    /// [단순 확인용] 초록색 예고 문구를 지웁니다. (예: 강화 창에서 마우스를 뗐을 때)
    /// </summary>
    public void ClearPreviewAugment()
    {
        _nextPreviewAugment = "";
        UpdateDescription();
    }

    /// <summary>
    /// [실제 적용용] 현재 프리뷰 중인 강화 내용을 '진짜 내역'으로 옮기고 프리뷰를 초기화합니다.
    /// </summary>
    public void ConfirmAugment()
    {
        if (string.IsNullOrEmpty(_nextPreviewAugment)) return;

        // 프리뷰 내용을 진짜 내역에 추가
        _appliedAugments.Add(_nextPreviewAugment);

        // 프리뷰 값 초기화 (이제 초록색 글씨는 사라지고 흰색 내역으로 올라감)
        _nextPreviewAugment = "";

        UpdateDescription();

        _itemAbility.SetPotionItemAbility(this);
    }


    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (BtnIdx == -1 || !ItemManager.Instance.HavePotionItem[BtnIdx])
            return;
        base.OnPointerEnter(eventData);
    }
    public async override UniTask UseTask()
    {
        if (ItemCanUse == ItemCanUse.OnlyBattle && !TurnManager.Instance.InBattle.Value)        // 사용 중 배틀이 끝나는 경우
        {
            ItemManager.Instance.HavePotionItem[BtnIdx] = true;
            return;
        }
        await base.UseTask();
    }
}
