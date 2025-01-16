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
    //    //_queue.Enqueue(usedCard.CardAction?.Invoke(usedCard));
    //}

    public async UniTaskVoid DoNext()
    {
        if (_queue.Count == 0)
        {
            _isPending = false;
            ButtonManager.Instance.TurnEndButtonInvert(!_isPending);
            return;
        }

        _isPending = true;      // 턴매니저에 있는 로딩과는 느낌이 다름.
        ButtonManager.Instance.TurnEndButtonInvert(!_isPending);
        if (_queue.Peek() is Card cardEvent)
        {
            _queue.Dequeue();
            //Card cardEvent = _queue.Dequeue();
            if (cardEvent.Data.CardTag == CardTag.SingleAttack && (cardEvent.TargetEnemy.Death|| cardEvent.TargetEnemy is null))       // TargetEnemy missing 상태 점검 필요. null로 적용 안 됨.
            {
                Debug.Log(cardEvent.TargetEnemy.ToString());
                CardManager.Instance.PutDownCard(cardEvent).Forget();
                cardEvent.Used = false;
                DoNext().Forget();
                return;
            }
            await CardManager.Instance.CheckCanUseCard(cardEvent);
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
    //    if (!TurnManager.Instance.isLoading.Value)
    //    {
    //        ButtonManager.Instance.TurnEndButtonInvert(true);
    //    }
    //}
}
