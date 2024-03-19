using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public List<Vector3Int> haveDirect = new List<Vector3Int>
    {
        new Vector3Int( 0, 0,  1),       // down
        new Vector3Int( 1, 0,  0),       // right
        new Vector3Int(-1, 0,  0),       // left
        new Vector3Int( 0, 0, -1)        // up
    };

    public string mapID;
    public string mapName;
    public string mapType;

    // 현재 방(개별)의 위치
    public Vector3Int center_Position;
    //// 부모 방의 위치
    //public Vector3Int parent_Position;
    //// 해당 방(통합)의 중앙 위치
    //public Vector3 mergeCenter_Position;
    // 해당 방의 상태 설정(true : 방 셋팅, false : 빈방)
    public bool isValidMap;
    // 시작 방에서 부터 해당 방까지의 거리
    public float distance = -1;

}