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
    [SerializeField] protected Image hpBar;
    [SerializeField] protected GameObject shieldObj;
    [SerializeField] protected TMP_Text hpText;
    [SerializeField] protected TMP_Text shieldText;
    [SerializeField] protected BoxCollider2D col2d;
    [SerializeField] protected Transform canvas;

    protected Animator animator;
    protected ReactiveProperty<int> maxHp = new();
    protected ReactiveProperty<int> curHp = new();
    protected ReactiveProperty<int> shield = new();

    float _hpRatio;

    //private void Awake()    // start로 할 경우, Subscribe가 실행되지 않음. Awake로 하면 위험할 것 같아서 일단 함수로 빼고 자식 오브젝트에서 Start로 호출
    //{
    //    maxHp.Subscribe(hp =>
    //    {
    //        slider.maxValue = hp;
    //    });
    //    curHp.Subscribe(hp =>
    //    {
    //        slider.value = hp;
    //        hpText.text = hp.ToString();
    //    });
    //}
    public void SetupEntity(int hp)
    {
        //col2d = GetComponent<BoxCollider2D>();
        maxHp.Value = hp;
        curHp.Value = maxHp.Value;
        //slider.value = maxHp.Value;
        //hpText.text = maxHp.ToString();
    }
    public bool TakeDamage(int dmg)
    {
        if (shield.Value >= dmg)
        {
            shield.Value -= dmg;
        }
        else
        {
            dmg -= shield.Value;
            shield.Value = 0;
            curHp.Value -= dmg;
        }
        //animator.Play("Hit", 0);  // 타격 당하는 애니메이션 실행
        if (curHp.Value > 0)
            return false;
        //col2d.enabled = false;
        //slider.gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
        return true;
    }
    public virtual async UniTask DieAnimation()
    {
        //animator.Play("Die", 0);  // 사망 애니메이션 실행
        await UniTask.Delay(1000);
        //await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        Destroy(gameObject);
    }
    public virtual void Heal(int amount)
    {
        curHp.Value = Mathf.Clamp(curHp.Value + amount, 0, maxHp.Value);
    }

    public virtual async UniTask Shield(int amount)
    {
        shield.Value += amount;
        await UniTask.WaitForSeconds(0.5f);
    }

    public virtual void DefenceReset()
    {
        shield.Value = 0;
    }

    protected void EntitySubScribe()
    {
        StartEntity();
        maxHp.Subscribe(hp =>
        {
            //slider.maxValue = hp;
            if (hp > 0)
            {
                hpBar.fillAmount = curHp.Value / (float)hp;
            }
        });
        curHp.Subscribe(hp =>
        {
            //slider.value = hp;
            if (maxHp.Value > 0)
            {
                hpBar.fillAmount = (float)hp / maxHp.Value;
                hpText.text = hp.ToString();
            }
        });
        shield.Subscribe(shield =>
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
    }

    protected void StartEntity()
    {
        //entitySprite = GetComponent<SpriteRenderer>();
        canvas = transform.Find("EntityCanvas");
        hpBar = canvas.Find("HPBar").GetComponent<Image>();
        shieldObj = canvas.Find("Shield").gameObject;
        //slider = GetComponentInChildren<Slider>();
        hpText = hpBar.transform.Find("HPText").GetComponent<TMP_Text>();
        shieldText = shieldObj.transform.Find("ShieldText").GetComponent<TMP_Text>();
        col2d = GetComponent<BoxCollider2D>();
    }

}
