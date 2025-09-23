using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAnimEvent : MonoBehaviour
{
    [Header("엔티티 관련")]
    public Entity ParentEntity;

    [Header("이펙트 관련")]
    public bool RepeatEffect = false;
    public bool AllEnemies = false;

    void OnDieAnimEnd()
    {
        ParentEntity.DieAnimEnd();
    }

    void OnAtkAnimTiming()
    {
        ParentEntity.AtkAnimtiming();
    }

    void OnCardUseTiming()
    {
        if (CardManager.Instance.NowPlayedCard)
        {
            CardManager.Instance.NowPlayedCard.CardTiming(RepeatEffect, AllEnemies);
        }
    }
}
