//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using static UnityEngine.RuleTile.TilingRuleOutput;

//public class MapController : MonoBehaviour
//{
//    public static MapController Instance { get; private set; }

//    public string globalMapTitle = "Basement";

//    public MapInfo currentLoadMapData;
//    public Map currStage;

//    public List<Map> loadedMaps = new List<Map>();

//    public Material DefaultBackground;
//    public Material VisitedBack;
//    public Material currMaterial;


//    public bool isLoadingMap = false;
//    void Awake() => Instance = this;

//    public void CreatedMap()
//    {
//        isLoadingMap = false;

//        for (int i = 0; i < transform.childCount; i++)
//            Destroy(transform.GetChild(i).gameObject);

//        loadedMaps.Clear();

//        SettingMap.Instance.CreatedMap();
//        SetMapPath();


//    }

//    void SetMapPath()
//    {
//        if (isLoadingMap)
//            return;

//        if (loadedMaps.Count > 0)
//        {
//            foreach (Map room in loadedMaps)
//            {
//                room.RemoveUnconnectedWalls();
//            }
//            isLoadingMap = true;
//        }
//    }

//    public void LoadMap(MapInfo settingMap)
//    {
//        if (DoesMapExist(settingMap.array_Position.x, settingMap.array_Position.y, settingMap.array_Position.z))
//        {
//            return;
//        }

//        string roomPreName = settingMap.mapName;

//        GameObject room = Instantiate(MapPrefabsSet.Instance.roomPrefabs[roomPreName]);

//        room.transform.position = new Vector3(
//                    (settingMap.array_Position.x * room.transform.GetComponent<Map>().Width),
//                     settingMap.array_Position.y,
//                    (settingMap.array_Position.z * room.transform.GetComponent<Map>().Height)
//        );

//        room.transform.localScale = new Vector3(
//                    (room.transform.GetComponent<Map>().Width / 10),
//                     1,
//                    (room.transform.GetComponent<Map>().Height / 10)
//        );
//        room.transform.GetComponent<Map>().array_Position = settingMap.array_Position;
//        room.Name = globalMapTitle + "-" + settingMap.roomName + " " + settingMap.array_Position.x + ", " + settingMap.array_Position.z;

//        room.transform.GetComponent<Map>().roomName = settingMap.roomName;
//        room.transform.GetComponent<Map>().roomType = settingMap.roomType;
//        room.transform.GetComponent<Map>().roomId = settingMap.roomID;
//        room.transform.GetComponent<Map>().parent_Position = settingMap.parent_Position;
//        room.transform.GetComponent<Map>().mergeCenter_Position = settingMap.mergeCenter_Position;
//        room.transform.GetComponent<Map>().distance = settingMap.distance;

//        room.transform.parent = transform;

//        loadedMaps.Add(room.GetComponent<Map>());
//    }

//    // 빈 데이터 혹은 삭제된 방이 있을 경우를 위한 예외처리
//    public bool DoesMapExist(int x, int y, int z)
//    {
//        return loadedMaps.Find(item => item.array_Position.x == x && item.array_Position.y == y && item.array_Position.z == z) != null;
//    }

//    //    
//    public Map FindMap(int x, int y, int z)
//    {
//        // List.Find : item 변수 조건에 맞는 Map을 찾아 반환
//        return loadedMaps.Find(item => item.array_Position.x == x && item.array_Position.y == y && item.array_Position.z == z);
//    }

//    // 해당 Map에서 Player가 있는 방을 반환
//    public void OnPlayerEnterMap(Map room)
//    {
//        CameraFollow.Instance.currStage = room;

//        currStage = room;

//        for (int i = 0; i < loadedMaps.Count; i++)
//        {
//            if (room.parent_Position == loadedMaps[i].parent_Position)
//                loadedMaps[i].childMaps.minimapUpdate();
//        }
//    }

//}