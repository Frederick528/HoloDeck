//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingMap : MonoBehaviour
{
    private List<Vector3Int> direction4 = new List<Vector3Int>
    {
        new Vector3Int( 0, 1,  0),       // down
        new Vector3Int( 1, 0,  0),       // right
        new Vector3Int(-1, 0,  0),       // left
        new Vector3Int( 0, -1, 0)        // up
    };
    //public Vector3Int downPattern = new Vector3Int(0, 0, 1);
    //public Vector3Int upPattern = new Vector3Int(0, 0, -1);

    //public Vector3Int leftPattern = new Vector3Int(-1, 0, 0);
    //public Vector3Int rightPattern = new Vector3Int(1, 0, 0);

    public List<MapInfo> validMapList = new List<MapInfo>();
    public List<MapInfo> availableMapList = new List<MapInfo>();

    public int creatMapCnt;                       // 생성할 방 갯수
    //public int maxMapCnt;                       // 최대 방 갯수
    //public int currMapCnt;                        // 현재 방 갯수
    public int maxDistance;                        // 최대 거리 제한

    //public int validMapCount;

    public Vector3Int startMapPosition;                        // 시작 포지션
    public Vector3Int bossMapPosition;                         // 보스 방 포지션

    public MapInfo[,] posArr;                       // 방 좌표에 대한 2차원 배열

    public List<Map> maps;

    Queue<Vector3Int> queue = new();

    int mapDistance = 100;

    [SerializeField] GameObject[] _mapPrefab;
    [SerializeField] GameObject _markPrefab;
    GameObject _mark;
    Vector3 _markDefaultPos;
    //[SerializeField] Transform mapCanvas;
    //[SerializeField] GameObject cardRewardCanvas;
    //[SerializeField] GameObject enlargePanel;

    //[SerializeField] GameObject itemRewardCanvas;

    //[SerializeField] GameObject shopCanvas;
    //[SerializeField] GameObject shopenlargePanel;
    //[SerializeField] GameObject shopPanel;
    private void Start()
    {
        creatMapCnt = (int)Mathf.Clamp(creatMapCnt, 1, Mathf.Pow(maxDistance * 2 + 1, 2));
        CreatedMap();
    }

    public void CreatedMap()
    {
        //// 배열 ReSize
        //posArr = (MapInfo[,])ResizeArray(posArr, new int[] { maxDistance * 2 + 1, maxDistance * 2 + 1 });
        posArr = new MapInfo[maxDistance * 2 + 1, maxDistance * 2 + 1];

        validMapList.Clear();
        availableMapList.Clear();
        //RealaseMap();  // 초기화

        startMapPosition = new Vector3Int(maxDistance, maxDistance, 0);                        // 시작 좌표

        posArr[startMapPosition.x, startMapPosition.y] = AddSingleMap(new MapInfo(), startMapPosition, "Single");
        //posArr[startMapPosition.x, startMapPosition.y].distance = 0;
        //validMapList.Add(posArr[startMapPosition.x, startMapPosition.y]);
        //availableMapList.Add(posArr[startMapPosition.x, startMapPosition.y]);

        while (!MapCountCheck())
        {
            int randMapIdx = Random.Range(0, availableMapList.Count - 1);

            Vector3Int arrPosition = new Vector3Int(availableMapList[randMapIdx].array_Position.x, availableMapList[randMapIdx].array_Position.y, 0);
            MakeMapArray(arrPosition);
        }
        FindMapDistanceQueue(startMapPosition);

        SortMapList(validMapList);

        SettingStage();

        MapManager.Instance.SetupStart(direction4, maps);

        //SettingStage();


        //FindMapDistance(startMapPosition, startMapPosition);

        //// 특수방 BOSS 방 생성
        //AddBossMap();

    }

    //public void SettingStage()        // 다른 함수랑 통합됨.
    //{
    //    List<Map> stageList = maps.ToList();
    //    int index;

    //    // Start
    //    stageList[0].stage = stageList[0].AddComponent<StartStage>();
    //    stageList[0].State = Map.StageState.Start;
    //    SetClickStage(stageList[0]);
    //    stageList[0].GetComponentInChildren<TMP_Text>().text = "Start";             // 이미지로 대체할 예정이라 따로 캐싱하지는 않음.
    //    stageList[0].GetComponentInChildren<TMP_Text>().color = Color.gray;
    //    stageList.RemoveAt(0);

    //    // Boss
    //    stageList[^1].stage = stageList[^1].AddComponent<BossStage>();
    //    stageList[^1].State = Map.StageState.Boss;
    //    SetClickStage(stageList[^1]);
    //    stageList[^1].GetComponentInChildren<TMP_Text>().text = "Boss";
    //    stageList[^1].GetComponentInChildren<TMP_Text>().color = Color.red;
    //    stageList.RemoveAt(stageList.Count - 1);
    //    //map.Find(x => x.transform.position == (validMapList[^1].transform_Position * mapDistance)).GetComponent<Image>().color = Color.red;

    //    // Treasure
    //    index = Random.Range(0, stageList.Count);
    //    stageList[index].stage = stageList[index].AddComponent<TreasureStage>();
    //    stageList[index].State = Map.StageState.Treasure;
    //    SetClickStage(stageList[index]);
    //    stageList[index].GetComponentInChildren<TMP_Text>().text = "Treasure";
    //    stageList[index].GetComponentInChildren<TMP_Text>().color = Color.yellow;
    //    //stageList[index].ClearMap();
    //    stageList.RemoveAt(index);

    //    // Shop
    //    index = Random.Range(0, stageList.Count);
    //    stageList[index].stage = stageList[index].AddComponent<ShopStage>();
    //    ShopManager.Instance.SettingCardShop();
    //    stageList[index].State = Map.StageState.Shop;
    //    SetClickStage(stageList[index]);
    //    stageList[index].GetComponentInChildren<TMP_Text>().text = "Shop";
    //    stageList[index].GetComponentInChildren<TMP_Text>().color = Color.blue;
    //    //stageList[index].ClearMap();
    //    stageList.RemoveAt(index);

    //    foreach (Map stage in stageList)
    //    {
    //        int percent = Random.Range(0, 4);
    //        switch (percent)
    //        {
    //            case 0:
    //            case 1:
    //            case 2:
    //                stage.stage = stage.AddComponent<EnemyStage>();
    //                stage.State = Map.StageState.Enemy;
    //                SetClickStage(stage);
    //                stage.GetComponentInChildren<TMP_Text>().text = "Enemy";
    //                break;

    //            case 3:
    //                stage.stage = stage.AddComponent<EventStage>();
    //                stage.State = Map.StageState.Event;
    //                SetClickStage(stage);
    //                stage.GetComponentInChildren<TMP_Text>().text = "Event";
    //                stage.GetComponentInChildren<TMP_Text>().color = Color.cyan;
    //                break;
    //        }
    //    }


    //}

    void SetClickStage(Map stage)
    {
        stage.stageContext = new StageContext(stage);

        stage.btn.onClick.AddListener(() =>
        {
            if (!MapManager.Instance.canMove)
                return;
            else if (!stage.cleared)
            {
                stage.LookingStage(direction4, maps);
                if (stage.State == Map.StageState.Treasure || stage.State == Map.StageState.Shop)
                {
                    MapManager.Instance.ClearStage(stage).Forget();
                }
                else
                {
                    MapManager.Instance.canMove = false;
                }
            }

            // 떠나려는 방에 보상이 떴는데, 그 보상을 받지 않고 떠난다면, 잠시 해당 스테이지 보상을 숨김. 
            if (MapManager.Instance.currStage.rewardBox != -1 && !MapManager.Instance.currStage.rewarded)
            {
                //MapManager.Instance.rewardCanvas.GetChild(MapManager.Instance.currStage.rewardBox).gameObject.SetActive(false);
                UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, false, MapManager.Instance.currStage.rewardBox);
            }

            // 보상과 상관없이 EnlargePanel와 RewardCanvas는 새로운 방에 들어갈 때마다 숨김 처리.
            UIManager.Instance.MoveMap();

            MapManager.Instance.currStage = stage;

            if (_mark != null)
            {
                _mark.transform.position = stage.transform.position + _markDefaultPos;
            }
            
            // 방 입장 코드 추가
            stage.stageContext.Transition(stage.stage);

            // 들어간 방에 보상이 떴었는데, 예전에 보상을 받지 않았다면, 그 보상을 다시 시각화함.
            if (stage.rewardBox != -1 && !stage.rewarded)
            {
                //MapManager.Instance.rewardCanvas.GetChild(stage.rewardBox).gameObject.SetActive(true);
                UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.RewardBox, true, stage.rewardBox);
                if (stage.rewardBox != 0)
                {
                    UIManager.Instance.ShowRewardCard(stage.CardReward);
                }
            }


            // 이동 모션 코드 추가

            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.Map, false);
        });
    }

    public MapInfo AddSingleMap(MapInfo map, Vector3Int pos, string name)
    {
        //MapInfo single = Map;
        map.mapID = name + "(" + pos.x + ", " + pos.y + ", " + pos.z + ")";
        map.mapName = name;
        map.array_Position = pos;
        map.transform_Position = pos * mapDistance - startMapPosition * mapDistance/* + new Vector3Int(Screen.width/2, Screen.height/2)*/;
        //single.parent_Position = pos;
        map.mapType = "Single";
        //map.isValidMap = true;
        map.isCheck = false;

        validMapList.Add(map);
        availableMapList.Add(map);

        return map;
    }

    // 시작 방에서 해당 방까지의 거리 계산
    //public void FindMapDistance(Vector3Int currentPos, Vector3Int prePos)
    //{
    //    // currentPos = 현재 위치
    //    // prePos     = 이전 위치
    //    print($"{currentPos.x}, {currentPos.y}");
    //    if (!PossibleArr(currentPos))
    //        return;

    //    int _distance = posArr[currentPos.x, currentPos.y].distance;


    //    for (int i = 0; i < direction4.Count; i++)
    //    {
    //        print("SS");
    //        Vector3Int adjustPosition = currentPos + direction4[i];

    //        if (PossibleArr(adjustPosition) && adjustPosition != prePos)
    //        {
    //            // 새로운 위치가 활성화가 되었을 경우
    //            if (posArr[adjustPosition.x, adjustPosition.y] != null)
    //            {
    //                // 새로운 위치가 탐색했던 곳일 경우
    //                if (posArr[adjustPosition.x, adjustPosition.y].distance != -1)
    //                {
    //                    if ((_distance + 1) <= posArr[adjustPosition.x, adjustPosition.y].distance)
    //                    {
    //                        posArr[adjustPosition.x, adjustPosition.y].distance = _distance + 1;
    //                        FindMapDistance(adjustPosition, currentPos);
    //                    }
    //                }// 새로운 위치가 탐색하지 않은 곳일 경우
    //                else
    //                {
    //                    posArr[adjustPosition.x, adjustPosition.y].distance = _distance + 1;
    //                    FindMapDistance(adjustPosition, currentPos);
    //                }
    //            }
    //        }
    //    }
    //}

    //더 나은 알고리즘
    public void FindMapDistanceQueue(Vector3Int currentPos)
    {
        int _distance = 0;
        queue.Enqueue(currentPos);
        posArr[currentPos.x, currentPos.y].isCheck = true;
        posArr[currentPos.x, currentPos.y].distance = _distance;

        while (queue.Count != 0)
        {
            Vector3Int node = queue.Dequeue();
            foreach (Vector3Int movVec in direction4)
            {
                Vector3Int adjustPosition = node + movVec;
                if (PossibleArr(adjustPosition) && posArr[adjustPosition.x, adjustPosition.y] != null && !posArr[adjustPosition.x, adjustPosition.y].isCheck)
                {
                    posArr[adjustPosition.x, adjustPosition.y].isCheck = true;
                    posArr[adjustPosition.x, adjustPosition.y].distance = posArr[node.x, node.y].distance + 1;
                    queue.Enqueue(adjustPosition);
                }
            }
        }
    }

    public void SortMapList(List<MapInfo> root)
    {
        root.Sort(delegate (MapInfo A, MapInfo B)
        {
            if (A.distance > B.distance)
                return 1;
            else if (A.distance < B.distance)
                return -1;
            else
                return 0;
        });
    }

    public bool PossibleArr(Vector3Int pos)
    {
        if ((0 <= (pos).x && (pos).x < (maxDistance * 2 + 1))
            && (0 <= (pos).y && (pos).y < (maxDistance * 2 + 1)))
        {
            return true;
        }
        else
            return false;
    }

    //public void AddBossMap()
    //{
    //    SortMapList(validMapList);

    //    bool selectBossMapStatus = false;

    //    for (int idx = validMapList.Count - 1; 0 < idx; idx--)
    //    {
    //        if (!selectBossMapStatus)
    //        {
    //            int setLIstCnt = idx;
    //            Vector3Int pos = validMapList[setLIstCnt].array_Position;

    //            for (int i = 0; i < direction4.Count; i++)
    //            {
    //                selectBossMapStatus = false;
    //                Vector3Int bossMapPos = posArr[pos.z, pos.x].array_Position + direction4[i];

    //                if (PossibleArr(bossMapPos))
    //                {
    //                    if ((AroundMapCount(bossMapPos) < 2)
    //                        && !posArr[bossMapPos.z, bossMapPos.x].isValidMap)
    //                    {
    //                        posArr[bossMapPos.z, bossMapPos.x].mapName = "Boss";
    //                        posArr[bossMapPos.z, bossMapPos.x].isValidMap = true;
    //                        posArr[bossMapPos.z, bossMapPos.x].array_Position = bossMapPos;
    //                        //posArr[bossMapPos.z, bossMapPos.x].parent_Position = bossMapPos;
    //                        //posArr[bossMapPos.z, bossMapPos.x].mergeCenter_Position = bossMapPos;
    //                        posArr[bossMapPos.z, bossMapPos.x].distance = posArr[pos.z, pos.x].distance + 1;
    //                        posArr[bossMapPos.z, bossMapPos.x].mapType = "Single";

    //                        bossMapPosition = bossMapPos;
    //                        selectBossMapStatus = true;

    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //// 방의 배열을 초기화
    //public void RealaseMapPos()
    //{
    //    for (int i = 0; i < (maxDistance * 2 + 1); i++)
    //    {
    //        for (int j = 0; j < (maxDistance * 2 + 1); j++)
    //        {
    //            posArr[j, i] = new MapInfo();
    //            posArr[j, i].isValidMap = false;
    //            posArr[j, i].distance = -1;
    //        }
    //    }
    //}
    // 모든 변수를 초기화
    //public void RealaseMap()
    //{
    //    for (int i = 0; i < (maxDistance * 2 + 1); i++)
    //    {
    //        for (int j = 0; j < (maxDistance * 2 + 1); j++)
    //        {
    //            posArr[j, i] = new MapInfo();
    //            posArr[j, i].isValidMap = false;
    //            posArr[j, i].distance = -1;
    //        }
    //    }
    //    //validMapList.Clear();

    //    //currMapCnt = 0;
    //}

    // 배열의 방들을 MapController의 List로 변환
    public void SettingStage()
    {
        //List<MapInfo> MapsList = new List<MapInfo>();

        //for (int i = 0; i < (maxDistance * 2 + 1); i++)
        //{
        //    for (int j = 0; j < (maxDistance * 2 + 1); j++)
        //    {
        //        if (posArr[j, i].isValidMap)
        //        {
        //            Vector3Int tmpArrayPosition = new Vector3Int(i, 0, j);

        //            posArr[j, i] = SingleMap(posArr[j, i], posArr[j, i].mapName);
        //            posArr[j, i].array_Position = tmpArrayPosition - startMapPosition;
        //            //posArr[j, i].parent_Position = posArr[j, i].parent_Position - startMapPosition;
        //            //posArr[j, i].mergeCenter_Position = posArr[j, i].mergeCenter_Position - startMapPosition;

        //            MapsList.Add(posArr[j, i]);
        //        }
        //    }
        //}
        //validMapCount = validMapList.Count;

        Transform mapCanvas = UIManager.Instance.Canvas(UIManager.CanvasName.Map);

        int treasureIdx = Random.Range(1, validMapList.Count-1);
        int shopIdx = Random.Range(1, validMapList.Count-1);
        while (treasureIdx == shopIdx)
        {
            shopIdx = Random.Range(1, validMapList.Count - 1);
        }

        for (int i = 0; i < validMapList.Count; ++i)
        {
            int mapIdx;
            if (i == 0)
                mapIdx = 0; // Start
            else if (i == validMapList.Count - 1)
                mapIdx = 5; // Boss
            else if (i == treasureIdx)
                mapIdx = 1; // Treasure
            else if (i == shopIdx)
                mapIdx = 2; // Shop
            else
            {
                int percent = Random.Range(0, 4);
                if (percent == 0)
                    mapIdx = 3; // Event
                else
                    mapIdx = 4; // Enemy
            }
            MapInfo validMap = validMapList[i];
            GameObject mapObject = Instantiate(_mapPrefab[mapIdx], mapCanvas);/*PoolManager.Instance.MapPool.Get();*/
            Map map = mapObject.GetComponent<Map>();
            //mapObject.transform.GetComponentInChildren<TextMeshProUGUI>().text = validMap.distance.ToString();
            mapObject.transform.localPosition = validMap.transform_Position;

            //map.img.color = Color.black;
            map.btn.interactable = false;
            map.array_Position = validMap.array_Position;

            SetClickStage(map);

            switch (mapIdx)
            {
                case 0:
                    map.stage = map.GetComponent<StartStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Start";
                    //map.GetComponentInChildren<TMP_Text>().color = Color.gray;
                    break;
                case 1:
                    map.stage = map.GetComponent<TreasureStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Treasure";
                    //map.GetComponentInChildren<TMP_Text>().color = Color.yellow;
                    break;
                case 2:
                    map.stage = map.GetComponent<ShopStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Shop";
                    //map.GetComponentInChildren<TMP_Text>().color = Color.blue;
                    break;
                case 3:
                    map.stage = map.GetComponent<EventStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Event";
                    //map.GetComponentInChildren<TMP_Text>().color = Color.cyan;
                    break;
                case 4:
                    map.stage = map.GetComponent<EnemyStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Enemy";
                    break;
                case 5:
                    map.stage = map.GetComponent<BossStage>();
                    //map.GetComponentInChildren<TMP_Text>().text = "Boss";
                    //map.GetComponentInChildren<TMP_Text>().color = Color.red;
                    break;
            }

            maps.Add(map);
            mapObject.gameObject.SetActive(false);
        }
        if (_markPrefab != null)
        {
            _mark = Instantiate(_markPrefab, mapCanvas);
            _markDefaultPos = _mark.transform.localPosition;
        }
        //foreach (Map map in maps)
        //{
        //    map.btn.onClick.AddListener(() =>
        //    {
        //        if (!canMove)
        //            return;
        //        else if (map.cleared)
        //        {
        //            currStage = map;
        //            UIManager.Instance.LookMap();
        //            return;
        //        }
        //        canMove = false;
        //        map.LookingStage(direction4, maps);

        //        currStage = map;

        //        // 이동 모션 코드 추가
        //        // 방 입장 코드 추가

        //        UIManager.Instance.LookMap();
        //    });
        //}

        //foreach (Map mapObject in maps)
        //    mapObject.gameObject.SetActive(false); /*MapRelease();*/
        
        //foreach (Vector3Int direction in direction4)
        //{
        //    Map connectMap = maps.Find(x => x.array_Position == maps[0].array_Position + direction);
        //    if (connectMap != null)
        //    {
        //        connectMap.gameObject.SetActive(true);
        //        connectMap.img.color = Color.white;
        //        connectMap.btn.interactable = true;
        //    }
        //}
        

        //for (int i = 0; i < validMapList.Count; i++)
        //{
        //    map[i].SetActive(true);
        //    map[i].transform.GetChild(0).GetComponent<TextMeshPro>().text = validMapList[i].distance.ToString();
        //    Vector3 mapPos;
        //    mapPos = validMapList[i].array_Position - startMapPosition;
        //    //map[i].transform.position = new Vector3(mapPos.x, mapPos.z, 0);
        //    map[i].transform.position = mapPos;
        //}

    }
    //void SetupVisited()
    //{
    //    // 시작 장소 활성화 코드 5줄
    //    InGameManager.Instance.currStage = maps[0];
    //    InGameManager.Instance.currStage.gameObject.SetActive(true);
    //    //currStage.img.color = Color.white;
    //    InGameManager.Instance.currStage.btn.interactable = true;
    //    InGameManager.Instance.currStage.LookingStage(direction4, maps);
    //    InGameManager.Instance.ClearStage();
    //}
    //public void AddMapLIst()
    //{
    //    validMapList.Clear();

    //    for (int i = 0; i < (maxDistance * 2 + 1); i++)
    //    {
    //        for (int j = 0; j < (maxDistance * 2 + 1); j++)
    //        {
    //            if (posArr[j, i].isValidMap)
    //            {
    //                validMapList.Add(posArr[j, i]);
    //            }
    //        }
    //    }
    //}

    public MapInfo SingleMap(MapInfo pos, string name)
    {
        MapInfo single = pos;
        single.mapID = name + "(" + pos.array_Position.x + ", " + pos.array_Position.y + ", " + pos.array_Position.z + ")";
        single.mapName = name;
        single.array_Position = pos.array_Position;
        single.transform_Position = pos.array_Position - startMapPosition/* + new Vector3Int(Screen.width/2, Screen.height/2)*/;
        //single.mergeCenter_Position = pos.mergeCenter_Position;
        single.mapType = pos.mapType;
        single.distance = pos.distance;

        return single;
    }

    public bool PossiblePattern(Vector3Int pos, Vector3Int move)
    {

        Vector3Int next = pos + move;

        //if (!PossibleArr(next))
        //    return false;

        //posArr[next.z, next.x] = new MapInfo();
        //return true;

        if (PossibleArr(next))
        {
            if (posArr[next.x, next.y] != null)
            {
                posArr[pos.x, pos.y].haveDirect.Remove(move);
                posArr[next.x, next.y].haveDirect.Remove(-move);
                if (posArr[pos.x, pos.y].haveDirect.Count == 0)
                    availableMapList.Remove(posArr[pos.x, pos.y]);
                if (posArr[next.x, next.y].haveDirect.Count == 0)
                    availableMapList.Remove(posArr[next.x, next.y]);
                return false;
            }
        }
        else
        {
            posArr[pos.x, pos.y].haveDirect.Remove(move);
            if (posArr[pos.x, pos.y].haveDirect.Count == 0)
                availableMapList.Remove(posArr[pos.x, pos.y]);
            return false;
        }
        //posArr[next.x, next.y] = new MapInfo();
        return true;
    }

    //public int AroundMapCount(Vector3Int pos)
    //{
    //    int Count = 0;

    //    // LEFT
    //    if ((0 <= (pos.x - 1) && (pos.x - 1) < (maxDistance * 2 + 1)))
    //    {
    //        if (posArr[pos.z, pos.x - 1].isValidMap)
    //        {
    //            Count += 1;
    //        }
    //    }

    //    // RIGHT
    //    if ((0 <= (pos.x + 1) && (pos.x + 1) < (maxDistance * 2 + 1)))
    //    {
    //        if (posArr[pos.z, pos.x + 1].isValidMap)
    //        {
    //            Count += 1;
    //        }
    //    }

    //    // TOP
    //    if ((0 <= (pos.z - 1) && (pos.z - 1) < (maxDistance * 2 + 1)))
    //    {
    //        if (posArr[pos.z - 1, pos.x].isValidMap)
    //        {
    //            Count += 1;
    //        }
    //    }
    //    // DOWN
    //    if ((0 <= (pos.z + 1) && (pos.z + 1) < (maxDistance * 2 + 1)))
    //    {
    //        if (posArr[pos.z + 1, pos.x].isValidMap)
    //        {
    //            Count += 1;
    //        }
    //    }

    //    return Count;
    //}



    // Map 위치 및 방의 크키 지정
    public void MakeMapArray(Vector3Int start)
    {
        //if (start.x >= (maxDistance * 2 + 1) || start.z >= (maxDistance * 2 + 1))
        //    return;
        //Vector3Int direction = direction4[Random.Range(0, direction4.Count)];
        Vector3Int direction = posArr[start.x, start.y].haveDirect[Random.Range(0, posArr[start.x, start.y].haveDirect.Count)];

        if (!PossiblePattern(start, direction))
            return;

        //Vector3Int lastMove;
        //Vector3 currCenterPos;
        //Vector3Int startPosition = direction;
        //Vector3Int otherPosition = direction;

        //currCenterPos = new Vector3((float)(startPosition.x + otherPosition.x) / 2, 0, (float)(startPosition.z + otherPosition.z) / 2);

        Vector3Int move = start + direction;

        posArr[move.x, move.y] = AddSingleMap(new MapInfo(), move, "Single");
        posArr[start.x, start.y].haveDirect.Remove(direction);

        //posArr[move.x, move.y].isValidMap = true;
        //posArr[move.x, move.y].mapID = $"map ({move.x}, {move.y})";
        //posArr[move.x, move.y].mapName = "Room";
        //posArr[move.x, move.y].mapType = "Single";
        //posArr[move.x, move.y].array_Position = start + direction;
        //posArr[move.x, move.y].isCheck = false;

        //posArr[move.z, move.x].parent_Position = start + direction;
        //posArr[move.z, move.x].mergeCenter_Position = start + currCenterPos;
        posArr[move.x, move.y].haveDirect.Remove(-direction);

        if (posArr[start.x, start.y].haveDirect.Count == 0)
            availableMapList.Remove(posArr[start.x, start.y]);
        //lastMove = move;

        // 방의 갯수 증가
        //currMapCnt++;
        //MakeMapArray(lastMove);
    }

    //public void ConnectMapCheck(Vector3Int move)
    //{
    //    List<Vector3Int> moveConnect = posArr[move.z, move.x].haveDirect;
    //    for (int i = moveConnect.Count - 1; i >= 0; i--)
    //    {
    //        Vector3Int connectMap = move + moveConnect[i];
    //        if (!PossiblePattern(move, moveConnect[i]))
    //            continue;
    //        if (posArr[connectMap.z, connectMap.x] != null)
    //        {
    //            posArr[connectMap.z, connectMap.x].haveDirect.Remove(-moveConnect[i]);
    //            moveConnect.Remove(moveConnect[i]);
    //        }
    //    }
    //}

    public bool MapCountCheck()
    {
        return (creatMapCnt <= validMapList.Count);
    }

    // 방의 갯수가 최소, 최대크기에 적합한지 체크

    private System.Array ResizeArray(System.Array arr, int[] newSizes)
    {
        if (newSizes.Length != arr.Rank)
            return null;

        var temp = System.Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
        int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
        System.Array.ConstrainedCopy(arr, 0, temp, 0, length);
        return temp;
    }
}