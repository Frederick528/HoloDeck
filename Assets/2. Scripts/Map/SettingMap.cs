using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SettingMap : MonoBehaviour
{
    public List<Vector3Int> direction4 = new List<Vector3Int>
    {
        new Vector3Int( 0, 0,  1),       // down
        new Vector3Int( 1, 0,  0),       // right
        new Vector3Int(-1, 0,  0),       // left
        new Vector3Int( 0, 0, -1)        // up
    };
    //public Vector3Int downPatten = new Vector3Int(0, 0, 1);
    //public Vector3Int upPatten = new Vector3Int(0, 0, -1);

    //public Vector3Int leftPatten = new Vector3Int(-1, 0, 0);
    //public Vector3Int rightPatten = new Vector3Int(1, 0, 0);

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

    public List<GameObject> map;

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

        startMapPosition = new Vector3Int(maxDistance, 0, maxDistance);                        // 시작 좌표

        posArr[startMapPosition.z, startMapPosition.x] = AddSingleMap(new MapInfo(), startMapPosition, "Single");
        posArr[startMapPosition.z, startMapPosition.x].distance = 0;
        validMapList.Add(posArr[startMapPosition.z, startMapPosition.x]);
        availableMapList.Add(posArr[startMapPosition.z, startMapPosition.x]);

        while (!MapCountCheck())
        {
            int randMapIdx = Random.Range(0, availableMapList.Count - 1);

            Vector3Int position = new Vector3Int(availableMapList[randMapIdx].center_Position.x, 0, availableMapList[randMapIdx].center_Position.z);
            MakeMapArray(position);
        }
        SetupPosition();

        CreateBossMap();
        //FindMapDistance(startMapPosition, startMapPosition);

        SortMapList(validMapList);

        //// 특수방 BOSS 방 생성
        //AddBossMap();

    }

    public void CreateBossMap()
    {
        List<MapInfo> bossMapList = availableMapList.FindAll(x => x.haveDirect.Count == 3);
        MapInfo bossMap = new MapInfo();
        for (int i = bossMapList.Count - 1; i >= 0; i--)
        {
            bossMapList[i].distance = Vector3.Distance(bossMapList[i].center_Position, startMapPosition);
            if (bossMap.distance < bossMapList[i].distance)
                bossMap = bossMapList[i];
        }
        print((bossMap.center_Position - startMapPosition));
        map.Find(x =>
            x.transform.position.x == (bossMap.center_Position - startMapPosition).x
            && x.transform.position.y == (bossMap.center_Position - startMapPosition).z
        ).GetComponent<SpriteRenderer>().color = Color.red;
    }

    public MapInfo AddSingleMap(MapInfo Map, Vector3Int pos, string name)
    {
        MapInfo single = Map;
        single.mapID = name + "(" + pos.x + ", " + pos.y + ", " + pos.z + ")";
        single.mapName = name;
        single.center_Position = pos;
        //single.parent_Position = pos;
        single.mapType = "Single";
        single.isValidMap = true;

        return single;
    }

    // 시작 방에서 해당 방까지의 거리 계산
    public void FindMapDistance(Vector3Int currentPos, Vector3Int prePos)
    {
        // currentPos = 현재 위치
        // prePos     = 이전 위치
        if (!PossibleArr(currentPos))
            return;

        float _distance = posArr[currentPos.z, currentPos.x].distance;

        for (int i = 0; i < direction4.Count; i++)
        {
            Vector3Int adjustPosition = currentPos + direction4[i];

            if (PossibleArr(adjustPosition) && adjustPosition != prePos)
            {
                // 새로운 위치가 활성화가 되었을 경우
                if (posArr[adjustPosition.z, adjustPosition.x] != null)
                {
                    // 새로운 위치가 탐색했던 곳일 경우
                    if (posArr[adjustPosition.z, adjustPosition.x].distance != -1)
                    {
                        if ((_distance + 1) <= posArr[adjustPosition.z, adjustPosition.x].distance)
                        {
                            posArr[adjustPosition.z, adjustPosition.x].distance = _distance + 1;
                            FindMapDistance(adjustPosition, currentPos);
                        }
                    }// 새로운 위치가 탐색하지 않은 곳일 경우
                    else
                    {
                        posArr[adjustPosition.z, adjustPosition.x].distance = _distance + 1;
                        FindMapDistance(adjustPosition, currentPos);
                    }
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
            && (0 <= (pos).z && (pos).z < (maxDistance * 2 + 1)))
        {
            return true;
        }
        else
            return false;
    }

    public void AddBossMap()
    {
        SortMapList(validMapList);

        bool selectBossMapStatus = false;

        for (int idx = validMapList.Count - 1; 0 < idx; idx--)
        {
            if (!selectBossMapStatus)
            {
                int setLIstCnt = idx;
                Vector3Int pos = validMapList[setLIstCnt].center_Position;

                for (int i = 0; i < direction4.Count; i++)
                {
                    selectBossMapStatus = false;
                    Vector3Int bossMapPos = posArr[pos.z, pos.x].center_Position + direction4[i];

                    if (PossibleArr(bossMapPos))
                    {
                        if ((AroundMapCount(bossMapPos) < 2)
                            && !posArr[bossMapPos.z, bossMapPos.x].isValidMap)
                        {
                            posArr[bossMapPos.z, bossMapPos.x].mapName = "Boss";
                            posArr[bossMapPos.z, bossMapPos.x].isValidMap = true;
                            posArr[bossMapPos.z, bossMapPos.x].center_Position = bossMapPos;
                            //posArr[bossMapPos.z, bossMapPos.x].parent_Position = bossMapPos;
                            //posArr[bossMapPos.z, bossMapPos.x].mergeCenter_Position = bossMapPos;
                            posArr[bossMapPos.z, bossMapPos.x].distance = posArr[pos.z, pos.x].distance + 1;
                            posArr[bossMapPos.z, bossMapPos.x].mapType = "Single";

                            bossMapPosition = bossMapPos;
                            selectBossMapStatus = true;

                            break;
                        }
                    }
                }
            }
        }
    }

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
    public void SetupPosition()
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
        //            posArr[j, i].center_Position = tmpArrayPosition - startMapPosition;
        //            //posArr[j, i].parent_Position = posArr[j, i].parent_Position - startMapPosition;
        //            //posArr[j, i].mergeCenter_Position = posArr[j, i].mergeCenter_Position - startMapPosition;

        //            MapsList.Add(posArr[j, i]);
        //        }
        //    }
        //}
        //validMapCount = validMapList.Count;


        foreach (GameObject mapObject in map)
            mapObject.SetActive(false);

        for (int i = 0; i < validMapList.Count; i++)
        {
            map[i].SetActive(true);
            Vector3 mapPos;
            mapPos = validMapList[i].center_Position - startMapPosition;
            map[i].transform.position = new Vector3(mapPos.x, mapPos.z, 0);
        }

    }
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
        single.mapID = name + "(" + pos.center_Position.x + ", " + pos.center_Position.y + ", " + pos.center_Position.z + ")";
        single.mapName = name;
        single.center_Position = pos.center_Position;
        //single.mergeCenter_Position = pos.mergeCenter_Position;
        single.mapType = pos.mapType;
        single.distance = pos.distance;

        return single;
    }

    public bool PossiblePatten(Vector3Int pos, Vector3Int move)
    {

        Vector3Int next = pos + move;

        //if (!PossibleArr(next))
        //    return false;

        //posArr[next.z, next.x] = new MapInfo();
        //return true;

        if (PossibleArr(next))
        {
            if (posArr[next.z, next.x] != null)
            {
                posArr[pos.z, pos.x].haveDirect.Remove(move);
                posArr[next.z, next.x].haveDirect.Remove(-move);
                if (posArr[pos.z, pos.x].haveDirect.Count == 0)
                    availableMapList.Remove(posArr[pos.z, pos.x]);
                if (posArr[next.z, next.x].haveDirect.Count == 0)
                    availableMapList.Remove(posArr[next.z, next.x]);
                return false;
            }
        }
        else
            return false;
        posArr[next.z, next.x] = new MapInfo();
        return true;
    }

    public int AroundMapCount(Vector3Int pos)
    {
        int count = 0;

        // LEFT
        if ((0 <= (pos.x - 1) && (pos.x - 1) < (maxDistance * 2 + 1)))
        {
            if (posArr[pos.z, pos.x - 1].isValidMap)
            {
                count += 1;
            }
        }

        // RIGHT
        if ((0 <= (pos.x + 1) && (pos.x + 1) < (maxDistance * 2 + 1)))
        {
            if (posArr[pos.z, pos.x + 1].isValidMap)
            {
                count += 1;
            }
        }

        // TOP
        if ((0 <= (pos.z - 1) && (pos.z - 1) < (maxDistance * 2 + 1)))
        {
            if (posArr[pos.z - 1, pos.x].isValidMap)
            {
                count += 1;
            }
        }
        // DOWN
        if ((0 <= (pos.z + 1) && (pos.z + 1) < (maxDistance * 2 + 1)))
        {
            if (posArr[pos.z + 1, pos.x].isValidMap)
            {
                count += 1;
            }
        }

        return count;
    }



    // Map 위치 및 방의 크키 지정
    public void MakeMapArray(Vector3Int start)
    {
        //if (start.x >= (maxDistance * 2 + 1) || start.z >= (maxDistance * 2 + 1))
        //    return;

        //Vector3Int direction = direction4[Random.Range(0, direction4.Count)];
        Vector3Int direction = posArr[start.z, start.x].haveDirect[Random.Range(0, posArr[start.z, start.x].haveDirect.Count)];
        print("sss");

        if (!PossiblePatten(start, direction))
            return;

        //Vector3Int lastMove;
        //Vector3 currCenterPos;
        //Vector3Int startPosition = direction;
        //Vector3Int otherPosition = direction;

        //currCenterPos = new Vector3((float)(startPosition.x + otherPosition.x) / 2, 0, (float)(startPosition.z + otherPosition.z) / 2);

        Vector3Int move = start + direction;
        posArr[start.z, start.x].haveDirect.Remove(direction);

        posArr[move.z, move.x].isValidMap = true;
        posArr[move.z, move.x].mapName = "Room";
        posArr[move.z, move.x].mapType = "Single";
        posArr[move.z, move.x].center_Position = start + direction;
        //posArr[move.z, move.x].parent_Position = start + direction;
        //posArr[move.z, move.x].mergeCenter_Position = start + currCenterPos;
        posArr[move.z, move.x].haveDirect.Remove(-direction);
        posArr[move.z, move.x].distance = -1;
        posArr[move.z, move.x] = SingleMap(posArr[move.z, move.x], posArr[move.z, move.x].mapName);

        validMapList.Add(posArr[move.z, move.x]);
        availableMapList.Add(posArr[move.z, move.x]);

        if (posArr[start.z, start.x].haveDirect.Count == 0)
            availableMapList.Remove(posArr[start.z, start.x]);
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
    //        if (!PossiblePatten(move, moveConnect[i]))
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