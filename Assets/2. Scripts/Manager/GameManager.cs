using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CancellationTokenSource Cts;

    //public List<Card> Deck;

    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Screen.SetResolution(1920, 1080, true);
        DontDestroyOnLoad(gameObject);

        //StartCoroutine(ReadSpreadSheet.LoadData("https://docs.google.com/spreadsheets/d/1CqNR2Rh_OIVe8n0CG8vC7YVpbNUn_-0rXeBab72gXvs", "A3:D14", 0));
        
        //if (!CardDataDeserializer.TryGetData(1015, out CardData row))
        //{
        //    Debug.Log("데이터 테이블을 불러오는 과정에서 문제가 발생했습니다.");
        //}

        Cts = new CancellationTokenSource();
        await TokenRebuilder();
    }

    private void Start()
    {
        //SoundManager.instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }

    private async UniTask TokenRebuilder()
    {
        while (true)
        {
            if (Cts.Token.IsCancellationRequested)
            {
                Cts.Cancel();
                Cts.Dispose();
                Thread.MemoryBarrier();

                Cts = new CancellationTokenSource();
            }

            if (this.gameObject == null) break;

            await UniTask.Delay(100);
        }
    }
}
