using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entitiy : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer entitySprite;
    [SerializeField] protected Slider slider;   // 나중에 이미지로 변경
    [SerializeField] protected TMP_Text hpText;


    protected int maxHp;
    protected int curHp;

    public virtual void SetupEntity(int hp)
    {
        maxHp = hp;
        curHp = maxHp;
        slider.value = maxHp;
        hpText.text = maxHp.ToString();
    }
    public virtual void TakeDamage(int dmg)
    {
        curHp -= dmg;
        if (curHp <= 0)
            Destroy(gameObject);
    }
    public virtual void Heal(int amount)
    {
        curHp = Mathf.Clamp(curHp + amount, 0, maxHp);
    }
}
