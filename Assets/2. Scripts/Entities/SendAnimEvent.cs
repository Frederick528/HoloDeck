using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAnimEvent : MonoBehaviour
{
    public Entity ParentEntity;

    void OnDieAnimEnd()
    {
        ParentEntity.DieAnimEnd();
    }

    void OnAtkAnimTiming()
    {
        ParentEntity.AtkAnimtiming();
    }

    void OnCardAtkTiming()
    {
        CardManager.Instance.CardAtkAnimTiming();
    }
}
