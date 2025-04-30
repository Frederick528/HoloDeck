using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
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
    protected BoxCollider2D col2d;
    protected Canvas canvas;


    Transform _statusEffectContent;
    public List<TMP_Text> StatusEffectAmountText;

    Transform _statusEffectDescContent;
    Transform _statusDescWindow;
    public List<TMP_Text> StatusEffectDescText;
    public List<TMP_Text> StatusEffectDurationText;

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
    public void SetupEntity(int hp, int criticalChance = 10, int criticalDamage = 150)
    {
        //col2d = GetComponent<BoxCollider2D>();
        _maxHP.Value = hp;
        _curHP.Value = _maxHP.Value;
        _criticalChance.Value = criticalChance;
        _criticalDamage.Value = criticalDamage;
        //slider.value = _maxHP.Value;
        //hpText.text = _maxHP.ToString();
    }
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
            if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Player && BattleManager.Instance.HitEntity.Item1 != null)
            {
                BattleManager.Instance.HitEntity.Item1.TakeDamage(amount, false).Forget();
            }
            else if (TurnManager.Instance.CurTurnType == TurnManager.TurnType.Enemy && BattleManager.Instance.HitEntity.Item2 != null)
            {
                BattleManager.Instance.HitEntity.Item2.TakeDamage(amount, false).Forget();
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
        col2d = GetComponent<BoxCollider2D>();

        _statusEffectContent = FindTransform.ContinueFindChildByName(canvas.transform.Find("StatusEffect"), "Content");
        
        _statusDescWindow = canvas.transform.Find("StatusEffectDesc");
        _statusDescWindow.GetComponent<ChildMouseHandler>().ParentEntity = this;
        
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
        _statusEffectDescContent.localPosition = Vector3.zero;
        canvas.sortingOrder = 1;        // 이거 없으면 체력 UI에 가려짐
        _statusDescWindow.gameObject.SetActive(true);
    }

    public void OnChildMouseExit()
    {
        canvas.sortingOrder = 0;
        _statusDescWindow.gameObject.SetActive(false);
    }

    public void AddAndApplyStatusEffect((StatusEffect, StatusEffectType) statusEffect, int amount, int duration = -1)
    {
        AddStatusEffect(statusEffect, amount, duration);
        if (ApplyStatusEffect(statusEffect.Item1, out int StatusEffectAmount))    // 사실 위에서 Get하고 적용하는 거라 항상 true 값이긴 함. (조건문 없이 그냥 진행해도 괜찮다는 뜻)
        {
            switch (statusEffect.Item1)
            {
                case StatusEffect.ATKUp:
                    AttackPower.Value = StatusEffectAmount;
                    break;
            }
        }
    }

    public void AddStatusEffect((StatusEffect, StatusEffectType) statusEffect, int amount, int duration = -1)
    {
        if (!CurStatusEffect.ContainsKey(statusEffect.Item1))
        {
            CurStatusEffect.Add(statusEffect.Item1,
                new Dictionary<StatusEffectType, (int, int)>
                {
                    { statusEffect.Item2, (amount, duration) }
                });

            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);

            return;
        }

        if (!CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2))
        {
            CurStatusEffect[statusEffect.Item1].Add(statusEffect.Item2, (amount, duration));

            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
            return;
        }

        (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];

        if (info.Item2 == 0)               // 상태효과 지속시간이 0일 경우
        {
            CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (amount, duration);

            ActivateStatusEffect(statusEffect, true);

            ChangeStatusEffectDesc(statusEffect);
            return;
        }
        else if (info.Item2 == -1)     // 상태효과 지속시간이 없는 경우(계속 유지)
        {
            if (info.Item1 == 0)
            {
                CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (amount, -1);

                ActivateStatusEffect(statusEffect, true);

                ChangeStatusEffectDesc(statusEffect);
                return;
            }
            else
            {
                if (info.Item1 + amount > 0)
                {
                    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (info.Item1 + amount, -1);
                    ChangeStatusEffectDesc(statusEffect);
                    return;
                }
                else
                {
                    CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, -1);
                    ActivateStatusEffect(statusEffect, false);
                    return;
                }
            }
            //else if (CurStatusEffect[statusEffect].Item1 < amount)
            //{
            //    CurStatusEffect[statusEffect] = (amount, duration);

            //    ChangeStatusEffectDesc(statusEffect, amount, duration);
            //    return;
            //}
        }
        else
        {
            if (info.Item1 + amount > 0)
            {
                CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (
                        info.Item1 + amount,
                        info.Item2 > duration ? info.Item2 : duration
                        );
                ChangeStatusEffectDesc(statusEffect);
                return;
            }
            else
            {
                CurStatusEffect[statusEffect.Item1][statusEffect.Item2] = (0, 0);
                ActivateStatusEffect(statusEffect, false);
                return;
            }
        }



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

    void ChangeStatusEffectDesc((StatusEffect, StatusEffectType) statusEffect)
    {
        (int, int) info = CurStatusEffect[statusEffect.Item1][statusEffect.Item2];

        StatusEffectAmountText[StatusEffectTextIdx[statusEffect]].text = info.Item1.ToString();

        if (info.Item2 != -1)
        {
            StatusEffectDurationText[StatusEffectTextIdx[statusEffect]].text = $"지속시간: {info.Item2}";
        }
        else
        {
            StatusEffectDurationText[StatusEffectTextIdx[statusEffect]].text = "지속시간: ∞";
        }
        float descHeight = StatusEffectDescText[StatusEffectTextIdx[statusEffect]].preferredHeight;

        StatusEffectDescText[StatusEffectTextIdx[statusEffect]].text = InGameManager.Instance.SESO.SEDatas[(int)statusEffect.Item1].Descript.Replace("{n}", info.Item1.ToString());

        if (!_statusDescWindow.gameObject.activeSelf)
        {
            _statusDescWindow.gameObject.SetActive(true);           // 켰다가 꺼야 텍스트 크기 가져올 수 있음.. 나중에 좀 고치고 싶음.
            _statusDescWindow.gameObject.SetActive(false);
        }

        if (descHeight == StatusEffectDescText[StatusEffectTextIdx[statusEffect]].preferredHeight)
        {
            return;
        }
        RectTransform rectTransform = StatusEffectDescText[StatusEffectTextIdx[statusEffect]].transform.parent.GetComponent<RectTransform>();
        Vector2 newSize = rectTransform.sizeDelta;
        newSize.y = 30 + StatusEffectDescText[StatusEffectTextIdx[statusEffect]].preferredHeight;
        rectTransform.sizeDelta = newSize;

    }

    void ActivateStatusEffect((StatusEffect, StatusEffectType) statusEffect, bool isOn)
    {
        if (!StatusEffectTextIdx.ContainsKey(statusEffect))
        {
            StatusEffectTextIdx.Add(statusEffect, StatusEffectTextIdx.Count);
            //Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent);
            StatusEffectAmountText.Add(Instantiate(InGameUIManager.Instance.StatusEffectPrefab, _statusEffectContent).GetComponentInChildren<TMP_Text>());
            var texts = Instantiate(InGameUIManager.Instance.StatusEffectDescPrefab, _statusEffectDescContent).GetComponentsInChildren<TMP_Text>();
            StatusEffectDurationText.Add(texts[0]);
            StatusEffectDescText.Add(texts[1]);
        }
        StatusEffectAmountText[StatusEffectTextIdx[statusEffect]].transform.parent.gameObject.SetActive(isOn);
        StatusEffectDescText[StatusEffectTextIdx[statusEffect]].transform.parent.gameObject.SetActive(isOn);
        if (isOn)
        {
            StatusEffectAmountText[StatusEffectTextIdx[statusEffect]].transform.parent.SetAsLastSibling();
            StatusEffectDescText[StatusEffectTextIdx[statusEffect]].transform.parent.SetAsLastSibling();
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

    public bool ApplyStatusEffect(StatusEffect statusEffect, out int amount)
    {
        //if (CurStatusEffect.ContainsKey(statusEffect.Item1) && CurStatusEffect[statusEffect.Item1].ContainsKey(statusEffect.Item2)
        //    && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item1 != 0 && CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item2 != 0)
        if (CurStatusEffect.ContainsKey(statusEffect))
        {
            amount = 0;
            foreach (var dict in CurStatusEffect[statusEffect])
            {
                if (dict.Value.Item1 != 0 && dict.Value.Item2 != 0)
                    amount += dict.Value.Item1;
                switch (dict.Key)
                {
                    case StatusEffectType.UseAmountInfiniteDuration:
                    case StatusEffectType.UseAmountTurnDuration:
                        CurStatusEffect[statusEffect][dict.Key] = (dict.Value.Item1 - 1, dict.Value.Item2);
                        break;
                }
            }
            //amount = CurStatusEffect[statusEffect.Item1][statusEffect.Item2].Item1;
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
