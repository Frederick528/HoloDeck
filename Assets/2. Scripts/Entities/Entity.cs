using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    protected ReactiveProperty<int> _maxHP = new();
    protected ReactiveProperty<int> _curHP = new();
    protected ReactiveProperty<int> _shield = new();

    public ReactiveProperty<int> AttackPower { get; private set; } = new();
    public ReactiveProperty<int> DefensePower { get; private set; } = new();
    public ReactiveProperty<int> HealPower { get; private set; } = new();

    protected ReactiveProperty<int> _useCritical { get; private set; } = new(100);
    protected ReactiveProperty<int> _curCritical { get; private set; } = new();
    protected ReactiveProperty<int> _criticalChance { get; private set; } = new();
    protected ReactiveProperty<int> _criticalDamage { get; private set; } = new();

    float _hpRatio;
    /// <summary>
    /// StatusEffect : <StatusEffectType : (amount, duration)>
    /// </summary>
    public Dictionary<StatusEffect, Dictionary<StatusEffectType, (int, int)>> CurStatusEffect = new();

    public Dictionary<(StatusEffect, StatusEffectType), int> StatusEffectTextIdx = new();

    int _numberOfStatusEffects = 0;

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
    protected virtual async UniTask BeforeTakeDamage(bool isHit)
    {
        if (!isHit) { return; }
        if (ApplyStatusEffect(StatusEffect.Protect, out int amount))
        {
            await Shield(amount);
        }
        else
        {
            await UniTask.CompletedTask;
        }
    }
    protected virtual async UniTask AfterTakeDamage(bool isHit)
    {
        if (!isHit) { return; }
        if (ApplyStatusEffect(StatusEffect.Reflection, out int amount))
        {
            if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Player/* && BattleManager.Instance.HitEntity.Item1 != null*/)
            {
                InGameManager.Instance.Player.TakeDamage(amount, false).Forget();
            }
            else if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Enemy/* && BattleManager.Instance.HitEntity.Item2 != null*/)
            {
                EnemyManager.Instance.HitEnemy.CheckIfDead(amount, 1, false);
                EnemyManager.Instance.HitEnemy.TakeDamage(amount, false).Forget();
                //BattleManager.Instance.HitEntity.Item2.CheckIfDead(amount, 1, false);
                //BattleManager.Instance.HitEntity.Item2.TakeDamage(amount, false).Forget();
            }
        }
        else
        {
            await UniTask.CompletedTask;
        }
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
    public async virtual UniTask<bool> TakeDamage(int dmg, bool isHit)
    {
        await BeforeTakeDamage(isHit);
        if (dmg == 0)       // 데미지가 0일 경우, 맞기 전 효과 발동, 맞은 후 효과는 발동 X
            return false;
        TextEffect(-dmg).Forget();
        if (_shield.Value >= dmg)
        {
            _shield.Value -= dmg;
        }
        else
        {
            dmg -= _shield.Value;
            _shield.Value = 0;
            _curHP.Value -= dmg;
        }
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //if (stateInfo.IsName("Attack"))
        animator.Play("Hit", -1, 0);  // 타격 당하는 애니메이션 실행        => 공격 중에는 딜레이를 주거나 무시하는 코드가 필요할 듯.
        if (_curHP.Value > 0)
        {
            await AfterTakeDamage(isHit);
            return false;
        }

        if (ApplyStatusEffect(StatusEffect.Resurrection, out _))
        {
            _curHP.Value = (int)(_maxHP.Value * 0.5f);
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
        animator.Play("Attack", -1, 0);  // 공격 애니메이션 실행
        if (checkAtkTiming)
        {
            await UniTask.WaitUntil(() => _isAtk/*, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token*/);    // 공격하는 모션 중에는 게임이 끝나지 않을 것
        }
    }

    async UniTask TextEffect(int value)
    {
        TMP_Text textEffect = PoolManager.Instance.GetText(/*out TMP_Text textEffect*/);
        textEffect.transform.position = transform.position;
        textEffect.transform.localScale = Vector3.one;
        textEffect.color = value < 0 ? Color.red : Color.green;
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

    public virtual async UniTask DieAnimation()
    {
        animator.Play("Die", -1, 0);  // 사망 애니메이션 실행
        //await UniTask.Delay(1000);
        await UniTask.WaitUntil(() => _isDied);
        Destroy(gameObject);
    }
    public virtual async UniTask Heal(int amount)
    {
        _curHP.Value = Mathf.Clamp(_curHP.Value + amount, 0, _maxHP.Value);
        TextEffect(amount).Forget();        // 텍스트 뜨는 건 1초 고정으로 하고 패턴 넘어가는 건 밑에서 적당히 정해줘야 보기 편할 듯
        await UniTask.WaitForSeconds(0.2f);
    }

    public virtual async UniTask Shield(int amount)
    {
        _shield.Value += amount;
        await UniTask.WaitForSeconds(0.2f);
    }

    public int CheckCritical(int damage)
    {
        if (_curCritical.Value >= _useCritical.Value)
        {
            int criticalDamage = Mathf.RoundToInt(damage * _criticalDamage.Value * 0.01f);
            Critical(-_useCritical.Value);
            return criticalDamage;
        }
        else
        {
            Critical(_criticalChance.Value);        // 크리티컬이 안 터질 때만 찬스가 올라감.
            return damage;
        }
    }

    public void Critical(int amount)
    {
        _curCritical.Value += amount;
    }

    public virtual void ShieldReset()
    {
        _shield.Value = 0;
    }

    protected void EntitySubScribe()
    {
        StartEntity();
        _maxHP.Subscribe(hp =>
        {
            //slider.maxValue = hp;
            if (hp > 0)
            {
                hpBar.fillAmount = _curHP.Value / (float)hp;
                hpText.text = $"{_curHP.Value}/{hp}";
            }
        });
        _curHP.Subscribe(hp =>
        {
            //slider.value = hp;
            if (_maxHP.Value > 0)
            {
                hpBar.fillAmount = (float)hp / _maxHP.Value;
                hpText.text = $"{hp}/{_maxHP.Value}";
            }
        });
        _shield.Subscribe(shield =>
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
        });
        _useCritical.Subscribe(critical =>
        {
            if (critical <= 0) return;
            _criticalBar.fillAmount = _curCritical.Value / (float)critical;
            _criticalText.text = $"{_curCritical.Value}/{critical}";
        });
        _curCritical.Subscribe(critical =>
        {
            if (_useCritical.Value <= 0) return;
            _criticalBar.fillAmount = (float)critical / _useCritical.Value;
            _criticalText.text = $"{critical}/{_useCritical.Value}";
        });
    }

    protected void StartEntity()
    {
        animator = transform.GetComponentInChildren<Animator>();
        _animEvent = transform.GetComponentInChildren<SendAnimEvent>();
        _animEvent.ParentEntity = this;

        //entitySprite = GetComponent<SpriteRenderer>();
        canvas = transform.Find("EntityCanvas").GetComponent<Canvas>();
        hpBar = canvas.transform.Find("HPBar").GetComponent<Image>();
        _criticalBar = canvas.transform.Find("CriticalBar").GetComponent<Image>();
        shieldObj = canvas.transform.Find("Shield").gameObject;
        //slider = GetComponentInChildren<Slider>();
        hpText = hpBar.transform.Find("HPText").GetComponent<TMP_Text>();
        _criticalText = _criticalBar.transform.Find("CriticalText").GetComponent<TMP_Text>();
        shieldText = shieldObj.transform.Find("ShieldText").GetComponent<TMP_Text>();
        _col2D = GetComponent<BoxCollider2D>();

        _statusEffectContent = FindTransform.ContinueFindChildByName(canvas.transform.Find("StatusEffect"), "Content");
        
        _statusDescWindow = canvas.transform.Find("StatusEffectDesc");
        _statusDescWindow.GetComponent<ChildMouseHandler>().ParentEntity = this;
        _statusDescCol2D = _statusDescWindow.GetComponent<BoxCollider2D>();
        
        _statusEffectDescContent = FindTransform.ContinueFindChildByName(_statusDescWindow, "Content");

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
    private void OnMouseEnter()
    {
        if (_numberOfStatusEffects == 0) return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (BattleManager.Instance.ArrowCursor.gameObject.activeSelf) return;
        if (InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf) return;
        _statusEffectDescContent.localPosition = Vector3.zero;
        canvas.sortingOrder = 1;        // 이거 없으면 체력 UI에 가려짐
        if (transform.position.x > 4 && _statusDescWindow.localPosition.x > 0)
        {
            _statusDescWindow.localPosition = new Vector3(-_statusDescWindow.localPosition.x, _statusDescWindow.localPosition.y, -1);       // 체력바보다 앞에 있어야 콜라이더에 문제 안 생김.
            _statusDescCol2D.offset = new Vector2(-_statusDescCol2D.offset.x, 0);
        }
        else if (transform.position.x <= 4 && _statusDescWindow.localPosition.x < 0)
        {
            _statusDescWindow.localPosition = new Vector3(-_statusDescWindow.localPosition.x, _statusDescWindow.localPosition.y, -1);
            _statusDescCol2D.offset = new Vector2(-_statusDescCol2D.offset.x, 0);
        }
        _statusDescWindow.gameObject.SetActive(true);
    }

    public void OnChildMouseExit()
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
    public void ReduceStatusEffect((StatusEffect, StatusEffectType) statusEffect, int amount = 0, int duration = 1)        // 턴 감소를 디폴트로 만듦.
    {
        (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];
        if (info.Item2 - duration > 0 && info.Item1 - amount > 0)
        {
            CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 - amount, info.Item2 - duration);

            ChangeStatusEffectDesc(statusEffect);
        }
        else
        {
            switch (statusEffect.Item2)
            {
                case StatusEffectType.InfiniteDuration:
                case StatusEffectType.UseAmountInfiniteDuration:
                case StatusEffectType.Perpetual:
                case StatusEffectType.UseAmountPerpetual:
                    if (info.Item1 - amount > 0)
                    {
                        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 - amount, -1);

                        ChangeStatusEffectDesc(statusEffect);
                    }
                    else
                    {
                        amount = info.Item1;

                        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, -1);

                        ActivateStatusEffect(statusEffect, false);
                    }
                    break;
                default:
                    amount = info.Item1;

                    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, 0);

                    ActivateStatusEffect(statusEffect, false);
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

        switch (statusEffect.Item1)
        {
            case StatusEffect.HPUp:
                _maxHP.Value -= amount;
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
                _criticalDamage.Value -= amount;
                break;
        }
    }

    public void AddStatusEffect((StatusEffect, StatusEffectType) statusEffect, int amount, int duration = 1)
    {
        switch (statusEffect.Item2)
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

        if (!CurStatusEffect.ContainsKey(statusEffect.Item1))
        {
            CurStatusEffect.Add(statusEffect.Item1,
                new Dictionary<StatusEffectType, (int, int)>
                {
                    { statusEffect.Item2, (amount, duration) }
                });

            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
        }
        else if (!CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2))
        {
            CurStatusEffect[statusEffect.Item1].Add(statusEffect.Item2, (amount, duration));
            print(CurStatusEffect[statusEffect.Item1][statusEffect.Item2]);
            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
        }
        else
        {
            (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];

            if (info.Item1 == 0 || info.Item2 == 0)               // 상태효과 지속시간이나 값이 0일 경우 (지속시간이 -1일 경우가 있어서 일단 둘 다 체크함.)
            {
                CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (amount, duration);

                ActivateStatusEffect(statusEffect, true);

                ChangeStatusEffectDesc(statusEffect);
            }
            else
            {
                switch (statusEffect.Item2)
                {
                    case StatusEffectType.InfiniteDuration:
                    case StatusEffectType.UseAmountInfiniteDuration:
                    case StatusEffectType.Perpetual:
                    case StatusEffectType.UseAmountPerpetual:
                        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, duration);
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                    case StatusEffectType.DurationIsAmount:
                        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, info.Item2 + duration);
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                    default:
                        CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (
                                    info.Item1 + amount,
                                    info.Item2 > duration ? info.Item2 : duration
                                    );
                        ChangeStatusEffectDesc(statusEffect);
                        break;
                }
            }
        }
        switch (statusEffect.Item1)     // 능력치는 UI에 띄우기 때문에 바로바로 적용되어야 함.
        {
            case StatusEffect.HPUp:
                _maxHP.Value += amount;
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
                _criticalDamage.Value += amount;
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
        foreach (var dict in CurStatusEffect)
        {
            (int, int) info;
            if (dict.Value.TryGetValue(StatusEffectType.InfiniteDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.InfiniteDuration), info.Item1, info.Item2);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.TurnDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.TurnDuration), info.Item1, info.Item2);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.DurationIsAmount, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.DurationIsAmount), info.Item1, info.Item2);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountInfiniteDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountInfiniteDuration), info.Item1, info.Item2);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountTurnDuration), info.Item1, info.Item2);
                }
            }
            //var keysToCheck = new List<StatusEffectType>(dict.Value.Keys);

            //foreach (var key in keysToCheck)
            //{
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
    public void RemoveStatusEffect((StatusEffect, StatusEffectType) statusEffect)
    {
        if (!CurStatusEffect.ContainsKey(statusEffect.Item1))
            return;
        if (!CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2))
            return;
        switch (statusEffect.Item2)
        {
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                return;
        }

        (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];

        if (info.Item1 == 0 || info.Item2 == 0)               // 상태효과 지속시간이나 값이 0일 경우 (지속시간이 -1일 경우가 있어서 일단 둘 다 체크함.)
            return;
        else
        {
            ReduceStatusEffect(statusEffect, info.Item1, info.Item2);
        }
    }

    void ChangeStatusEffectDesc((StatusEffect, StatusEffectType) statusEffect)
    {
        (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];

        StatusEffectText[StatusEffectTextIdx[statusEffect]][0].text = info.Item1.ToString();

        switch (statusEffect.Item2)
        {
            case StatusEffectType.InfiniteDuration:
            case StatusEffectType.UseAmountInfiniteDuration:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = "∞";
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color> / 지속시간: <color=yellow>∞</color>";
                break;
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = null;
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color>"/* / 지속시간: <color=yellow>∞</color>"*/;
                break;
            default:
                StatusEffectText[StatusEffectTextIdx[statusEffect]][1].text = info.Item2.ToString();
                string color;
                if (info.Item2 > 5)
                {
                    color = "green";
                }
                else if (info.Item2 > 2)
                {
                    color = "orange";
                }
                else
                {
                    color = "red";
                }
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][1].text = $"LV: <color=green>{info.Item1}</color> / 지속시간: <color={color}>{info.Item2}</color>";
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

        StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text = InGameManager.Instance.SESO.SEDatas[(int)statusEffect.Item1].Descript.Replace("{n}", $"<color=green>{info.Item1}</color>");
        switch (statusEffect.Item2)
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
        switch (statusEffect.Item2)
        {
            //case StatusEffectType.InfiniteDuration:
            //case StatusEffectType.UseAmountInfiniteDuration:
            //    StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=yellow>(전투가 종료되면 사라집니다.)</color></size>";
            //    break;
            case StatusEffectType.Perpetual:
            case StatusEffectType.UseAmountPerpetual:
                StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].text += " <size=10><color=red>(해당 상태 효과는 사라지지 않습니다.)</color></size>";
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

    void ActivateStatusEffect((StatusEffect, StatusEffectType) statusEffect, bool isOn)
    {
        if (!StatusEffectTextIdx.ContainsKey(statusEffect))
        {
            StatusEffectTextIdx.Add(statusEffect, StatusEffectTextIdx.Count);
            //Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent);
            StatusEffectText.Add(Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent).GetComponentsInChildren<TMP_Text>());
            //var texts = Instantiate(InGameUIManager.Instance.StatusEffectDescPrefab, _statusEffectDescContent).GetComponentsInChildren<TMP_Text>();
            //StatusEffectDurationText.Add(texts[0]);
            //StatusEffectDescText.Add(texts[1]);
            StatusEffectDescText.Add(Instantiate(InGameUIManager.Instance.StatusEffectDescPrefab, _statusEffectDescContent).GetComponentsInChildren<TMP_Text>());
        }
        StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(isOn);
        StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.gameObject.SetActive(isOn);
        if (isOn)
        {
            StatusEffectText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
            StatusEffectDescText[StatusEffectTextIdx[statusEffect]][0].transform.parent.SetAsLastSibling();
            ++_numberOfStatusEffects;
        }
        else
        {

            --_numberOfStatusEffects;
        }
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

        foreach (var dict in CurStatusEffect)
        {
            (int, int) info;
            if (dict.Value.TryGetValue(StatusEffectType.TurnDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.TurnDuration));
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.DurationIsAmount, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.DurationIsAmount), 1);
                }
            }
            if (dict.Value.TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((dict.Key, StatusEffectType.UseAmountTurnDuration));
                }
            }
        }
    }

    public bool ApplyStatusEffect(StatusEffect statusEffect, out int amount)
    {
        //if (CurStatusEffect.ContainsKey(statusEffect.Item1) && CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2)
        //    && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item1 != 0 && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item2 != 0)
        if (CurStatusEffect.ContainsKey(statusEffect))
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
            foreach (var typeDictValue in CurStatusEffect[statusEffect].Values)
            {
                if (typeDictValue.Item1 == 0 || typeDictValue.Item2 == 0)
                    continue;
                amount += typeDictValue.Item1;
                //switch (typeDict.Key)                 // foreach문 내에서 Dict 변경 안돼서 그냥 밖으로 뺌.
                //{
                //    case StatusEffectType.UseAmountInfiniteDuration:
                //    case StatusEffectType.UseAmountTurnDuration:
                //        CurStatusEffect[statusEffect][typeDict.Key] = (typeDict.Value.Item1 - 1, typeDict.Value.Item2);
                //        break;
                //}
            }
            (int, int) info;
            bool once = false;
            if (CurStatusEffect[statusEffect].TryGetValue(StatusEffectType.UseAmountTurnDuration, out info))
            {
                if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountTurnDuration), 1, 0);
                    //CurStatusEffect[statusEffect][StatusEffectType.UseAmountTurnDuration] = (--info.Item1, info.Item2);
                    if (statusEffect == StatusEffect.Resurrection)
                    {
                        once = true;
                    }
                }
            }
            if (CurStatusEffect[statusEffect].TryGetValue(StatusEffectType.UseAmountInfiniteDuration, out info))
            {
                if (once)       // 해당 상태효과가 1회 사용이고, 이미 turnduration에서 사용됐을 경우, 위에서 전부 더한 amount에 해당 값은 제외하는 코드 (그러나 1회 사용인 경우에는 amount값이 크게 중요하지 않아서 안 할 수도 있음.)
                {
                    amount -= info.Item1;
                }
                else if (info.Item1 != 0 && info.Item2 != 0)
                {
                    ReduceStatusEffect((statusEffect, StatusEffectType.UseAmountInfiniteDuration), 1, 0);
                    //CurStatusEffect[statusEffect][StatusEffectType.UseAmountInfiniteDuration] = (--info.Item1, info.Item2);       // 해당 타입은 무한 지속시간일 수가 없으므로, 그냥 -1 진행. 그러나, -2를 하게되는 경우에는 예외처리가 필요함.
                    if (statusEffect == StatusEffect.Resurrection)
                    {
                        once = true;
                    }
                }
            }

            if (CurStatusEffect[statusEffect].TryGetValue(StatusEffectType.UseAmountPerpetual, out info))
            {
                if (once)       // 해당 상태효과가 1회 사용이고, 이미 turnduration에서 사용됐을 경우, 위에서 전부 더한 amount에 해당 값은 제외하는 코드 (그러나 1회 사용인 경우에는 amount값이 크게 중요하지 않아서 안 할 수도 있음.)
                {
                    amount -= info.Item1;
                }
                else if (info.Item1 != 0 && info.Item2 != 0)
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
