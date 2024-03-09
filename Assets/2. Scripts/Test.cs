using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] Arrow arrow;

    private void OnMouseEnter()
    {
        if (CardManager.Instance.isSingleTarget && this.CompareTag("Enemy"))
        {
            CardManager.Instance.useSingleTargetCard = true;
            for (int i = 0; i < arrow.arrowRenderer.Count; i++)
            {
                arrow.arrowRenderer[i].color = Color.red;
            }
        }
        //if (CardManager.Instance.isSingleTarget && this.CompareTag("Player"))
        //{
        //    CardManager.Instance.useSingleTargetCard = true;
        //}
    }

    private void OnMouseExit()
    {
        if (CardManager.Instance.isSingleTarget && this.CompareTag("Enemy"))
        {
            CardManager.Instance.useSingleTargetCard = false;
            for (int i = 0; i < arrow.arrowRenderer.Count; i++)
            {
                arrow.arrowRenderer[i].color = Color.white;
            }
        }
        //if (CardManager.Instance.isSingleTarget && this.CompareTag("Player"))
        //{
        //    CardManager.Instance.useSingleTargetCard = false;
        //}
    }
}
