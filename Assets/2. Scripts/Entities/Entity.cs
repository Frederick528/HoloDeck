using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer entitySprite;
    [SerializeField] protected Slider slider;   // 나중에 이미지로 변경
    [SerializeField] protected TMP_Text hpText;
    [SerializeField] protected BoxCollider2D col2d;

    protected Animator animator;
    protected ReactiveProperty<int> maxHp = new();
    protected ReactiveProperty<int> curHp = new();

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
    public virtual void SetupEntity(int hp)
    {
        //col2d = GetComponent<BoxCollider2D>();
        maxHp.Value = hp;
        curHp.Value = maxHp.Value;
        //slider.value = maxHp.Value;
        //hpText.text = maxHp.ToString();
    }
    public virtual bool TakeDamage(int dmg)
    {
        curHp.Value -= dmg;
        //animator.Play("Hit", 0);  // 타격 당하는 애니메이션 실행
        if (curHp.Value > 0)
            return false;
        col2d.enabled = false;
        slider.gameObject.SetActive(false);
        DieAnimation().Forget();
        return true;
    }
    public virtual async UniTaskVoid DieAnimation()
    {
        //animator.Play("Die", 0);  // 사망 애니메이션 실행
        await UniTask.Delay(100);
        //await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        Destroy(gameObject);
    }
    public virtual void Heal(int amount)
    {
        curHp.Value = Mathf.Clamp(curHp.Value + amount, 0, maxHp.Value);
    }

    protected void EntitySubScribe()
    {
        maxHp.Subscribe(hp =>
        {
            slider.maxValue = hp;
        });
        curHp.Subscribe(hp =>
        {
            slider.value = hp;
            hpText.text = hp.ToString();
        });
    }

}
