using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventQueue
{
    readonly Queue<object> _queue = new Queue<object>();
    bool _isPending;
    public EventQueue()
    {
        _queue = new Queue<object>();

        _isPending = false;
    }

    public void Enqueue(Card usedCard)      // 체크하려고 했는데, 굳이 is 써서 체크할 바에 그냥 함수 2개 만들기로 함.
    {
        if (usedCard.Used) return;
        _queue.Enqueue(usedCard);

        usedCard.Used = true;

        if (!_isPending)
        {
            DoNext().Forget();
        }
    }
    public void Enqueue(object usedObject)
    {
        _queue.Enqueue(usedObject);

        if (!_isPending )
        {
            DoNext().Forget();
        }
    }

    //public void Enqueue(Card usedCard)
    //{
    //    //_queue.Enqueue(usedCard.CardTask?.Invoke(usedCard));
    //}

    public async UniTaskVoid DoNext()
    {
        if (_queue.Count == 0)
        {
            _isPending = false;
            if (!EnemyManager.Instance.MapClear)
                ButtonManager.Instance.TurnEndBtnInvert(!_isPending);
            if (InGameManager.Instance.player.CurHolo == 0)
                TurnManager.Instance.EndTurn().Forget();
            return;
        }

        _isPending = true;      // 턴매니저에 있는 로딩과는 느낌이 다름.
        ButtonManager.Instance.TurnEndBtnInvert(!_isPending);
        if (_queue.Peek() is Card cardEvent)
        {
            _queue.Dequeue();
            //Card cardEvent = _queue.Dequeue();        // 밑에 코드 삭제하고, 카드 쓰는 순간 적들한테 데미지 줘서 0이 된 카드들은 미리 삭제. 딜은 카드 쓰는 순간 들어가고, 보이는 체력은 천천히 깎이는 느낌!
            
            //if (cardEvent.Data.CardTag == CardTag.SingleAttack && (cardEvent.TargetEnemy.Death|| cardEvent.TargetEnemy is null))       // TargetEnemy missing 상태 점검 필요. null로 적용 안 됨. => 그냥 죽은 적한테 사용불가
            //{
            //    Debug.Log(cardEvent.TargetEnemy.ToString());
            //    CardManager.Instance.PutDownCard(cardEvent).Forget();
            //    cardEvent.Used = false;
            //    DoNext().Forget();
            //    return;
            //}

            //await CardManager.Instance.CheckCanUseCard(cardEvent);
            await CardManager.Instance.UsedCard(cardEvent);
        }
        //else if (!CardManager.Instance.CanUseHolo(cardEvent))
        //{
        //    CardManager.Instance.PutDownCard(cardEvent).Forget();
        //    DoNext().Forget();
        //    return;
        //}

        //await UniTask.WaitForSeconds(cardEvent.Data.CardUseDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);

        //CardManager.Instance.UsedCard(cardEvent).Forget();


        DoNext().Forget();


        //gameEvent.AddListener(() =>
        //{

        //});

        //gameEvent.Invoke();
    }

    public void QueueClear()
    {
        //_isPending = false;         // Ability를 Action으로 할 경우에는 사용해야 함.
        for (int i = 0; i < _queue.Count; ++i)
        {
            if (_queue.Peek() is Card card)
            {
                _queue.Dequeue();
                QueueClearCard(card).Forget();
            }
            --i;
        }
        ////_isPending = false;
        //_queue.Clear();
    }

    async UniTask QueueClearCard(Card card)
    {
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        CardManager.Instance.FailedUseCard(card);
    }

    //async UniTaskVoid OnBtnInteract()
    //{
    //    await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
    //    if (!TurnManager.Instance.IsLoading.Value)
    //    {
    //        ButtonManager.Instance.TurnEndButtonInvert(true);
    //    }
    //}
}
