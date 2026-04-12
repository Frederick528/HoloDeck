using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UniRx;

public enum AbilityTag
{
    None,
    PrePreparation, // 0: 사전 준비 (데이터 설정, 버프 등)
    ChangeHP,       // 1: 체력 변동
    Attack,         // 2: 공격 (메인 액션)
    Shield,         // 3: 방어
    Draw,           // 4: 드로우
    PostEffect      // 5: 모든 효과 완료 후 (상태이상 부여, 후속 연출 등)
}

public partial class CardAbility
{
    CancellationTokenSource _cts;

    Dictionary<MasterTag, Action<Card, List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)>, MasterTagData>> _specialAbilityMap;
    //(Action _cardImmediately, Action _cardFailure)? _checkCard;  // 즉시 실행하는 코드 + 해당 카드가 실패했을 때 효과
    //Func<UniTask> _cardTask;
    ////Func<UniTask<bool>?> _conditionTask;
    //List<Func<UniTask<bool>>> _conditionTasks = new();
    private Player _player;

    public CardAbility()
    {
        // 생성자에서 특수 능력들을 등록합니다 (아래 2번 파일에서 정의)
        InitSpecialAbilities();
    }
    public void SetCardAbility(Card card)
    {
        if (!_player)
        {
            _player = InGameManager.Instance.Player;
            if (_player == null)
                return;
        }

        card.AbilityRebuild();

        Action immediateActions = null;
        Action failureActions = null;
        Action successActions = null;
        // --- 1. 이번 조립에만 쓸 '지역 변수' 바구니들 ---
        var abilityTasks = new List<(float order, AbilityTag tag, Func<PlayContext, UniTask> task)>();
        var conditionTasks = new List<Func<UniTask<bool>>>();
        (Action, Action, Action)? finalCheckCard = null;
        Func<PlayContext, UniTask> finalCardTask = null;

        // --- 2. 조립 시작 (바구니를 인자로 넘겨줌) ---
        SettingImmediately(card, ref immediateActions, ref failureActions, ref successActions); // Specials에서 처리
        BuildBaseAbilities(card, abilityTasks);  // Simple에서 처리
        BuildSpecialAbilities(card, abilityTasks); // Specials에서 처리

        // 조건도 바구니에 담기
        if (card.Data.HasCondition)
        {
            SettingCondition(card, conditionTasks, ref immediateActions, ref failureActions, ref successActions);
            SettingSpecialCondition(card, conditionTasks, ref immediateActions, ref failureActions, ref successActions);
        }
        // --- 3. 최종 포장 및 전달 ---
        if (immediateActions != null || failureActions != null || successActions != null)
        {
            finalCheckCard = (immediateActions, failureActions, successActions);
        }

        foreach (var tagData in card.AddedAbilities)
        {
            if (_specialAbilityMap.TryGetValue(tagData.Tag, out var addAction))
            {
                // 기존 맵을 그대로 활용해서 태그에 맞는 로직을 tasks에 추가합니다.
                addAction(card, abilityTasks, tagData);
            }
        }

        if (abilityTasks.Count > 0)
        {
            // 지역 변수를 사용하여 래핑
            finalCardTask = (ctx) => AddCardEvent(card, abilityTasks.ToArray(), ctx);
        }

        // 카드 객체에 직접 할당 (여기서 끝!)
        card.SetCardImmediately(finalCheckCard);
        card.SetCardTask(finalCardTask);
        card.SetUseConditions(conditionTasks);
        //ResetAllState();

        //SettingImmediately(card);       // 밑에 조건에서도 설정하지만, 조건이 아닌데 카드 사용을 멈추는 경우도 존재. 이때는 여기서 설정해줘야 함.

        //if (card.Data.HasCondition)
        //{
        //    SettingCondition(card);
        //    SettingSpecialCondition(card);
        //}

        ////else
        ////{
        ////    //_conditionTask = null;
        ////    _conditionTasks.Clear();
        ////}
        //SettingSimpleAB(card);
        //SettingSpecialAB(card);
        ////if (card.Data.IsSimpleAB)
        ////{
        ////    SettingSimpleAB(card);
        ////}
        ////else
        ////{
        ////    SettingSpecialAB(card);
        ////}
        //card.SetCardImmediately(_checkCard);
        //card.SetUseConditions(_conditionTasks);
        //card.SetCardTask(_cardTask);
        ////card.SetUseConditions(_conditionTask);
    }
    //T GetEnum<T>(string input) where T : struct, Enum
    //{
    //    if (Enum.TryParse<T>(input, true, out T result)) return result;

    //    Debug.LogWarning($"[CardAbility] {typeof(T).Name} 파싱 실패: {input}. 기본값으로 설정합니다.");
    //    return default;
    //}

    void ForEachEnemyTarget(Card card, Action<Enemy> action)
    {
        // 1. 전체 대상 카드인 경우
        if (card.Data.CardTag == CardTag.AllAttack || card.Data.CardTag == CardTag.SkillTargetAll)
        {
            foreach (var enemy in EnemyManager.Instance.EnemyList)
            {
                action(enemy);
            }
        }
        // 2. 그 외 (단일, 랜덤 등 - 이미 card.TargetEnemy가 선정된 상태)
        else
        {
            if (card.TargetEnemy != null)
            {
                action(card.TargetEnemy);
            }
        }
    }

    //private void ResetAllState()
    //{
    //    _conditionTasks.Clear();
    //    _cardTask = null;
    //    _checkCard = null;
    //}

    //void SettingImmediately(Card card)
    //{
    //    switch (card.Data.ID)
    //    {
    //        case 105:
    //            _checkCard = (_cardImmediately: () =>
    //            {
    //                CardManager.Instance.SetCardState(1);
    //            }, _cardFailure: () =>
    //            {
    //                CardManager.Instance.SetCardState(2);
    //            });
    //            break;
    //        default:
    //            _checkCard = null;
    //            break;
    //    }
    //}

    //void SettingCondition(Card card)
    //{
    //    _conditionTasks.Clear();
    //    //Func<UniTask<bool>?> uniTaskCondition = null;
    //    if (card.Data.Hp < 0)
    //    {
    //        _conditionTasks.Add(() => ConditionHpCheck(card));
    //    }

    //    if (card.Data.Discard > 0)
    //    {
    //        _checkCard = (_cardImmediately: () =>
    //        {
    //            CardManager.Instance.SetCardState(1);
    //        }, _cardFailure: () =>
    //        {
    //            CardManager.Instance.SetCardState(2);
    //        });
    //        _conditionTasks.Add(() => ConditionDiscardAB(card));
    //    }

    //    if (card.Data.Remove > 0)
    //    {
    //        _checkCard = (_cardImmediately: () =>
    //        {
    //            CardManager.Instance.SetCardState(1);
    //        }, _cardFailure: () =>
    //        {
    //            CardManager.Instance.SetCardState(2);
    //        });
    //        _conditionTasks.Add(() => ConditionRemoveAB(card));
    //    }
    //    //card.SetUseConditions(uniTaskCondition);
    //}

    //void SettingSimpleAB(Card card)
    //{
    //    var tasks = new List<(int order, Func<UniTask> effectTask)>();

    //    switch (card.Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            tasks.Add((0, () => SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _))));
    //            break;
    //        case CardTag.AllAttack:
    //            tasks.Add((0, () => AllAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _))));
    //            break;
    //    }

    //    if (card.Data.Hp != 0)
    //    {
    //        // 사용자님이 말씀하신 대로 음수면 0번 순서(공격과 동급), 양수면 2번 순서(유틸리티)
    //        int hpOrder = card.Data.Hp < 0 ? 0 : 2;
    //        tasks.Add((hpOrder, () => HpEffectAB(card)));
    //    }

    //    if (card.Data.Shield > 0)
    //    {
    //        tasks.Add((1, () => ShieldAB(card)));
    //    }

    //    // 5. 드로우 처리 (Order 2: 가장 마지막)
    //    if (card.Data.Draw > 0)
    //    {
    //        tasks.Add((2, () => DrawAB(card)));
    //    }

    //    if (tasks.Count > 0)
    //    {
    //        // params 키워드 덕분에 ToArray()로 넘겨주면 됩니다.
    //        _cardTask = () => AddCardEvent(card, tasks.ToArray());
    //    }
    //    else
    //    {
    //        _cardTask = null;
    //    }
    //    //Func<UniTask> uniTaskAB = null;
    //    //bool critical = _player.GetStatusEffect(StatusEffect.UseCritical, out _);
    //    //switch (card.Data.CardTag)
    //    //{
    //    //    case CardTag.SingleAttack:
    //    //        if (card.Data.Shield > 0 || card.Data.Draw > 0)
    //    //            _cardTask = () =>
    //    //                AddCardEvent(card, delay,
    //    //                    (0, () => SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _))),
    //    //                    (1, () => ShieldAB(card)),
    //    //                    (1, () => DrawAB(card))
    //    //                );
    //    //        else
    //    //            _cardTask = () => 
    //    //                AddCardEvent(card, delay,
    //    //                    (0, () => SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _)))
    //    //                );
    //    //        break;
    //    //    case CardTag.AllAttack:
    //    //        if (card.Data.Shield > 0 || card.Data.Draw > 0)
    //    //            _cardTask = () =>
    //    //                AddCardEvent(card, delay,
    //    //                     (0, () => AllAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _))),
    //    //                     (1, () => ShieldAB(card)),
    //    //                     (1, () => DrawAB(card))
    //    //                 );
    //    //        else
    //    //            _cardTask = () =>
    //    //                AddCardEvent(card, delay,
    //    //                     (0, () => AllAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _)))
    //    //                 );
    //    //        break;
    //    //    case CardTag.SkillTargetSelf:
    //    //        _cardTask = () =>
    //    //            AddCardEvent(card, delay,
    //    //                     (0, () => ShieldAB(card)),
    //    //                     (0, () => DrawAB(card))
    //    //                 );
    //    //        break;
    //    //    default:
    //    //        _cardTask = null;
    //    //        break;
    //    //}
    //    //card.SetCardTask(uniTaskAB);

    //}

    //void SettingCardAB(Card card)
    //{
    //    switch (card.Data.ID)
    //    {
    //        case 105:
    //            _cardTask = () =>
    //                //CardManager.Instance.SetCardState(1);
    //                AddCardEvent(card,
    //                    (0, () => DrawAB(card)),
    //                    (1, () => ConfirmedDiscardAB(card))
    //                );
    //            break;
    //        case 108:
    //            _cardTask = () =>
    //                AddCardEvent(card,
    //                    (0, async() =>
    //                        {
    //                            int statusEffectCount = (card.TargetEnemy.CurStatusEffectList.Count() + card.TargetEnemy.CurStatusEffectPerpetualList.Count());
    //                            card.Data.Damage += statusEffectCount;
    //                            await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
    //                            card.Data.Damage -= statusEffectCount;
    //                        }
    //                    )
    //                );
    //            break;
    //        case 503:
    //            _cardTask = () =>
    //                AddCardEvent(card,
    //                    (0, () => {
    //                        _player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
    //                        return UniTask.CompletedTask;
    //                        })
    //                );
    //                //await DelayTask(0.5f);
    //                //await SpawnEffect(card);
    //                //_player.AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.InfiniteDuration), card.Data.Cost);
    //            break;
    //        case 801:
    //            _cardTask = () =>
    //                AddCardEvent(card,
    //                    (0, async () =>
    //                        {
    //                            card.Data.Count = card.Data.Cost;
    //                            await SingleAttackAB(card, _player.GetStatusEffect(StatusEffect.UseCritical, out _));
    //                        }
    //                    )
    //                );
    //            break;
    //        default:
    //            _cardTask = () => UniTask.CompletedTask;
    //            break;
    //    }
    //}

    //async UniTask PlayCardEvent(Card card, SortedDictionary<int, List<Func<UniTask>>> cardEvent)
    //{
    //    _cts = new CancellationTokenSource();

    //    //float originalDelay = delay;
    //    for (int i = 0; i < card.Data.Count; i++) // 카드 횟수만큼 반복
    //    {
    //        //if (i != 0)
    //        //{
    //        //    isStart = false;
    //        //    delay = originalDelay * 0.75f;
    //        //}
    //        //if (isStart || card.RepeatEffect)
    //        //{
    //        //    await DelayTask(delay);
    //        //}

    //        //await DelayTask(delay);
    //        await EffectManager.Instance.SpawnEffect(card, i == 0, _cts.Token);

    //        EffectManager.Instance.SlowSppedEffect(card);
    //        //if (EffectManager.Instance.GetCurCardEffect() != null)
    //        //{
    //        //    //if (isStart)
    //        //    //{
    //        //    //    animator = EffectManager.Instance.GetCurCardEffect().GetComponent<Animator>();
    //        //    //    particleSystem = EffectManager.Instance.GetCurCardEffect();
    //        //    //    particleMain = particleSystem.main;

    //        //    //}
    //        //    if (card.Data.Count > 1 && !card.RepeatEffect)
    //        //    {
    //        //        animator.speed = 0.1f;
    //        //        particleMain.simulationSpeed = 0.1f;
    //        //    }
    //        //}
    //        foreach (var eventTask in cardEvent) // 0, 1, 2... 순서대로 실행
    //        {
    //            var tasksToRun = eventTask.Value.Select(func => func()).ToList();
    //            await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow(); // 동시 실행 및 대기
    //        }

    //        EffectManager.Instance.OriginSpeedEffect();
    //        //if (EffectManager.Instance.GetCurCardEffect() != null)
    //        //{
    //        //    animator.speed = 1f;
    //        //    particleMain.simulationSpeed = 1f;
    //        //    //particleSystem.Play(true);
    //        //}

    //        if (_cts.IsCancellationRequested)
    //        {
    //            break;
    //        }
    //    }

    //    switch (card.Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            card.Target(null);
    //            _player.CheckCritical();
    //            break;
    //        case CardTag.AllAttack:
    //            _player.CheckCritical();
    //            break;
    //        //case CardTag.SkillTargetSelf:
    //        default:
    //            break;
    //    }
    //    if (!_cts.IsCancellationRequested)
    //    {
    //        _cts.Cancel();
    //        _cts.Dispose();
    //    }

    //    await UniTask.WaitForSeconds(CardUtils.NextCardUseDelay);
    //    //await UniTask.Create(async () =>
    //    //{
    //    //    _cts = new CancellationTokenSource();

    //    //    Animator animator = null;
    //    //    ParticleSystem particleSystem = null;
    //    //    ParticleSystem.MainModule particleMain;

    //    //    bool isStart = true;
    //    //    float originalDelay = delay;
    //    //    for (int i = 0; i < card.Data.Count; i++) // 카드 횟수만큼 반복
    //    //    {
    //    //        if (i != 0)
    //    //        {
    //    //            isStart = false;
    //    //            delay = originalDelay * 0.75f;
    //    //        }
    //    //        //if (isStart || card.RepeatEffect)
    //    //        //{
    //    //        //    await DelayTask(delay);
    //    //        //}
    //    //        await DelayTask(delay);
    //    //        await EffectManager.Instance.SpawnEffect(card, isStart, _cts.Token);

    //    //        if (EffectManager.Instance.GetCurCardEffect() != null)
    //    //        {
    //    //            if (isStart)
    //    //            {
    //    //                animator = EffectManager.Instance.GetCurCardEffect().GetComponent<Animator>();
    //    //                particleSystem = EffectManager.Instance.GetCurCardEffect();
    //    //                particleMain = particleSystem.main;

    //    //            }
    //    //            if (card.Data.Count > 1 &&!card.RepeatEffect)
    //    //            {
    //    //                animator.speed = 0.1f;
    //    //                particleMain.simulationSpeed = 0.1f;
    //    //            }
    //    //        }
    //    //        foreach (var eventTask in cardEvent) // 0, 1, 2... 순서대로 실행
    //    //        {
    //    //            var tasksToRun = eventTask.Value.Select(func => func()).ToList();
    //    //            await UniTask.WhenAll(tasksToRun).SuppressCancellationThrow(); // 동시 실행 및 대기
    //    //        }

    //    //        if (EffectManager.Instance.GetCurCardEffect() != null)
    //    //        {
    //    //            animator.speed = 1f;
    //    //            particleMain.simulationSpeed = 1f;
    //    //            //particleSystem.Play(true);
    //    //        }

    //    //        if (_cts.IsCancellationRequested)
    //    //        {
    //    //            break;
    //    //        }
    //    //    }

    //    //    switch (card.Data.CardTag)
    //    //    {
    //    //        case CardTag.SingleAttack:
    //    //            card.Target(null);
    //    //            _player.CheckCritical();
    //    //            break;
    //    //        case CardTag.AllAttack:
    //    //            _player.CheckCritical();
    //    //            break;
    //    //        //case CardTag.SkillTargetSelf:
    //    //        default:
    //    //            break;
    //    //    }
    //    //    if (!_cts.IsCancellationRequested)
    //    //    {
    //    //        _cts.Cancel();
    //    //        _cts.Dispose();
    //    //    }
    //    //});
    //}

    //async UniTask SpawnEffect(Card card, bool isStart = true)
    //{
    //    if (card.Data.Effect == null)
    //    {
    //        //await DelayTask(0.5f);
    //        return;
    //    }
    //    if (!isStart && !card.RepeatEffect)
    //    {
    //        // 반복 카드인데 이펙트를 반복하지 않고, 첫 스타트도 아닌 경우, => 원본 이펙트에서 공격 타이밍만 받는다는 뜻
    //        card.UseTimingReset();
    //        await UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, _cts.Token);
    //        card.UseTimingReset();
    //        return;
    //    }
    //    Vector3 originEffectAngle = card.Data.Effect.transform.eulerAngles;
    //    originEffectAngle.x -= 5;
    //    Quaternion effectAngle = Quaternion.Euler(originEffectAngle);
    //    switch (card.Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            await UniTask.WhenAny(
    //                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
    //                , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, _cts.Token)
    //                //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
    //                );
    //            break;
    //        case CardTag.AllAttack:
    //            if (card.AllEnemies)
    //            {
    //                await UniTask.WhenAny(
    //                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale * 2.5f))
    //                    , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, _cts.Token)
    //                    //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
    //                    );
    //            }
    //            else
    //            {
    //                await UniTask.WhenAll(EnemyManager.Instance.EnemyList.Select(async enemy =>
    //                {
    //                    if (enemy != null)
    //                    {
    //                        await UniTask.WhenAny(
    //                            PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
    //                            , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, _cts.Token)
    //                            //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
    //                            );
    //                        //await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
    //                    }
    //                }));
    //            }
    //            break;  
    //        case CardTag.SkillTargetSelf:
    //            await UniTask.WhenAny(
    //                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(InGameManager.Instance.Player.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
    //                    , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, _cts.Token)
    //                    //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
    //                    );
    //            break;
    //    }
    //}

    //async UniTask AddCardEvent(Card card, params (int, Func<UniTask>)[] taskOrder)
    //{
    //    var cardEventDict = new SortedDictionary<int, List<Func<UniTask>>>();
    //    foreach (var (order, effectTask) in taskOrder)
    //    {
    //        if (!cardEventDict.ContainsKey(order))
    //        {
    //            cardEventDict[order] = new List<Func<UniTask>>();
    //        }
    //        cardEventDict[order].Add(effectTask);
    //    }
    //    await PlayCardEvent(card, cardEventDict);
    //}

    //async UniTask SingleAttackAB(Card card, bool critical)             // 컨티뉴 single이랑 그냥 single 합침. -> 2025년 10월 말에 코드 다 바꾸면서 그냥 단일 타켓 코드로만 작동.
    //{

    //    bool killEnemy = await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
    //    if (killEnemy)
    //    {
    //        //Debug.Log(_cts);
    //        _cts.Cancel();
    //        _cts.Dispose();
    //    }
    //}
    //async UniTask AllAttackAB(Card card, bool critical)              // 컨티뉴 multi랑 그냥 multi 합침. -> 2025년 10월 말에 코드 다 바꾸면서 그냥 멀티 타켓 코드로만 작동.
    //{


    //    var enemyList = EnemyManager.Instance.EnemyList.ToList();

    //    await UniTask.WhenAll(enemyList.Select(async enemy =>
    //    {
    //        if (enemy != null)
    //        {
    //            await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical), _player);
    //        }
    //    }));
    //    if (EnemyManager.Instance.NoEnemy) // 여긴 InBattle로 체크 안 함. InBattle은 적 죽는 모션 끝나는 것까지 기다려야 함. (+클리어 판정이 아닌, 현재 필드에 남아있는 적이 없다는 뜻)
    //    {
    //        _cts.Cancel();
    //        _cts.Dispose();
    //    }

    //}
    //async UniTask ShieldAB(Card card)
    //{

    //    // 쉴드가 다른 공격, 드로우에 비해 시간이 짧아서 같이 쓰려면 무조건 이펙트가 있어야 함. 안 그러면 순서가 이상해질 수 있음.
    //    // -> 오더 순서 설정 이후, 괜찮아졌으나 그래도 같은 오더로 설정하면 이펙트가 있는 게 맞음. 확실하게 할 거면 1, 2, 3 순서대로 진행하게 변경해야함.
    //    await _player.Shield(card.Data.Shield);

    //}
    //async UniTask DrawAB(Card card)
    //{

    //    await CardManager.Instance.DrawCard(card.Data.Draw);

    //}
    //async UniTask AfterDrawAB(Card card)
    //{
    //    //TurnManager.Instance.DrawTask().Forget();
    //    CardManager.Instance.SetCardState(1);
    //    //await CardManager.Instance.DrawCard();       // 최하위 UniTask에서 Cancel를 확인하는데... 혹시 문제가 발생할 수도 있나..?
    //    await DrawAB(card);
    //    CardManager.Instance.SetCardState(2);
    //}

    //async UniTask<bool> ConditionHpCheck(Card card)
    //{
    //    // 1. 현재 플레이어 체력 확인 (InGameManager 등 참조)
    //    int currentHp = InGameManager.Instance.Player.CurHP.Value;

    //    if (currentHp > -card.Data.Hp)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        Debug.Log("HP 부족으로 사용 실패");
    //        return false;
    //    }
    //}

    //private async UniTask HpEffectAB(Card card)
    //{
    //    int val = card.Data.Hp;
    //    if (val < 0)
    //    {
    //        // 음수라면 데미지 로직 (-를 붙여 양수로 변환)
    //        await _player.TakeDamage(-val);
    //    }
    //    else
    //    {
    //        // 양수라면 회복 로직
    //        await _player.Heal(val);
    //    }
    //}

    //async UniTask ConfirmedDiscardAB(Card card)
    //{
    //    card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);

    //    InGameButtonManager.Instance.DiscardBtnInvert(false);
    //    InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
    //    CardManager.Instance.ChangeDiscard(true);
    //    OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.
    //    await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
    //    CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //    //await UniTask.Create(async () =>
    //    //{
    //    //    await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
    //    //    CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //    //});
    //    InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
    //    CardManager.Instance.ChangeDiscard(false);
    //}
    //async UniTask<bool> ConditionDiscardAB(Card card)
    //{
    //    card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
    //    //bool discarded = false;
    //    InGameButtonManager.Instance.DiscardBtnInvert(false);
    //    CardManager.Instance.ChangeDiscard(true);
    //    CancellationTokenSource cts = new();
    //    // 1. 네 가지 조건 중 가장 먼저 일어나는 녀석의 '인덱스'를 가져옵니다.
    //    int winnerIndex = await UniTask.WhenAny(
    //        InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token),      // 0번
    //        InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token),// 1번
    //        UniTask.WaitUntil(() => !InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf, cancellationToken: cts.Token), // 2번
    //        TurnManager.Instance.InBattle.Where(x => !x).ToUniTask(cancellationToken: cts.Token) // 3번
    //    );

    //    // 2. 경주가 끝났으니 나머지 대기 작업들은 모두 취소합니다.
    //    cts.Cancel();
    //    cts.Dispose();

    //    // 3. 누가 이겼는지(어떤 이벤트가 발생했는지)에 따라 결과 처리
    //    bool isDiscarded = false;

    //    switch (winnerIndex)
    //    {
    //        case 0: // 버리기 성공
    //            CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //            isDiscarded = true;
    //            break;
    //        case 1: // 취소 버튼
    //        case 2: // UI 닫힘
    //        case 3: // 전투 종료
    //            CardManager.Instance.ReturnSelectedCard();
    //            isDiscarded = false;
    //            break;
    //    }

    //    CardManager.Instance.ChangeDiscard(false);
    //    return isDiscarded;
    //    //var task1 = UniTask.Create(async () =>
    //    //{
    //    //    await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
    //    //    CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //    //    discarded = true;
    //    //});
    //    //var task2 = UniTask.Create(async () =>
    //    //{
    //    //    await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
    //    //    CardManager.Instance.ReturnSelectedCard();
    //    //    discarded = false;
    //    //});
    //    //var task3 = UniTask.Create(async () =>
    //    //{
    //    //    await UniTask.WaitUntil(() => !InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf, cancellationToken: cts.Token);
    //    //    CardManager.Instance.ReturnSelectedCard();
    //    //    discarded = false;
    //    //});
    //    //var task4 = UniTask.Create(async () =>
    //    //{
    //    //    // 버리는 와중에 전투가 끝나면(전투 bool이 변경되면) 초기화
    //    //    //await UniTask.WaitUntil(() => !TurnManager.Instance.InBattle, cancellationToken: cts.Token);
    //    //    await TurnManager.Instance.InBattle.Where(inBattle => !inBattle).ToUniTask(cancellationToken: cts.Token);
    //    //    CardManager.Instance.ReturnSelectedCard();
    //    //    discarded = false;
    //    //});
    //    //await UniTask.WhenAny(task1, task2, task3, task4);
    //    //cts.Cancel();

    //    //CardManager.Instance.ChangeDiscard(false);
    //    //return discarded;

    //}

    //async UniTask ConfirmedRemoveAB(Card card)
    //{
    //    card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
    //    InGameButtonManager.Instance.DiscardBtnInvert(false);
    //    InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
    //    CardManager.Instance.ChangeRemove(true);
    //    await UniTask.Create(async () =>
    //    {
    //        await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
    //        CardManager.Instance.ThrowAwaySelectedCard().Forget();
    //    });
    //    InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
    //    CardManager.Instance.ChangeDiscard(false);
    //}
    //async UniTask<bool> ConditionRemoveAB(Card card)
    //{
    //    card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
    //    //bool removed = false;
    //    InGameButtonManager.Instance.DiscardBtnInvert(false);
    //    CardManager.Instance.ChangeRemove(true);
    //    CancellationTokenSource cts = new();
    //    int winnerIndex = await UniTask.WhenAny(
    //        InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token),
    //        InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token)
    //     );

    //    // 3. 한 쪽이 결정됐으니 나머지 대기 취소 및 정리
    //    cts.Cancel();
    //    cts.Dispose();

    //    bool isRemoved = false;

    //    // 4. 결과에 따른 후속 처리
    //    if (winnerIndex == 0) // 제거 버튼 클릭 시
    //    {
    //        // 여기서 실제로 카드를 제거하는 로직을 호출하세요. (예: CardManager.Instance.RemoveSelectedCard().Forget();)
    //        isRemoved = true;
    //    }
    //    else // 취소 버튼 클릭 시
    //    {
    //        CardManager.Instance.ReturnSelectedCard();
    //        isRemoved = false;
    //    }

    //    // 5. UI 상태 원복
    //    CardManager.Instance.ChangeRemove(false);

    //    return isRemoved;
    //    //var task1 = UniTask.Create(async () =>
    //    //{
    //    //    await InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token);
    //    //    //CardManager.Instance.ThrowAwaySelectedCard().Forget();        // 제거하는 코드로 변경
    //    //    removed = true;
    //    //});
    //    //var task2 = UniTask.Create(async () =>
    //    //{
    //    //    await InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token);
    //    //    CardManager.Instance.ReturnSelectedCard();
    //    //    removed = false;
    //    //});
    //    //await UniTask.WhenAny(task1, task2);
    //    //cts.Cancel();
    //    //CardManager.Instance.ChangeRemove(false);
    //    //return removed;
    //}

    //void ReduceHpAB(Card card)
    //{

    //}

    //void HealAB(Card card)
    //{

    //}

    //void CureAB(Card card)
    //{

    //}

    //async UniTask DelayTask(float delay = 0.3f)
    //{
    //    await UniTask.WaitForSeconds(delay, cancellationToken: TurnManager.Instance.CancelSource.Token);
    //}
}


