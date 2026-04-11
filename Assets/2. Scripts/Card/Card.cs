using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


//public struct CardData
//{
//    public string Name; // = "이름";
//    public int Cost; // = 0;
//    public string Descript; // = "카드 종류에 대한 설명";
//    public Sprite Sprite; // = "카드 이미지";
//}
public class Card : MonoBehaviour
{

    [SerializeField] SpriteRenderer _card;
    [SerializeField] SpriteRenderer _character;
    [SerializeField] SpriteRenderer _descBG;

    [SerializeField] SpriteRenderer[] _rararityBG;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _costText;
    [SerializeField] TMP_Text _descText;
    [SerializeField] TMP_Text _tagText;
    [SerializeField] SpriteRenderer _outline;
    private Tween curOutlineTween;


    public PRS OriginPRS;
    public Order CardOrder;
    //private Animator _anim;
    CardData _defaultData = null;
    public CardData DefaultData => _defaultData;
    //string _defaultDesc = null;
    public CardData Data { get; private set; }
    public void SetupForClone(Card original)
    {
        // 1. 데이터는 새로 클론하지 않고 원본의 '현재 데이터'를 그대로 공유합니다.
        // (이렇게 해야 원본의 공격력 버프 등이 유지됩니다.)
        this.Data = original.Data;
        this._defaultData = original.DefaultData;

        // 2. 이펙트 연출 관련 정보 복사 (이게 없으면 연출이 깨집니다)
        this.RepeatEffect = original.RepeatEffect;
        this.AllEnemies = original.AllEnemies;

        // 3. 강제 실행 플래그 설정
        this._isForce = true;

        // 4. [가장 중요] 새로운 몸에 맞는 로직 재조립
        CardAbility.SetCardAbility(this);

        // 5. 시각적으로 수치 갱신 (선택 사항)
        RefreshAllDesc();
    }
    public string Desc;
    public int ID;      // 일단 혹시 몰라서 만들었으나, Data.Id로 받을 수 있음.

    public bool Block;

    public bool Used;

    public bool Selected;       // SelectCard = 내가 지금 들고있는 카드 (카드 사용 용), selectedCard = 내가 선택한 카드 (선택해서 버리기 용)

    public bool IsEnqueued;

    public CardAbility CardAbility = new();

    public Action ImmediatelyUseCard { get; private set; }
    public Action FailureBeforeUseCard { get; private set; }
    public Action SuccessBeforeUseCard { get; private set; }

    public List<Func<UniTask<bool>>> UseConditions { get; private set; }

    //public Action<Card> CardAction { get; private set; }
    //public Action CardAction { get; private set; }
    public Func<PlayContext, UniTask> CardTask { get; private set; }
    //public AsyncLazy CardLazy { get; private set; }

    private readonly StringBuilder _sb = new StringBuilder();

    bool _isForce = false;
    //public void SetForce(bool value) => _isForce = value;
    public bool IsForce => _isForce;

    public struct CardDataValue
    {
        public int Damage, Shield, Count, Draw, Discard, Remove, HP, Cost;

        // 원본 데이터로 초기화하는 생성자 (꼬임 방지용)
        public CardDataValue(CardData defaultData)
        {
            Damage = defaultData.Damage;
            Shield = defaultData.Shield;
            Count = defaultData.Count;
            Draw = defaultData.Draw;
            Discard = defaultData.Discard;
            Remove = defaultData.Remove;
            HP = defaultData.HP;
            Cost = defaultData.Cost;
        }
    }
    private CardDataValue _displayState;

    private CardDataValue _upgradeState;

    public void ResetForNextBattle()
    {
        _upgradeState = default; // 장부 초기화
    }

    public void AddCardBuff(SpecialTagType type, int amount)
    {
        switch (type)
        {
            case SpecialTagType.AddDamage:
                _upgradeState.Damage += amount;
                break;
            case SpecialTagType.AddShield:
                _upgradeState.Shield += amount;
                break;
            case SpecialTagType.AddCount:
                _upgradeState.Count += amount;
                break;
            case SpecialTagType.AddDraw:
                _upgradeState.Draw += amount;
                break;
            case SpecialTagType.AddDiscard:
                _upgradeState.Discard += amount;
                break;
            case SpecialTagType.AddRemove:
                _upgradeState.Remove += amount;
                break;
            case SpecialTagType.AddHealHP:
                _upgradeState.HP += amount;
                break;
            case SpecialTagType.AddCost:
                _upgradeState.Cost += amount;
                break;
        }

        // 수치를 바꿨으니 화면에도 반영해줘야겠죠?
        RefreshCardStats();
    }

    public bool Enhanced = false;

    public Enemy CheckTarget { get; private set; } = null;          // 카드 사용 전에 계속 적 체크
    public Enemy TargetEnemy { get; private set; } = null;        // 카드 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    private ReactiveProperty<bool> _isCardUseTiming = new();

    public IReadOnlyReactiveProperty<bool> IsCardUseTiming => _isCardUseTiming;

    public bool CardUseTiming { get; private set; }
    public bool RepeatEffect { get; private set; }
    public bool AllEnemies { get; private set; }

    public bool UnableEffect = false;

    //Dictionary<SpecialTagType, int> _xValueBonuses = new();

    //public int TotalUseDamage = 0;
    //public int IndividualUseDamage = 0;

    //public bool IsKillEnemy = false;



    //public AsyncLazy PlayEffect = null;

    //BoxCollider2D _boxCollider2;

    // Start is called before the first frame update

    //private void OnEnable()
    //{
    //    var a = this.GetComponent<Animator>();
    //    //if (CardManager.Instance.sortBtn != null)
    //    //    CardManager.Instance.sortBtn.interactable = false;
    //    //Destroy(a);
    //}
    public void SetDefaultDate(CardData data)
    {
        _defaultData = data;
    }
    public void Setup(CardData data)
    {
        CardOrder = GetComponent<Order>();
        //_boxCollider2 = GetComponent<BoxCollider2D>();

        SetDefaultDate(data);
        Data = DefaultData.Clone();
        
        //StringBuilder sb = new StringBuilder(_defaultData.Descript);
        //sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
        //sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
        //sb.Replace("{Count}", (_defaultData.Count).ToString());
        //sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        //sb.Replace("{Discard}", (_defaultData.Discard).ToString());
        //_defaultDesc = sb.ToString();
        //Desc = sb.ToString();


        _nameText.text = Data.Name;
        _character.sprite = Data.Sprite;
        _character.size = new Vector2(7.8f, 4.6f);
        switch (Data.CardTag)
        {
            case CardTag.SingleAttack:
            case CardTag.AllAttack:
            case CardTag.RandomAttack:
                _tagText.text = "Attack";
                break;
            case CardTag.SkillTargetSelf:
            case CardTag.SkillTargetSingle:
            case CardTag.SkillTargetAll:
            case CardTag.SkillTargetRandom:
                _tagText.text = "Skill";
                break;
        }
        switch (Data.CardRarity)
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
        CardAbility.SetCardAbility(this);
        //CardAction = CardAbility.SetCardActionAbility(this);     // Action<Card> 버전 (드로우 시간 체크 때문에 일단 사용하지 않음.)
        //CardTask = CardAbility.SetCardAbility(this);              // 그냥 카드어빌리티 실행하면 Task 바꾸도록 함.
        //CardAbility.SetCardAbility(this);     // 사용 전에 받기 때문에 굳이 사용 안 해도 됨. 나중에 따로 필요하면 킬 것.
        //CardLazy = CardAbility.SetCardLazyAbility(this);        // 중복 해결을 위해 Lazy를 써봄.

        //SendAnimEvent sendAnimEvent = Data.Effect.GetComponent<SendAnimEvent>();
        if (Data.Effect != null && Data.Effect.TryGetComponent<SendAnimEvent>(out SendAnimEvent sendAnimEvent))     // 나중에 모든 이펙트에 SendAnimEvent 넣으면 그냥 GetComponent 하면 됨.
        {
            RepeatEffect = sendAnimEvent.RepeatEffect;
            AllEnemies = sendAnimEvent.AllEnemies;
        }

        RefreshAllDesc();        // 글(string) 데이터만 초기화
        //Data = _defaultData;
        ////Data.Name = data.Name;
        ////Data.ID = data.ID;
        ////Data.Cost = data.Cost;
        ////Data.Damage = data.Damage;
        ////Data.EnhancedDamage = data.EnhancedDamage;
        ////Data.Shield = data.Shield;
        ////Data.EnhancedDefence = data.EnhancedDefence;
        ////Data.Count = data.Count;
        ////Data.EnhancedCount = data.EnhancedCount;
        ////Data.Draw = data.Draw;
        ////Data.EnhancedDraw = data.EnhancedDraw;
        ////Data.cardUseDelay = data.cardUseDelay;
        ////Data.Descript = data.Descript;
        ////Data.Sprite = data.Sprite;
        ////Data.CardTag = data.CardTag;

        //nameText.text = Data.Name;
        //costText.text = Data.Cost.ToString();
        //desText.text = Data.Descript;
        //character.sprite = Data.Sprite;

        //CardTask = CardAbility.SetCardTaskAbility(Data.ID);
    }
    private void OnEnable()
    {
        // 구독: 스탯이나 타겟이 바뀌면 '기본 스탯 갱신' 실행
        GameEvents.OnBaseStatsChanged += RefreshCardStats;
        GameEvents.OnTargetChanged += RefreshTargetCardStats;

        // 구독: 플레이 상태(첫 카드 등)가 바뀌면 '특수 조건 갱신' 실행 => 그냥 처음부터 쭉 실행하도록 변경
        GameEvents.OnPlayStateChanged += RefreshCardStats;
        //GameEvents.OnPlayStateChanged += RefreshSpecialCondition;

        if (_defaultData != null)
            RefreshAllDesc();
    }

    private void OnDisable()
    {
        GameEvents.OnBaseStatsChanged -= RefreshCardStats;
        GameEvents.OnTargetChanged -= RefreshTargetCardStats;
        GameEvents.OnPlayStateChanged -= RefreshCardStats;
        //GameEvents.OnPlayStateChanged -= RefreshSpecialCondition;
    }

    public void RefreshAllDesc()
    {
        RefreshCardStats(); // 특수 조건도 그냥 여기서 계산함.
        //RefreshSpecialCondition();
    }
    public void SetCardImmediately((Action immediately, Action failure, Action success)? action)
    {
        this.ImmediatelyUseCard = action?.immediately;
        this.FailureBeforeUseCard = action?.failure;
        this.SuccessBeforeUseCard = action?.success;
    }

    public void SetCardTask(Func<PlayContext, UniTask> cardTask)
    {
        this.CardTask = cardTask;
    }

    public void SetUseConditions(List<Func<UniTask<bool>>> condition)
    {
        this.UseConditions = condition;
    }

    public async UniTask<bool> CheckUseConditions()
    {
        //CardAbility.SetCardAbility(this);
        if (UseConditions == null || UseConditions.Count == 0)
        {
            return true;
        }

        foreach (var task in UseConditions)
        {
            if (task == null) continue;

            // 실행 후 결과 확인
            var result = await task();

            // 하나라도 실패하면 즉시 false 반환
            if (!result) return false;
        }
        return true;
    }

    public async UniTask UseTask(PlayContext context)
    {
        //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
        //UniTask uniTask = UniTask.Create(() => CardTask);
        //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
        if (CardTask == null) return;
        await CardTask(context);
        if (Data.DamageOrder >= 0)
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.ATKUp, out _);     // 공격 이후 공격력 감소 효과 적용되는 경우
        }
        if (Data.DamageOrder >= 0)        // 기존 데이터값에서 쉴드값이 0이 아닌 경우 -> 방어 관련 카드라는 뜻
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.DEFUp, out _);     // 마찬가지
        }
    }
    //public async UniTask UseLazy()
    //{
    //    await CardLazy.Task;
    //}
    //public void UseAction()
    //{
    //    CardAction?.Invoke();
    //}

    // --- [1] 기본 수치 및 스탯 계산 (RefreshCardStats) ---
    public void RefreshCardStats()
    {
        // 1. 데미지 계산
        if (Data.DamageOrder >= 0)
        {
            int baseDmg = Mathf.Max(0, DefaultData.Damage + InGameManager.Instance.Player.AttackPower.Value);
            Data.Damage = baseDmg + _upgradeState.Damage;

            //if (CheckTarget != null)
            //{
            //    if (CheckTarget.GetStatusEffect(StatusEffect.Vulnerable, out _))
            //    {
            //        damage = MathUtil.MultiplierToInt(Data.Damage, 1.5f);
            //    }
            //}

            //if (damage == -1) damage = Data.Damage;
        }

        // 2. 방어력 계산
        //int shield = -1;
        if (Data.ShieldOrder >= 0)
        {
            int baseShield = Mathf.Max(0, DefaultData.Shield + InGameManager.Instance.Player.DefensePower.Value);
            Data.Shield = baseShield + _upgradeState.Shield;
        }

        // 3. 기타 수치 갱신 (Count, Draw, Discard, Remove, HP)
        Data.Count = DefaultData.Count + _upgradeState.Count;
        Data.Draw = DefaultData.Draw + _upgradeState.Draw;
        Data.Discard = DefaultData.Discard + _upgradeState.Discard;
        Data.Remove = DefaultData.Remove + _upgradeState.Remove;
        Data.HP = DefaultData.HP + _upgradeState.HP;

        // 4. [중요] 기본 코스트 초기화 (레이어 1: 원본 + 유물/포션 등 스탯)
        if (DefaultData.Cost == -1) 
        {
            Data.Cost = -1;
        }
        else 
        {
            // 여기에 유물/포션에 의한 코스트 변동이 있다면 더해줌 (예: + InGameManager.Instance.Player.GlobalCostMod)
            Data.Cost = DefaultData.Cost + _upgradeState.Cost; 
        }

        // 스탯 계산이 끝났으니, 이어서 특수 조건(첫 카드 등)을 계산하러 갑니다.
        // 이렇게 하면 스탯이 바뀔 때 코스트 조건도 항상 최신화됩니다.
        RefreshSpecialCondition();
    }
    public void RefreshSpecialCondition()
    {
        // 값 변경 또는 덧셈뺄셈
        if (DefaultData.Cost != -1)
        {
            int finalCost = Data.Cost;

            foreach (var tag in Data.MasterTags)
            {
                if (tag.Tag.Is(SpecialTag.FirstCard) && !TurnManager.Instance.GetFirstCardPlayed())
                {
                    int amt = tag.XAmount ? InGameManager.Instance.Player.CurHolo : (int)tag.Amount;
                    if (tag.Type.Is(SpecialTagType.ReduceCost)) finalCost -= amt;
                    else if (tag.Type.Is(SpecialTagType.ChangeCost)) finalCost = amt;
                }
            }
            if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.CostZero, out _)) finalCost = 0;
            _displayState.Cost = Mathf.Max(0, finalCost);
            Data.Cost = _displayState.Cost;
        }
        if (Data.MasterTags.Any(t => t.Tag.Is(SpecialTag.CardCountValue)))
        {
            // CheckUsedCardCount 태그를 찾습니다.
            var checkTag = Data.MasterTags.Find(t => t.Tag.Is(SpecialTag.CheckUsedCardCount));
            if (!checkTag.Tag.Is(SpecialTag.None))
            {
                // 짝꿍인 CardCountValue 태그도 찾습니다.
                var valueTag = Data.MasterTags.Find(t => t.Tag.Is(SpecialTag.CardCountValue));
                if (!valueTag.Tag.Is(SpecialTag.None))
                {
                    // 현재 매니저에서 카운트 가져오기
                    int count = GetCurrentPlayedCount(checkTag.Type.Special);

                    if (valueTag.Type.Is(SpecialTagType.Over))
                    {
                        int amt = valueTag.XAmount ? InGameManager.Instance.Player.CurHolo : (int)valueTag.Amount;
                        if (count > amt)
                        {
                            UnableEffect = false;
                        }
                        else
                        {
                            UnableEffect = true;
                        }
                    }

                    // 보너스 수치 계산 (count * 배율)
                    int bonus = MathUtil.MultiplierToInt(count, valueTag.Amount);

                    // 실제 데이터에 즉시 반영
                    ApplyStatBonus(valueTag.Type.Special, bonus);
                }
            }
        }

        if (Data.Cost == 0)
        {
            if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.ZeroCostDamage, out int zeroCostDamage))
            {
                Data.Damage += zeroCostDamage;
            }
        }



        // 곱하는 건 맨 마지막에
        if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.Weaking, out _))
        {
            Data.Damage = MathUtil.MultiplierToInt(Data.Damage, 0.75f);
        }

        RefreshTargetCardStats(CheckTarget);
    }

    public void RefreshTargetCardStats(Enemy enemy = null)
    {
        _displayState.Damage = Data.Damage;
        _displayState.Shield = Data.Shield;
        _displayState.Cost = Data.Cost;
        _displayState.Count = Data.Count;
        _displayState.Draw = Data.Draw;
        _displayState.Discard = Data.Discard;
        _displayState.Remove = Data.Remove;
        _displayState.HP = Data.HP;

        this.CheckTarget = enemy;

        if (CardManager.Instance.SelectCard == this && CheckTarget != null)
        {
            if (CheckTarget.GetStatusEffect(StatusEffect.Vulnerable, out _))
            {
                _displayState.Damage = MathUtil.MultiplierToInt(Data.Damage, 1.5f);
            }
        }

        // 최종 출력용 데미지만 업데이트하고 UI 다시 그림
        UpdateDescriptionUI();
    }

    public void UpdateDescriptionUI()
    {
        _sb.Clear();
        _sb.Append(DefaultData.Descript);

        // [최적화 핵심] Data가 아니라 보정값이 다 들어있는 _displayState를 사용합니다!
        _sb.Replace("{Damage}", GetColorValue(_displayState.Damage, DefaultData.Damage));
        _sb.Replace("{Shield}", GetColorValue(_displayState.Shield, DefaultData.Shield));
        _sb.Replace("{Count}", GetColorValue(_displayState.Count, DefaultData.Count));
        _sb.Replace("{Draw}", GetColorValue(_displayState.Draw, DefaultData.Draw));
        _sb.Replace("{Discard}", GetColorValue(_displayState.Discard, DefaultData.Discard));
        _sb.Replace("{Remove}", GetColorValue(_displayState.Remove, DefaultData.Remove));
        _sb.Replace("{HP}", GetColorValue(_displayState.HP, DefaultData.HP));

        Desc = _sb.ToString();
        _descText.text = Desc;

        // 코스트 텍스트 출력
        if (DefaultData.Cost == -1)
        {
            _costText.text = "X";
        }
        else
        {
            _costText.text = Data.Cost.ToString();
            if (Data.Cost < DefaultData.Cost) _costText.text = $"<color=#4CAF50>{Data.Cost}</color>";
            else if (Data.Cost > DefaultData.Cost) _costText.text = $"<color=#B71C1C>{Data.Cost}</color>";
            // 코스트 색상 피드백
            //_costText.color = (Data.Cost < DefaultData.Cost) ? Color.green : Color.white;
        }
    }

    private string GetColorValue(int current, int original)
    {
        if (current > original) return $"<color=#4CAF50>{current}</color>";
        if (current < original) return $"<color=#B71C1C>{current}</color>";
        return current.ToString();
    }

    private int GetCurrentPlayedCount(SpecialTagType type)
    {
        var tm = TurnManager.Instance;
        if (tm == null) return 0;

        return type switch
        {
            SpecialTagType.TurnAttack => tm.GetTurnAttackCount(),
            SpecialTagType.TurnSkill => tm.GetTurnSkillCount(),
            SpecialTagType.TurnAll => tm.GetTurnAttackCount() + tm.GetTurnSkillCount(),
            SpecialTagType.BattleAttack => tm.GetBattleAttackCount(),
            SpecialTagType.BattleSkill => tm.GetBattleSkillCount(),
            SpecialTagType.BattleAll => tm.GetBattleAttackCount() + tm.GetBattleSkillCount(),
            SpecialTagType.BattleZeroCost => tm.GetBattleZeroCostCount(),
            _ => 0
        };
    }
    private void ApplyStatBonus(SpecialTagType type, int amount)
    {
        switch (type)
        {
            case SpecialTagType.AddCount: Data.Count = amount; break;
            case SpecialTagType.AddDamage: Data.Damage += amount; break;
            case SpecialTagType.AddShield: Data.Shield += amount; break;
            case SpecialTagType.AddDraw: Data.Draw += amount; break;
            case SpecialTagType.AddHealHP: Data.HP += amount; break;
            case SpecialTagType.AddDamageHP: Data.HP -= amount; break;
        }
    }
    //public void RefreshCardDesc(/*bool release = false*/)
    //{
    //    //if (release)
    //    //{
    //    //    Data.Damage = _defaultData.Damage;
    //    //    Data.Shield = _defaultData.Shield;
    //    //    Data.Count = _defaultData.Count;
    //    //    Data.Draw = _defaultData.Draw;
    //    //    costText.text = _defaultData.Cost.ToString();
    //    //    desText.text = _defaultDesc;
    //    //}
    //    //else
    //    //{
    //    int cost = -1;
    //    int damage = -1;
    //    int shield = -1;
    //    int count = -1;
    //    int draw = -1;
    //    int discard = -1;
    //    int remove = -1;
    //    int hp = -1;
    //    if (Data.DamageOrder >= 0)
    //    {
    //        Data.Damage = Mathf.Max(0, DefaultData.Damage + InGameManager.Instance.Player.AttackPower.Value);
    //        if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.Weaking, out _))
    //        {
    //            Data.Damage = MathUtil.MultiplierToInt(Data.Damage, 0.75f);
    //            //damage = Mathf.FloorToInt(Data.Damage * 0.75f + 0.50001f);
    //        }
    //        if (CheckTarget != null)        // 해당 수치는 실제 적용이 아닌 보여주기 값.
    //        {
    //            if (CheckTarget.GetStatusEffect(StatusEffect.Vulnerable, out _))
    //            {
    //                damage = MathUtil.MultiplierToInt(Data.Damage, 1.5f);
    //            }
    //        }
    //        if (damage == -1)
    //        {
    //            damage = Data.Damage;
    //        }
    //    }
    //    if (Data.ShieldOrder >= 0)
    //    {
    //        Data.Shield = Mathf.Max(0, DefaultData.Shield + InGameManager.Instance.Player.DefensePower.Value);

    //        if (shield == -1)
    //        {
    //            shield = Data.Shield;
    //        }
    //    }

    //    Data.Count = DefaultData.Count + 0;
    //    count = Data.Count;
    //    Data.Draw = DefaultData.Draw + 0;
    //    draw = Data.Draw;
    //    Data.Discard = DefaultData.Discard + 0;
    //    discard = Data.Discard;
    //    Data.Remove = DefaultData.Remove + 0;
    //    remove = Data.Remove;
    //    Data.HP = DefaultData.HP + 0;
    //    hp = Data.HP;

    //    string GetColorValue(int current, int original)
    //    {
    //        if (current > original)
    //            // 차분한 딥 그린 (성장/버프 느낌)
    //            return $"<color=#4CAF50>{current}</color>";
    //        else if (current < original)
    //            // 묵직한 다크 레드 (상처/디버프 느낌)
    //            return $"<color=#B71C1C>{current}</color>";
    //        else                         // 동일: 검정색 (기본 색상이 검정이라면 태그를 빼도 됩니다)
    //            return $"{current}";
    //    }

    //    StringBuilder sb = new StringBuilder(DefaultData.Descript);
    //    sb.Replace("{Damage}", GetColorValue(damage, DefaultData.Damage));
    //    sb.Replace("{Shield}", GetColorValue(Data.Shield, DefaultData.Shield));
    //    sb.Replace("{Count}", GetColorValue(Data.Count, DefaultData.Count));
    //    sb.Replace("{Draw}", GetColorValue(Data.Draw, DefaultData.Draw));
    //    sb.Replace("{Discard}", GetColorValue(Data.Discard, DefaultData.Discard));
    //    sb.Replace("{Remove}", GetColorValue(Data.Remove, DefaultData.Remove));
    //    sb.Replace("{HP}", GetColorValue(Data.HP, DefaultData.HP));
    //    Desc = sb.ToString();

    //    if (DefaultData.Cost == -1)
    //    {
    //        Data.Cost = -1;
    //        _costText.text = "X";
    //    }
    //    else
    //    {
    //        Data.Cost = DefaultData.Cost + 0;
    //        foreach (var tag in Data.MasterTags)
    //        {
    //            if (tag.Tag.Is(SpecialTag.FirstCard) && !TurnManager.Instance.GetFirstCardPlayed())
    //            {
    //                if (tag.Type.Is(SpecialTagType.ReduceCost))
    //                {
    //                    Data.Cost = Mathf.Max(0, Data.Cost - (int)tag.Amount);
    //                }

    //                if (tag.Type.Is(SpecialTagType.ChangeCost))
    //                {
    //                    Data.Cost = (int)tag.Amount;
    //                }
    //            }

    //            if (tag.Tag.Is(StatusEffect.CostZero))
    //            {
    //                Data.Cost = 0;
    //            }
    //        }
    //        _costText.text = (DefaultData.Cost + 0).ToString();
    //    }
    //    _descText.text = Desc;
    //    //}

    //    //nameText.text = Data.Name;
    //    //costText.text = Data.Cost.ToString();
    //    ////desText.text = release? _defaultDesc : Desc;
    //    //character.sprite = Data.Sprite;
    //}

    //public void ChangeCardDesc(/*string data*/)
    //{
    //    StringBuilder sb = new StringBuilder(_defaultData.Descript);
    //    sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
    //    sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
    //    sb.Replace("{Count}", (_defaultData.Count).ToString());
    //    sb.Replace("{Draw}", (_defaultData.Draw).ToString());
    //    sb.Replace("{Discard}", (_defaultData.Discard).ToString());
    //    Desc = sb.ToString();
    //    RefreshCardDesc();
    //    //switch (data)
    //    //{
    //    //    case "Attack":
    //    //        sb.Replace("{Damage}", (_defaultData.Damage+InGameManager.Instance.player.AttackPower.Value).ToString());
    //    //        Desc = sb.ToString();
    //    //        break;
    //    //    case "Shield":
    //    //        sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
    //    //        Desc = sb.ToString();
    //    //        break;
    //    //    case "Count":
    //    //        break;
    //    //    case "Draw":
    //    //        break;
    //    //}
    //}

    //public void Setup(int id)
    //{
    //    Data = InGameManager.Instance.FindCardData(id);

    //    _nameText.text = Data.Name;
    //    _costText.text = Data.Cost.ToString();
    //    _descText.text = Data.Descript;
    //    _character.sprite = Data.Sprite;

    //    //CardAction = CardAbility.SetCardActionAbility(this);
    //    //CardAbility.SetCardAbility(this);
    //    //CardLazy = CardAbility.SetCardLazyAbility(this);

    //}

    //public void EnhancedCard()
    //{
    //    if (Enhanced) return;
    //    Enhanced = true;
    //    Setup(Data.ID * 10);
    //}

    public void ResetCard()
    {
        _outline.gameObject.SetActive(false);
    }

    //public void CheckTargetTemp(Enemy enemy)
    //{
    //    CheckTarget = enemy;
    //    if (TargetEnemy != null && enemy == null) return;
    //    RefreshCardDesc();
    //}

    public void Target(Enemy enemy)     // 이거 필요없음. 죽는 적은 애초에 지정이 안 되기 때문에 따로 타켓 안 해도 됨.
    {
        TargetEnemy = enemy;
        //if (enemy == null)
        //    RefreshCardDesc();
    }

    public async UniTask TurnOnOutline(bool isOn)
    {
        if (isOn)
        {
            curOutlineTween?.Kill();
            _outline.material.SetFloat("_Thickness", 0.9f);
            _outline.gameObject.SetActive(true);
            curOutlineTween = _outline.material.DOFloat(1f, "_Thickness", 0.3f).SetUpdate(true);
        }
        else
        {
            curOutlineTween?.Kill();
            curOutlineTween = _outline.material.DOFloat(0.9f, "_Thickness", 0.5f).SetUpdate(true);
            await curOutlineTween.ToUniTask(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            _outline.gameObject.SetActive(false);
        }
    }

    public async UniTask TaskMoveTransform(PRS prs, CancellationToken cancellationToken, float dotweenTime = 0, Ease ease = Ease.OutQuad)
    {
        //AutoSyncTr(dotweenTime).Forget();           // 그냥 여기서 켰다가 밑에서 꺼도 되지만, 그냥 함수 하나로 퉁치기
        await UniTask.WhenAll(
        //AutoSyncTr(dotweenTime),
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/,
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/,
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/
            );
        //if (battleCancel)
        //{
        //    //AutoSyncTr(dotweenTime).Forget();           // 그냥 여기서 켰다가 밑에서 꺼도 되지만, 그냥 함수 하나로 퉁치기
        //    await UniTask.WhenAll(
        //    //AutoSyncTr(dotweenTime),
        //    transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
        //    transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
        //    transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/
        //    );
        //}
        //else
        //{
        //    await UniTask.WhenAll(
        //    //AutoSyncTr(dotweenTime),
        //    transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy()),
        //    transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy()),
        //    transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy())
        //    );
        //}
    }
    public void MoveTransform(PRS prs, bool useDotween = false, float dotweenTime = 0, Ease ease = Ease.OutQuad/*, bool ignoreTimeScale = false*/)
    {
        //Physics2D.SyncTransforms();
        //Physics2D.autoSyncTransforms = true;
        if (useDotween)
        {
            //AutoSyncTr(dotweenTime).Forget();
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease);
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease);
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
            //if (InGameManager.Instance.PauseInt != 0)
            //{
            //    Physics2D.SyncTransforms();
            //    //SetPRSCollider();
            //}
        }
    }

    public void CardTiming()
    {
        //CardUseTiming = true;
        _isCardUseTiming.Value = true;
    }

    public void UseTimingReset()
    {
        //CardUseTiming = false;
        _isCardUseTiming.Value = false;
    }

    public async UniTaskVoid WaitUnblock(float waitTime)
    {
        Block = true;
        await UniTask.WaitForSeconds(waitTime, true);       // 카드를 가져오기 위해 블락하는 거라, TimeScale은 무시함.
        Block = false;
    }

    public void BlockCard()
    {
        Block = true;
    }
    public void UnblockCard()
    {
        Block = false;
    }

    public void FailedUseCard()
    {
        Block = false;
        Used = false;
    }

    //public void CheckEnemyDead()
    //{
    //    int count = (Data.Count == 0) ? 1 : Data.Count;
    //    switch (Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _))
    //                TargetEnemy.CheckIfDead(Mathf.RoundToInt(Data.Damage * InGameManager.Instance.Player.CriticalDamage.Value * 0.01f +0.0001f), count);
    //            else
    //            {
    //                TargetEnemy.CheckIfDead(Data.Damage, count);
    //            }
    //            break;
    //        case CardTag.AllAttack:
    //            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //            {
    //                if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _))
    //                    enemy.CheckIfDead(Mathf.RoundToInt(Data.Damage * InGameManager.Instance.Player.CriticalDamage.Value * 0.01f + 0.0001f), count);
    //                else
    //                {
    //                    enemy.CheckIfDead(Data.Damage, count);
    //                }
    //            }
    //            break;
    //        default: break;
    //    }
    //}

    public async UniTask<bool> BeforeUsingCard()
    {
        if (DefaultData.Cost == -1)
        {
            if (InGameManager.Instance.Player.CurHolo >= 0)
            {
                Data.Cost = InGameManager.Instance.Player.CurHolo;
            }
            else
            {
                Target(null);
                return false;
            }
        }
        else
        {
            if (!_isForce && InGameManager.Instance.Player.CurHolo < Data.Cost)
            {
                Target(null);
                return false;
            }
        }

        if (Data.CardTag == CardTag.SingleAttack || Data.CardTag == CardTag.SkillTargetSingle)
        {
            if (_isForce && TargetEnemy == null)
            {
                var enemies = EnemyManager.Instance.EnemyList;
                if (enemies != null && enemies.Count > 0)
                {
                    // 3. 0부터 enemies.Count - 1 사이의 무작위 인덱스 선택
                    // Random.Range(int min, int max)에서 int 버전은 max가 제외(Exclusive)되므로 
                    // .Count를 그대로 넣으면 딱 맞습니다.
                    int randomIndex = UnityEngine.Random.Range(0, enemies.Count);

                    // 4. 무작위로 선택된 적을 타겟으로 설정
                    Target(enemies[randomIndex]);
                }
                else
                {
                    // 5. 만약 적이 없다면 카드를 사용할 수 없으므로 false 반환
                    Target(null);
                    return false;
                }
            }
            else if (TargetEnemy == null)
            {
                Target(null);
                return false;
            }
        }

        //MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        if (!await CheckUseConditions())
        {
            Target(null);
            return false;
        }

        if (!_isForce)
        {
            InGameManager.Instance.Player.AddCurHolo(-Data.Cost);
        }

        // XValue 태그가 있다면 스탯 뻥튀기 (Data.Cost 기준)
        //ApplyXValueEffects();

        Used = true;
        //CardOrder.SetOriginOrder(-10);

        //CheckEnemyDead();

        return true;
    }

    public async UniTask AfterCardAbility(bool endBattle = false)
    {
        //RevertXValueEffects();
        if (IsForce)
        {
            await CardManager.Instance.RemoveCopyCard(this);
            CardManager.Instance.NowPlayedCard = null;
            return;
        }
        await TaskMoveTransform(new PRS(CardManager.Instance.CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), this.GetCancellationTokenOnDestroy(), CardUtils.ThrowAwayCardDelay);

        if (!endBattle)
        {
            CardManager.Instance.CardDummy.Add(this);
            InGameUIManager.Instance.SetDummyCount();
        }
        Block = false;
        Used = false;
        gameObject.SetActive(false);
    }

    //private void ApplyXValueEffects()
    //{
    //    if (DefaultData.Cost != -1) return;
    //    if (Data.MasterTags == null || Data.MasterTags.Count == 0) return;

    //    int xValue = Data.Cost;
    //    _xValueBonuses.Clear();

    //    foreach (var tag in Data.MasterTags)
    //    {
    //        if (!tag.Tag.Is(SpecialTag.XValue)) continue;

    //        SpecialTagType target = tag.Type.Special;
    //        //float.TryParse(tag.Amount, out float multiple);
            
    //        //int totalValue = Mathf.FloorToInt(xValue * multiple + 0.50001f);
    //        int totalValue = MathUtil.MultiplierToInt(xValue, tag.Amount);

    //        print(totalValue);

    //        _xValueBonuses[target] = totalValue;


    //        switch (target)
    //        {
    //            case SpecialTagType.AddCount: Data.Count = totalValue; break;
    //            case SpecialTagType.AddDamage: Data.Damage += totalValue; break;
    //            case SpecialTagType.AddShield: Data.Shield += totalValue; break;
    //            case SpecialTagType.AddDraw: Data.Draw += totalValue; break;
    //            case SpecialTagType.AddHealHP: Data.HP += totalValue; break;
    //            case SpecialTagType.AddDamageHP: Data.HP -= totalValue; break;
    //        }
    //    }
    //}

    //private void RevertXValueEffects()
    //{
    //    if (DefaultData.Cost != -1) return;
    //    // 1. 스탯 원복
    //    foreach (var bonus in _xValueBonuses)
    //    {
    //        switch (bonus.Key)
    //        {
    //            case SpecialTagType.AddCount: Data.Count = 1; break; // 기본값 복구
    //            case SpecialTagType.AddDamage: Data.Damage -= bonus.Value; break;
    //            case SpecialTagType.AddShield: Data.Shield -= bonus.Value; break;
    //            case SpecialTagType.AddDraw: Data.Draw -= bonus.Value; break;
    //            case SpecialTagType.AddHealHP: Data.HP -= bonus.Value; break;
    //            case SpecialTagType.AddDamageHP: Data.HP += bonus.Value; break;
    //        }
    //    }
    //    _xValueBonuses.Clear();
    //    Data.Cost = -1;

    //    //// 2. 코스트 원복 (X코스트 카드인 경우)
    //    //if (DefaultData.Cost == -1)
    //    //{
    //    //    Data.Cost = -1;
    //    //}
    //}



    //async UniTask AutoSyncTr(float time)              // TimeScale = 0 되는 곳에서 그냥 true함.
    //{
    //    Physics2D.autoSyncTransforms = true;
    //    await UniTask.WaitForSeconds(time, true).SuppressCancellationThrow();
    //    Physics2D.autoSyncTransforms = false;
    //}

    void OnMouseOver()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseDown(this);
        //if (InGameManager.Instance.blockClick || TurnManager.Instance.IsLoading)
        //    return;
        //comeBackCard = false;
        //draggable = true;
        //InGameManager.Instance.blockClick = true;
    }

    void OnMouseUp()
    {
        CardManager.Instance.CardMouseUp(this);


        //if (comeBackCard || TurnManager.Instance.IsLoading)
        //    return;
        
        //_rigid.isKinematic = false;


        //CollisionChecker(RayCastToken);

        //SoundManager.Instance.Play("Sounds/Effect/CardHoldSound");
        //CardManager.Instance.sortBtn.interactable = false;


        //CardManager.Instance.sortBtn.interactable = true;
    }
    void OnMouseDrag()
    {
        CardManager.Instance.CardDrag(this);

        ////if (!draggable)
        ////    return;
        ////float distance = Camera.main.WorldToScreenPoint(transform.position).z;
        ////print(distance);
        //Vector2 _temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //transform.position = _temp/*new Vector3(_temp.x, _temp.y, -5f)*/;

    }

    public void CardRelease()
    {
        PoolManager.Instance.ReleaseCard(/*this.gameObject, */this);
    }

    //private void OnEnable()
    //{
    //    CardAbility.CancelSource = new();
    //}

    //private void OnDisable()
    //{
    //    CardAbility.CancelSource.Cancel();
    //}

    //private void OnDestroy()
    //{
    //    CardAbility.CancelSource.Cancel();
    //    CardAbility.CancelSource.Dispose();
    //}
}
