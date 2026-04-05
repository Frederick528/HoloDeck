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

    // ??? 諛?媛??)?????
    public Vector3Int array_Position;
    public Vector3Int transform_Position;

    //??? ?곗???? ???由ъ???? ??????吏 泥댄?
    public bool isCheck = false;
    public int distance = -1;

}