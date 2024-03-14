using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entitiy
{
    Arrow arrow;
    int spawnPos;
    void OnMouseEnter()
    {
        if (CardManager.Instance.isSingleTarget)
        {
            CardManager.Instance.useSingleTargetCard = true;
            CardManager.Instance.targetEnemy = this;
            for (int i = 0; i < arrow.arrowRenderer.Count; i++)
            {
                arrow.arrowRenderer[i].color = Color.red;
            }
        }
    }

    void OnMouseExit()
    {
        if (CardManager.Instance.isSingleTarget)
        {
            CardManager.Instance.useSingleTargetCard = false;
            CardManager.Instance.targetEnemy = null;
            for (int i = 0; i < arrow.arrowRenderer.Count; i++)
            {
                arrow.arrowRenderer[i].color = Color.white;
            }
        }
    }

    public void SetupEnemy(int hp, int pos)
    {
        base.SetupEntity(hp);
        spawnPos = pos;
    }

    public override void TakeDamage(int dmg)
    {
        if (curHp - dmg <= 0)
        {
            EnemyManager.Instance.enemies.Remove(this);
            EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);
        }
        base.TakeDamage(dmg);
    }

    // Start is called before the first frame update
    void Start()
    {
        arrow = FindObjectOfType<Arrow>(true);
    }
}
