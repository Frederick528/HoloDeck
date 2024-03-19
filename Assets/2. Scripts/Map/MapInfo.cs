using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public List<Vector3Int> connectDirect = new();

    public string mapID;
    public string mapName;
    public string mapType;

    // 현재 방(개별)의 위치
    public Vector3Int center_Position;
    //// �θ� ���� ��ġ
    //public Vector3Int parent_Position;
    //// �ش� ��(����)�� �߾� ��ġ
    //public Vector3 mergeCenter_Position;
    // �ش� ���� ���� ����(true : �� ����, false : ���)
    public bool isValidMap = false;
    //너비 우선탐색 알고리즘에서 확인했는지 체크
    public bool isCheck = false;
    // ���� �濡�� ���� �ش� ������� �Ÿ�
    public int distance = -1;

}