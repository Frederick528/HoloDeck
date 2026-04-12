using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;

// [Actions] 모든 카드가 공통으로 사용하는 '원자적 기능'들
public partial class CardAbility
{
    // --- 1. 공격 관련 ---
    async UniTask SingleAttackAB(Card card, bool crit, PlayContext context)
    {
        (bool kill, int actualDamage) = await card.TargetEnemy.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player);
        // [장부 기록]
        context.LastDamageDealt = actualDamage;   // 이번 타격 데미지 기록
        context.TotalDamage += actualDamage;      // 누적 데미지 합산
        if (kill)
        {
            context.KilledEnemie = true;
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    async UniTask AllAttackAB(Card card, bool crit, PlayContext context)
    {
        var enemies = EnemyManager.Instance.EnemyList.ToList();
        (bool kill, int actualDamage)[] results = await UniTask.WhenAll(enemies.Select(e => e != null
            ? e.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player)
            : UniTask.FromResult((false, 0))));

        int totalDamageThisWave = 0;
        bool anyKilled = false;

        foreach (var result in results)
        {
            totalDamageThisWave += result.actualDamage;
            if (result.kill) anyKilled = true;
        }

        // [장부 기록]
        context.LastDamageDealt = totalDamageThisWave; // 전체 공격은 이 '한 파동'의 총합을 이번 데미지로 봅니다.
        context.TotalDamage += totalDamageThisWave;

        if (anyKilled) context.KilledEnemie = true;

        if (EnemyManager.Instance.NoEnemy)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    async UniTask RandomAttackAB(Card card, bool crit, PlayContext context)
    {
        var enemies = EnemyManager.Instance.EnemyList.ToList();
        if (enemies.Count == 0) return;

        int randomIndex = Random.Range(0, enemies.Count);
        var target = enemies[randomIndex];

        (bool kill, int actualDamage) = await target.TakeDamage(_player.CheckCriticalDamage(card.Data.Damage, crit), _player);

        // [장부 기록]
        context.LastDamageDealt = actualDamage;
        context.TotalDamage += actualDamage;

        if (kill) context.KilledEnemie = true;

        if (EnemyManager.Instance.NoEnemy)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    // --- 2. 방어 및 수치 관련 ---
    async UniTask ShieldAB(Card card, PlayContext context)
    {
        await _player.Shield(card.Data.Shield);
    }

    async UniTask HpEffectAB(Card card, PlayContext context)
    {
        if (card.Data.HP < 0) await _player.TakeDamage(-card.Data.HP, null, true);
        else await _player.Heal(card.Data.HP);
    }

    // --- 3. 카드 시스템 관련 ---
    async UniTask DrawAB(Card card, PlayContext context)
    {
        // DrawCard가 UniTask<List<Card>>를 반환한다고 가정합니다.
        var drawnCards = await CardManager.Instance.DrawCard(card.Data.Draw);

        // [장부 기록] 이번 드로우로 뽑힌 카드들을 장부에 저장
        if (drawnCards != null && drawnCards.Count > 0)
        {
            context.DrawnCards.AddRange(drawnCards);
        }
    }
    async UniTask ConfirmedDiscardAB(Card card, PlayContext context)
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

    async UniTask ConfirmedRemoveAB(Card card, PlayContext context)
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
    //private async UniTask<bool> ConditionHpCheck(Card card)
    //{
    //    if (_player.CurHP.Value > -card.Data.HP)
    //    {
    //        await _player.TakeDamage(-card.Data.HP, null, true);
    //        return true;
    //    }
    //    return false;

    //}
    //private async UniTask<bool> ConditionHpCheckXValue(Card card)
    //{
    //    if (_player.CurHP.Value > card.Data.Cost)
    //    {
    //        await _player.TakeDamage(card.Data.Cost, null, true);
    //        return true;
    //    }
    //    return false;

    //}

    async UniTask<bool> ConditionDiscardAB(Card card)
    {
        if (card.IsForce)
        {
            // 1-1. 버릴 카드가 충분한지 먼저 체크 (본인 제외)
            var otherCards = CardManager.Instance.HandCard.Where(c => c != card).ToList();
            int discardAmount = card.Data.Discard;

            if (otherCards.Count < discardAmount)
            {
                card.FailureBeforeUseCard?.Invoke();
                return false;
            }

            // 1-2. 랜덤으로 카드 선택 및 버리기
            // (Order를 섞어서 앞에서부터 필요한 만큼 가져옴)
            var targetCards = otherCards.OrderBy(x => Random.value).Take(discardAmount).ToList();

            // 1-3. 실제 버리기 처리 (CardManager에 해당 기능이 있다면 호출)
            foreach (var target in targetCards)
            {
                // 이 부분은 CardManager 구조에 맞춰 "한 장 버리기" 함수를 호출하세요.
                CardManager.Instance.ThrowAwayCard(target).Forget();
            }

            // 1-4. 연출을 위해 아주 잠깐 대기 (카드가 슉 날아가는 느낌)
            await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);

            card.SuccessBeforeUseCard?.Invoke();
            return true; // 강제 실행 성공!
        }

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
        if (card.IsForce)
        {
            // 1-1. 버릴 카드가 충분한지 먼저 체크 (본인 제외)
            var otherCards = CardManager.Instance.HandCard.Where(c => c != card).ToList();
            int removeAmount = card.Data.Remove;

            if (otherCards.Count < removeAmount)
            {
                return false;
            }

            // 1-2. 랜덤으로 카드 선택 및 버리기
            // (Order를 섞어서 앞에서부터 필요한 만큼 가져옴)
            var targetCards = otherCards.OrderBy(x => Random.value).Take(removeAmount).ToList();

            // 1-3. 실제 버리기 처리 (CardManager에 해당 기능이 있다면 호출)
            foreach (var target in targetCards)
            {
                // 이 부분은 CardManager 구조에 맞춰 "한 장 제거" 함수를 호출하세요.       => 제거로 변경해야 함.
                CardManager.Instance.ThrowAwayCard(target).Forget();
            }

            // 1-4. 연출을 위해 아주 잠깐 대기 (카드가 슉 날아가는 느낌)     => 제거 시간으로 변경해야 함.
            await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);

            return true; // 강제 실행 성공!
        }
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