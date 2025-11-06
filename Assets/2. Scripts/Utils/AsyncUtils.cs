using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public static class UniRxExtensions
{
    public static UniTask AwaitTrueAsync(IReadOnlyReactiveProperty<bool> property, CancellationToken token = default)
    {
        //if (property.Value)
        //{
        //    return UniTask.CompletedTask;
        //}
        return property
            .Where(isTrue => isTrue) // "true°¡ µÇ´Â"
            .First()
            .ToUniTask(cancellationToken: token);
    }

    public static UniTask WaitForZeroAsync(IReadOnlyReactiveProperty<int> property, CancellationToken token = default)
    {
        if (property.Value == 0)
        {
            return UniTask.CompletedTask;
        }

        return property
            .Where(count => count == 0)
            .Do(ld => Debug.Log(ld))
            .ToUniTask(cancellationToken: token);
    }
}