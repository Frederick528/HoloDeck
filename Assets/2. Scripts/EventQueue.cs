using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventQueue
{
    readonly Queue<(Card, bool)> _queue = new Queue<(Card, bool)>();
    bool _isPending;
    public EventQueue()
    {
        _queue = new Queue<(Card, bool)>();

        _isPending = false;
    }


    public void Enqueue(Card usedCard, bool isSingleAtk = false)
    {
        if (usedCard.Used) return;
        _queue.Enqueue((usedCard, isSingleAtk));

        usedCard.Used = true;

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
            OnBtnInteract().Forget();
            return;
        }

        _isPending = true;
        ButtonManager.instance.TurnEndButtonInvert(!_isPending);
        (Card cardEvent, bool isSingleAtk) = _queue.Dequeue();
        if (isSingleAtk && (cardEvent.TargetEnemy.Death|| cardEvent.TargetEnemy is null))       // TargetEnemy missing 상태 점검 필요. null로 적용 안 됨.
        {
            Debug.Log(cardEvent.TargetEnemy.ToString());
            CardManager.Instance.PutDownCard(cardEvent).Forget();
            DoNext().Forget();
            return;
        }
        //else if (!CardManager.Instance.CanUseHolo(cardEvent))
        //{
        //    CardManager.Instance.PutDownCard(cardEvent).Forget();
        //    DoNext().Forget();
        //    return;
        //}

        //await UniTask.WaitForSeconds(cardEvent.Data.CardUseDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);

        //CardManager.Instance.UsedCard(cardEvent).Forget();

        await CardManager.Instance.UseCard(cardEvent);
        Debug.Log(_queue.Count);

        DoNext().Forget();


        //gameEvent.AddListener(() =>
        //{

        //});

        //gameEvent.Invoke();
    }

    public void QueueClear()
    {
        //_isPending = false;
        _queue.Clear();
    }

    async UniTaskVoid OnBtnInteract()
    {
        await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);
        if (!TurnManager.Instance.isLoading.Value)
        {
            ButtonManager.instance.TurnEndButtonInvert(true);
        }
    }
}
