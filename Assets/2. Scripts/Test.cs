using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    bool start =false;
    bool play = false;

    AsyncLazy test = null;
    async UniTask TestTask1()
    {
        await UniTask.WhenAll(
            UniTask.Create(async () =>
            {
                print("1 start");
                if (test == null)
                {
                    print("1 check1");
                    print("1 check1");
                    print("1 check1");
                    print("1 check1");
                    await (test = UniTask.Lazy(async () => await UniTask.Delay(3000)));
                    print("1 end");
                    await test;
                    print("1 check2");
                    test = null;
                    print("1 play");
                }
                else
                {
                    await test;
                    test = null;
                    print("1 play");
                }
            }),
            UniTask.Create(async () =>
            {
                print("2 start");
                if (test == null)
                {
                    await (test = UniTask.Lazy(async () => await UniTask.Delay(2000)));
                    print("2 end");
                    await test;
                    test = null;
                    print("2 start play");
                }
                else
                {

                    print("2 check1");
                    await test;
                    print("2 check2");
                    test = null;
                    print("2 play");
                }
            })
        );
    }

    private void Start()
    {
        TestTask1().Forget();
    }
}
