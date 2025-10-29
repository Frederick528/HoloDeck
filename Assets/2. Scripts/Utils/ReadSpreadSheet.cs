using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ReadSpreadSheet : MonoBehaviour
{
    private static string _dataCardGS;
    //private static string dataEnhancedCardGS;
    private static string _dataEnemyGS;
    //private static Dictionary<int, CardData> dataDict = null;
    private static string _dataItemGS;
    public CardSO CardSO;

    public EnemySO EnemySO;
    //public AssetLabelReference AssetLabel;

    //Dictionary<string, GameObject> _enemyPath = new();

    public ItemSO ItemSO;

    //public SpriteRenderer gameSprite;
    //public Sprite[] s = new Sprite[5];

    //string imagePath = "Assets/9. Images/CharacterSprite";

    private void Start()
    {
        LoadStart().Forget();
    }

    async UniTaskVoid LoadStart()
    {
        Debug.Log("Load Start");
        await UniTask.WhenAll(
            LoadCardSO(),
            LoadEnemySO(),
            LoadItemSO()
            );
        Debug.Log("Load End");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
    //public static async UniTaskVoid LoadData(/*string address, string range, ulong sheetID*/)
    //{
    //    using (UnityWebRequest www =
    //        //UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=0"))  // 0 = 원본, 1809511646 = 테스트용
    //        UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:P&gid=1809511646"))  // 0 = 원본, 1809511646 = 테스트용
    //        //UnityWebRequest.Get($"{address}/export?format=csv&range={range}&gid={sheetID}"))
    //    {
    //        await www.SendWebRequest();
    //        dataCardGS = www.downloadHandler.text;


    //        if (www.isDone)
    //        {
    //            CreateDB();
    //        }
    //    }
    //}

    async UniTask LoadCardSO()
    {
        Debug.Log("Card Load Start");
        string address = "https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8";
        string range = "A4:O";
        string cardSheetID = "1809511646";
        string enhancedCardSheetID = "1928890928";
        using UnityWebRequest wwwC =
            //UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1zMmdkBnHjdpRvZV6WzHDfPSj76XQ227xlB8fwNBRe-8/export?format=csv&range=A3:Q&gid=0"))  // 0 = 원본, 1809511646 = 테스트용
            UnityWebRequest.Get($"{address}/export?format=tsv&range={range}&gid={cardSheetID}");
        using UnityWebRequest wwwEC =
            UnityWebRequest.Get($"{address}/export?format=tsv&range={range}&gid={enhancedCardSheetID}");

        await wwwC.SendWebRequest();
        _dataCardGS = wwwC.downloadHandler.text;

        await wwwEC.SendWebRequest();
        _dataCardGS += "\n" + wwwEC.downloadHandler.text;

        if (wwwEC.isDone)
        {
            SetCardSO();
        }
    }

    async UniTask LoadEnemySO()
    {
        Debug.Log("Enemy Load Start");
        using UnityWebRequest wwwE =
            UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1ReoyeaeB220v3EhNHWEefLY9KB2ScyttVtCqg6Rj6IA/export?format=tsv&range=A4:I&gid=0");
        await wwwE.SendWebRequest();
        _dataEnemyGS = wwwE.downloadHandler.text;


        if (wwwE.isDone)
        {
            SetEnemySO();
        }
    }

    async UniTask LoadItemSO()
    {
        Debug.Log("Item Load Start");
        string address = "https://docs.google.com/spreadsheets/d/1Vgmh15r623LlejfgjKGYfoWfYRTz9tK385wjA79b4eU";
        string range = "A4:O";
        string passiveSheetID = "0";
        string activeSheetID = "652559595";
        string potionSheetID = "971911695";

        using UnityWebRequest wwwPassive =
            UnityWebRequest.Get($"{address}/export?format=tsv&range={range}&gid={passiveSheetID}");
        using UnityWebRequest wwwActive =
            UnityWebRequest.Get($"{address}/export?format=tsv&range={range}&gid={activeSheetID}");
        using UnityWebRequest wwwPotion =
            UnityWebRequest.Get($"{address}/export?format=tsv&range={range}&gid={potionSheetID}");

        await wwwPassive.SendWebRequest();
        _dataItemGS = wwwPassive.downloadHandler.text;
        await wwwActive.SendWebRequest();
        _dataItemGS += "\n" + wwwActive.downloadHandler.text;
        await wwwPotion.SendWebRequest();
        _dataItemGS += "\n" + wwwPotion.downloadHandler.text;


        if (wwwPotion.isDone)
        {
            SetItemSO();
        }
    }

    void SetCardSO()
    {
        string[] rows = _dataCardGS.Split("\n");
        CardSO.Cards = new CardData[rows.Length];
        //_cardSO.CardSprites = new Sprite[rows.Length];
        //SystemIOFileLoad();
        for (int idx = 0;  idx < CardSO.ClassifyCardRarityID.Length; ++idx)
        {
            CardSO.ClassifyCardRarityID[idx].x = -1;
        }
        for (int idx = 0; idx < CardSO.ClassifyEnhancedCardRarityID.Length; ++idx)
        {
            CardSO.ClassifyEnhancedCardRarityID[idx].x = -1;
        }
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split("\t");
            CardData data = new()
            {
                ID = ConvertInt32(cells[0]),
                Name = LineBreakStr(cells[1]),
                Cost = ConvertInt32(cells[2]),
                Damage = ConvertInt32(cells[3]),
                Shield = ConvertInt32(cells[4]),
                Count = ConvertInt32(cells[5]) == 0 ? 1 : ConvertInt32(cells[5]),
                Draw = ConvertInt32(cells[6]),
                Discard = ConvertInt32(cells[7]),
                Remove = ConvertInt32(cells[8]),
                //data.CardUseDelay = ConvertSingle(cells[7]);
                Price = ConvertInt32(cells[9]),
                Descript = LineBreakStr(cells[10]),
                IsSimpleAB = NullTrueBool(cells[11]),
                HasSimpleCondition = NullFalseBool(cells[12]),
                CardTag = (CardTag)Enum.Parse(typeof(CardTag), cells[13]),
                CardRarity = (CardRarity)Enum.Parse(typeof(CardRarity), cells[14])
            };
            data.Sprite = Array.Find(CardSO.CardSprites, x => x.name == data.ID.ToString());
            data.Effect = Array.Find(CardSO.CardEffects, x => x.name == data.ID.ToString());
            //try
            //{
            //    data.Sprite = Array.Find(CardSO.CardSprites, x => x.name == data.ID.ToString());
            //}
            //catch (UnassignedReferenceException)
            //{
            //    data.Sprite = CardSO.CardSprites[0];
            //    Debug.Log("스프라이트가 없습니다.");
            //}
            //data.ID = ConvertInt32(cells[0]);
            //data.Name = LineBreakStr(cells[1]);
            //data.Cost = ConvertInt32(cells[2]);
            //data.EnhancedCost = ConvertInt32(cells[3]);
            //data.Damage = ConvertInt32(cells[4]);
            //data.EnhancedDamage = ConvertInt32(cells[5]);
            //data.Shield = ConvertInt32(cells[6]);
            //data.EnhancedDefence = ConvertInt32(cells[7]);
            //data.Count = ConvertInt32(cells[8]);                                                    
            //data.EnhancedCount = ConvertInt32(cells[9]);
            //data.Draw = ConvertInt32(cells[10]);
            //data.EnhancedDraw = ConvertInt32(cells[11]);
            //data.CardUseDelay = ConvertSingle(cells[12]);
            //data.Price = ConvertInt32(cells[13]);
            //data.Descript = LineBreakStr(cells[14]);
            //data.EnhancedDescript = LineBreakStr(cells[15]);
            //try
            //{
            //    data.Sprite = Array.Find(_cardSO.CardSprites, x => x.name == data.ID.ToString());
            //}
            //catch (UnassignedReferenceException)
            //{
            //    data.Sprite = null;
            //    Debug.Log("스프라이트가 없습니다.");
            //}
            //data.CardTag = (CardTag)Enum.Parse(typeof(CardTag), cells[16]);
            if (data.ID < 10000)
            {
                switch (data.CardRarity)
                {
                    case CardRarity.Common:
                        if (CardSO.ClassifyCardRarityID[0].x == -1)
                        {
                            CardSO.ClassifyCardRarityID[0].x = data.ID;
                        }
                        CardSO.ClassifyCardRarityID[0].y = data.ID;
                        break;
                    case CardRarity.Rare:
                        if (CardSO.ClassifyCardRarityID[1].x == -1)
                        {
                            CardSO.ClassifyCardRarityID[1].x = data.ID;
                        }
                        CardSO.ClassifyCardRarityID[1].y = data.ID;
                        break;
                    case CardRarity.Epic:
                        if (CardSO.ClassifyCardRarityID[2].x == -1)
                        {
                            CardSO.ClassifyCardRarityID[2].x = data.ID;
                        }
                        CardSO.ClassifyCardRarityID[2].y = data.ID;
                        break;
                    case CardRarity.Legendary:
                        if (CardSO.ClassifyCardRarityID[3].x == -1)
                        {
                            CardSO.ClassifyCardRarityID[3].x = data.ID;
                        }
                        CardSO.ClassifyCardRarityID[3].y = data.ID;
                        break;
                }
            }
            else
            {
                switch (data.CardRarity)
                {
                    case CardRarity.Common:
                        if (CardSO.ClassifyEnhancedCardRarityID[0].x == -1)
                        {
                            CardSO.ClassifyEnhancedCardRarityID[0].x = data.ID;
                        }
                        CardSO.ClassifyEnhancedCardRarityID[0].y = data.ID;
                        break;
                    case CardRarity.Rare:
                        if (CardSO.ClassifyEnhancedCardRarityID[1].x == -1)
                        {
                            CardSO.ClassifyEnhancedCardRarityID[1].x = data.ID;
                        }
                        CardSO.ClassifyEnhancedCardRarityID[1].y = data.ID;
                        break;
                    case CardRarity.Epic:
                        if (CardSO.ClassifyEnhancedCardRarityID[2].x == -1)
                        {
                            CardSO.ClassifyEnhancedCardRarityID[2].x = data.ID;
                        }
                        CardSO.ClassifyEnhancedCardRarityID[2].y = data.ID;
                        break;
                    case CardRarity.Legendary:
                        if (CardSO.ClassifyEnhancedCardRarityID[3].x == -1)
                        {
                            CardSO.ClassifyEnhancedCardRarityID[3].x = data.ID;
                        }
                        CardSO.ClassifyEnhancedCardRarityID[3].y = data.ID;
                        break;
                }
            }
            CardSO.Cards[i] = data;
            ++i;
        }
        Debug.Log("Card Load End");
#if UNITY_EDITOR
        EditorUtility.SetDirty(CardSO);
#endif
    }

    void SetEnemySO()
    {
        string[] rows = _dataEnemyGS.Split("\n");
        EnemySO.EnemyDatas = new EnemyData[rows.Length];
        //Addressables.LoadResourceLocationsAsync(AssetLabel).Completed +=
        //    (handle) =>
        //    {
        //        _locations = handle.Result;
        //    };
        //Addressables.LoadAssetsAsync<GameObject>(AssetLabel, null).Completed +=
        //    (handle) =>
        //    {
        //        if (handle.Status != AsyncOperationStatus.Succeeded)
        //        {
        //            return;
        //        }
        //        for (int i = 0; i < handle.Result.Count; ++i)
        //        {
        //            _enemyPath.Add(handle.Result[i].name, handle.Result[i]);
        //        }
        //        int j = 0;
        //        foreach (string row in rows)
        //        {
        //            string[] cells = row.Split("\t");
        //            EnemyData data = new()
        //            {
        //                id = ConvertInt32(cells[0]),
        //                name = LineBreakStr(cells[1]),
        //                hp = ConvertInt32(cells[2]),
        //                damage = ConvertInt32(cells[3]),
        //                dropCoin = ConvertInt32(cells[4]),
        //                descript = LineBreakStr(cells[5]),
        //                enemyTag = (EnemyTag)Enum.Parse(typeof(EnemyTag), cells[6])
        //            };
        //            try
        //            {
        //                data.sprite = Array.Find(EnemySO.enemySprites, x => x.name == data.id.ToString());
        //            }
        //            catch (UnassignedReferenceException)
        //            {
        //                data.sprite = null;
        //                Debug.Log("스프라이트가 없습니다.");
        //            }
        //            try
        //            {
        //                data.enemyPrefab = Array.Find(EnemySO.Prefabs, x => x.name == data.name);
        //            }
        //            catch (UnassignedReferenceException)
        //            {
        //                data.enemyPrefab = null;
        //                Debug.Log("프리팹이 없습니다.");
        //            }
        //            //if (_enemyPath.ContainsKey(data.name))
        //            //{
        //            //    data.enemyPrefab = _enemyPath[data.name];
        //            //}

        //            EnemySO.enemyDatas[j] = data;
        //            ++j;
        //        }
        //        Addressables.Release(handle);
        //    };
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split("\t");
            EnemyData data = new()
            {
                ID = ConvertInt32(cells[0]),
                Name = LineBreakStr(cells[1]),
                HP = ConvertInt32(cells[2]),
                Damage = ConvertInt32(cells[3]),
                CriticalChance = ConvertInt32(cells[4]),
                CriticalDamage = ConvertInt32(cells[5]),
                DropCoin = ConvertInt32(cells[6]),
                Descript = LineBreakStr(cells[7]),
                EnemyTag = (EnemyTag)Enum.Parse(typeof(EnemyTag), cells[8])
            };
            try
            {
                data.Sprite = Array.Find(EnemySO.EnemySprites, x => x.name == data.ID.ToString());
            }
            catch (UnassignedReferenceException)
            {
                data.Sprite = null;
                Debug.Log("스프라이트가 없습니다.");
            }
            try
            {
                data.EnemyPrefab = Array.Find(EnemySO.EnemyPrefabs, x => x.name == data.Name);
            }
            catch (UnassignedReferenceException)
            {
                data.EnemyPrefab = null;
                Debug.Log("프리팹이 없습니다.");
            }

            EnemySO.EnemyDatas[i] = data;
            ++i;
        }
        Debug.Log("Enemy Load End");
#if UNITY_EDITOR
        EditorUtility.SetDirty(EnemySO);
#endif
    }

    void SetItemSO()
    {
        string[] rows = _dataItemGS.Split("\n");
        ItemSO.Items = new ItemData[rows.Length];
        //_cardSO.CardSprites = new Sprite[rows.Length];
        //SystemIOFileLoad();
        ItemSO.PassiveID.x = -1;
        ItemSO.ActiveID.x = -1;
        ItemSO.PotionID.x = -1;
        int i = 0;
        foreach (string row in rows)
        {
            string[] cells = row.Split("\t");
            ItemData data = new()
            {
                ID = ConvertInt32(cells[0]),
                Name = LineBreakStr(cells[1]),
                ItemTag = (ItemTag)Enum.Parse(typeof(ItemTag), cells[cells.Length-2]),
                ItemRarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), cells[cells.Length-1])
                //MaxCharge = ConvertInt32(cells[2]),
                //CurCharge = ConvertInt32(cells[3]),
                //Damage = ConvertInt32(cells[4]),
                //Shield = ConvertInt32(cells[5]),
                //Draw = ConvertInt32(cells[6]),
                //Heal = ConvertInt32(cells[7]),
                //Duration = ConvertInt32(cells[8]),
                //Price = ConvertInt32(cells[9]),
                //Descript = LineBreakStr(cells[10]),
            };
            switch (data.ItemTag)
            {
                case ItemTag.Passive:
                    if (ItemSO.PassiveID.x == -1)
                    {
                        ItemSO.PassiveID.x = data.ID;
                    }
                    ItemSO.PassiveID.y = data.ID;

                    data.Price = ConvertInt32(cells[2]);
                    data.Descript = LineBreakStr(cells[3]);

                    break;
                case ItemTag.Active:
                    if (ItemSO.ActiveID.x == -1)
                    {
                        ItemSO.ActiveID.x = data.ID;
                    }
                    ItemSO.ActiveID.y = data.ID;

                    data.MaxCharge = ConvertInt32(cells[2]);
                    data.Damage = ConvertInt32(cells[3]);
                    data.Shield = ConvertInt32(cells[4]);
                    data.Draw = ConvertInt32(cells[5]);
                    data.Heal = ConvertInt32(cells[6]);
                    data.Duration = ConvertInt32(cells[7]);
                    data.Price = ConvertInt32(cells[8]);
                    data.Descript = LineBreakStr(cells[9]);
                    data.AttackType = (AttackType)Enum.Parse(typeof(AttackType), ItemEnumCellCheck(cells[10]));
                    data.ItemCanUse = (ItemCanUse)Enum.Parse(typeof(ItemCanUse), ItemEnumCellCheck(cells[11]));
                    data.StartCharge = NullTrueBool(cells[12]);

                    break;
                case ItemTag.Potion:
                    if (ItemSO.PotionID.x == -1)
                    {
                        ItemSO.PotionID.x = data.ID;
                    }
                    ItemSO.PotionID.y = data.ID;

                    data.Damage = ConvertInt32(cells[2]);
                    data.Shield = ConvertInt32(cells[3]);
                    data.Draw = ConvertInt32(cells[4]);
                    data.Heal = ConvertInt32(cells[5]);
                    data.Duration = ConvertInt32(cells[6]);
                    data.Price = ConvertInt32(cells[7]);
                    data.Descript = LineBreakStr(cells[8]);
                    data.AttackType = (AttackType)Enum.Parse(typeof(AttackType), ItemEnumCellCheck(cells[9]));
                    data.ItemCanUse = (ItemCanUse)Enum.Parse(typeof(ItemCanUse), ItemEnumCellCheck(cells[10]));

                    break;
            }
            //if (data.ItemTag != ItemTag.Passive)
            //{
            //    data.AttackType = (AttackType)Enum.Parse(typeof(AttackType), ItemEnumCellCheck(cells[13]));
            //    data.ItemCanUse = (ItemCanUse)Enum.Parse(typeof(ItemCanUse), ItemEnumCellCheck(cells[14]));
            //}

            try
            {
                data.Sprite = Array.Find(ItemSO.ItemSprites, x => x.name == data.ID.ToString());
            }
            catch (UnassignedReferenceException)
            {
                data.Sprite = null;
                Debug.Log("스프라이트가 없습니다.");
            }
            ItemSO.Items[i] = data;
            ++i;
        }
        Debug.Log("Item Load End");
#if UNITY_EDITOR
        EditorUtility.SetDirty(ItemSO);
#endif
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
    //        Sprite Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    //        Sprite.Name = imageName;
    //        gameSprite.Sprite = Sprite;
    //        s[j] = (Sprite);
    //        _cardSO.CardSprites[j] = Sprite;
    //        j++;
    //    }
    //}

    float ConvertSingle(string str)    // 구글스프레드시트는 엑셀 빈 칸을 ""로 가져오기 때문에 Convert.ToSingle가 에러가 뜸.
    {
        //float _value = Convert.ToSingle(string.IsNullOrEmpty(str) ? null : str);
        return Convert.ToSingle(string.IsNullOrEmpty(str) ? null : str);
    }

    int ConvertInt32(string str)    // 구글스프레드시트는 엑셀 빈 칸을 ""로 가져오기 때문에 Convert.ToInt32가 에러가 뜸.
    {
        //int _value = Convert.ToInt32(string.IsNullOrEmpty(str) ? null : str);
        return Convert.ToInt32(string.IsNullOrEmpty(str) ? null : str);
    }

    bool NullFalseBool(string str)
    {
        return string.IsNullOrEmpty(str) ? false : bool.Parse(str);
    }
    bool NullTrueBool(string str)
    {
        return string.IsNullOrEmpty(str) ? true : bool.Parse(str);
    }

    string ItemEnumCellCheck(string str)
    {
        return string.IsNullOrEmpty(str) ? "None" : str;
    }

    string LineBreakStr(string str)     // 구글스프레드시트에서 줄바꿈을 하면, csv에서 쉼표로 읽어옴. 따라서 개행문자(\n)를 이용해야 하나. 이 또한, \\n으로 인식하기 때문에 Replace가 필요함.
    {
        //string _return = str.Replace("\\n", "\n");
        return str.Replace("\\n", "\n");
    }

    //private static Dictionary<int, CardData> CreateDB()
    //{
    //    string[] rows = dataCardGS.Split("\n");
    //    Dictionary<int, CardData> cardDB = new Dictionary<int, CardData>();
    //    foreach (string row in rows)
    //    {
    //        string[] cells = row.Split(",");
    //        var data = new CardData();
    //        data.Name = cells[1];
    //        data.Descript = cells[2];
    //        print(cells[2]);

    //        cardDB.Add(Convert.ToInt32(cells[0].ToString()), data);
    //        //InGameManager.Instance.ArtifactDict.Add(Convert.ToInt32(cells[0].ToString()), false);
    //        //InGameManager.Instance.ObtainableArtifact.Add(Convert.ToInt32(cells[0].ToString()));
    //    }
    //    dataDict = cardDB;
    //    return dataDict;
    //}
    //public static bool TryGetData(int key, out CardData data)
    //{
    //    dataDict ??= CreateDB();
    //    var result = true;
    //    data = dataDict[key];
    //    return result;
    //}


}