using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer entitySprite;
    [SerializeField] protected Slider slider;   // 나중에 이미지로 변경
    [SerializeField] protected TMP_Text hpText;

    protected Animator animator;
    protected int maxHp;
    protected int curHp;

    public virtual void SetupEntity(int hp)
    {
        maxHp = hp;
        curHp = maxHp;
        slider.maxValue = maxHp;
        slider.value = maxHp;
        hpText.text = maxHp.ToString();
    }
    public virtual void TakeDamage(int dmg)
    {
        curHp -= dmg;
        //animator.Play("Hit", 0);  // 타격 당하는 애니메이션 실행
        if (curHp <= 0)
        {
            DieAnimation().Forget();
        }
    }
    public virtual async UniTaskVoid DieAnimation()
    {
        //animator.Play("Die", 0);  // 사망 애니메이션 실행
        await UniTask.Delay(10);
        //await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        Destroy(gameObject);
    }
    public virtual void Heal(int amount)
    {
        curHp = Mathf.Clamp(curHp + amount, 0, maxHp);
    }

    private void Update()   // (수정할 것) UniRX로 변경
    {
        slider.value = curHp;
        hpText.text = curHp.ToString();
    }
}
