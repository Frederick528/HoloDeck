using Cysharp.Threading.Tasks;
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
    }

    public ParticleSystem GetCurCardEffect()
    {
        return CurCardEffect;
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
        if (!isStart && !card.RepeatEffect)
        {
            // 반복 카드인데 이펙트를 반복하지 않고, 첫 스타트도 아닌 경우, => 원본 이펙트에서 공격 타이밍만 받는다는 뜻
            card.UseTimingReset();
            await UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts);
            card.UseTimingReset();
            return;
        }
        Vector3 originEffectAngle = card.Data.Effect.transform.eulerAngles;
        originEffectAngle.x -= 5;
        Quaternion effectAngle = Quaternion.Euler(originEffectAngle);
        switch (card.Data.CardTag)
        {
            case CardTag.SingleAttack:
                await UniTask.WhenAny(
                    PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(card.TargetEnemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
                    , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                    //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
                    );
                break;
            case CardTag.MultiAttack:
                if (card.AllEnemies)
                {
                    await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(EnemyManager.Instance.EnemyCenterSpawnPos + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale * 2.5f))
                        , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                        //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
                        );
                }
                else
                {
                    await UniTask.WhenAll(EnemyManager.Instance.EnemyList.Select(async enemy =>
                    {
                        if (enemy != null)
                        {
                            await UniTask.WhenAny(
                                PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(enemy.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
                                , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                                //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
                                );
                            //await enemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, critical));
                        }
                    }));
                }
                break;
            case CardTag.SkillTargetMe:
                await UniTask.WhenAny(
                        PoolManager.Instance.GetEffect(card.Data.Effect, new PRS(InGameManager.Instance.Player.transform.position + card.Data.Effect.transform.position, effectAngle, card.Data.Effect.transform.localScale))
                        , UniRxExtensions.AwaitTrueAsync(card.IsCardUseTiming, cts)
                        //, card.IsCardUseTiming.Where(timing => timing).ToUniTask(cancellationToken: _cts.Token)
                        );
                break;
        }
    }
}
