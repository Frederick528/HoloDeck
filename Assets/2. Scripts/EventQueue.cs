using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventQueue
{
    readonly Queue<object> _queue;
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

        usedCard.CheckEnemyDead();
        usedCard.Used = true;

        if (!_isPending)
        {
            DoNext().Forget();
        }
    }
    public void Enqueue(object usedObject)
    {
        _queue.Enqueue(usedObject);
        if (usedObject is Item item)
        {
            item.CheckEnemyDead();
        }

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
            //if (InGameManager.Instance.player.CurHolo == 0)           // 강제 턴종은 포션이나 스킬 효과를 못 쓰게 만드므로 그냥 제거
            //    TurnManager.Instance.EndTurn().Forget();
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
            await CardManager.Instance.PlayedCard(cardEvent);
        }
        else if (_queue.Peek() is Item itemEvent)
        {
            _queue.Dequeue();
            await itemEvent.UseTask();
        }
        //else if (!CardManager.Instance.CanUseHolo(cardEvent))
        //{
        //    CardManager.Instance.PutDownCard(cardEvent).Forget();
        //    DoNext().Forget();
        //    return;
        //}

        //await UniTask.WaitForSeconds(cardEvent.Data.CardUseDelay, false, PlayerLoopTiming.Update, TurnManager.Instance.CancelSource.Token);

        //CardManager.Instance.PlayedCard(cardEvent).Forget();


        DoNext().Forget();


        //gameEvent.AddListener(() =>
        //{

        //});

        //gameEvent.Invoke();
    }

    public void QueueClear()        // 큐에 있는 능력들로 적이 다 죽었다고 판단되면, 더 이상 카드를 못 쓰기 때문에 사실상 필요없음. 그래도 혹시 모르니깐.. (가능성 생겨서 쓰는 게 맞음.)
    {
        //_isPending = false;         // Ability를 Action으로 할 경우에는 사용해야 함.
        int count = _queue.Count;
        for (int i = 0; i < count; ++i)
        {
            if (_queue.Peek() is Card card)
            {
                _queue.Dequeue();
                QueueClearCard(card)/*.Forget()*/;
            }
            else if (_queue.Peek() is Item item)
            {
                _queue.Dequeue();
                if (item.Data.ItemTag == ItemTag.Active)
                {
                    ItemManager.Instance.Charge(item.Data.MaxCharge);
                }
            }
            //--i;
        }
        _isPending = false;
        //_queue.Clear();
    }

    void QueueClearCard(Card card)
    {
        //await UniTask.WaitForSeconds(CardUtils.ThrowAwayCardDelay);
        //CardManager.Instance.FailedUseCard(card);
        card.AfterCardAbility(true).Forget();
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
