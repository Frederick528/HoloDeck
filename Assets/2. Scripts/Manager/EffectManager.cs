using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    //public GameObject ShieldEffect;
    //public GameObject HealEffect;
    public ParticleSystem CurCardEffect { get; private set; }

    Animator _animator;
    ParticleSystem _particleSystem;
    ParticleSystem.MainModule _particleMain;

    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
    }

    public void SetCurCardEffect(ParticleSystem ps, PRS prs)
    {
        CurCardEffect = ps;
        if (CurCardEffect == null || prs == null) return;
        CurCardEffect.transform.SetPositionAndRotation(prs.pos, prs.rot);
        CurCardEffect.transform.localScale = prs.scale;
        _animator = CurCardEffect.GetComponent<Animator>();
        _particleSystem = CurCardEffect;
        _particleMain = _particleSystem.main;
    }

    public ParticleSystem GetCurCardEffect()
    {
        return CurCardEffect;
    }

    public void SlowSppedEffect(Card card)
    {
        if (CurCardEffect != null)
        {
            if (card.Data.Count > 1 && !card.RepeatEffect)
            {
                _animator.speed = 0.1f;
                _particleMain.simulationSpeed = 0.1f;
            }
        }
    }

    public void OriginSpeedEffect()
    {
        if (CurCardEffect != null)
        {
            _animator.speed = 1f;
            _particleMain.simulationSpeed = 1f;
            //particleSystem.Play(true);
        }
    }

    public bool IsAlivingEffect()
    {
        if (CurCardEffect == null) return false;
        return CurCardEffect.IsAlive();
    }

    //public async UniTask ShieldSpawnEffect(Vector3 spawnPos, IReadOnlyReactiveProperty<bool> timing, CancellationToken cts = default)
    //{
    //    if (ShieldEffect == null)
    //    {
    //        return;
    //    }
    //    Vector3 originEffectAngle = ShieldEffect.transform.eulerAngles;
    //    originEffectAngle.x -= 5;
    //    Quaternion effectAngle = Quaternion.Euler(originEffectAngle);
    //    await UniTask.WhenAny(
    //        PoolManager.Instance.GetEffect(ShieldEffect, new PRS(spawnPos + ShieldEffect.transform.position, effectAngle, ShieldEffect.transform.localScale))
    //        , UniRxExtensions.AwaitTrueAsync(timing, cts)
    //        );
    //}
    //public async UniTask HealSpawnEffect(Vector3 spawnPos, IReadOnlyReactiveProperty<bool> timing, CancellationToken cts = default)
    //{
    //    if (HealEffect == null)
    //    {
    //        return;
    //    }
    //    Vector3 originEffectAngle = HealEffect.transform.eulerAngles;
    //    originEffectAngle.x -= 5;
    //    Quaternion effectAngle = Quaternion.Euler(originEffectAngle);
    //    await UniTask.WhenAny(
    //        PoolManager.Instance.GetEffect(HealEffect, new PRS(spawnPos + HealEffect.transform.position, effectAngle, HealEffect.transform.localScale))
    //        , UniRxExtensions.AwaitTrueAsync(timing, cts)
    //        );
    //}

    public async UniTask SpawnEffect(Card card, bool isStart = true, CancellationToken cts = default)
    {
        if (card.Data.Effect == null)
        {
            //await DelayTask(0.5f);
            return;
        }
        if (!isStart)
        {
            if (!card.RepeatEffect)
            {
                // 반복 카드인데 이펙트를 반복하지 않고, 첫 스타트도 아닌 경우, => 원본 이펙트에서 공격 타이밍만 받는다는 뜻
                card.UseTimingReset();
                await UniTask.WhenAny(
                    UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts),
                    UniTask.WaitForSeconds(3f, cancellationToken: cts)
                );
                card.UseTimingReset();
                return;
            }
            else
            {
                await UniTask.WaitForSeconds(CardUtils.NextCardUseDelay * 0.5f, cancellationToken: cts);
            }
        }
        Vector3 originEffectAngle = card.Data.Effect.transform.eulerAngles;
        originEffectAngle.x -= 5;
        Quaternion effectAngle = Quaternion.Euler(originEffectAngle);
        switch (card.Data.CardTag)
        {
            // 1. 적 하나를 타겟팅하는 모든 경우 (공격, 스킬, 랜덤 선택된 타겟 포함)
            case CardTag.SingleAttack:
            case CardTag.SkillTargetSingle:
            case CardTag.RandomAttack:
            case CardTag.SkillTargetRandom:
                // card.TargetEnemy가 이미 로직상에서 결정되어 있다고 가정합니다.
                if (card.TargetEnemy != null)
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale)),
                        UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                    );
                }
                break;

            // 2. 적 전체를 타겟팅하는 모든 경우 (공격, 스킬)
            case CardTag.AllAttack:
            case CardTag.SkillTargetAll:
                if (card.AllEnemies) // 화면 중앙에서 거대한 이펙트 하나 발생
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale * 2.5f)),
                        UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                    );
                }
                else // 모든 적 머리 위에 각각 이펙트 발생
                {
                    await UniTask.WhenAll(EnemyManager.Instance.EnemyList.Select(async enemy =>
                    {
                        if (enemy != null)
                        {
                            await UniTask.WhenAny(
                                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale)),
                                UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                            );
                        }
                    }));
                }
                break;

            // 3. 플레이어 자신에게 스킬 발동
            case CardTag.SkillTargetSelf:
                await UniTask.WhenAny(
                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(InGameManager.Instance.Player.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale)),
                    UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                );
                break;
        }
        card.UseTimingReset();
    }
}
