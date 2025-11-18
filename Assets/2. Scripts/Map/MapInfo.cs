using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public List<Vector3Int> haveDirect = new List<Vector3Int>
    {
        new Vector3Int( 0, 1,  0),       // down
        new Vector3Int( 1, 0,  0),       // right
        new Vector3Int(-1, 0,  0),       // left
        new Vector3Int( 0, -1, 0)        // up
    };

    // 현재 방(개별)의 위치
    public Vector3Int array_Position;
    public Vector3Int transform_Position;

    //너비 우선탐색 알고리즘에서 확인했는지 체크
    public bool isCheck = false;
    public int distance = -1;

}