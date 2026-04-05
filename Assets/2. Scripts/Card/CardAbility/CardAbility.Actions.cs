using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;

// [Actions] 모든 카드가 공통으로 사용하는 '원자적 기능'들
public partial class CardAbility
{
    // --- 1. 공격 관련 ---
    async UniTask SingleAttackAB(Card card, bool crit)
    {
        bool kill = await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player);
        if (kill)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    async UniTask AllAttackAB(Card card, bool crit)
    {
        var enemies = EnemyManager.Instance.EnemyList.ToList();
        await UniTask.WhenAll(enemies.Select(e => e != null
            ? e.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player)
            : UniTask.FromResult(false)));

        if (EnemyManager.Instance.NoEnemy)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    async UniTask RandomAttackAB(Card card, bool crit)
    {
        // 1. 현재 살아있는(null이 아닌) 적 리스트를 가져옵니다.
        var enemies = EnemyManager.Instance.EnemyList.ToList();

        // 2. 공격할 대상이 없으면 종료합니다.
        if (enemies.Count == 0) return;

        // 3. 리스트 중 무작위로 하나를 선택합니다.
        int randomIndex = Random.Range(0, enemies.Count);
        var target = enemies[randomIndex];

        // 4. 데미지 처리를 진행합니다. (기존 SingleAttackAB 로직 활용)
        bool kill = await target.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player);

        // 5. 적이 죽었거나 모든 적이 사라졌을 경우 전투 종료(Cts 취소) 처리를 확인합니다.
        if (kill)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    // --- 2. 방어 및 수치 관련 ---
    async UniTask ShieldAB(Card card)
    {
        await _player.Shield(card.Data.Shield);
    }

    async UniTask HpEffectAB(Card card)
    {
        if (card.Data.HP < 0) await _player.TakeDamage(-card.Data.HP);
        else await _player.Heal(card.Data.HP);
    }

    // --- 3. 카드 시스템 관련 ---
    async UniTask DrawAB(Card card)
    {
        await CardManager.Instance.DrawCard(card.Data.Draw);
    }
    async UniTask ConfirmedDiscardAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);
        CardManager.Instance.ChangeDiscard(true);

        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.

        await InGameButtonManager.Instance.DiscardButton.OnClickAsync(TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
        CardManager.Instance.ThrowAwaySelectedCard().Forget();

        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);
        CardManager.Instance.ChangeDiscard(false);
    }

    async UniTask ConfirmedRemoveAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        InGameButtonManager.Instance.DiscardBtnInvert(false);               // Remove로 변경해야함.
        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(false);      // Remove로 변경해야함.
        CardManager.Instance.ChangeRemove(true);

        OutGameUIManager.Instance.RemoveOpenUIOrder(InGameUIManager.CanvasName.SelectedCard.ToString());        // 강제 조건확인이라 뒤로가기를 미리 막음.

        await InGameButtonManager.Instance.DiscardButton.OnClickAsync(TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();     // Remove로 변경해야함.
        CardManager.Instance.ThrowAwaySelectedCard().Forget();

        InGameButtonManager.Instance.SetActiveDiscardCancelBtn(true);       // Remove로 변경해야함.
        CardManager.Instance.ChangeRemove(false);
    }

    // --- 4. 조건 검사 관련 ---
    private async UniTask<bool> ConditionHpCheck(Card card)
    {
        if (_player.CurHP.Value > -card.Data.HP)
        {
            await _player.TakeDamage(-card.Data.HP);
            return true;
        }
        return false;
        
    }
    private async UniTask<bool> ConditionHpCheckXValue(Card card)
    {
        if (_player.CurHP.Value > card.Data.Cost)
        {
            await _player.TakeDamage(card.Data.Cost);
            return true;
        }
        return false;

    }

    async UniTask<bool> ConditionDiscardAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        InGameButtonManager.Instance.DiscardBtnInvert(false);
        CardManager.Instance.ChangeDiscard(true);
        CancellationTokenSource cts = new();
        // 1. 네 가지 조건 중 가장 먼저 일어나는 녀석의 '인덱스'를 가져옵니다.
        int winnerIndex = await UniTask.WhenAny(
            InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token),      // 0번
            InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token),// 1번
            UniTask.WaitUntil(() => !InGameUIManager.Instance.Canvas(InGameUIManager.CanvasName.SelectedCard).gameObject.activeSelf, cancellationToken: cts.Token), // 2번
            TurnManager.Instance.InBattle.Where(x => !x).ToUniTask(cancellationToken: cts.Token) // 3번
        );

        // 2. 경주가 끝났으니 나머지 대기 작업들은 모두 취소합니다.
        cts.Cancel();
        cts.Dispose();

        // 3. 누가 이겼는지(어떤 이벤트가 발생했는지)에 따라 결과 처리
        bool isDiscarded = false;

        switch (winnerIndex)
        {
            case 0: // 버리기 성공
                CardManager.Instance.ThrowAwaySelectedCard().Forget();
                isDiscarded = true;
                break;
            case 1: // 취소 버튼
            case 2: // UI 닫힘
            case 3: // 전투 종료
                CardManager.Instance.ReturnSelectedCard();
                isDiscarded = false;
                break;
        }

        CardManager.Instance.ChangeDiscard(false);
        return isDiscarded;
    }
    async UniTask<bool> ConditionRemoveAB(Card card)
    {
        card.MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        //bool removed = false;
        InGameButtonManager.Instance.DiscardBtnInvert(false);        // Remove로 변경해야함.
        CardManager.Instance.ChangeRemove(true);
        CancellationTokenSource cts = new();
        int winnerIndex = await UniTask.WhenAny(
            InGameButtonManager.Instance.DiscardButton.OnClickAsync(cts.Token),          // Remove로 변경해야함.
            InGameButtonManager.Instance.DiscardCancelButton.OnClickAsync(cts.Token)     // Remove로 변경해야함.
         );

        // 3. 한 쪽이 결정됐으니 나머지 대기 취소 및 정리
        cts.Cancel();
        cts.Dispose();

        bool isRemoved = false;

        // 4. 결과에 따른 후속 처리
        if (winnerIndex == 0) // 제거 버튼 클릭 시
        {
            // 여기서 실제로 카드를 제거하는 로직을 호출하세요. (예: CardManager.Instance.RemoveSelectedCard().Forget();)
            isRemoved = true;
        }
        else // 취소 버튼 클릭 시
        {
            CardManager.Instance.ReturnSelectedCard();
            isRemoved = false;
        }

        // 5. UI 상태 원복
        CardManager.Instance.ChangeRemove(false);

        return isRemoved;
    }
}