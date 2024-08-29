using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventQueue
{
    readonly Queue<UnityEvent> _queue = new Queue<UnityEvent>();
    bool _isPending;
    public EventQueue()
    {
        _queue = new Queue<UnityEvent>();

        _isPending = false;
    }


    public void Enqueue(UnityEvent e)
    {
        _queue.Enqueue(e);

        if (!_isPending )
        {
            DoNext();
        }
    }

    public void Enqueue(Card usedCard)
    {
        //_queue.Enqueue(usedCard.cardAction?.Invoke(usedCard));
    }

    public void DoNext()
    {
        if (_queue.Count == 0)
            return;

        UnityEvent gameEvent = _queue.Dequeue();
        _isPending = true;

        gameEvent.AddListener(() =>
        {

        });

        gameEvent.Invoke();
    }
}
