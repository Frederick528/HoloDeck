using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    Arrow arrow;
    int spawnPos;
    void OnMouseEnter()
    {
        if (CardManager.Instance.isSingleTarget)
        {
            CardManager.Instance.useSingleTargetCard = true;
            EnemyManager.Instance.targetEnemy = this;
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
            EnemyManager.Instance.targetEnemy = null;
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
        base.TakeDamage(dmg);   // 죽는 애니매이션 이후 삭제(만약 죽는 애니메이션이 0초라면, 오류가 날 수 있음.)
        if (curHp <= 0)
        {
            EnemyManager.Instance.enemies.Remove(this);
            EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);
            //await DieAnimation();
            //Destroy(gameObject);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        arrow = FindObjectOfType<Arrow>(true);
    }
}
