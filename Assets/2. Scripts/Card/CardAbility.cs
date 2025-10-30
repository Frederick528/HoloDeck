using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardAbility
{
    //public UniTask Task;
    //public UniTask<Action<Card>> cardActionTask;

    //int _startTask;
    //int _endTask;
    //int _checkTask;
    CancellationTokenSource _cts;

    Action _cardImmediately;
    Func<UniTask> _cardTask;
    Func<UniTask<bool>?> _conditionTask;

    Player _player;
    public void SetCardAbility(Card card)
    {
        _player = InGameManager.Instance.Player;
        SettingImmediately(card);

        if (card.Data.HasSimpleCondition)
        {
            SettingCondition(card);
        }

        if (card.Data.IsSimpleAB)
        {
            SettingSimpleAB(card);
        }
        else
        {
            SettingCardAB(card);
        }
        card.SetCardImmediately(_cardImmediately);
        card.SetCardTask(_cardTask);
        card.SetUseConditions(_conditionTask);
        //if (!_firstInitialize)
        //{
        //    _firstInitialize = true;
        //    _player = InGameManager.Instance.Player;
        //    if (card.Data.HasSimpleCondition)
        //    {
        //        SettingCondition(card);
        //    }
        //    if (card.Data.IsSimpleAB)
        //    {
        //        SettingSimpleAB(card);
        //    }
        //    else
        //    {
        //        switch (card.Data.ID)
        //        {
        //            case 105:
        //                //card.UseConditions = () => UniTask.Create(async () =>
        //                //{
        //                //    CardManager.Instance.SetCardState(1);
        //                //    await UniTask.CompletedTask;        // 사실 없어도 됨.
        //                //    return true;

        //                //});
        //                _cardTask = () => UniTask.Create(async () =>
        //                {
        //                    CardManager.Instance.SetCardState(1);
        //                    await AddCardEvent(card, 0.5f,
        //                        (0, () => DrawAB(card)),
        //                        (1, () => ConfirmedDiscardAB())
        //                    );
        //                    //await ConfirmedDiscardAB();
        //                });
        //                break;
        //            case 503:
        //                _cardTask = () => UniTask.Create(async () =>
        //                {
        //                    await DelayTask(0.5f);
        //                    _player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
        //                });
        //                break;
        //            case 801:
        //                _cardTask = () => UniTask.Create(async () =>
        //                {
        //                    await AddCardEvent(card, 0.5f,
        //                        (0, async () =>
        //                        {
        //                            card.Data.Count = card.Data.Cost;
        //                            await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
        //                        }
        //                    )
        //                    );
        //                    //await DelayTask(0.5f);
        //                    //card.Data.Count = card.Data.Cost;
        //                    //await SingleAttackAB(card, true);
        //                });
        //                break;
        //            default:
        //                _cardTask = () => UniTask.CompletedTask;
        //                break;
        //        }
        //    }
        //    card.SetCardTask(_cardTask);
        //    card.SetUseConditions(_conditionTask);
        //}
        //card.SetUseConditions(null);          // 능력과 조건문은 초기화 해줘야 함. 능력은 모두 초기화 시키지만, 조건문은 일부 초기화가 안 되서 오류가 발생하는 경우 존재.
        //if (card.Data.HasSimpleCondition)
        //{
        //    SettingCondition(card);
        //}
        //if (card.Data.IsSimpleAB)
        //{
        //    SettingSimpleAB(card);
        //}
        //else
        //{
        //    switch (card.Data.ID)
        //    {
        //        case 105:
        //            //card.UseConditions = () => UniTask.Create(async () =>
        //            //{
        //            //    CardManager.Instance.SetCardState(1);
        //            //    await UniTask.CompletedTask;        // 사실 없어도 됨.
        //            //    return true;

        //            //});
        //            card.SetCardTask(() => UniTask.Create(async () =>
        //            {
        //                CardManager.Instance.SetCardState(1);
        //                await AddCardEvent(card, 0.5f,
        //                    (0, () => DrawAB(card)),
        //                    (1, () => ConfirmedDiscardAB())
        //                );
        //                //await ConfirmedDiscardAB();
        //            }));
        //            break;
        //        case 503:
        //            card.SetCardTask(() => UniTask.Create(async () =>
        //            {
        //                await DelayTask(0.5f);
        //                _player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
        //            }));
        //            break;
        //        case 801:
        //            card.SetCardTask(() => UniTask.Create(async () =>
        //            {
        //                await AddCardEvent(card, 0.5f,
        //                    (0, async () =>
        //                    {
        //                        card.Data.Count = card.Data.Cost;
        //                        await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
        //                    })
        //                );
        //                //await DelayTask(0.5f);
        //                //card.Data.Count = card.Data.Cost;
        //                //await SingleAttackAB(card, true);
        //            }));
        //            break;
        //        default:
        //            card.SetCardTask(() => UniTask.CompletedTask);
        //            break;
        //    }
        //}
        //return cardActTask;
    }
    void SettingImmediately(Card card)
    {
        switch (card.Data.ID)
        {
            case 105:
                _cardImmediately = () =>
                {
                    CardManager.Instance.SetCardState(1);
                };
                break;
            default:
                _cardImmediately = null ;
                break;
        }
    }

    void SettingCondition(Card card)
    {
        //Func<UniTask<bool>?> uniTaskCondition = null;
        if (card.Data.Discard > 0)
        {
            _conditionTask = () => UniTask.Create(async () =>
            {
                return await ConditionDiscardAB();
            });
        }
        else if (card.Data.Remove > 0)
        {
            _conditionTask = () => UniTask.Create(async () =>
            {
                return await ConditionRemoveAB();
            });
        }
        //card.SetUseConditions(uniTaskCondition);
    }

    void SettingSimpleAB(Card card, float delay = 0.3f)
    {
        //Func<UniTask> uniTaskAB = null;
        bool critical = _player.GetStatusEffect(StatusEffect.UseCritical, out _);
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                            (0, () => SingleAttackAB(card, critical)),
                            (1, () => ShieldAB(card)),
                            (1, () => DrawAB(card))
                        );
                    });
                else
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                            (0, () => SingleAttackAB(card, critical))
                        );
                    });
                break;
            case CardTag.MultiAttack:
                if (card.Data.Shield > 0 || card.Data.Draw > 0)
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                             (0, () => MultiAttackAB(card, critical)),
                             (1, () => ShieldAB(card)),
                             (1, () => DrawAB(card))
                         );
                    });
                else
                    _cardTask = () => UniTask.Create(async () =>
                    {
                        await AddCardEvent(card, delay,
                             (0, () => MultiAttackAB(card, critical))
                         );
                    });
                break;
            case CardTag.Skill:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await AddCardEvent(card, delay,
                             (0, () => ShieldAB(card)),
                             (0, () => DrawAB(card))
                         );
                });
                break;
        }
        //card.SetCardTask(uniTaskAB);

    }

    void SettingCardAB(Card card)
    {
        switch (card.Data.ID)
        {
            case 105:
                _cardTask = () => UniTask.Create(async () =>
                {
                    //CardManager.Instance.SetCardState(1);
                    await AddCardEvent(card, 0.5f,
                        (0, () => DrawAB(card)),
                        (1, () => ConfirmedDiscardAB())
                    );
                });
                break;
            case 503:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await DelayTask(0.5f);
                    _player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
                });
                break;
            case 801:
                _cardTask = () => UniTask.Create(async () =>
                {
                    await AddCardEvent(card, 0.5f,
                        (0, async () =>
                        {
                            card.Data.Count = card.Data.Cost;
                            await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
                        }
                    )
                    );
                });
                break;
            default:
                _cardTask = () => UniTask.CompletedTask;
                break;
        }
    }

    async UniTask PlayCardEvent(Card card, SortedDictionary<int, List<Func<UniTask>>> cardEvent, float delay)
    {
        //var cardEventDict = new SortedDictionary<int, List<Func<UniTask>>>();
        //foreach (var (order, effectTask) in cardEvent)
        //{
        //    if (!cardEventDict.ContainsKey(order))
        //    {
        //        cardEventDict[order] = new List<Func<UniTask>>();
        //    }
        //    cardEventDict[order].Add(effectTask);
        //}
        await UniTask.Create(async () =>
        {
            _cts = new CancellationTokenSource();

            //await SpawnEffect(card);

            //foreach (var eventTask in cardEventDict) // 0, 1, 2... 순서대로 실행
            //{
            //    await UniTask.WhenAll(eventTask.Value); // 동시 실행 및 대기
            //}
            bool isStart = true;
            for (int i = 0; i < card.Data.Count; i++) // 카드 횟수만큼 반복
            {
                if (i != 0)
                {
                    isStart = false;
                }
                await DelayTask(delay);
                await SpawnEffect(card, isStart);
                foreach (var eventTask in cardEvent) // 0, 1, 2... 순서대로 실행
                {
                    var tasksToRun = eventTask.Value.Select(func => func()).ToList();
                    await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow(); // 동시 실행 및 대기
                }
                if (_cts.IsCancellationRequested)
                    break;
            }

            switch (card.Data.CardTag)
            {
                case CardTag.SingleAttack:
                    card.Target(null);
                    _player.CheckCritical();
                    break;
                case CardTag.MultiAttack:
                    _player.CheckCritical();
                    break;
                case CardTag.Skill:
                    break;
            }
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        });
    }

    async UniTask SpawnEffect(Card card, bool start = true)
    {
        if (card.Data.Effect == null)
        {
            //await DelayTask(0.5f);
            return;
        }
        card.UseTimingReset();
        if (!start && !card.RepeatEffect)
        {
            await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
            return;
        }
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                await UniTask.WhenAny(
                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
                    , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                    );
                break;
            case CardTag.MultiAttack:
                if (card.AllEnemies)
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                        );
                }
                else
                {
                    await UniTask.WhenAll(EnemyManager.Instance.EnemyList.Select(async enemy =>
                    {
                        if (enemy != null)
                        {
                            await UniTask.WhenAny(
                                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
                                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
                                );
                            //await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
                        }
                    }));
                }
                break;  
            case CardTag.Skill:
                break;
        }
    }

    async UniTask AddCardEvent(Card card, float delay = 0.3f, params (int, Func<UniTask>)[] taskOrder)
    {
        var cardEventDict = new SortedDictionary<int, List<Func<UniTask>>>();
        foreach (var (order, effectTask) in taskOrder)
        {
            if (!cardEventDict.ContainsKey(order))
            {
                cardEventDict[order] = new List<Func<UniTask>>();
            }
            cardEventDict[order].Add(effectTask);
        }
        await PlayCardEvent(card, cardEventDict, delay);
    }

    
    //async UniTask CheckAllEndTask(bool end = true)
    //{
    //    if (_endTask > 0)
    //        Debug.Log(--_endTask);
    //    await UniTask.WaitUntil(() => _endTask == 0);   // 다른 곳에서 게임 끝났는지 확인하고, 애초에 애는 다 끝났을 때, 값만 0이 됐는지 확인하는 거라 굳이 토큰 필요없음.

    //    if (end)
    //    {
    //        if (!_cts.IsCancellationRequested)
    //        {
    //            _cts.Dispose();
    //            _cts.Cancel();
    //        }
    //        Debug.Log(--_startTask);
    //    }
    //}
    //async UniTask CheckTaskOrder(Card card, int order, bool start = true)
    //{
    //    // 반복인 경우, 반복에서 첫 번째 시작인가?
    //    if (start)
    //    {
    //        _isFirstOrder = true;
    //        ++_startTask;
    //        await UniTask.WaitUntil(() => _endTask == order, cancellationToken: TurnManager.Instance.CancelSource.Token);
    //        Debug.Log($"{order}번째 실행");
    //        //if (card.Data.Effect != null && _isFirstOrder)
    //        //{
    //        //    _isFirstOrder = false;
    //        //    _cts = new CancellationTokenSource();
    //        //    card.UseTimingReset();

    //        //    if (card.Data.CardTag == CardTag.SingleAttack)
    //        //    {
    //        //        await UniTask.WhenAny(
    //        //            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
    //        //            , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
    //        //            );
    //        //    }
    //        //    else if (card.Data.CardTag == CardTag.MultiAttack)
    //        //    {
    //        //        if (card.AllEnemies)
    //        //        {
    //        //            await UniTask.WhenAny(
    //        //                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
    //        //                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
    //        //                );
    //        //        }
    //        //        else
    //        //        {
    //        //            var enemyList = EnemyManager.Instance.EnemyList.ToList();
    //        //            await UniTask.WhenAll(enemyList.Select(async enemy =>
    //        //            {
    //        //                if (enemy != null)
    //        //                {
    //        //                    await UniTask.WhenAny(
    //        //                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
    //        //                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
    //        //                        );
    //        //                    //await UniTask.WaitUntil(() => card.CardUseTiming);
    //        //                    await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
    //        //                }
    //        //            }));
    //        //        }
    //        //    }
    //        //    else
    //        //    {

    //        //    }
    //        //}
    //    }
    //    else
    //    {
    //        await CheckAllEndTask(false);
    //        _isFirstOrder = true;
    //        await UniTask.WaitUntil(() => _endTask == order, cancellationToken: TurnManager.Instance.CancelSource.Token);
    //        Debug.Log($"{order}번째 실행");
    //    }
    //}

    // 카드 사용 시, 사용하는 카드 이벤트 개수만큼 startTask가 증가함. 그 후, 각각의 이벤트가 끝날 때마다 endTask값을 올림.
    // endTask가 startTask만큼 즉, 모든 카드 이벤트가 끝났으면, 해당 카드의 공격타이밍을 초기화하고 초기화한 카드 개수가 startTask와 같을 때까지 기다림.
    // 초기화한 카드 개수를 구하지 않으면, 반복 카드일 경우, endTask를 낮췄다가 다시 올리기에 조건 검사에 문제가 생김.
    //async UniTask CheckTaskCount(Card card)
    //{
    //    // 카드에 시작한 모든 이벤트가 끝났는지 확인(반복에서 사용)
    //    _checkTask = 0;
    //    await UniTask.WaitUntil(() => _startTask == _endTask, cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
    //    if (card.CardUseTiming)
    //        card.UseTimingReset();
    //    ++_checkTask;
    //    // 밑에 내용 상관없음. 그냥 WaitUntil 쓰기로 함.
    //    // 작거나 같은 경우를 쓰는 이유: 각각의 카드 이벤트가 마무리 될 때마다 startTask를 1씩 감소시킴. 이 때, checkTask값이 startTask 값보다 커지게 되는데
    //    // 굳이 WaitUntil 써서 밑에 endTask가 0이 될 때까지 대기하는 것보단, 이 방식이 더 나을 것 같음. 
    //    await UniTask.WaitUntil(() => _startTask == _checkTask, cancellationToken: TurnManager.Instance.CancelSource.Token);
    //    //if (await UniTask.WaitUntil(() => _startTask == _endTask, cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow())
    //    //{
    //    //    Debug.Log("ASDASD");
    //    //    _startTask = 0;
    //    //    _endTask = 0;
    //    //}
    //    //if (card.RepeatEffect && card.PlayEffect != null)
    //    //{
    //    //    card.PlayEffect = null;
    //    //}
    //}

    async UniTask SingleAttackAB(Card card, bool critical)             // 컨티뉴 single이랑 그냥 single 합침.
    {
        //await CheckTaskOrder(card, order);

        //bool critical = _player.GetStatusEffect(StatusEffect.UseCritical, out _);


        //if (card.Data.Effect != null && _isFirstOrder)
        //{
        //    _isFirstOrder = false;
        //    _cts = new CancellationTokenSource();
        //    card.UseTimingReset();
        //    await UniTask.WhenAny(
        //        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
        //        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //        );
        //}
        //else if (card.Data.Effect != null && order == 0)
        //{
        //    await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
        //}

        bool killEnemy = await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
        if (killEnemy)
        {
            //Debug.Log(_cts);
            _cts.Cancel();
            _cts.Dispose();
        }
        //Debug.Log(++_endTask);
        //if (!killEnemy)
        //{
        //    for (int i = 1; i < card.Data.Count; ++i)
        //    {
        //        if (await CheckTaskCount(card).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }
        //        if (await CheckTaskOrder(card, order, false).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }

        //        if (card.Data.Effect != null && card.RepeatEffect && _isFirstOrder)     // 이펙트가 없고 이펙트를 재생성해야 하는데, 가장 먼저 실행되는 오더일 경우, 이펙트 소환
        //        {
        //            _isFirstOrder = false;
        //            await DelayTask(delay);
        //            await UniTask.WhenAny(
        //                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
        //                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //                );
        //        }
        //        else if (card.Data.Effect != null && order == 0)        // 이펙트가 있고(공격타이밍을 받아올 수 있다는 뜻) 우선순위가 0인 오더일 경우(0인 오더가 여러 개이면 가장 먼저는 아닐 수 있음.) 이펙트 소환 없이 타이밍만 받아옴.
        //        {
        //            await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
        //        }
        //        else if (card.Data.Effect == null)
        //        {
        //            await DelayTask(delay);
        //        }
        //        //card.UseTimingReset();
        //        //if (card.Data.Effect != null && order == 0)
        //        //{
        //        //    if (card.RepeatEffect)
        //        //    {
        //        //        await DelayTask(delay);
        //        //        await UniTask.WhenAny(
        //        //            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
        //        //            , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
        //        //            );
        //        //    }
        //        //    else
        //        //    {
        //        //        await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
        //        //    }
        //        //}
        //        //damage = _player.CheckCritical(card.Data.Damage);
        //        if (await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical)))
        //        {
        //            Debug.Log(++_endTask);
        //            //i = card.Data.Count;
        //            break;
        //        }
        //        else
        //        {
        //            Debug.Log(++_endTask);
        //        }
        //    }
        //}
        //_cts.Cancel();
        //_cts.Dispose();
        //card.Target(null);
        //_player.CheckCritical();
        //await CheckTaskCount(card).SuppressCancellationThrow();
        //await CheckAllEndTask();

        //Debug.Log(--_startTask);
    }
    async UniTask MultiAttackAB(Card card, bool critical)              // 컨티뉴 multi랑 그냥 multi 합침.
    {
        //Debug.Log(++_startTask);
        //await CheckTaskOrder(card, order);
        //if (EnemyManager.Instance.EnemyList.Count == 0)
        //{
        //    Debug.Log(--_startTask);
        //    return;
        //}
        //bool critical = _player.CheckCritical();
        //bool critical = _player.GetStatusEffect(StatusEffect.UseCritical, out _);
        //int damage = _player.CheckCritical(card.Data.Damage);


        var enemyList = EnemyManager.Instance.EnemyList.ToList();

        await UniTask.WhenAll(enemyList.Select(async enemy =>
        {
            if (enemy != null)
            {
                await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
            }
        }));
        if (EnemyManager.Instance.NoEnemy) // 여긴 InBattle로 체크 안 함. InBattle은 적 죽는 모션 끝나는 것까지 기다려야 함. (+클리어 판정이 아닌, 현재 필드에 남아있는 적이 없다는 뜻)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        ////var cts = new CancellationTokenSource();

        ////card.UseTimingReset();
        //if (card.AllEnemies)
        //{
        //    //if (card.PlayEffect == null)
        //    //{
        //    //card.PlayEffect = UniTask.Lazy(async () => await PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3)));
        //    await UniTask.WhenAny(
        //        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
        //        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //        );
        //    //}
        //    //else
        //    //{
        //    //    await UniTask.WhenAny(
        //    //        card.PlayEffect.Task
        //    //        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
        //    //        );
        //    //}

        //    await UniTask.WhenAll(enemyList.Select(async enemy =>
        //    {
        //        if (enemy != null)
        //        {
        //            await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        //        }
        //    }));
        //    Debug.Log(++_endTask);

        //    for (int i = 1; i < card.Data.Count; ++i)
        //    {
        //        if (await CheckTaskCount(card).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }
        //        if (await CheckTaskOrder(card, order, false).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }

        //        if (EnemyManager.Instance.EnemyList.Count == 0)
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }
        //        else
        //        {
        //            enemyList = EnemyManager.Instance.EnemyList.ToList();
        //        }
        //        //card.UseTimingReset();
        //        if (card.Data.Effect != null && order == 0)
        //        {
        //            // 이펙트가 반복 이펙트인 경우(한 번 소환하고 끝이 아니라 계속 소환하는 경우)
        //            if (card.RepeatEffect)
        //            {
        //                await DelayTask(delay);
        //                // 이펙트가 끝나거나 타이밍을 받아올 때까지 대기.
        //                await UniTask.WhenAny(
        //                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos, Quaternion.identity, Vector3.one * 3))
        //                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //                );
        //            }
        //            else
        //            {
        //                await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);        // 해당 경우에는 await를 얘만 하기에 따로 토큰 줘도 의미 없긴 함.
        //            }
        //        }

        //        await UniTask.WhenAll(enemyList.Select(async enemy =>
        //        {
        //            if (enemy != null)
        //            {
        //                await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        //            }
        //        }));
        //        Debug.Log(++_endTask);
        //    }
        //}
        //else
        //{
        //    await UniTask.WhenAll(enemyList.Select(async enemy =>
        //    {
        //        if (enemy != null)
        //        {
        //            await UniTask.WhenAny(
        //                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
        //                , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //                );
        //            //await UniTask.WaitUntil(() => card.CardUseTiming);
        //            await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        //        }
        //    }));

        //    Debug.Log(++_endTask);

        //    for (int i = 1; i < card.Data.Count; ++i)
        //    {
        //        if (await CheckTaskCount(card).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }
        //        if (await CheckTaskOrder(card, order, false).SuppressCancellationThrow())
        //        {
        //            //i = card.Data.Count;
        //            break;
        //        }

        //        if (EnemyManager.Instance.EnemyList.Count == 0)
        //        {

        //            break;
        //        }
        //        else
        //        {
        //            enemyList = EnemyManager.Instance.EnemyList.ToList();
        //        }
        //        //enemyList = EnemyManager.Instance.EnemyList.ToList();
        //        //card.UseTimingReset();
        //        await UniTask.WhenAll(enemyList.Select(async enemy =>
        //        {
        //            if (enemy != null && card.Data.Effect != null && order == 0)
        //            {
        //                if (card.RepeatEffect)
        //                {
        //                    await DelayTask(delay);
        //                    await UniTask.WhenAny(
        //                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
        //                        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //                        );
        //                }
        //                else
        //                {
        //                    await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
        //                }
        //                await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        //            }
        //            else if (enemy != null)
        //            {
        //                await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        //            }
        //        }));

        //        Debug.Log(++_endTask);
        //    }
        //}
        ////await UniTask.WhenAll(Enumerable.Range(0, enemyCount)
        ////    .Select(async j =>
        ////    {
        ////        await PoolManager.Instance.GetEffect(card.Data.Effect, EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].transform.position, Quaternion.identity);
        ////        await EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));

        ////    }));

        ////for (int i = /*0*/1; i < card.Data.Count; ++i)
        ////{
        ////    //if (i != 0)
        ////    //{
        ////    //    await DelayTask(continuousDelay);
        ////    //}
        ////    enemyList = EnemyManager.Instance.EnemyList.ToList();
        ////    card.UseTimingReset();
        ////    await UniTask.WhenAll(enemyList.Select(async enemy =>
        ////    {
        ////        if (enemy != null)
        ////        {
        ////            if (card.RepeatEffect)
        ////            {
        ////                await DelayTask(delay);
        ////                await UniTask.WhenAny(
        ////                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position, Quaternion.identity, Vector3.one))
        ////                    , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token)
        ////                    );
        ////            }
        ////            else
        ////            {
        ////                await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: cts.Token);
        ////            }
        ////            await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
        ////        }
        ////    }));
        ////    //damage = _player.CheckCritical(card.Data.Damage);
        ////    //enemyCount = EnemyManager.Instance.EnemyList.Count;
        ////    //await UniTask.WhenAll(Enumerable.Range(0, enemyCount).
        ////    //    Select(j => EnemyManager.Instance.EnemyList[(enemyCount - 1) - j].TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical))));
        ////}
        ///
        //_cts.Cancel();
        //_cts.Dispose();
        //_player.CheckCritical();
        //await CheckTaskCount(card).SuppressCancellationThrow();
        //await CheckAllEndTask();

        //Debug.Log(--_startTask);
    }
    async UniTask ShieldAB(Card card)
    {
        //Debug.Log(++_startTask);
        //await CheckTaskOrder(card, order);

        //if (card.Data.Effect != null && _isFirstOrder)
        //{
        //    _isFirstOrder = false;
        //    _cts = new CancellationTokenSource();
        //    card.UseTimingReset();
        //    await UniTask.WhenAny(
        //        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
        //        , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //        );
        //}
        //else if (card.Data.Effect != null && order == 0)
        //{
        //    await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
        //}

        //if (EnemyManager.Instance.EnemyList.Count == 0)
        //{
        //    Debug.Log(--_startTask);
        //    return;
        //}
        // 원래는 공격 타이밍 되면 바로 실행이었는데, 이벤트 순서 생기면서 첫 번째 이벤트가 아니면 공격 타이밍을 받을 필요가 없어졌음.
        //if (card.Data.Effect != null && order == 0)       // effect가 널이 아닐 경우를 확인하지만, 공격 모션이나 특수 모션이 있을 경우를 확인하는 것이고, 모션이 있다면, 해당 모션을 기다린 후 실행. 아니면 쉴드 사용(사용 시, 전용 이펙트 실행) 
        //{
        //    await UniTask.WaitUntil(() => card.CardUseTiming);
        //}
        // 쉴드가 다른 공격, 드로우에 비해 시간이 짧아서 같이 쓰려면 무조건 이펙트가 있어야 함. 안 그러면 순서가 이상해질 수 있음.
        await _player.Shield(card.Data.Shield);
        //Debug.Log(++_endTask);
        //for (int i = 1; i < card.Data.Count; ++i)
        //{
        //    if (await CheckTaskCount(card).SuppressCancellationThrow())
        //    {
        //        //i = card.Data.Count;
        //        break;
        //    }
        //    if (await CheckTaskOrder(card, order, false).SuppressCancellationThrow())
        //    {
        //        //i = card.Data.Count;
        //        break;
        //    }


        //    if (card.Data.Effect != null && _isFirstOrder)
        //    {
        //        _isFirstOrder = false;
        //        await DelayTask(delay);
        //        await UniTask.WhenAny(
        //            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position, Quaternion.identity, Vector3.one))
        //            , UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token)
        //            );
        //    }
        //    else if (card.Data.Effect != null && order == 0)
        //    {
        //        await UniTask.WaitUntil(() => card.CardUseTiming, cancellationToken: _cts.Token);
        //    }
        //    else
        //    {
        //        await DelayTask(delay);
        //    }
        //    await _player.Shield(card.Data.Shield);
        //    Debug.Log(++_endTask);
        //}
        //await CheckTaskCount(card).SuppressCancellationThrow();
        //await CheckAllEndTask();
        //Debug.Log(--_startTask);
    }
    async UniTask DrawAB(Card card)
    {
        //Debug.Log(++_startTask);
        //await CheckTaskOrder(card, order);
        ////if (EnemyManager.Instance.EnemyList.Count == 0)
        ////{
        ////    Debug.Log(--_startTask);
        ////    return;
        ////}
        //if (card.Data.Effect != null && order == 0)
        //{
        //    await UniTask.WaitUntil(() => card.CardUseTiming);
        //}
        await CardManager.Instance.DrawCard(card.Data.Draw);
        //Debug.Log(++_endTask);
        //for (int i = 1; i < card.Data.Count; ++i)
        //{
        //    if (await CheckTaskCount(card).SuppressCancellationThrow())
        //    {
        //        //i = card.Data.Count;
        //        break;
        //    }
        //    if (await CheckTaskOrder(card, order, false).SuppressCancellationThrow())
        //    {
        //        //i = card.Data.Count;
        //        break;
        //    }
        //    if (card.Data.Effect != null && order == 0)
        //    {
        //        await UniTask.WaitUntil(() => card.CardUseTiming);
        //    }
        //    else
        //    {
        //        await DelayTask(delay);
        //    }
        //    await CardManager.Instance.DrawCard(card.Data.Draw);
        //    Debug.Log(++_endTask);
        //}
        //await CheckTaskCount(card).SuppressCancellationThrow();
        //await CheckAllEndTask();
        //Debug.Log(--_startTask);
    }
    async UniTask AfterDrawAB(Card card)
    {
        //TurnManager.Instance.DrawTask().Forget();
        CardManager.Instance.SetCardState(1);
        //await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
        await DrawAB(card);
        CardManager.Instance.SetCardState(2);
    }

    //async UniTask ContinuousDrawAB(Card card)     // 드로우 같은 경우, 덱에 남아있는 카드를 확인하기 위해 Data.Count 값이 아닌 Data.Draw 값으로 얼마나 뽑을지 정함.
    //{
    //    //TurnManager.Instance.DrawTask(card.Data.Draw).Forget();
    //    await CardManager.Instance.DrawCard(card.Data.Draw);
    //    //await CardManager.Instance.DrawCards(card.Data.Draw);
    //    //if (!card.Enhanced)
    //    //    CardManager.Instance.DrawCards(card.Data.Draw).Forget();
    //    //else
    //    //    CardManager.Instance.DrawCards(card.Data.EnhancedDraw).Forget();
    //}
    //async UniTask ShieldAB(Card card)
    //{
    //    await _player.Shield(card.Data.Shield);
    //    //if (!card.Enhanced)
    //    //    _player.Shield(card.Data.Shield);
    //    //else
    //    //    _player.Shield(card.Data.EnhancedDefence);
    //}

    async UniTask ConfirmedDiscardAB()
    {
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeDiscard(true);
        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionDiscardAB()
    {
        bool discarded = false;
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeDiscard(true);
        CancellationTokenSource cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
            discarded = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        //var task3 = UniTask.Create(async () =>
        //{
        //    // 예전에 창이 바뀌는 경우 취소로 받아왔는데, 이게 의미가 있는 거였던가..?
        //    await UniTask.WaitUntil(() => !InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf, cancellationToken: cts.Token);
        //    CardManager.Instance.ReturnSelectedCard();
        //    discarded = false;
        //});
        var task4 = UniTask.Create(async () =>
        {
            // 버리는 와중에 전투가 끝나면(전투 bool이 변경되면) 초기화
            await UniTask.WaitUntil(() => !TurnManager.Instance.InBattle, cancellationToken: cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            discarded = false;
        });
        await UniTask.WhenAny(task1, task2, /*task3,*/ task4);
        cts.Cancel();
        //await UniTask.WhenAny(
        //    UniTask.Create(async () =>
        //    {
        //        await ButtonManager.Instance.DiscardButton.OnClickAsync();
        //        CardManager.Instance.ThrowAwaySelectedCard().Forget();
        //        discarded = true;


        //    }),
        //    UniTask.Create(async () =>
        //    {
        //        await ButtonManager.Instance.DiscardCancelButton.OnClickAsync();
        //        CardManager.Instance.ReturnSelectedCard();
        //        discarded = false;

        //    })
        //    );
        CardManager.Instance.ChangeDiscard(false);
        return discarded;

    }

    async UniTask ConfirmedRemoveAB()
    {
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeRemove(true);
        await UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            CardManager.Instance.ThrowAwaySelectedCard().Forget();
        });
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }
    async UniTask<bool> ConditionRemoveAB()
    {
        bool removed = false;
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeRemove(true);
        CancellationTokenSource cts = new();
        var task1 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
            //CardManager.Instance.ThrowAwaySelectedCard().Forget();        // 제거하는 코드로 변경
            removed = true;
        });
        var task2 = UniTask.Create(async () =>
        {
            await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
            CardManager.Instance.ReturnSelectedCard();
            removed = false;
        });
        await UniTask.WhenAny(task1, task2);
        cts.Cancel();
        CardManager.Instance.ChangeRemove(false);
        return removed;
    }

    void ReduceHpAB(Card card)
    {

    }

    void HealAB(Card card)
    {

    }

    void CureAB(Card card)
    {

    }

    async UniTask DelayTask(float delay = 0.3f)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    }
}


