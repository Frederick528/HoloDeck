using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;

public abstract class Enemy : Entity
{
    public int spawnPosIdx;
    protected EnemyData _defaultEnemyData;
    protected EnemyData enemyData;
    public bool CanClear = false;
    protected Player player;

    protected Image _nextActImg;
    protected TMP_Text _nextActText;

    protected List<Func<UniTask>> _nextPattern = new();

    int _checkRepeat = 1;

    //public bool Death;

    protected override bool BoolOnMouseEnter()
    {
        if (!base.BoolOnMouseEnter())
        {
            return false;
        }
        EnemyManager.Instance.EnemyInfo = this;
        return true;
    }
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
        MaxHP.Value = enemyData.HP;
        CurHP.Value = MaxHP.Value;
        _criticalChance.Value = enemyData.CriticalChance;
        CriticalDamage.Value = 150;
        spawnPosIdx = pos;
        player = InGameManager.Instance.Player;
        EnemySubScribe();
    }
    public override async UniTask<bool> TakeDamage(int dmg, Entity attacker = null)
    {
        //if (isHit)
        //{
        //    BattleManager.Instance.HitEntity.Item1 = player;
        //}
        if (!await base.TakeDamage(dmg, attacker))
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

    //public virtual void CheckIfDead(int damage, int count, bool isHit = true)
    //{
    //    int resistDamage = damage;
    //    if (isHit && ApplyStatusEffect(StatusEffect.Protect, out int amount))
    //    {
    //        if (damage > amount)
    //        {
    //            resistDamage = damage - amount;      // BeforeTakeDamage로 얻을 _shield 양만큼 빼서 계산.
    //        }
    //        else
    //        {
    //            resistDamage = 0;
    //        }
    //    }
    //    //int resistDamage = ResistDamage(damage);
    //    if (((CurHP.Value + CurShield.Value) - (resistDamage * count)) <= 0)
    //    {
    //        _col2D.enabled = false;
    //        CanClear = true;
    //        foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //        {
    //            if (!enemy.CanClear)
    //            {
    //                //EnemyManager.Instance.MapClear = false;
    //                return;
    //            }
    //        }
    //        EnemyManager.Instance.MapClear = true;
    //        CardManager.Instance.SetCardState(1);       // Over
    //    }
    //}

    public async UniTaskVoid KillEnemy()        // 클리어 체크도 같이 함.
    {
        //Death = true;
        //EnemyManager.Instance.enemies.Remove(this);
        //bool clear = EnemyManager.Instance.enemies.Count == 0;
        //await base.DieAnimation();  // destroy(gameObject)가 들어가있기 때문에, 만약 죽고 난 다음에 추가 행동이 있다면, 이 함수 내에서 작동해야 함.

        // 자기 드랍템을 상자에 넣는 코드 필요.

        /*bool clear = */await EnemyManager.Instance.KillEnemyCheck(this, base.DieAnimation(true));
        
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
        if (EnemyManager.Instance.NoEnemy)
        {
            ClearCheck();
        }
    }

    public void ClearCheck()
    {
        //if (!EnemyManager.Instance.MapClear/* || (EnemyManager.Instance.enemies.Count != 0 || MapManager.Instance.currStage.rewardBox != -1)*/)
        //    return;
        //EnemyManager.Instance.MapClear = false;


        //for (int i = 0; i < EnemyManager.Instance.enemySpawnPosition.Count; ++i)
        //{
        //    if (!EnemyManager.Instance.enemySpawnPosition[i].gameObject.activeSelf)     // 몬스터가 다 죽어있으면 게임이 클리어되고, 한 마리라도 살아있으면 리턴되어 그냥 몬스터만 죽고 끝.
        //        return;
        //    //spawn++;
        //}
        //if (spawn == EnemyManager.Instance.enemySpawnPosition.Count)
        MapManager.Instance.ClearStage().Forget();
        MapManager.Instance.RewardStage();
        //MapManager.Instance.currStage.DropLootBox();
        ItemManager.Instance.Charge(1);
    }

    protected async UniTask Attack(int damage)          // 크리티컬 판정 때문에 Attack Pattern에서 실행해야함.
    {
        await AttackAnimation(true);
        //BattleManager.Instance.HitEntity.Item2 = this;
        //EnemyManager.Instance.HitEnemy = this;
        //int criticalDamage = CheckCritical(damage);
        if (ApplyStatusEffect(StatusEffect.Thievery, out int amount) )
        {
            enemyData.DropCoin += -InGameManager.Instance.ChangeCoinValue(-amount);
        }

        await player.TakeDamage(damage, this);

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

    //int CheckCriticalDamage(int dmamge, bool critical)
    //{
    //    int criticalDamage = dmamge;
    //    //int criticalDamage = Mathf.RoundToInt(enemyData.Damage * multiple);
    //    if (critical)
    //        criticalDamage = Mathf.RoundToInt(dmamge * CriticalDamage.Value * 0.01f + 0.0001f);     // 부동소수점 오류
    //    //criticalDamage = Mathf.RoundToInt(enemyData.Damage * multiple * CriticalDamage.Value * 0.01f);
    //    return criticalDamage;
    //}

    public abstract void NextPattern();

    protected virtual void AttackPattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3f)
    {
        _checkRepeat = repeat;
        //GetStatusEffect(StatusEffect.ATKUp, out int addATK);      // 어택 파워 값으로 바로 확인 가능
        int damage = value + AttackPower.Value;
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));       턴을 1턴으로 만들어서 굳이 제거 안 해도 됨.
        //if (!GetStatusEffect(StatusEffect.UseCritical, out _))        // 크리티컬 터지면 에너지 회복 안 됨
            //AddStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information), _criticalChance.Value);     // 치명타 획득 확률을 원래 보여줬는데, 그냥 우클릭으로 확인하게 하고, 전투 중에 확인 할 수 없게 변경
        AddStatusEffect((StatusEffect.Attack, StatusEffectType.Information), damage * repeat);
        //AddStatusEffect((StatusEffect.Attack, StatusEffectType.Information), Mathf.RoundToInt(enemyData.Damage * multiple * repeat));

        //string valueText = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

        //Func<UniTask> func = async () => await UniTask.Create(async () =>
        //{
        //    print("A");
        //    await Attack(Mathf.RoundToInt(enemyData.Damage * multiple));         // 여기 부분 고쳐야 함.
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
        //        await Attack(Mathf.RoundToInt(enemyData.Damage * multiple));
        //    }
        //});
        if (GetStatusEffect(StatusEffect.UseCritical, out _))
        {
            if (addPattern)
            {
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
                _nextActText.text = (repeat > 1 ? $"{damage}*{repeat}<color=green>+{CheckCriticalDamage(damage, true)-damage}*{repeat}" : $"{CheckCriticalDamage(damage, true)}</color>") + "/" + _nextActText.text;
            }
            else
            {
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                _nextActText.text = (repeat > 1 ? $"{damage}*{repeat}<color=green>+{CheckCriticalDamage(damage, true)-damage}*{repeat}" : $"{CheckCriticalDamage(damage, true)}</color>");
            }
        }
        else
        {
            if (addPattern)
            {
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
                _nextActText.text = (repeat > 1 ? $"{damage}*{repeat}" : damage.ToString()) + "/" + _nextActText.text;
            }
            else
            {
                _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);
                _nextActText.text = repeat > 1 ? $"{damage}*{repeat}" : damage.ToString();
            }
        }
        //if (addPattern)
        //{
        //    _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
        //    _nextActText.text = (repeat > 1 ? $"{damage}*{repeat}" : damage.ToString()) + "/" + _nextActText.text;
        //    //_nextPattern.Add(func);
        //}
        //else
        //{
        //    _nextActImg.sprite = EnemyManager.Instance.NextActImg(0);

        //    _nextActText.text = repeat > 1 ? $"{damage}*{repeat}" : damage.ToString();
        //    //_nextActText.text = repeat > 1 ? $"{Mathf.RoundToInt(enemyData.Damage * multiple)}*{repeat}" : (Mathf.RoundToInt(enemyData.Damage * multiple)).ToString();
        //    //_nextPattern.Add(func);
        //}

        _nextPattern.Add(async () => await UniTask.Create(async () =>
        {
            //bool critical = CheckCritical();      // 공격하기 전에 크리티컬 확인
            bool critical = GetStatusEffect(StatusEffect.UseCritical, out _);      // 공격하기 전에 크리티컬 확인
            //if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
            //    return;
            //await Attack(Mathf.RoundToInt(enemyData.Damage * multiple));         // 여기 부분 고쳐야 함.
            for (int i = repeat; i > 0; --i)
            {
                //GetStatusEffect(StatusEffect.ATKUp, out int addATK);
                damage = value + AttackPower.Value;
                //_nextActText.text = _checkRepeat > 1 ? $"{damage}*{_checkRepeat--}" : damage.ToString();
                await UniTask.WaitForSeconds(delay/*, cancellationToken: TurnManager.Instance.CancelSource.Token*/);
                if (this.CurHP.Value > 0)
                    await Attack(CheckCriticalDamage(damage, critical));
                //await Attack(CheckCriticalDamage(multiple, critical));
                //await Attack(Mathf.RoundToInt(criticalDamage * multiple));
            }
            CheckCritical();
        }));
        //_nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        //_nextPattern = () => UniTask.Create(async () =>
        //{
        //    await Attack(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Attack(value);
        //    }
        //});
    }
    protected virtual void DefensePattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3f)
    {
        _checkRepeat = repeat;
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));
        AddStatusEffect((StatusEffect.Defense, StatusEffectType.Information), value * repeat);

        //Func<UniTask> func = async () => await UniTask.Create(async () =>
        //{
        //    print("D");
        //    await Shield(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
        //        await Shield(value);
        //    }
        //});

        if (addPattern)
        {
            _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
            _nextActText.text = (repeat > 1 ? $"{value}*{repeat}" : value.ToString()) + "/" + _nextActText.text;
            //_nextPattern = (Func<UniTask>)Delegate.Combine(func, _nextPattern);
        }
        else
        {
            _nextActImg.sprite = EnemyManager.Instance.NextActImg(1);
            _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
            //_nextPattern = func;
        }

        _nextPattern.Add(async () => await UniTask.Create(async () =>
        {
            //if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
            //    return;
            //await Shield(value);         // 여기 부분 고쳐야 함.
            for (int i = repeat; i > 0; --i)
            {
                await UniTask.WaitForSeconds(delay/*, cancellationToken: TurnManager.Instance.CancelSource.Token*/);
                if (this.CurHP.Value > 0)
                    await Shield(value);
            }
        }));

        //_nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        //_nextPattern = () => UniTask.Create(async () =>
        //{
        //    await Shield(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Shield(value);
        //    }
        //});
    }
    protected virtual void HealPattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3f)
    {
        _checkRepeat = repeat;
        //RemoveStatusEffect((_nextPattern.Item1, StatusEffectType.Information));
        AddStatusEffect((StatusEffect.Heal, StatusEffectType.Information), value * repeat);

        //string valueText = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

        //Func<UniTask> func = async () => await UniTask.Create(async () =>
        //{
        //    print("H");
        //    await Heal(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
        //        await Heal(value);
        //    }
        //});

        if (addPattern)
        {
            _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
            _nextActText.text = (repeat > 1 ? $"{value}*{repeat}" : value.ToString()) + "/" + _nextActText.text;
            //_nextPattern = (Func<UniTask>)Delegate.Combine(func, _nextPattern);
        }
        else
        {
            _nextActImg.sprite = EnemyManager.Instance.NextActImg(2);
            _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
            //_nextPattern = func;
        }

        _nextPattern.Add(async () => await UniTask.Create(async () =>
        {
            //if (TurnManager.Instance.CancelSource.Token.IsCancellationRequested)
            //    return;
            //await Heal(value);
            for (int i = repeat; i > 0; --i)
            {
                await UniTask.WaitForSeconds(delay/*, cancellationToken: TurnManager.Instance.CancelSource.Token*/);
                if (this.CurHP.Value > 0)
                    await Heal(value);
            }
        }));

        //_nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
        //_nextPattern = () => UniTask.Create(async () =>
        //{
        //    await Heal(value);
        //    for (int i = repeat - 1; i > 0; --i)
        //    {
        //        await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        //        await Heal(value);
        //    }
        //});
    }

    protected virtual void SpecialPattern(int value, int repeat = 1, bool addPattern = false, float delay = 0.3f)
    {
        _checkRepeat = repeat;
        AddStatusEffect((StatusEffect.Special, StatusEffectType.Information), value * repeat);

        if (addPattern)
        {
            _nextActText.text = (repeat > 1 ? $"{value}*{repeat}" : value.ToString()) + "/" + _nextActText.text;
            //_nextPattern += func;
        }
        else
        {
            _nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();
            //_nextPattern = func;
        }
        _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
        //_nextActText.text = repeat > 1 ? $"{value}*{repeat}" : value.ToString();

    }

    //protected virtual void MultiplePatterns(params Action<int, int, float>[] pattern)
    //{
    //    if (pattern.Length == 0) return;
    //    pattern();
    //    _nextActImg.sprite = EnemyManager.Instance.NextActImg(3);
    //}

    public async UniTask PlayPattern()
    {
        //if (_nextPattern == null) await UniTask.CompletedTask;
        _nextActImg.gameObject.SetActive(false);
        RemoveStatusEffect((StatusEffect.Attack, StatusEffectType.Information));
        //RemoveStatusEffect((StatusEffect.GetCritical, StatusEffectType.Information));
        RemoveStatusEffect((StatusEffect.Heal, StatusEffectType.Information));
        RemoveStatusEffect((StatusEffect.Defense, StatusEffectType.Information));
        RemoveStatusEffect((StatusEffect.Special, StatusEffectType.Information));
        for (int i = _nextPattern.Count - 1; i >= 0; --i)
        {
            await _nextPattern[i]();
        }
        //foreach (var pattern in _nextPattern)
        //{
        //    await pattern();
        //}
        if (!TurnManager.Instance.InBattle)
            return;
        _nextPattern.Clear();
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
        _nextActImg = canvas.transform.Find("NextAct").GetComponent<Image>();
        _nextActText = _nextActImg.transform.GetComponentInChildren<TMP_Text>();
        _nextActImg.gameObject.SetActive(true);

        AttackPower.Pairwise().Subscribe(atk =>
        {
            enemyData.Damage = _defaultEnemyData.Damage + atk.Current;
            if (GetStatusEffect(StatusEffect.Attack, out int attack))
            {
                if ((atk.Current - atk.Previous) >= 0)
                {
                    AddStatusEffect((StatusEffect.Attack, StatusEffectType.Information), (atk.Current - atk.Previous) * _checkRepeat);
                }
                else
                {
                    ReduceStatusEffect((StatusEffect.Attack, StatusEffectType.Information), (atk.Current - atk.Previous) * _checkRepeat, 0);
                }
                //AddStatusEffect((StatusEffect.Attack, StatusEffectType.Information), (atk.Current - atk.Previous) * _checkRepeat);
                if (_nextActImg.sprite != EnemyManager.Instance.NextActImg(3))
                {
                    int damagePerHit = attack / _checkRepeat + atk.Current - atk.Previous;
                    if (GetStatusEffect(StatusEffect.UseCritical, out _))
                    {
                        if (_checkRepeat > 1)
                        {
                            _nextActText.text = $"{damagePerHit}*{_checkRepeat}<color=green>+{CheckCriticalDamage(damagePerHit, true) - damagePerHit}*{_checkRepeat}</color>";
                        }
                        else
                        {
                            _nextActText.text = $"{attack + atk.Current - atk.Previous}<color=green>+{CheckCriticalDamage(damagePerHit, true) - damagePerHit}</color>";
                        }
                    }
                    else
                    {
                        if (_checkRepeat > 1)
                        {
                            _nextActText.text = $"{damagePerHit}*{_checkRepeat}";
                        }
                        else
                        {
                            _nextActText.text = $"{attack + atk.Current - atk.Previous}";
                        }
                    }
                }
            }
        }).AddTo(this);

        _curCritical.Pairwise().Subscribe(critical =>
        {
            if (GetStatusEffect(StatusEffect.Attack, out int attack) && _nextActImg.sprite != EnemyManager.Instance.NextActImg(3))
            {
                int damagePerHit = attack / _checkRepeat;
                if (critical.Previous >= _useCritical.Value && critical.Current < _useCritical.Value)
                {
                    if (_checkRepeat > 1)
                    {
                        _nextActText.text = $"{damagePerHit}*{_checkRepeat}";
                    }
                    else
                    {
                        _nextActText.text = $"{attack}";
                    }
                }
                else if (critical.Previous < _useCritical.Value && critical.Current >= _useCritical.Value)
                {
                    if (_checkRepeat > 1)
                    {
                        _nextActText.text = $"{damagePerHit}*{_checkRepeat}<color=green>+{CheckCriticalDamage(damagePerHit, true) - damagePerHit}*{_checkRepeat}</color>";
                    }
                    else
                    {
                        _nextActText.text = $"{attack}<color=green>+{CheckCriticalDamage(damagePerHit, true) - damagePerHit}</color>";
                    }
                }
            }
        }).AddTo(this);

    }

    protected override void AddStatusEffectDesc((StatusEffect, StatusEffectType) statusEffect, StringBuilder sb, (int, int) info)
    {
        switch (statusEffect.Item1)
        {
            case StatusEffect.Attack:
                if (GetStatusEffect(StatusEffect.UseCritical, out _))
                {
                    if (_checkRepeat > 1)
                    {
                        int damagePerHit = info.Item1 / _checkRepeat;
                        sb.Replace("{n}", $"<color=green>{info.Item1} + 치명타 피해({(CheckCriticalDamage(damagePerHit, true) - damagePerHit)*_checkRepeat}) </color>");
                    }
                    else
                    {
                        sb.Replace("{n}", $"<color=green>{info.Item1} + 치명타 피해({CheckCriticalDamage(info.Item1, true) - info.Item1}) </color>");
                    }
                }
                else
                    sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
                break;
            case StatusEffect.Defense:
                sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
                break;
            case StatusEffect.Heal:
                sb.Replace("{n}", $"<color=green>{info.Item1}</color>");
                break;
        }
        base.AddStatusEffectDesc(statusEffect, sb, info);
    }

    protected override string ChangeInformationLV((StatusEffect, StatusEffectType) statusEffect, (int, int) info)
    {
        if (statusEffect.Item1 == StatusEffect.Attack && GetStatusEffect(StatusEffect.UseCritical, out _))
        {
            if (_checkRepeat > 1)
            {
                int damagePerHit = info.Item1 / _checkRepeat;
                return $"LV: <color=green>{info.Item1} + {(CheckCriticalDamage(damagePerHit, true) - damagePerHit) * _checkRepeat} </color>";
            }
            else
            {
                return $"LV: <color=green>{info.Item1} + {CheckCriticalDamage(info.Item1, true) - info.Item1} </color>";
            }
        }
        return base.ChangeInformationLV(statusEffect, info);
    }

    //void Start()      // 모든 상위 코드에 적용시켜야 함.
    //{
    //    EntitySubScribe();
    //    //ArrowCursor = FindObjectOfType<Arrow>(true);
    //}
}
