using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ReadSpreadSheet : MonoBehaviour
{
    private static string dataGS;
    private static Dictionary<int, CardData> dataDtct = null;
    public CardSO cardSO;
    //public SpriteRenderer gameSprite;
    //public Sprite[] s = new Sprite[5];

    //string imagePath = "Assets/9. Images/CharacterSprite";

    private void Start()
    {
        LoadCardSO().Forget();
    }
    public static async UniTaskVoid LoadData(/*string address, string range, ulong sheetID*/)
    {
        using (UnityWebRequest www =
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:O&gid=0"))
            //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
        {
            await www.SendWebRequest();
            dataGS = www.downloadHandler.text;


            if (www.isDone)
            {
                CreateDB();
            }
        }
    }

    async UniTaskVoid LoadCardSO()
    {
        using (UnityWebRequest www =
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:O&gid=0"))
        //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
        {
            await www.SendWebRequest();
            dataGS = www.downloadHandler.text;


            if (www.isDone)
            {
                SetCardSO();
            }
        }
    }

    void SetCardSO()
    {
        string[] rows = dataGS.Split("\n");
        cardSO.cards = new CardData[rows.Length];
        //cardSO.cardSprites = new Sprite[rows.Length];
        //SystemIOFileLoad();
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split(",");
            var data = new CardData();
            data.id = ConvertInt32(cells[0]);
            data.name = cells[1];
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
            data.descript = cells[12];
            data.enhancedDescript = cells[13];
            try
            {
                data.sprite = Array.Find(cardSO.cardSprites, x => x.name == data.id.ToString());
            }
            catch (UnassignedReferenceException)
            {
                data.sprite = null;
                Debug.Log("스프라이트가 없습니다.");
            }
            data.cardTag = (CardTag)Enum.Parse(typeof(CardTag) ,cells[14]);

            cardSO.cards[i] = data;
            i++;
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
        int value = Convert.ToInt32(string.IsNullOrEmpty(str) ? null : str);
        return value;
    }

    private static Dictionary<int, CardData> CreateDB()
    {
        string[] rows = dataGS.Split("\n");
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
        dataDtct = cardDB;
        return dataDtct;
    }
    public static bool TryGetData(int key, out CardData data)
    {
        dataDtct ??= CreateDB();
        var result = true;
        data = dataDtct[key];
        return result;
    }


}