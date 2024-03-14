using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] bool fastMode;
    public bool throwAwayCard;

    public int num;

    void Awake()
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
    }

    private void Start()
    {
        if (fastMode)
        {
            Time.timeScale = 10;
        }
        else { Time.timeScale = 1; }
        //SoundManager.instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }

    void Update()
    {
#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TurnManager.OnAddCard?.Invoke();
        //}
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnManager.Instance.DrawTask().Forget();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            TurnManager.Instance.EndTurn().Forget();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            int i = 0;
            while (!EnemyManager.Instance.SpawnEnemy(10, i))
            {
                i++;
                if (i > EnemyManager.Instance.enemySpawnPosition.Count - 1)
                    break; 
            }
        }
#endif
    }
}
