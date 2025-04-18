using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
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
    protected Transform canvas;

    SendAnimEvent _animEvent;

    bool _isAtk;
    bool _isDied;

    protected Animator animator;
    protected ReactiveProperty<int> _maxHP = new();
    protected ReactiveProperty<int> _curHP = new();
    protected ReactiveProperty<int> _shield = new();

    protected ReactiveProperty<int> _useCritical { get; private set; } = new(100);
    protected ReactiveProperty<int> _curCritical { get; private set; } = new();
    protected ReactiveProperty<int> _criticalChance { get; private set; } = new();
    protected ReactiveProperty<int> _criticalDamage { get; private set; } = new();

    float _hpRatio;

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
    public virtual bool TakeDamage(int dmg)
    {
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
        animator.Play("Hit", -1, 0);  // 타격 당하는 애니메이션 실행
        if (_curHP.Value > 0)
            return false;
        //col2d.enabled = false;
        //slider.gameObject.SetActive(false);
        //canvas.gameObject.SetActive(false);
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
        await UniTask.WaitForSeconds(0.1f);
    }

    public virtual async UniTask Shield(int amount)
    {
        _shield.Value += amount;
        await UniTask.WaitForSeconds(0.1f);
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
            }
        });
        _curHP.Subscribe(hp =>
        {
            //slider.value = hp;
            if (_maxHP.Value > 0)
            {
                hpBar.fillAmount = (float)hp / _maxHP.Value;
                hpText.text = hp.ToString();
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
        });
        _curCritical.Subscribe(critical =>
        {
            if (_useCritical.Value <= 0) return;
            _criticalBar.fillAmount = (float)critical / _useCritical.Value;
            _criticalText.text = _curCritical.ToString();
        });
    }

    protected void StartEntity()
    {
        animator = transform.GetComponentInChildren<Animator>();
        _animEvent = transform.GetComponentInChildren<SendAnimEvent>();
        _animEvent.ParentEntity = this;

        //entitySprite = GetComponent<SpriteRenderer>();
        canvas = transform.Find("EntityCanvas");
        hpBar = canvas.Find("HPBar").GetComponent<Image>();
        _criticalBar = canvas.Find("CriticalBar").GetComponent<Image>();
        shieldObj = canvas.Find("Shield").gameObject;
        //slider = GetComponentInChildren<Slider>();
        hpText = hpBar.transform.Find("HPText").GetComponent<TMP_Text>();
        _criticalText = _criticalBar.transform.Find("CriticalText").GetComponent<TMP_Text>();
        shieldText = shieldObj.transform.Find("ShieldText").GetComponent<TMP_Text>();
        col2d = GetComponent<BoxCollider2D>();
    }

}
