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
        SetupEntity(hp);
        spawnPos = pos;
    }

    public override bool TakeDamage(int dmg)
    {
        if (!base.TakeDamage(dmg))   // 죽는 애니매이션 이후 삭제(만약 죽는 애니메이션이 0초라면, 오류가 날 수 있음.)
            return false;
        //int spawn = 0;
        EnemyManager.Instance.enemies.Remove(this);
        EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);
        for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
        {
            if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 밑에 if문으로 들어가서 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
                return true;
            //spawn++;
        }
        //if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
        MapManager.Instance.ClearStage();
        MapManager.Instance.RewardStage();

        return true;
        //await DieAnimation();
        //Destroy(gameObject);

    }

    //public void EnemyTakeDamage(int dmg)
    //{
    //    if (base.TakeDamage(dmg))   // 죽는 애니매이션 이후 삭제(만약 죽는 애니메이션이 0초라면, 오류가 날 수 있음.)
    //        return;
    //    int spawn = 0;
    //    EnemyManager.Instance.enemies.Remove(this);
    //    EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);
    //    for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
    //    {
    //        if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 밑에 if문으로 들어가서 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
    //            return; // 밑에가 클리어 코드라서 return을 쓰지만, 만약 다른 코드를 추가하게 된다면, return이 아닌 break를 사용할 것
    //        spawn++;
    //    }
    //    if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
    //        SettingMap.ClearStage();
    //    //await DieAnimation();
    //    //Destroy(gameObject);
    //}

    // Start is called before the first frame update
    void Start()
    {
        EntitySubScribe();
        arrow = FindObjectOfType<Arrow>(true);
    }
}
