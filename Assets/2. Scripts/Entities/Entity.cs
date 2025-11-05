using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract class Entity : MonoBehaviour
{
    //[SerializeField] protected SpriteRenderer entitySprite;
    //[SerializeField] protected Slider slider;   // 나중에 이미지로 변경
    protected Image hpBar;
    protected Image _criticalBar;
    protected GameObject shieldObj;
    protected TMP_Text hpText;
    protected TMP_Text _criticalText;
    protected TMP_Text shieldText;
    protected BoxCollider2D _col2D;
    protected Canvas canvas;


    Transform _statusEffectContent;
    /// <summary>
    /// 0 = amount, 1 = duration
    /// </summary>
    public List<TMP_Text[]> StatusEffectText = new();

    Transform _statusEffectDescContent;
    Transform _statusDescWindow;
    BoxCollider2D _statusDescCol2D;
    /// <summary>
    /// 0 = desc, 1 = duration
    /// </summary>
    public List<TMP_Text[]> StatusEffectDescText = new();
    //public List<TMP_Text> StatusEffectDurationText;

    SendAnimEvent _animEvent;

    bool _isAtk;
    bool _isDied;

    protected Animator animator;
    readonly int _hitAnim = Animator.StringToHash("Hit");
    readonly int _attackAnim = Animator.StringToHash("Attack");
    readonly int _dieAnim = Animator.StringToHash("Die");


    public ReactiveProperty<int> _maxHP = new();
    public ReactiveProperty<int> _curHP = new();
    public ReactiveProperty<int> _shield = new();

    public ReactiveProperty<int> MaxHP { get; private set; } = new();
    public ReactiveProperty<int> CurHP { get; private set; } = new();
    public ReactiveProperty<int> CurShield { get; private set; } = new();

    public ReactiveProperty<int> AttackPower { get; private set; } = new();
    public ReactiveProperty<int> DefensePower { get; private set; } = new();
    public ReactiveProperty<int> HealPower { get; private set; } = new();

    protected ReactiveProperty<int> _useCritical { get; private set; } = new(100);
    protected ReactiveProperty<int> _curCritical { get; private set; } = new();
    protected ReactiveProperty<int> _criticalChance { get; private set; } = new();
    public ReactiveProperty<int> CriticalDamage { get; private set; } = new();

    protected string _specialDesc;

    float _hpRatio;
    /// <summary>
    /// StatusEffect : <StatusEffectType : (amount, duration)>
    /// </summary>
    public Dictionary<StatusEffect, Dictionary<StatusEffectType, (int amount, int duration)>> CurStatusEffectDict = new();

    public Dictionary<(StatusEffect, StatusEffectType), int> StatusEffectTextIdx = new();

    public List<(StatusEffect, StatusEffectType)> CurStatusEffectList = new();
    public List<(StatusEffect, StatusEffectType)> CurStatusEffectPerpetualList = new();

    int _numberOfStatusEffects = 0;
    int _numberOfInformation = 0;

    Color _black = new Color(0f, 0f, 0f);      // 검은색 (#000000)
    Color _white = new Color(1f, 1f, 1f);      // 흰색 (#FFFFFF)
    Color _navy = new Color(0f, 0.137f, 0.4f); // 짙은 남색 (#002366)
    Color _gray = new Color(0.827f, 0.827f, 0.827f); // 연한 회색 (#D3D3D3)
    Color _brown = new Color(0.396f, 0.258f, 0.125f); // 다크 브라운 (#654321)
    Color _purple = new Color(0.294f, 0f, 0.509f); // 딥 퍼플 (#4B0082)
    Color _teal = new Color(0f, 0.502f, 0.502f); // 청록색 (#008080)

    Color _red = new Color(0.85f, 0f, 0f); // 빨간색 (#FF0000)


    //private void Awake()    // start로 할 경우, Subscribe가 실행되지 않음. Awake로 하면 위험할 것 같아서 일단 함수로 빼고 자식 오브젝트에서 Start로 호출
    //{
    //    _maxHP.Subscribe(hp =>
    //    {
    //        slider.maxValue = hp;
    //    });
    //    _curHP.Subscribe(hp =>
    //    {
    //        slider.value = hp;
    //        hpText.text = hp.ToString();
    //    });
    //}
    //public void SetupEntity(int hp, int criticalChance = 10, int criticalDamage = 150)
    //{
    //    //col2d = GetComponent<BoxCollider2D>();
    //    _maxHP.Value = hp;
    //    _curHP.Value = _maxHP.Value;
    //    _criticalChance.Value = criticalChance;
    //    _criticalDamage.Value = criticalDamage;
    //    //slider.value = _maxHP.Value;
    //    //hpText.text = _maxHP.ToString();
    //}
    protected virtual async UniTask<int> BeforeTakeDamage(int damage, Entity attacker = null)
    {
        if (ApplyStatusEffect(StatusEffect.Immunity, out _)) { return 0; }
        if (attacker == null) { return damage; }
        if (ApplyStatusEffect(StatusEffect.Protect, out int amount))
        {
            await Shield(amount);
        }
        return damage;
    }
    protected virtual async UniTask AfterTakeDamage(Entity attacker = null)
    {
        if (attacker == null) { return; }

        if (ApplyStatusEffect(StatusEffect.Vampire, out int vampire))
        {
            await attacker.Heal(vampire);
        }

        if (ApplyStatusEffect(StatusEffect.Reflection, out int amount))
        {
            await attacker.TakeDamage(amount);
        }


        //if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Player)        // 현재 턴이 플레이어 턴일 경우 => 플레이어 턴에 직접 타격을 받은 엔티티는 적
        //{
        //    // 뱀파이어 효과로 적이 공격 받으면, 직접 타격한 엔티티(여기선 플레이어)가 회복.
        //    if (InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.Vampire, out int vampire))
        //    {
        //        await InGameManager.Instance.Player.Heal(vampire);
        //    }

        //    if (ApplyStatusEffect(StatusEffect.Reflection, out int amount))
        //    {
        //        await InGameManager.Instance.Player.TakeDamage(amount);
        //    }
        //}
        //else if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Enemy)    // 반대 상황
        //{
        //    if (EnemyManager.Instance.HitEnemy.ApplyStatusEffect(StatusEffect.Vampire, out int vampire))
        //    {
        //        await EnemyManager.Instance.HitEnemy.Heal(vampire);
        //    }

        //    if (ApplyStatusEffect(StatusEffect.Reflection, out int amount))
        //    {
        //        await EnemyManager.Instance.HitEnemy.TakeDamage(amount);
        //    }
        //}
        //if (ApplyStatusEffect(StatusEffect.Reflection, out int amount))
        //{
        //    if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Player/* && BattleManager.Instance.HitEntity.Item1 != null*/)
        //    {
        //        await InGameManager.Instance.Player.TakeDamage(amount, false);
        //    }
        //    else if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Enemy/* && BattleManager.Instance.HitEntity.Item2 != null*/)
        //    {
        //        //EnemyManager.Instance.HitEnemy.CheckIfDead(amount, 1, false);
        //        await EnemyManager.Instance.HitEnemy.TakeDamage(amount, false);
        //        //BattleManager.Instance.HitEntity.Item2.CheckIfDead(amount, 1, false);
        //        //BattleManager.Instance.HitEntity.Item2.TakeDamage(amount, false).Forget();
        //    }
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dmg">
    /// 데미지 값
    /// </param>
    /// <param name="isHit">
    /// 상대가 직접 타격한 것인지 아닌지 확인.
    /// </param>
    /// <returns></returns>
    public async virtual UniTask<bool> TakeDamage(int dmg, Entity attacker = null)
    {
        dmg = await BeforeTakeDamage(dmg, attacker);
        if (dmg == 0)       // 데미지가 0일 경우, 맞은 후 효과는 발동 X
            return false;
        TextEffect(-dmg).Forget();

        if (CurShield.Value >= dmg)
        {
            CurShield.Value -= dmg;
        }
        else
        {
            dmg -= CurShield.Value;
            CurShield.Value = 0;
            CurHP.Value -= dmg;
            if (ApplyStatusEffect(StatusEffect.Berserker, out int berserker))       // 버서커 효과는 피해를 받을 때만 발동하기에 쉴도로 막혀도 발동하는 AfterTakeDamage와 다르게, 해당 위치에서 체크함.
            {
                AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), berserker);
            }
        }
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //if (stateInfo.IsName("Attack"))
        //animator.SetTrigger(_hitAnim);        // play를 해야 맞을 때마다 실행 가능
        animator.Play(_hitAnim, -1, 0);  // 타격 당하는 애니메이션 실행        => 공격 중에는 딜레이를 주거나 무시하는 코드가 필요할 듯.
        if (CurHP.Value > 0)
        {
            await AfterTakeDamage(attacker);
            return false;
        }

        if (ApplyStatusEffect(StatusEffect.Resurrection, out _))
        {
            CurHP.Value = (int)(MaxHP.Value * 0.5f);
            // 부활 수치 감소 코드 추가
            //_resurrection = true;
            return false;
        }

        //col2d.enabled = false;
        //slider.gameObject.SetActive(false);
        //canvas.gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
        return true;
    }

    public void AtkAnimtiming()
    {
        _isAtk = true;
    }

    public virtual async UniTask AttackAnimation(bool checkAtkTiming = false)
    {
        if (_animEvent == null)
            checkAtkTiming = false;
        //animator.SetTrigger(_attackAnim);
        animator.Play(_attackAnim, -1, 0);  // 공격 애니메이션 실행
        if (checkAtkTiming)
        {
            var cts = new CancellationTokenSource();

            // 타임아웃 설정
            var timeoutTask = UniTask.WaitForSeconds(1f, cancellationToken: cts.Token);

            // UniTask.WaitUntil을 사용하여 조건 충족 대기
            var waitUntilTask = UniTask.WaitUntil(() => _isAtk, cancellationToken: cts.Token);

            // 둘 중 먼저 완료되는 작업에 대한 처리
            await UniTask.WhenAny(waitUntilTask, timeoutTask);
            //await UniTask.WaitUntil(() => { f += Time.deltaTime; return _isAtk; }/*, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token*/);    // 공격하는 모션 중에는 게임이 끝나지 않을 것
            _isAtk = false;
            cts.Cancel();
            cts.Dispose();
        }
    }

    async UniTask TextEffect(int value)
    {
        //if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested) return;
        TMP_Text textEffect = PoolManager.Instance.GetText(/*out TMP_Text textEffect*/);
        textEffect.transform.position = transform.position;
        textEffect.transform.localScale = Vector3.one;
        textEffect.color = value < 0 ?_red : Color.green;
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        textEffect.text = value < 0 ? value.ToString() : $"+{value}";
        float elapsedTime = 0;
        float timeLimit = 1;
        while (elapsedTime < timeLimit)
        {
            textEffect.transform.localScale = Vector3.one * (1+elapsedTime * 0.5f);
            textEffect.color = new Color(textEffect.color.r, textEffect.color.g, textEffect.color.b, 1f - elapsedTime / timeLimit);
            textEffect.transform.Translate(dir * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
        PoolManager.Instance.ReleaseText(textEffect);
    }

    public void DieAnimEnd()
    {
        _isDied = true;
    }

    public virtual async UniTask DieAnimation(bool checkAnim = false)
    {
        if (_animEvent == null)
            checkAnim = false;
        //animator.SetTrigger(_dieAnim);        // play로 해야 바로 사망 가능
        animator.Play(_dieAnim, -1, 0);  // 사망 애니메이션 실행
        //await UniTask.Delay(1000);
        if (checkAnim)
        {
            var cts = new CancellationTokenSource();

            // 타임아웃 설정
            var timeoutTask = UniTask.WaitForSeconds(1f, cancellationToken: cts.Token);

            // UniTask.WaitUntil을 사용하여 조건 충족 대기
            var waitUntilTask = UniTask.WaitUntil(() => _isDied, cancellationToken: cts.Token);

            // 둘 중 먼저 완료되는 작업에 대한 처리
            await UniTask.WhenAny(waitUntilTask, timeoutTask);

            //await UniTask.WaitUntil(() => _isDied);
            cts.Cancel();
        }
        Destroy(gameObject);
    }
    public virtual async UniTask Heal(int amount)
    {
        CurHP.Value = Mathf.Clamp(CurHP.Value + amount, 0, MaxHP.Value);
        TextEffect(amount).Forget();        // 텍스트 뜨는 건 1초 고정으로 하고 패턴 넘어가는 건 밑에서 적당히 정해줘야 보기 편할 듯
        await UniTask.WaitForSeconds(0.2f);
    }

    public virtual async UniTask Shield(int amount)
    {
        CurShield.Value += amount;
        await UniTask.WaitForSeconds(0.2f);
    }

    public virtual async UniTask Shield()
    {
        CurShield.Value = _shield.Value;
        await UniTask.WaitForSeconds(0.2f);
    }

    public bool CheckCritical(/*int damage*/)            // 공격 이후 크리티컬 효과 사용됨
    {
        //bool critical = false;
        //int criticalDamage = damage;
        if (ApplyStatusEffect(StatusEffect.UseCritical, out _))
        {
            //criticalDamage = Mathf.RoundToInt(damage * CriticalDamage.Value * 0.01f);
            Critical(-_useCritical.Value);
            //critical = true;
            //return criticalDamage;
            return true;
        }
        else
        {
            Critical(_criticalChance.Value);        // 크리티컬이 안 터질 때만 찬스가 올라감.
            //if (_curCritical.Value >= _useCritical.Value)
            //{
            //    AddStatusEffect((StatusEffect.UseCritical, StatusEffectType.UseAmountPerpetual), _curCritical.Value/_useCritical.Value);
            //}
            //return damage;
            return false;
        }
        //// 일단 그냥 찬스 올리기로 함.
        //Critical(_criticalChance.Value);
        //if (_curCritical.Value >= _useCritical.Value)
        //{
        //    AddStatusEffect((StatusEffect.UseCritical, StatusEffectType.UseAmountPerpetual), 1);
        //}
        //return critical;
    }

    public int CheckCriticalDamage(int dmamge, bool critical)
    {
        int criticalDamage = dmamge;
        //int criticalDamage = Mathf.RoundToInt(enemyData.Damage * multiple);
        if (critical)
            criticalDamage = Mathf.RoundToInt(dmamge * CriticalDamage.Value * 0.01f + 0.0001f);     // 부동소수점 오류
        //criticalDamage = Mathf.RoundToInt(enemyData.Damage * multiple * CriticalDamage.Value * 0.01f);
        return criticalDamage;
    }

    public void Critical(int amount)
    {
        _curCritical.Value = Mathf.Max(_curCritical.Value + amount, 0);
    }

    public virtual void ShieldReset()
    {
        CurShield.Value = 0;
    }

    protected void EntitySubScribe()
    {
        StartEntity();
        MaxHP
            .Where(hp => hpBar && hp > 0)
            .Subscribe(hp =>
            {
                hpBar.fillAmount = CurHP.Value / (float)hp;
                hpText.text = $"{CurHP.Value}/{hp}";
            }).AddTo(this);
        CurHP
            .Where(_ => hpBar && MaxHP.Value > 0)
            .Subscribe(hp =>
            {
                hpBar.fillAmount = hp / (float)MaxHP.Value;
                hpText.text = $"{hp}/{MaxHP.Value}";
            }).AddTo(this);
        CurShield
            .Where(_ => shieldObj)
            .Subscribe(shield =>
            {
                if (shield <= 0)
                {
                    shieldObj.SetActive(false);
                }
                else
                {
                    shieldObj.SetActive(true);
                    shieldText.text = shield.ToString();
                }
            }).AddTo(this);
            _useCritical
            .Where(critical =>_criticalBar && critical > 0)
            .Subscribe(critical =>
            {
                _criticalBar.fillAmount = _curCritical.Value / (float)critical;
                _criticalText.text = $"{_curCritical.Value}/{critical}";
            }).AddTo(this);
        _curCritical
            .Where(_ => _criticalBar && _useCritical.Value > 0)
            .Subscribe(critical =>
            {
                _criticalBar.fillAmount = (float)critical / _useCritical.Value;
                _criticalText.text = $"{critical}/{_useCritical.Value}";

                GetStatusEffect(StatusEffect.UseCritical, out int amount);
                if (critical >= _useCritical.Value)       // 수치 변경 후, 치명타 가능한 상태
                {
                    if (critical / _useCritical.Value - amount > 0)       // 치명타 뎀증 상태 효과의 개수가 치명타 적중 가능한 상태를 계산한 값보다 적으면, 상태 효과 추가
                        AddStatusEffect((StatusEffect.UseCritical, StatusEffectType.UseAmountPerpetual), critical / _useCritical.Value - amount);
                    else if (critical / _useCritical.Value - amount < 0)  // 치명타 확률이 변경됐는데, 치명타 적중 가능한 상태를 계산한 값보다 치명타 뎀증 상태 효과의 개수가 더 많으면, 상태 효과 제거 (보통 치명타 확률을 감소시켰는데도 불구하고, 상대가 치명타 상태인 경우로 200 / 100 에서 10 감소하여, 190 / 100이 된 경우.)
                        ReduceStatusEffect((StatusEffect.UseCritical, StatusEffectType.UseAmountPerpetual), amount - critical / _useCritical.Value);
                }
                else                                                // 수치 변경 후, 치명타 불가능 상태
                {
                    if (amount > 0)
                        ReduceStatusEffect((StatusEffect.UseCritical, StatusEffectType.UseAmountPerpetual), amount);
                }
            }).AddTo(this);
    }

    protected void StartEntity()
    {
        animator = transform.GetComponentInChildren<Animator>();
        _animEvent = transform.GetComponentInChildren<SendAnimEvent>();
        _animEvent.ParentEntity = this;

        //entitySprite = GetComponent<SpriteRenderer>();
        canvas = transform.Find("EntityCanvas").GetComponent<Canvas>();
        hpBar = FindTransform.ContinueFindChildUIByName(canvas.transform, "HPBar").GetComponent<Image>();
        _criticalBar = FindTransform.ContinueFindChildUIByName(canvas.transform, "CriticalBar").GetComponent<Image>();
        shieldObj = canvas.transform.Find("Shield").gameObject;
        //slider = GetComponentInChildren<Slider>();
        hpText = canvas.transform.Find("HPText").GetComponent<TMP_Text>();
        _criticalText = canvas.transform.Find("CriticalText").GetComponent<TMP_Text>();
        shieldText = shieldObj.transform.Find("ShieldText").GetComponent<TMP_Text>();
        _col2D = GetComponent<BoxCollider2D>();

        _statusEffectContent = FindTransform.ContinueFindChildUIByName(canvas.transform.Find("StatusEffect"), "Content");
        
        _statusDescWindow = canvas.transform.Find("StatusEffectDesc");
        _statusDescWindow.GetComponent<ChildMouseHandler>().ParentEntity = this;
        _statusDescCol2D = _statusDescWindow.GetComponent<BoxCollider2D>();
        
        _statusEffectDescContent = FindTransform.ContinueFindChildUIByName(_statusDescWindow, "Content");

        //StatusEffectAmountText = canvas.transform.Find("StatusEffect").GetComponentsInChildren<TMP_Text>(true);

        //TMP_Text[] statusDesc = _statusDescWindow.GetComponentsInChildren<TMP_Text>(true);
        //int statusEffectIndex = 0;
        //int i = 0;
        //foreach (TMP_Text status in statusDesc)
        //{
        //    if (i++ % 2 == 0)
        //    {
        //        StatusEffectDurationText.Add(status);
        //    }
        //    else
        //    {
        //        StatusEffectDescText.Add(status);
        //    }
        //}
    }
    void OnMouseEnter()
    {
        BoolOnMouseEnter();
    }

    protected virtual bool BoolOnMouseEnter()
    {
        if (_numberOfStatusEffects == 0 && _numberOfInformation == 0) return false;
        if (EventSystem.current.IsPointerOverGameObject())
            return false;
        if (!Cursor.visible) return false;
        if (InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf) return false;
        _statusEffectDescContent.localPosition = Vector3.zero;
        canvas.sortingOrder = 1;        // 이거 없으면 체력 UI에 가려짐
        if (_statusDescWindow.position.x > 6.8f/* && _statusDescWindow.localPosition.x > 0*/)
        {
            _statusDescWindow.localPosition = new Vector3(-_statusDescWindow.localPosition.x, _statusDescWindow.localPosition.y, -1);       // 체력바보다 앞에 있어야 콜라이더에 문제 안 생김.
            _statusDescCol2D.offset = new Vector2(-_statusDescCol2D.offset.x, _statusDescCol2D.offset.y);
        }
        //else if (_statusDescWindow.position.x <= 6.8f && _statusDescWindow.localPosition.x < 0)
        //{
        //    _statusDescWindow.localPosition = new Vector3(-_statusDescWindow.localPosition.x, _statusDescWindow.localPosition.y, -1);
        //    _statusDescCol2D.offset = new Vector2(-_statusDescCol2D.offset.x, 0);
        //}
        _statusDescWindow.gameObject.SetActive(true);
        return true;
    }

    public virtual void OnChildMouseExit()
    {
        canvas.sortingOrder = 0;
        _statusDescWindow.gameObject.SetActive(false);
    }

    //public void AddAndApplyStatusEffect((StatusEffect, StatusEffectType) statusEffect, int amount, int duration = 1)        // Add에 통합됨.
    //{
    //    AddStatusEffect(statusEffect, amount, duration);
    //    ApplyStatusEffect(statusEffect.Item1, out int _);
    //    //if (ApplyStatusEffect(statusEffect.Item1, out int StatusEffectAmount))    // 사실 위에서 Get하고 적용하는 거라 항상 true 값이긴 함. (조건문 없이 그냥 진행해도 괜찮다는 뜻)
    //    //{
    //    //    switch (statusEffect.Item1)
    //    //    {
    //    //        case StatusEffect.ATKUp:
    //    //            AttackPower.Value = StatusEffectAmount;
    //    //            break;
    //    //    }
    //    //}
    //}
    //public void ReduceStatusEffect(Dictionary<StatusEffectType, (int,int)> statusEffectTypeDict, int amount, int duration = -1)
    //{
    //    if (statusEffectTypeDict. == StatusEffectType.InfiniteDuration || statusEffect.Item2 == StatusEffectType.UseAmountInfiniteDuration)
    //        return;
    //}
    public void ReduceStatusEffect((StatusEffect effect, StatusEffectType type) statusEffect, int amount = 0, int duration = 1)        // 턴 감소를 디폴트로 만듦.
    {
        (int getAmount, int getDuration) info = CurStatusEffectDict[statusEffect.effect][statusEffect.type];
        if (info.getAmount - duration > 0 && info.getDuration - amount > 0)           // 감소된 값이 둘 다 양수 => 무한 지속은 기본 음수라서 제외하고, 나머지만 적용됨.
        {
            CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (info.getAmount - amount, info.getDuration - duration);

            ChangeStatusEffectDesc(statusEffect);
        }
        // 둘 중 하나라도 0 이하인 경우, 무한 지속은 예외 처리
        else
        {
            switch (statusEffect.type)
            {
                case StatusEffectType.InfiniteDuration:
                case StatusEffectType.UseAmountInfiniteDuration:
                case StatusEffectType.Perpetual:
                case StatusEffectType.UseAmountPerpetual:
                    if (info.getAmount - amount > 0)
                    {
                        CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (info.getAmount - amount, -1);

                        ChangeStatusEffectDesc(statusEffect);
                    }
                    else
                    {
                        amount = info.getAmount;

                        CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (0, -1);

                        ActivateStatusEffect(statusEffect, false);
                    }
                    break;
                default:
                    amount = info.getAmount;

                    CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (0, 0);

                    ActivateStatusEffect(statusEffect, false);
                    break;
            }

            // 제거되는 경우, 추가 효과가 필요한 녀석들 (기본 스탯은 밑에서 전부 처리)
            switch (statusEffect.effect)
            {
                case StatusEffect.UseCritical:
                    if (GetStatusEffect(StatusEffect.Attack, out _))
                    {
                        ChangeStatusEffectDesc((StatusEffect.Attack, StatusEffectType.Information));
                        //AddStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information), _criticalChance.Value);
                    }
                    break;
            }
            //if (statusEffect.Item2 == StatusEffectType.InfiniteDuration || statusEffect.Item2 == StatusEffectType.UseAmountInfiniteDuration)
            //{
            //    if (info.Item1 - amount > 0)
            //    {
            //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 - amount, -1);

            //        ChangeStatusEffectDesc(statusEffect);
            //    }
            //    else
            //    {
            //        amount = info.Item1;

            //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, -1);

            //        ActivateStatusEffect(statusEffect, false);
            //    }
            //}
            //else
            //{
            //    amount = info.Item1;

            //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, 0);

            //    ActivateStatusEffect(statusEffect, false);
            //}
        }

        switch (statusEffect.effect)
        {
            case StatusEffect.HPUp:
                MaxHP.Value -= amount;
                break;
            case StatusEffect.ATKUp:
                AttackPower.Value -= amount;
                break;
            case StatusEffect.DEFUp:
                DefensePower.Value -= amount;
                break;
            case StatusEffect.HealUp:
                HealPower.Value -= amount;
                break;
            case StatusEffect.CriticalChanceUp:
                _criticalChance.Value -= amount;
                break;
            case StatusEffect.CriticalDamageUp:
                CriticalDamage.Value -= amount;
                break;
            case StatusEffect.UseCritical:      // 적 개체 한정
                if (GetStatusEffect(StatusEffect.Attack, out _))
                {
                    ChangeStatusEffectDesc((StatusEffect.Attack, StatusEffectType.Information));
                }
                break;
        }
    }

    public void AddStatusEffect((StatusEffect effect, StatusEffectType type) statusEffect, int amount, int duration = 1)
    {
        switch (statusEffect.type)
        {
            case StatusEffectType.DurationIsAmount:
                amount = duration;
                break;
            case StatusEffectType.InfiniteDuration:
            case StatusEffectType.UseAmountInfiniteDuration:
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                duration = -1;
                break;
        }

        if (amount == 0 || duration == 0) return;

        if (!CurStatusEffectDict.ContainsKey(statusEffect.effect))
        {
            CurStatusEffectDict.Add(statusEffect.effect,
                new Dictionary<StatusEffectType, (int, int)>
                {
                    { statusEffect.type, (amount, duration) }
                });

            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
        }
        else if (!CurStatusEffectDict[statusEffect.effect].ContainsKey(statusEffect.type))
        {
            CurStatusEffectDict[statusEffect.effect].Add(statusEffect.type, (amount, duration));
            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
        }
        else
        {
            (int amount, int duration) info = CurStatusEffectDict[statusEffect.effect][statusEffect.type];

            if (info.amount == 0 || info.duration == 0)               // 상태효과 지속시간이나 값이 0일 경우 (지속시간이 -1일 경우가 있어서 일단 둘 다 체크함.)
            {
                CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (amount, duration);

                ActivateStatusEffect(statusEffect, true);

                ChangeStatusEffectDesc(statusEffect);
            }
            else
            {
                switch (statusEffect.type)
                {
                    case StatusEffectType.InfiniteDuration:
                    case StatusEffectType.UseAmountInfiniteDuration:
                    case StatusEffectType.Perpetual:
                    case StatusEffectType.UseAmountPerpetual:
                        CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (info.amount + amount, duration);
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                    case StatusEffectType.DurationIsAmount:
                        CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (info.amount + amount, info.duration + duration);
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                    default:
                        CurStatusEffectDict[statusEffect.effect][statusEffect.type] = (
                                    info.amount + amount,
                                    info.duration > duration ? info.duration : duration
                                    );
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                }
            }
        }
        switch (statusEffect.effect)     // 능력치는 UI에 띄우기 때문에 바로바로 적용되어야 함. 그 외 치명타 시스템 또한 포함.
        {
            case StatusEffect.HPUp:
                MaxHP.Value += amount;
                break;
            case StatusEffect.ATKUp:
                AttackPower.Value += amount;
                break;
            case StatusEffect.DEFUp:
                DefensePower.Value += amount;
                break;
            case StatusEffect.HealUp:
                HealPower.Value += amount;
                break;
            case StatusEffect.CriticalChanceUp:
                _criticalChance.Value += amount;
                break;
            case StatusEffect.CriticalDamageUp:
                CriticalDamage.Value += amount;
                break;
            case StatusEffect.UseCritical:      // 적 개체 한정
                if (GetStatusEffect(StatusEffect.Attack, out _))        // Attack은 공격을 하겠다는 설명이기에 플레이어는 얻어서는 안됨.
                {
                    ChangeStatusEffectDesc((StatusEffect.Attack,StatusEffectType.Information));
                }
                break;
        }
        //else if (info.Item2 == -1)     // 상태효과 지속시간이 없는 경우(계속 유지)
        //{
        //    if (info.Item1 == 0)
        //    {
        //        //if (amount > 0)
        //        //{
        //        //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (amount, -1);

        //        //    ActivateStatusEffect(statusEffect, true);

        //        //    ChangeStatusEffectDesc(statusEffect);
        //        //    return;
        //        //}
        //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (amount, duration);

        //        ActivateStatusEffect(statusEffect, true);

        //        ChangeStatusEffectDesc(statusEffect);
        //        return;
        //    }
        //    else
        //    {
        //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, duration);
        //        ChangeStatusEffectDesc(statusEffect);
        //        return;
        //        //if (info.Item1 + amount > 0)
        //        //{
        //        //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, -1);
        //        //    ChangeStatusEffectDesc(statusEffect);
        //        //    return;
        //        //}
        //        //else
        //        //{
        //        //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, -1);
        //        //    ActivateStatusEffect(statusEffect, false);
        //        //    return;
        //        //}
        //    }
        //    //else if (CurStatusEffect[statusEffect].Item1 < amount)
        //    //{
        //    //    CurStatusEffect[statusEffect] = (amount, duration);

        //    //    ChangeStatusEffectDesc(statusEffect, amount, duration);
        //    //    return;
        //    //}
        //}
        //else
        //{
        //    if (statusEffect.Item2 == StatusEffectType.DurationIsAmount)
        //    {
        //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, info.Item2 + duration);
        //        ChangeStatusEffectDesc(statusEffect);
        //        return;
        //    }
        //    else
        //    {
        //        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (
        //                        info.Item1 + amount,
        //                        info.Item2 > duration ? info.Item2 : duration
        //                        );
        //        ChangeStatusEffectDesc(statusEffect);
        //        return;

        //    }
        //    //if (info.Item1 + amount > 0)
        //    //{
        //    //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (
        //    //                info.Item1 + amount,
        //    //                info.Item2 > duration ? info.Item2 : duration
        //    //                );
        //    //    ChangeStatusEffectDesc(statusEffect);
        //    //    return;
        //    //    //if (duration >= 0)      // 지속시간 값이 양수이면, 원래 값과 비교해서 더 큰 값으로 적용
        //    //    //{
        //    //    //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (
        //    //    //            info.Item1 + amount,
        //    //    //            info.Item2 > duration ? info.Item2 : duration
        //    //    //            );
        //    //    //    ChangeStatusEffectDesc(statusEffect);
        //    //    //    return;
        //    //    //}
        //    //    //else if (info.Item2 + duration > 0)     // 지속시간 값이 음수인데, 원래 있던 값 + 지속시간 값(원래있던 값 - |지속시간 값|)이 양수이면, 지속시간 감소
        //    //    //{
        //    //    //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, info.Item2 - duration);
        //    //    //    ChangeStatusEffectDesc(statusEffect);
        //    //    //    return;
        //    //    //}
        //    //    //else                    // 지속시간 값이 음수인데, 결과값도 음수라면 UI 끄기
        //    //    //{
        //    //    //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, 0);
        //    //    //    ActivateStatusEffect(statusEffect, false);
        //    //    //    return;
        //    //    //}
        //    //}
        //    //else
        //    //{
        //    //    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, 0);
        //    //    ActivateStatusEffect(statusEffect, false);
        //    //    return;
        //    //}
        //}



        //if (CurStatusEffect[statusEffect].Item1 < amount)       // 상태효과 값에 따라 변화(값이 우선, 값이 같을 경우에는 지속시간이 더 긴 것.)
        //{
        //    CurStatusEffect[statusEffect] = (amount, duration);

        //    ChangeStatusEffectDesc(statusEffect, amount, duration);
        //    return;
        //}
        //else if (CurStatusEffect[statusEffect].Item1 == amount)
        //{
        //    if (CurStatusEffect[statusEffect].Item2 < duration)
        //    {
        //        CurStatusEffect[statusEffect] = (amount, duration);

        //        ChangeStatusEffectDesc(statusEffect, amount, duration);
        //        return;
        //    }
        //}
    }

    public void RemoveStatusEffect()
    {
        foreach (var dict in CurStatusEffectDict)
        {
            (int amount, int duration) info;
            if (dict.Value.TryGetValue(StatusEffectType.InfiniteDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.InfiniteDuration), info.amount, info.duration);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.TurnDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.TurnDuration), info.amount, info.duration);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.DurationIsAmount, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.DurationIsAmount), info.amount, info.duration);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountInfiniteDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountInfiniteDuration), info.amount, info.duration);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountTurnDuration), info.amount, info.duration);
                }
            }

            //var innerDict = dict.Value;

            //foreach (var innerDictInfo in innerDict)
            //{
            //    if (!innerDict.TryGetValue(dict.Key, out var info))
            //    {
            //        continue;
            //    }

            //    // 6. info.Item1 != 0 && info.Item2 != 0 조건은 그대로 유지
            //    if (info.Item1 != 0 && info.Item2 != 0)
            //    {
            //        ReduceStatusEffect((statusEffectKey, typeKey), info.Item1, info.Item2);
            //    }
            //}
            //var keysToCheck = new List<StatusEffectType>(dict.Value.Keys);

            //foreach (var innerDictInfo in innerDict)
            //{
            //    var key = innerDictInfo.Key;
            //    switch (key)
            //    {
            //        case StatusEffectType.Perpetual:
            //        case StatusEffectType.UseAmountPerpetual:
            //            break;
            //        default:
            //            var val = dict.Value[key];
            //            if (val.Item1 != 0 && val.Item2 != 0)
            //            {
            //                ReduceStatusEffect((dict.Key, key), val.Item1, val.Item2);
            //            }
            //            break;
            //    }
            //}
        }
    }
    public void RemoveStatusEffect((StatusEffect effect, StatusEffectType type)? statusEffect/*, bool information = false*/)
    {
        if (statusEffect == null)
            return;
        if (!CurStatusEffectDict.ContainsKey(statusEffect.Value.effect))
            return;
        if (!CurStatusEffectDict[statusEffect.Value.effect].ContainsKey(statusEffect.Value.type))
            return;
        switch (statusEffect.Value.type)
        {
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                return;
            
            //case StatusEffectType.Information:
            //    if (!information)
            //        return;
            //    break;

        }

        (int amount, int duration) info = CurStatusEffectDict[statusEffect.Value.effect][statusEffect.Value.type];

        if (info.amount == 0 || info.duration == 0)               // 상태효과 지속시간이나 값이 0일 경우 (지속시간이 -1일 경우가 있어서 일단 둘 다 체크함.)
            return;
        else
        {
            ReduceStatusEffect(statusEffect.Value, info.amount, info.duration);
        }
    }

    public (StatusEffect, StatusEffectType)? GetRandomStatusEffect(bool perpetual = false)
    {
        (StatusEffect effect, StatusEffectType type)? statusEffect = null;

        
        if (perpetual)
        {
            if (CurStatusEffectPerpetualList.Count == 0)
            {
                if (CurStatusEffectList.Count == 0)
                    return null;
                statusEffect = CurStatusEffectList[Random.Range(0, CurStatusEffectList.Count)];
            }
            else
            {
                if (CurStatusEffectList.Count == 0)
                    statusEffect = CurStatusEffectPerpetualList[Random.Range(0, CurStatusEffectPerpetualList.Count)];
                else
                {
                    int totalCount = CurStatusEffectList.Count + CurStatusEffectPerpetualList.Count;
                    int rand = Random.Range(0, totalCount);
                    if (rand < CurStatusEffectList.Count)
                        statusEffect = CurStatusEffectList[rand];
                    else
                        statusEffect = CurStatusEffectPerpetualList[rand - CurStatusEffectList.Count];
                }
            }
        }
        else
        {
            if (CurStatusEffectList.Count != 0)
                statusEffect = CurStatusEffectList[Random.Range(0, CurStatusEffectList.Count)];
        }

        return statusEffect;
    }

    void ChangeStatusEffectDesc((StatusEffect effect, StatusEffectType type) statusEffect)
    {
        (int amount, int duration) info = CurStatusEffectDict[statusEffect.effect][statusEffect.type];

        if (StatusEffectText[StatusEffectTextIdx[statusEffect]] != null)
        {
            StatusEffectText[StatusEffectTextIdx[statusEffect]][0].text = info.amount.ToString();
        }

        switch (statusEffect.type)
        {
            case StatusEffectType.InfiniteDuration:
            case StatusEffectType.UseAmountInfiniteDuration:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = "∞";
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.amount}</color> / 지속시간: <color=yellow>∞</color>";
                break;
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = null;
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.amount}</color>"/* / 지속시간: <color=yellow>∞</color>"*/;
                break;
            case StatusEffectType.Information:
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = ChangeInformationLV(statusEffect, info);
                //if (statusEffect.Item1 == StatusEffect.Attack && GetStatusEffect(StatusEffect.UseCritical, out _))
                //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1} + {Mathf.RoundToInt((CriticalDamage.Value - 100) * 0.01f * info.Item1 + 0.0001f)} </color>";
                //else
                //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color>";
                break;
            default:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = info.duration.ToString();
                string color;
                if (info.duration > 5)
                {
                    color = "green";
                }
                else if (info.duration > 2)
                {
                    color = "orange";
                }
                else
                {
                    color = "red";
                }
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.amount}</color> / 지속시간: <color={color}>{info.duration}</color>";
                break;
        }
        //if (statusEffect.Item2 == StatusEffectType.InfiniteDuration || statusEffect.Item2 == StatusEffectType.UseAmountInfiniteDuration)
        //{
        //    StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = "∞";
        //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color> / 지속시간: <color=yellow>∞</color>";
        //}
        //else
        //{
        //    StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = info.Item2.ToString();
        //    string color;
        //    if (info.Item2 > 5)
        //    {
        //        color = "green";
        //    }
        //    else if (info.Item2 > 2)
        //    {
        //        color = "orange";
        //    }
        //    else
        //    {
        //        color = "red";
        //    }
        //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color> / 지속시간: <color={color}>{info.Item2}</color>";
        //}
        float descHeight = StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].preferredHeight;

        string desc = InGameManager.Instance.SESO.SEDatas[(int)statusEffect.effect].Descript;        // 애도 나중에는 딕셔너리로 바꿔야 하려나
        if (string.IsNullOrWhiteSpace(desc))
        {
            StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text = _specialDesc.Replace("{n}", $"<color=green>{info.amount}</color>"); ;
        }
        else
        {
            StringBuilder sb = new(desc);
            AddStatusEffectDesc(statusEffect, sb, info);
            //switch (statusEffect.Item1)
            //{
            //    case StatusEffect.Attack:
            //        if (GetStatusEffect(StatusEffect.UseCritical, out _))
            //            sb.Replace("{n}", $"<color=green>{info.Item1} + 치명타 피해({CheckCriticalDamage(info.Item1, true) - info.Item1}) </color>");
            //        else
            //            sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
            //        break;
            //    case StatusEffect.Defense:
            //        sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
            //        break;
            //    case StatusEffect.Heal:
            //        sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
            //        break;
            //    default:
            //        sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
            //        break;
            //}
            StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text = sb.ToString();
        }
        //StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text = InGameManager.Instance.SESO.SEDatas[(int)statusEffect.Item1].Descript.Replace("{n}", $"<color=green>{info.Item1}</color>");

        switch (statusEffect.type)
        {
            case StatusEffectType.UseAmountTurnDuration:
            case StatusEffectType.UseAmountInfiniteDuration:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(능력 발동 시, 값이 감소되며, 0이 될 경우 사라잡니다.)</color></size>";
                break;
            case StatusEffectType.DurationIsAmount:
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(지속시간과 값이 같습니다.)</color></size>";
                break;
        }
        switch (statusEffect.type)
        {
            //case StatusEffectType.InfiniteDuration:
            //case StatusEffectType.UseAmountInfiniteDuration:
            //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(전투가 종료되면 사라집니다.)</color></size>";
            //    break;
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=red>(해당 상태 효과는 버프 제거로 사라지지 않습니다.)</color></size>";
                break;
        }
        //if (statusEffect.Item2 == StatusEffectType.UseAmountTurnDuration || statusEffect.Item2 == StatusEffectType.UseAmountInfiniteDuration)
        //{
        //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(능력 발동 시, 값이 감소합니다.)</color></size>";
        //}
        //else if (statusEffect.Item2 == StatusEffectType.DurationIsAmount)
        //{
        //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(지속시간과 값이 같습니다.)</color></size>";
        //}
        if (!_statusDescWindow.gameObject.activeSelf)
        {
            _statusDescWindow.gameObject.SetActive(true);           // 켰다가 꺼야 텍스트 크기 가져올 수 있음.. 나중에 좀 고치고 싶음.
            _statusDescWindow.gameObject.SetActive(false);
        }
        if (descHeight == StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].GetPreferredValues().y)        // GetPreferredValues가 좀 더 정확한 값을 가져옴.
        {
            return;
        }
        RectTransform rectTransform = StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.GetComponent<RectTransform>();
        Vector2 newSize = rectTransform.sizeDelta;
        newSize.y = 30 + StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].preferredHeight;
        rectTransform.sizeDelta = newSize;

    }

    protected virtual void AddStatusEffectDesc((StatusEffect effect, StatusEffectType type) statusEffect, StringBuilder sb, (int amount, int duration) info)
    {
        sb.Replace("{CriticalChance}", $"<color=green>{_criticalChance}</color>");
        sb.Replace("{CriticalDamage}", $"<color=green>{CriticalDamage}</color>");
        sb.Replace("{n}", $"<color=green>{info.amount}</color>");
    }
    protected virtual string ChangeInformationLV((StatusEffect effect, StatusEffectType type) statusEffect, (int amount, int duration) info)
    {
        return $"LV: <color=green>{info.amount}</color>";
    }

    void ActivateStatusEffect((StatusEffect effect, StatusEffectType type) statusEffect, bool isOn)
    {
        if (!StatusEffectTextIdx.ContainsKey(statusEffect))         // 상태 효과가 처음 들어왔을 경우.
        {
            StatusEffectTextIdx.Add(statusEffect, StatusEffectTextIdx.Count);                   // 각 상태 효과 인덱스 지정
            //Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent);
            if (statusEffect.type != StatusEffectType.Information)
            {
                GameObject effect = Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent);
                Image[] effectImage = effect.GetComponentsInChildren<Image>();      // 0 = 배경, 1 = 상태 효과 이미지
                switch (statusEffect.type)
                {
                    case StatusEffectType.InfiniteDuration:
                        effectImage[0].color = _black;
                        break;
                    case StatusEffectType.TurnDuration:
                        effectImage[0].color = _white;
                        break;
                    case StatusEffectType.DurationIsAmount:
                        effectImage[0].color = _navy;
                        break;
                    case StatusEffectType.UseAmountInfiniteDuration:
                        effectImage[0].color = _gray;
                        break;
                    case StatusEffectType.UseAmountTurnDuration:
                        effectImage[0].color = _brown;
                        break;
                    case StatusEffectType.Perpetual:
                        effectImage[0].color = _purple;
                        break;
                    case StatusEffectType.UseAmountPerpetual:
                        effectImage[0].color = _teal;
                        break;
                    default:
                        break;
                }
                StatusEffectText.Add(effect.GetComponentsInChildren<TMP_Text>());

            }
            else
            {
                StatusEffectText.Add(null);
            }
            //var texts = Instantiate(InGameUIManager.Instance.StatusEffectDescPrefab, _statusEffectDescContent).GetComponentsInChildren<TMP_Text>();
            //StatusEffectDurationText.Add(texts[0]);
            //StatusEffectDescText.Add(texts[1]);
            StatusEffectDescText.Add(Instantiate(InGameUIManager.Instance.StatusEffectDescPrefab, _statusEffectDescContent).GetComponentsInChildren<TMP_Text>());
        }


        StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(isOn);         // 설명창 내용
        switch (statusEffect.type)
        {
            case StatusEffectType.Information:
                //StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(false);
                if (isOn)
                {
                    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsFirstSibling();        // 정보 내용은 desc가 가장 위로 올라오게 함. 대신 상태 효과 개수는 변경되지 않고, 정보 값 변경
                    ++_numberOfInformation;
                }
                else
                {
                    --_numberOfInformation;
                }
                break;
            case StatusEffectType.InfiniteDuration:
            case StatusEffectType.TurnDuration:
            case StatusEffectType.DurationIsAmount:
            case StatusEffectType.UseAmountInfiniteDuration:
            case StatusEffectType.UseAmountTurnDuration:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(isOn);             // 체력바 하단 내용
                if (isOn)
                {
                    StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
                    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
                    CurStatusEffectList.Add(statusEffect);
                    ++_numberOfStatusEffects;
                }
                else
                {
                    CurStatusEffectList.Remove(statusEffect);
                    --_numberOfStatusEffects;
                }
                break;
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(isOn);             // 체력바 하단 내용
                if (isOn)
                {
                    StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
                    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
                    CurStatusEffectPerpetualList.Add(statusEffect);
                    ++_numberOfStatusEffects;
                }
                else
                {
                    CurStatusEffectPerpetualList.Remove(statusEffect);
                    --_numberOfStatusEffects;
                }
                break;
        }
        //if (isOn)
        //{
        //    StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
        //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
        //    ++_numberOfStatusEffects;
        //}
        //else
        //{

        //    --_numberOfStatusEffects;
        //}
    }

    //void LoseStatusEffect((StatusEffect, StatusEffectType) statusEffect)
    //{
    //    if (CurStatusEffect.ContainsKey(statusEffect))
    //    {
    //        CurStatusEffect[statusEffect] = (0, CurStatusEffect[statusEffect].Item2 == -1 ? -1 : 0);
    //        // 삭제하는 코드
    //        ActivateStatusEffect(statusEffect, false);
    //    }
    //}

    public void TurnStatusEffect()
    {
        //foreach (var dict in CurStatusEffect.Values)
        //{
        //    (int, int) info;
        //    if (dict.TryGetValue(StatusEffectType.TurnDuration, out info))
        //    {
        //        if (info.Item1 != 0 && info.Item2 != 0)
        //        {
        //            dict[StatusEffectType.TurnDuration] = (info.Item1, --info.Item2);       // 해당 타입은 무한 지속시간일 수가 없으므로, 그냥 -1 진행. 그러나, -2를 하게되는 경우에는 예외처리가 필요함.
        //        }
        //    }
        //    else if (dict.TryGetValue(StatusEffectType.AmountIsDuration, out info))
        //    {
        //        if (info.Item1 != 0 && info.Item2 != 0)
        //        {
        //            dict[StatusEffectType.AmountIsDuration] = (--info.Item1, --info.Item2);
        //        }
        //    }
        //    else if (dict.TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
        //    {
        //        if (info.Item1 != 0 && info.Item2 != 0)
        //        {
        //            dict[StatusEffectType.UseAmountTurnDuration] = (info.Item1, --info.Item2);
        //        }
        //    }
        //}

        //foreach (var dict in CurStatusEffect)
        //{
        //    var keysToCheck = new List<StatusEffectType>(dict.Value.Keys);

        //    foreach (var key in keysToCheck)
        //    {
        //        switch (key)
        //        {
        //            case StatusEffectType.TurnDuration:
        //            case StatusEffectType.UseAmountTurnDuration:
        //                if (dict.Value[key].Item1 != 0 && dict.Value[key].Item2 != 0)
        //                {
        //                    ReduceStatusEffect((dict.Key, key));
        //                }
        //                break;
        //            case StatusEffectType.DurationIsAmount:
        //                if (dict.Value[key].Item1 != 0 && dict.Value[key].Item2 != 0)
        //                {
        //                    ReduceStatusEffect((dict.Key, key), 1);
        //                }
        //                break;
        //        }
        //    }
        //}

        foreach (var dict in CurStatusEffectDict)
        {
            (int amount, int duration) info;
            if (dict.Value.TryGetValue(StatusEffectType.TurnDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.TurnDuration));
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.DurationIsAmount, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.DurationIsAmount), 1);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountTurnDuration));
                }
            }
            //if (dict.Value.TryGetValue(StatusEffectType.Information, out info))       // 직접 제거하기로 함.
            //{
            //    if (info.Item1 != 0 && info.Item2 != 0)
            //    {
            //        ReduceStatusEffect((dict.Key, StatusEffectType.Information));
            //    }
            //}
        }
    }

    public bool GetStatusEffect(StatusEffect statusEffect, out int amount)
    {
        amount = 0;
        if (CurStatusEffectDict.ContainsKey(statusEffect))
        {
            foreach (var typeDictValue in CurStatusEffectDict[statusEffect].Values)
            {
                if (typeDictValue.amount == 0 || typeDictValue.duration == 0)
                    continue;
                amount += typeDictValue.amount;

            }
        }
        return amount != 0;
    }
    public bool ApplyStatusEffect(StatusEffect statusEffect, out int amount)
    {
        //if (CurStatusEffect.ContainsKey(statusEffect.Item1) && CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2)
        //    && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item1 != 0 && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item2 != 0)
        if (CurStatusEffectDict.ContainsKey(statusEffect))
        {
            //switch (statusEffect)
            //{
            //    case StatusEffect.Resurrection:
            //        if (CurStatusEffect[statusEffect].TryGetValue(StatusEffectType.UseAmountTurnDuration))
            //        ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountTurnDuration), 1, 0);
            //        amount = 1;
            //        return true;
            //}
            amount = 0;
            foreach (var typeDictValue in CurStatusEffectDict[statusEffect].Values)
            {
                if (typeDictValue.amount == 0 || typeDictValue.duration == 0)
                    continue;
                amount += typeDictValue.amount;
                //switch (typeDict.Key)                 // foreach문 내에서 Dict 변경 안돼서 그냥 밖으로 뺌.
                //{
                //    case StatusEffectType.UseAmountInfiniteDuration:
                //    case StatusEffectType.UseAmountTurnDuration:
                //        CurStatusEffect[statusEffect][typeDict.Key] = (typeDict.Value.Item1 - 1, typeDict.Value.Item2);
                //        break;
                //}
            }
            (int amount, int duration) info;
            bool once = false;
            if (CurStatusEffectDict[statusEffect].TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountTurnDuration), 1, 0);
                    //CurStatusEffect[statusEffect][StatusEffectType.UseAmountTurnDuration] = (--info.Item1, info.Item2);
                    switch (statusEffect)
                    {
                        case StatusEffect.Resurrection:
                        case StatusEffect.Immunity:
                            once = true;
                            break;
                    }
                }
            }
            if (CurStatusEffectDict[statusEffect].TryGetValue(StatusEffectType.UseAmountInfiniteDuration, out info))
            {
                if (once)       // 해당 상태효과가 1회 사용이고, 이미 turnduration에서 사용됐을 경우, 위에서 전부 더한 amount에 해당 값은 제외하는 코드 (그러나 1회 사용인 경우에는 amount값이 크게 중요하지 않아서 안 할 수도 있음.)
                {
                    amount -= info.amount;
                }
                else if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountInfiniteDuration), 1, 0);
                    //CurStatusEffect[statusEffect][StatusEffectType.UseAmountInfiniteDuration] = (--info.Item1, info.Item2);       // 해당 타입은 무한 지속시간일 수가 없으므로, 그냥 -1 진행. 그러나, -2를 하게되는 경우에는 예외처리가 필요함.
                    switch (statusEffect)
                    {
                        case StatusEffect.Resurrection:
                        case StatusEffect.Immunity:
                            once = true;
                            break;
                    }
                }
            }

            if (CurStatusEffectDict[statusEffect].TryGetValue(StatusEffectType.UseAmountPerpetual, out info))
            {
                if (once)       // 해당 상태효과가 1회 사용이고, 이미 turnduration에서 사용됐을 경우, 위에서 전부 더한 amount에 해당 값은 제외하는 코드 (그러나 1회 사용인 경우에는 amount값이 크게 중요하지 않아서 안 할 수도 있음.)
                {
                    amount -= info.amount;
                }
                else if (info.amount != 0 && info.duration != 0)
                {
                    ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountPerpetual), 1, 0);
                    //CurStatusEffect[statusEffect][StatusEffectType.UseAmountInfiniteDuration] = (--info.Item1, info.Item2);       // 해당 타입은 무한 지속시간일 수가 없으므로, 그냥 -1 진행. 그러나, -2를 하게되는 경우에는 예외처리가 필요함.
                }
            }
            //amount = CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item1;
            //switch (statusEffect)       // 능력치는 UI에 띄우기 때문에 바로바로 적용되어야 함.
            //{
            //    case StatusEffect.HPUp:
            //        _maxHP.Value = amount;
            //        break;
            //    case StatusEffect.ATKUp:
            //        AttackPower.Value = amount;
            //        break;
            //    case StatusEffect.DEFUp:
            //        DefensePower.Value = amount;
            //        break;
            //    case StatusEffect.HealUp:
            //        HealPower.Value = amount;
            //        break;
            //    case StatusEffect.CriticalChanceUp:
            //        _criticalChance.Value = amount;
            //        break;
            //    case StatusEffect.CriticalDamageUp:
            //        _criticalDamage.Value = amount;
            //        break;
            //}
            if (amount == 0)
                return false;
            return true;
        }
        else
        {
            amount = 0;
            return false;
        }
        //return false;

        //return CurStatusEffect[statusEffect].Item1;
        //if (CurStatusEffect[statusEffect].Item1 == 0)
        //{
        //    return null;
        //}
        //else
        //{
        //    return CurStatusEffect[statusEffect].Item1;
        //}
    }
}
