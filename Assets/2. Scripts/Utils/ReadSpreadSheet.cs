using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ReadSpreadSheet : MonoBehaviour
{
    private static string dataCardGS;
    private static string dataEnemyGS;
    private static Dictionary<int, CardData> dataDict = null;
    public CardSO cardSO;

    public EnemySO enemySO;
    //public SpriteRenderer gameSprite;
    //public Sprite[] s = new Sprite[5];

    //string imagePath = "Assets/9. Images/CharacterSprite";

    private void Start()
    {
        LoadCardSO().Forget();
        LoadEnemySO().Forget();
    }
    public static async UniTaskVoid LoadData(/*string address, string range, ulong sheetID*/)
    {
        using (UnityWebRequest www =
            //UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=0"))  // 0 = 원본, 1809511646 = 테스트용
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=1809511646"))  // 0 = 원본, 1809511646 = 테스트용
            //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
        {
            await www.SendWebRequest();
            dataCardGS = www.downloadHandler.text;


            if (www.isDone)
            {
                CreateDB();
            }
        }
    }

    async UniTaskVoid LoadCardSO()
    {
        using (UnityWebRequest wwwC =
            //UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=0"))  // 0 = 원본, 1809511646 = 테스트용
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=1809511646"))  // 0 = 원본, 1809511646 = 테스트용
        //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
        {
            await wwwC.SendWebRequest();
            dataCardGS = wwwC.downloadHandler.text;


            if (wwwC.isDone)
            {
                SetCardSO();
            }
        }
    }

    async UniTaskVoid LoadEnemySO()
    {
        using (UnityWebRequest wwwE = 
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1ReoyeaeB220v3EhNHWEefLY9KB2ScyttVtCqg6Rj6IA/export?format=csv&range=A3:G&gid=0"))  // 0 = 원본, 1809511646 = 테스트용
            //UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1ReoyeaeB220v3EhNHWEefLY9KB2ScyttVtCqg6Rj6IA/export?format=csv&range=A3:G&gid=1809511646"))  // 0 = 원본, 1809511646 = 테스트용
        //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
        {
            await wwwE.SendWebRequest();
            dataEnemyGS = wwwE.downloadHandler.text;


            if (wwwE.isDone)
            {
                SetEnemySO();
            }
        }
    }

    void SetCardSO()
    {
        string[] rows = dataCardGS.Split("\n");
        cardSO.cards = new CardData[rows.Length];
        //cardSO.cardSprites = new Sprite[rows.Length];
        //SystemIOFileLoad();
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split(",");
            var data = new CardData();
            data.id = ConvertInt32(cells[0]);
            data.name = LineBreakStr(cells[1]);
            data.cost = ConvertInt32(cells[2]);
            data.enhancedCost = ConvertInt32(cells[3]);
            data.damage = ConvertInt32(cells[4]);
            data.enhancedDamage = ConvertInt32(cells[5]);
            data.defence = ConvertInt32(cells[6]);
            data.enhancedDefence = ConvertInt32(cells[7]);
            data.count = ConvertInt32(cells[8]);                                                    
            data.enhancedCount = ConvertInt32(cells[9]);
            data.draw = ConvertInt32(cells[10]);
            data.enhancedDraw = ConvertInt32(cells[11]);
            //data.cardUseDelay = float.Parse(cells[12]);
            data.descript = LineBreakStr(cells[13]);
            data.enhancedDescript = LineBreakStr(cells[14]);
            try
            {
                data.sprite = Array.Find(cardSO.cardSprites, x => x.name == data.id.ToString());
            }
            catch (UnassignedReferenceException)
            {
                data.sprite = null;
                Debug.Log("스프라이트가 없습니다.");
            }
            data.cardTag = (CardTag)Enum.Parse(typeof(CardTag), cells[15]);

            cardSO.cards[i] = data;
            ++i;
        }
    }

    void SetEnemySO()
    {
        string[] rows = dataEnemyGS.Split("\n");
        enemySO.enemyDatas = new EnemyData[rows.Length];
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split(",");
            var data = new EnemyData();
            data.id = ConvertInt32(cells[0]);
            data.name = LineBreakStr(cells[1]);
            data.hp = ConvertInt32(cells[2]);
            data.damage = ConvertInt32(cells[3]);
            data.dropCoin = ConvertInt32(cells[4]);
            data.descript = LineBreakStr(cells[5]);
            try
            {
                data.sprite = Array.Find(enemySO.enemySprites, x => x.name == data.id.ToString());
            }
            catch (UnassignedReferenceException)
            {
                data.sprite = null;
                Debug.Log("스프라이트가 없습니다.");
            }
            data.enemyTag = (EnemyTag)Enum.Parse(typeof(EnemyTag), cells[6]);
            data.enemyPrefab = Resources.Load<GameObject>($"Enemy/{data.name}");

            enemySO.enemyDatas[i] = data;
            ++i;
        }
    }

    //private void SystemIOFileLoad()
    //{
    //    string[] imageFiles = Directory.GetFiles(imagePath, "*.png");
    //    int j = 0;
    //    foreach (string imagePath in imageFiles)
    //    {
    //        string imageName = Path.GetFileNameWithoutExtension(imagePath);
    //        // 파일을 바이트 배열로 읽어옴
    //        byte[] imageData = File.ReadAllBytes(imagePath);
    //        Texture2D texture = new Texture2D(1, 1);
    //        texture.LoadImage(imageData);
    //        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    //        sprite.name = imageName;
    //        gameSprite.sprite = sprite;
    //        s[j] = (sprite);
    //        cardSO.cardSprites[j] = sprite;
    //        j++;
    //    }
    //}

    int ConvertInt32(string str)    // 구글스프레드시트는 엑셀 빈 칸을 ""로 가져오기 때문에 Convert.ToInt32가 에러가 뜸.
    {
        int _value = Convert.ToInt32(string.IsNullOrEmpty(str) ? null : str);
        return _value;
    }

    string LineBreakStr(string str)     // 구글스프레드시트에서 줄바꿈을 하면, csv에서 쉼표로 읽어옴. 따라서 개행문자(\n)를 이용해야 하나. 이 또한, \\n으로 인식하기 때문에 Replace가 필요함.
    {
        string _return = str.Replace("\\n", "\n");
        return _return;
    }

    private static Dictionary<int, CardData> CreateDB()
    {
        string[] rows = dataCardGS.Split("\n");
        Dictionary<int, CardData> cardDB = new Dictionary<int, CardData>();
        foreach (string row in rows)
        {
            string[] cells = row.Split(",");
            var data = new CardData();
            data.name = cells[1];
            data.descript = cells[2];
            print(cells[2]);

            cardDB.Add(Convert.ToInt32(cells[0].ToString()), data);
            //GameManager.Instance.ArtifactDict.Add(Convert.ToInt32(cells[0].ToString()), false);
            //GameManager.Instance.ObtainableArtifact.Add(Convert.ToInt32(cells[0].ToString()));
        }
        dataDict = cardDB;
        return dataDict;
    }
    public static bool TryGetData(int key, out CardData data)
    {
        dataDict ??= CreateDB();
        var result = true;
        data = dataDict[key];
        return result;
    }


}