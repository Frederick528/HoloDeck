using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : Entity
{
    public int spawnPosIdx;
    protected EnemyData enemyData;
    public bool CanClear = false;
    protected Player player;

    //public bool Death;

    //void OnMouseEnter()
    //{
    //    if (CardManager.Instance.isSingleTarget)
    //    {
    //        CardManager.Instance.useSingleTargetCard = true;
    //        EnemyManager.Instance.targetEnemy = this;
    //        for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
    //        {
    //            EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.red;
    //        }
    //    }
    //    else if (ItemManager.Instance.arrowOn)
    //    {
    //        EnemyManager.Instance.targetEnemy = this;
    //        for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
    //        {
    //            EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.red;
    //        }
    //    }
    //}

    //void OnMouseDown()
    //{
    //    if (ItemManager.Instance.AttackSingleTarget(this))
    //    {
    //        for (int i = 0; i < EnemyManager.Instance.arrow.arrowRenderer.Count; i++)
    //        {
    //            EnemyManager.Instance.arrow.arrowRenderer[i].color = Color.white;
    //        }
    //    }
    //}

    //void OnMouseExit()
    //{
    //    if (/*CardManager.Instance.isSingleTarget*/EnemyManager.Instance.ArrowCursor.arrowRenderer[0].color == Color.red)
    //    {
    //        CardManager.Instance.useSingleTargetCard = false;
    //        EnemyManager.Instance.targetEnemy = null;
    //        for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
    //        {
    //            EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.white;
    //        }
    //    }
    //    else if (ItemManager.Instance.arrowOn)
    //    {
    //        EnemyManager.Instance.targetEnemy = null;
    //        for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
    //        {
    //            EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.white;
    //        }
    //    }
    //}

    public void SetupEnemy(EnemyData eD, int pos)     // 데이터를 받는 형식으로 변경함.
    {
        enemyData = eD;
        SetupEntity(enemyData.HP, enemyData.CriticalChance);
        spawnPosIdx = pos;
        player = InGameManager.Instance.player;
    }

    public async UniTask<bool> TakeDamageEnemy(int dmg)
    {
        await BeforeTakeDamage();
        if (!TakeDamage(dmg))
        {
            await AfterTakeDamage();
            return false;
        }
        //int spawn = 0;
        //EnemyManager.Instance.enemies.Remove(this);
        //EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);

        KillEnemy().Forget();
        return true;

        //await base.DieAnimation();  // 죽는 애니매이션 이후 클리어 확인(만약 죽는 애니메이션이 0초라면, 오류가 날 수 있음.)
        //InGameManager.Instance.ChangeCoinValue(enemyData.dropCoin);
        //for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
        //{
        //    if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
        //        return true;
        //    //spawn++;
        //}
        ////if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
        //MapManager.Instance.ClearStage();
        //MapManager.Instance.RewardStage();

        //return true;
        ////await DieAnimation();
        ////Destroy(gameObject);

    }

    //protected virtual int ResistDamage(int damage)
    //{
    //    return damage;
    //}

    public virtual void CheckIfDead(int damage, int count)
    {
        //int resistDamage = ResistDamage(damage);
        if (((curHp.Value + shield.Value) - (/*resistDamage*/damage * count)) <= 0)
        {
            col2d.enabled = false;
            CanClear = true;
            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
            {
                if (!enemy.CanClear)
                {
                    //EnemyManager.Instance.MapClear = false;
                    return;
                }
            }
            EnemyManager.Instance.MapClear = true;
            CardManager.Instance.SetCardState(1);       // Over
        }
    }

    public async UniTaskVoid KillEnemy()        // 클리어 체크도 같이 함.
    {
        //Death = true;
        //EnemyManager.Instance.enemies.Remove(this);
        //bool clear = EnemyManager.Instance.enemies.Count == 0;
        //await base.DieAnimation();  // destroy(gameObject)가 들어가있기 때문에, 만약 죽고 난 다음에 추가 행동이 있다면, 이 함수 내에서 작동해야 함.

        bool clear = await EnemyManager.Instance.KillEnemyCheck(this, base.DieAnimation());
        
        //EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);      // 에너미 자리로 클리어 확인을 하기 때문에 적 죽는 모션 기다린 후, 자리 삭제  // 자리는 나중에 배열로 만들고 코드상으로만 확인하도록 변경
        InGameManager.Instance.ChangeCoinValue(enemyData.DropCoin);
        if (clear)
        {
            ClearCheck();
        }
    }

    public void ClearCheck()
    {
        if (!EnemyManager.Instance.MapClear/* || (EnemyManager.Instance.enemies.Count != 0 || MapManager.Instance.currStage.rewardBox != -1)*/)
            return;
        EnemyManager.Instance.MapClear = false;
        //for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
        //{
        //    if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
        //        return;
        //    //spawn++;
        //}
        //if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
        MapManager.Instance.ClearStage().Forget();
        MapManager.Instance.RewardStage();
        ItemManager.Instance.Charge(1);
    }

    protected async UniTask Attack(int damage)
    {
        await AttackAnimation();
        int criticalDamage = CheckCritical(damage);

        player.TakeDamagePlayer(criticalDamage).Forget();

        //Critical(_criticalChance.Value);
    }

    protected virtual async UniTask BeforeTakeDamage()
    {
        await UniTask.CompletedTask;
    }

    protected virtual async UniTask AfterTakeDamage()
    {
        await UniTask.CompletedTask;
    }

    public virtual async UniTask Pattern()
    {
        await UniTask.CompletedTask;        // 이거 고치자
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

    //void Start()      // 모든 상위 코드에 적용시켜야 함.
    //{
    //    EntitySubScribe();
    //    //ArrowCursor = FindObjectOfType<Arrow>(true);
    //}
}
