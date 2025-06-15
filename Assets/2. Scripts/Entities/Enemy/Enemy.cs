using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using TMPro;

public abstract class Enemy : Entity
{
    public int spawnPosIdx;
    protected EnemyData _defaultEnemyData;
    protected EnemyData enemyData;
    public bool CanClear = false;
    protected Player player;

    protected Image _nextActImg;
    protected TMP_Text _nextActText;

    protected Func<UniTask> _nextPattern;

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
        _defaultEnemyData = eD;
        enemyData = _defaultEnemyData.Clone();
        //SetupEntity(enemyData.HP, enemyData.CriticalChance/*, enemyData.CriticalDamage*/);      // 일단 크리티컬 데미지를 시트에 안 넣었음으로 그냥 잠시 주석 처리
        _maxHP.Value = enemyData.HP;
        _curHP.Value = _maxHP.Value;
        _criticalChance.Value = enemyData.CriticalChance;
        _criticalDamage.Value = 150;
        spawnPosIdx = pos;
        player = InGameManager.Instance.Player;
        EnemySubScribe();
    }
    public override async UniTask<bool> TakeDamage(int dmg, bool isHit = true)
    {
        //if (isHit)
        //{
        //    BattleManager.Instance.HitEntity.Item1 = player;
        //}
        if (!await base.TakeDamage(dmg, isHit))
        {
            return false;
        }
        KillEnemy().Forget();
        return true;

    }

    //public async UniTask<bool> TakeDamageEnemy(int dmg)
    //{
    //    await BeforeTakeDamage();
    //    if (!await base.TakeDamage(dmg))
    //    {
    //        await base.AfterTakeDamage();
    //        return false;
    //    }
    //    //int spawn = 0;
    //    //EnemyManager.Instance.enemies.Remove(this);
    //    //EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);
    //    canvas.gameObject.SetActive(false);
    //    KillEnemy().Forget();
    //    return true;

    //    //await base.DieAnimation();  // 죽는 애니매이션 이후 클리어 확인(만약 죽는 애니메이션이 0초라면, 오류가 날 수 있음.)
    //    //InGameManager.Instance.ChangeCoinValue(enemyData.dropCoin);
    //    //for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
    //    //{
    //    //    if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
    //    //        return true;
    //    //    //spawn++;
    //    //}
    //    ////if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
    //    //MapManager.Instance.ClearStage();
    //    //MapManager.Instance.RewardStage();

    //    //return true;
    //    ////await DieAnimation();
    //    ////Destroy(gameObject);

    //protected virtual int ResistDamage(int damage)
    //{
    //    return damage;
    //}

    public virtual void CheckIfDead(int damage, int count, bool isHit = true)
    {
        int resistDamage = damage;
        if (isHit && ApplyStatusEffect(StatusEffect.Protect, out int amount))
        {
            if (damage > amount)
            {
                resistDamage = damage - amount;      // BeforeTakeDamage로 얻을 _shield 양만큼 빼서 계산.
            }
            else
            {
                resistDamage = 0;
            }
        }
        //int resistDamage = ResistDamage(damage);
        if (((_curHP.Value + _shield.Value) - (resistDamage * count)) <= 0)
        {
            _col2D.enabled = false;
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

        bool clear = await EnemyManager.Instance.KillEnemyCheck(this, base.DieAnimation(true));
        
        //EnemyManager.Instance.enemySpawnPosition[spawnPos].gameObject.SetActive(true);      // 에너미 자리로 클리어 확인을 하기 때문에 적 죽는 모션 기다린 후, 자리 삭제  // 자리는 나중에 배열로 만들고 코드상으로만 확인하도록 변경
        if (player.ApplyStatusEffect(StatusEffect.CoinGained, out int coinGain))
        {
            InGameManager.Instance.ChangeCoinValue((int)(enemyData.DropCoin * (1f + coinGain * 0.01f)));
            GameManager.Instance.AddGoods((int)(enemyData.DropCoin * (1f + coinGain * 0.01f) * 0.5f));
        }
        else
        {
            InGameManager.Instance.ChangeCoinValue((enemyData.DropCoin));
            GameManager.Instance.AddGoods((int)(enemyData.DropCoin * 0.5f));
        }
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
        await AttackAnimation(true);
        //BattleManager.Instance.HitEntity.Item2 = this;
        EnemyManager.Instance.HitEnemy = this;
        int criticalDamage = CheckCritical(damage);

        if (ApplyStatusEffect(StatusEffect.Thievery, out int amount) )
        {
            enemyData.DropCoin += -InGameManager.Instance.ChangeCoinValue(-amount);
        }

        await player.TakeDamage(criticalDamage);

        //Critical(_criticalChance.Value);
    }

    //protected virtual async UniTask BeforeTakeDamage()
    //{
    //    if (ApplyStatusEffect(StatusEffect.Protect, out int amount) != 0)
    //    {
    //        await Shield(amount);
    //    }
    //    else
    //    {
    //        await UniTask.CompletedTask;
    //    }
    //}

    //protected virtual async UniTask AfterTakeDamage()
    //{
    //    if (ApplyStatusEffect(StatusEffect.Reflection, out int amount) != 0)
    //    {
    //        player.TakeDamagePlayer(amount).Forget();
    //    }
    //    else
    //    {
    //        await UniTask.CompletedTask;
    //    }
    //}

    public abstract void NextPattern();

    protected virtual void AttackPattern(int value, int repeat = 1/*, bool addPattern = false*/, float delay = 0.3f)
    {
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));       턴을 1턴으로 만들어서 굳이 제거 안 해도 됨.
        AddStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information), _criticalChance.Value * repeat);
        AddStatusEffect((StatusEffect.Attack, StatusEffectType.Information), value * repeat);
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);

        //string valueText = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

        //Func<UniTask> func = () => UniTask.Create(async () =>
        //{
        //    await Attack(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Attack(value);
        //    }
        //});

        //if (addPattern)
        //{
        //    _nextActText.text += "/" + valueText;
        //    _nextPattern += func;
        //}
        //else
        //{
        //    _nextActText.text = valueText;
        //    _nextPattern = func;
        //}

        _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        _nextPattern = () => UniTask.Create(async () =>
        {
            await Attack(value);
            for (int i = repeat - 1; i > 0; --i)
            {
                await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
                await Attack(value);
            }
        });
    }
    protected virtual void DefensePattern(int value, int repeat = 1/*, bool addPattern = false*/, float delay = 0.3f)
    {
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));
        AddStatusEffect((StatusEffect.Defense, StatusEffectType.Information), value * repeat);
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(1);

        //string valueText = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

        //Func<UniTask> func = () => UniTask.Create(async () =>
        //{
        //    await Shield(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Shield(value);
        //    }
        //});

        //if (addPattern)
        //{
        //    _nextActText.text += "/" + valueText;
        //    _nextPattern += func;
        //}
        //else
        //{
        //    _nextActText.text = valueText;
        //    _nextPattern = func;
        //}

        _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        _nextPattern = () => UniTask.Create(async () =>
        {
            await Shield(value);
            for (int i = repeat - 1; i > 0; --i)
            {
                await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
                await Shield(value);
            }
        });
    }
    protected virtual void HealPattern(int value, int repeat = 1/*, bool addPattern = false*/, float delay = 0.3f)
    {
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));
        AddStatusEffect((StatusEffect.Heal, StatusEffectType.Information), value * repeat);
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);

        //string valueText = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

        //Func<UniTask> func = () => UniTask.Create(async () =>
        //{
        //    await Heal(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Heal(value);
        //    }
        //});

        //if (addPattern)
        //{
        //    _nextActText.text += "/" + valueText;
        //    _nextPattern += func;
        //}
        //else
        //{
        //    _nextActText.text = valueText;
        //    _nextPattern = func;
        //}

        _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        _nextPattern = () => UniTask.Create(async () =>
        {
            await Heal(value);
            for (int i = repeat - 1; i > 0; --i)
            {
                await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
                await Heal(value);
            }
        });
    }

    protected virtual void SpecialPattern(int value, int repeat = 1, float delay = 0.3f)
    {
        AddStatusEffect((StatusEffect.Special, StatusEffectType.Information), value * repeat);
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
        _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
    }

    public async UniTask PlayPattern()
    {
        if (_nextPattern == null) await UniTask.CompletedTask;
        await _nextPattern();
        if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
            return;
        _nextActImg.gameObject.SetActive(false);
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
    void EnemySubScribe()
    {
        EntitySubScribe();
        AttackPower.Subscribe(atk =>
        {
            enemyData.Damage = _defaultEnemyData.Damage + atk;
        });

        _nextActImg = canvas.transform.Find("NextAct").GetComponent<Image>();
        _nextActText = _nextActImg.transform.GetComponentInChildren<TMP_Text>();
        _nextActImg.gameObject.SetActive(true);
    }
    //void Start()      // 모든 상위 코드에 적용시켜야 함.
    //{
    //    EntitySubScribe();
    //    //ArrowCursor = FindObjectOfType<Arrow>(true);
    //}
}
