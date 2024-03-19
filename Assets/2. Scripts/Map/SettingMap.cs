using System.Collections;
using System.Collections.Generic;
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
    public List<Vector3Int> direction8 = new List<Vector3Int>
    {   // Down                           // Up
        new Vector3Int( 0, 0,  1),        new Vector3Int( 0, 0, -1),        
        // Left                           // Right
        new Vector3Int(-1, 0,  0),        new Vector3Int( 1, 0,  0),        
        // UpLeft                         // UpRight
        new Vector3Int(-1, 0, -1),        new Vector3Int( 1, 0, -1),
        // DiwbLeft                       // DownRight
        new Vector3Int(-1, 0,  1),        new Vector3Int( 1, 0,  1)
    };
    public Dictionary<int, List<Vector3Int>> downPatten = new Dictionary<int, List<Vector3Int>>
    {
        {  0, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(-1, 0, 1),   new Vector3Int(0, 0, 2),   new Vector3Int(-1, 0, 2) } }, // ㅁ
        {  1, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(0, 0, 2),    new Vector3Int(-1, 0, 2)    } }, // ┓
        {  2, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(0, 0, 2),    new Vector3Int(1, 0, 2)     } }, // ┏
        {  3, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(0, 0, 2)                                 } }, // 아래 |
        {  4, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(-1, 0, 1)                                } }, // 아래 |
        {  5, new List<Vector3Int>      { new Vector3Int(0, 0, 1),   new Vector3Int(1, 0, 1)                                 } }, // 아래 |
        {  6, new List<Vector3Int>      { new Vector3Int(0, 0, 1)                                                            } }, // 아래 |
    };
    public Dictionary<int, List<Vector3Int>> upPatten = new Dictionary<int, List<Vector3Int>>
    {
        {  0, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(0, 0, -2),  new Vector3Int(-1, 0, -1),  new Vector3Int(-1, 0, -2)} }, // ㅁ
        {  1, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(0, 0, -2),   new Vector3Int(1, 0, -2)    } }, // ┏
        {  2, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(0, 0, -2),   new Vector3Int(-1, 0, -2)   } }, // ┐
        {  3, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(0, 0, -2)                                } }, // 위 |
        {  4, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(1, 0, -1)                                } }, // 위 |
        {  5, new List<Vector3Int>      { new Vector3Int(0, 0, -1),   new Vector3Int(-1, 0, -1)                               } }, // 위 |
        {  6, new List<Vector3Int>      { new Vector3Int(0, 0, -1)                                                            } }, // 위 |
    };
    public Dictionary<int, List<Vector3Int>> leftPatten = new Dictionary<int, List<Vector3Int>>
    {
        {  0, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-2, 0, 0),   new Vector3Int(-1, 0, -1),  new Vector3Int(-2, 0, -1) } }, // ㅁ
        {  1, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-2, 0, 0),   new Vector3Int(-2, 0, -1)   } }, // └ 
        {  2, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-2, 0, 0),   new Vector3Int(-2, 0, 1)    } }, // ┌
        {  3, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-2, 0, 0)                                } }, // 왼쪽  --
        {  4, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-1, 0, -1)                               } }, // 왼쪽  --
        {  5, new List<Vector3Int>      { new Vector3Int(-1, 0, 0),  new Vector3Int(-1, 0, 1)                                } }, // 왼쪽  --
        {  6, new List<Vector3Int>      { new Vector3Int(-1, 0, 0)                                                           } }, // 왼쪽  --
    };
    public Dictionary<int, List<Vector3Int>> rightPatten = new Dictionary<int, List<Vector3Int>>
    {
        {  0, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(2, 0, 0),    new Vector3Int(1, 0, 1) ,   new Vector3Int(2, 0, 1) } }, // ㅁ
        {  1, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(2, 0, 0),    new Vector3Int(2, 0, 1)     } }, // ┐
        {  2, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(2, 0, 0),    new Vector3Int(2, 0, -1)    } }, // ┛ 
        {  3, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(2, 0, 0)                                 } }, // 오른쪽  --
        {  4, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(1, 0, 1)                                 } }, // 오른쪽  --
        {  5, new List<Vector3Int>      { new Vector3Int(1, 0, 0),   new Vector3Int(1, 0, -1)                                } }, // 오른쪽  --
        {  6, new List<Vector3Int>      { new Vector3Int(1, 0, 0)                                                            } },
    };

    public List<MapInfo> validMapList = new List<MapInfo>();

    public int minMapCnt;                       // 최소 방 갯수
    public int maxMapCnt;                       // 최대 방 갯수
    public int currMapCnt;                        // 현재 방 갯수
    public int maxDistance;                        // 최대 거리 제한

    public int validMapCount;

    public Vector3Int startMapPosition;                        // 시작 포지션
    public Vector3Int bossMapPosition;                         // 보스 방 포지션

    public MapInfo[,] posArr = new MapInfo[10,10];       // 방 좌표에 대한 2차원 배열

    public List<GameObject> map;

    private void Start()
    {
        CreatedMap();
    }

    public void CreatedMap()
    {
        // 배열 ReSize
        posArr = (MapInfo[,])ResizeArray(posArr, new int[] { maxDistance * 2 + 1, maxDistance * 2 + 1 });

        RealaseMap();  // 초기화

        //int x = Random.Range(0, maxDistance) + (int)(maxDistance / 2);  // 최대 크기 X 좌표
        //int z = Random.Range(0, maxDistance) + (int)(maxDistance / 2);  // 최대 크기 Y 좌표

        startMapPosition = new Vector3Int(maxDistance, 0, maxDistance);                        // 시작 좌표

        posArr[startMapPosition.z, startMapPosition.x] = AddSingleMap(new MapInfo(), startMapPosition, "Single");
        posArr[startMapPosition.z, startMapPosition.x].distance = 0;
        currMapCnt++;

        while (true)
        {
            if (!(minMapCnt <= currMapCnt && currMapCnt <= maxMapCnt))
            {
                FindMapDistance(startMapPosition, startMapPosition);

                // 
                AddMapLIst();

                int randMapIdx = Random.Range(0, validMapList.Count - 1);

                Vector3Int position = new Vector3Int(validMapList[randMapIdx].center_Position.x, 0, validMapList[randMapIdx].center_Position.z);
                MakeMapArray(position);
            }
            else
                break;
        }
        FindMapDistance(startMapPosition, startMapPosition);

        AddMapLIst();
        SortMapList(validMapList);

        // 특수방 BOSS 방 생성
        AddBossMap();

        SetupPosition();
    }



    public MapInfo AddSingleMap(MapInfo Map, Vector3Int pos, string name)
    {
        MapInfo single = Map;
        single.mapID = name + "(" + pos.x + ", " + pos.y + ", " + pos.z + ")";
        single.mapName = name;
        single.center_Position = pos;
        single.parent_Position = pos;
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

        int _distance = posArr[currentPos.z, currentPos.x].distance;

        for (int i = 0; i < direction4.Count; i++)
        {
            Vector3Int adjustPosition = currentPos + direction4[i];

            if (PossibleArr(adjustPosition) && adjustPosition != prePos)
            {
                // 새로운 위치가 활성화가 되었을 경우
                if (posArr[adjustPosition.z, adjustPosition.x].isValidMap)
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
                            posArr[bossMapPos.z, bossMapPos.x].parent_Position = bossMapPos;
                            posArr[bossMapPos.z, bossMapPos.x].mergeCenter_Position = bossMapPos;
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

    // 방의 배열을 초기화
    public void RealaseMapPos()
    {
        for (int i = 0; i < (maxDistance * 2 + 1); i++)
        {
            for (int j = 0; j < (maxDistance * 2 + 1); j++)
            {
                posArr[j, i] = new MapInfo();
                posArr[j, i].isValidMap = false;
                posArr[j, i].distance = -1;
            }
        }
    }
    // 모든 변수를 초기화
    public void RealaseMap()
    {
        RealaseMapPos();
        validMapList.Clear();

        currMapCnt = 0;
    }

    // 배열의 방들을 MapController의 List로 변환
    public void SetupPosition()
    {
        List<MapInfo> MapsList = new List<MapInfo>();

        for (int i = 0; i < (maxDistance * 2 + 1); i++)
        {
            for (int j = 0; j < (maxDistance * 2 + 1); j++)
            {
                if (posArr[j, i].isValidMap)
                {
                    Vector3Int tmpArrayPosition = new Vector3Int(i, 0, j);

                    posArr[j, i] = SingleMap(posArr[j, i], posArr[j, i].mapName);
                    posArr[j, i].center_Position = tmpArrayPosition - startMapPosition;
                    posArr[j, i].parent_Position = posArr[j, i].parent_Position - startMapPosition;
                    posArr[j, i].mergeCenter_Position = posArr[j, i].mergeCenter_Position - startMapPosition;

                    MapsList.Add(posArr[j, i]);
                }
            }
        }
        validMapCount = validMapList.Count;


        foreach (GameObject mapObject in map)
            mapObject.SetActive(false);

        for (int i = 0; i < validMapCount; i++)
        {
            map[i].SetActive(true);
            Vector3 mapPos;
            mapPos = validMapList[i].center_Position;
            map[i].transform.position = new Vector3(mapPos.x, mapPos.z, 0);
        }

    }
    public void AddMapLIst()
    {
        validMapList.Clear();

        for (int i = 0; i < (maxDistance * 2 + 1); i++)
        {
            for (int j = 0; j < (maxDistance * 2 + 1); j++)
            {
                if (posArr[j, i].isValidMap)
                {
                    validMapList.Add(posArr[j, i]);
                }
            }
        }
    }

    public MapInfo SingleMap(MapInfo pos, string name)
    {
        MapInfo single = pos;
        single.mapID = name + "(" + pos.center_Position.x + ", " + pos.center_Position.y + ", " + pos.center_Position.z + ")";
        single.mapName = name;
        single.center_Position = pos.center_Position;
        single.mergeCenter_Position = pos.mergeCenter_Position;
        single.mapType = pos.mapType;
        single.distance = pos.distance;

        return single;
    }

    public bool PossiblePatten(Vector3Int pos, List<Vector3Int> move)
    {
        bool possible = true;
        for (int i = 0; i < move.Count; i++)
        {
            Vector3Int next = pos + move[i];

            if (PossibleArr(next))
            {
                if (posArr[next.z, next.x].isValidMap)
                    return false;
            }
            else
                return false;

        }

        return possible;
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
        if (start.x >= (maxDistance * 2 + 1) || start.z >= (maxDistance * 2 + 1))
            return;

        if ((minMapCnt <= currMapCnt && currMapCnt <= maxMapCnt))
            return;

        // 사각형 방, 기억자, 니은자, -, |
        double[] persent = { 0.01, 0.03, 0.03, 0.1, 0.1, 0.1, 1000.8 };

        // 각 방향 패턴을 List화
        List<Dictionary<int, List<Vector3Int>>> collectPatten
            = new List<Dictionary<int, List<Vector3Int>>> { downPatten, rightPatten, leftPatten, upPatten };


        for (int direction = 0; direction < direction4.Count; direction++)
        {
            bool directionsRand = (Random.value > 0.5f);

            if (directionsRand)
            {

                int selectPatten = (int)Choose(persent);
                print(selectPatten);

                if (!PossiblePatten(start, collectPatten[direction][selectPatten]))
                    return;

                if (!MapCountCheck())
                {
                    Vector3Int lastMove = start;
                    Vector3 currCenterPos = Vector3.zero;
                    int maxCount = collectPatten[direction][selectPatten].Count;
                    for (int i = 0; i < maxCount; i++)
                    {
                        Vector3Int startPosition = collectPatten[direction][selectPatten][0];
                        Vector3Int otherPosition = collectPatten[direction][selectPatten][maxCount - 1];

                        currCenterPos = new Vector3((float)(startPosition.x + otherPosition.x) / 2, 0, (float)(startPosition.z + otherPosition.z) / 2);
                        Vector3Int move = start + collectPatten[direction][selectPatten][i];

                        posArr[move.z, move.x].isValidMap = true;
                        posArr[move.z, move.x].mapName = "Single";
                        posArr[move.z, move.x].center_Position = start + collectPatten[direction][selectPatten][i];
                        posArr[move.z, move.x].distance = -1;

                        lastMove = move;

                        // 미니맵 아이콘을 띄우기 위한 중앙 지점값 삽입
                        switch (collectPatten[direction][selectPatten].Count)
                        {
                            //case 2:
                            //    posArr[move.z, move.x].mapType = "Double";
                            //    posArr[move.z, move.x].parent_Position = start + collectPatten[direction][selectPatten][0];
                            //    posArr[move.z, move.x].mergeCenter_Position = start + currCenterPos;
                            //    break;
                            //case 3:
                            //    posArr[move.z, move.x].mapType = "Triple";
                            //    posArr[move.z, move.x].parent_Position = start + collectPatten[direction][selectPatten][1];
                            //    posArr[move.z, move.x].mergeCenter_Position = start + collectPatten[direction][selectPatten][1];

                            //    break;
                            //case 4:
                            //    posArr[move.z, move.x].mapType = "Quad";
                            //    posArr[move.z, move.x].parent_Position = start + collectPatten[direction][selectPatten][0];
                            //    posArr[move.z, move.x].mergeCenter_Position = start + currCenterPos;
                            //    break;
                            default:
                                posArr[move.z, move.x].mapType = "Single";
                                posArr[move.z, move.x].parent_Position = start + collectPatten[direction][selectPatten][0];
                                posArr[move.z, move.x].mergeCenter_Position = start + currCenterPos;
                                break;
                        }
                    }
                    // 방의 갯수 증가
                    currMapCnt++;
                    MakeMapArray(lastMove);
                }
            }
        }
    }
    public bool MapCountCheck()
    {
        return ((minMapCnt <= currMapCnt && currMapCnt <= maxMapCnt));
    }

    // 확률을 계산하여 패턴을 구성
    public double Choose(double[] probs)
    {
        double total = 0;

        foreach (double elem in probs)
            total += elem;

        double randomPoint = Random.value * total;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
                return i;
            else
                randomPoint -= probs[i];
        }
        return probs.Length - 1;
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