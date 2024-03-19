//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using static UnityEngine.RuleTile.TilingRuleOutput;

//public class MapController : MonoBehaviour
//{
//    public static MapController instance { get; private set; }

//    public string globalMapTitle = "Basement";

//    public MapInfo currentLoadMapData;
//    public Map currMap;

//    public List<Map> loadedMaps = new List<Map>();

//    public Material DefaultBackground;
//    public Material VisitedBack;
//    public Material currMaterial;


//    public bool isLoadingMap = false;
//    void Awake() => instance = this;

//    public void CreatedMap()
//    {
//        isLoadingMap = false;

//        for (int i = 0; i < transform.childCount; i++)
//            Destroy(transform.GetChild(i).gameObject);

//        loadedMaps.Clear();

//        SettingMap.instance.CreatedMap();
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
//        if (DoesMapExist(settingMap.center_Position.x, settingMap.center_Position.y, settingMap.center_Position.z))
//        {
//            return;
//        }

//        string roomPreName = settingMap.mapName;

//        GameObject room = Instantiate(MapPrefabsSet.Instance.roomPrefabs[roomPreName]);

//        room.transform.position = new Vector3(
//                    (settingMap.center_Position.x * room.transform.GetComponent<Map>().Width),
//                     settingMap.center_Position.y,
//                    (settingMap.center_Position.z * room.transform.GetComponent<Map>().Height)
//        );

//        room.transform.localScale = new Vector3(
//                    (room.transform.GetComponent<Map>().Width / 10),
//                     1,
//                    (room.transform.GetComponent<Map>().Height / 10)
//        );
//        room.transform.GetComponent<Map>().center_Position = settingMap.center_Position;
//        room.name = globalMapTitle + "-" + settingMap.roomName + " " + settingMap.center_Position.x + ", " + settingMap.center_Position.z;

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
//        return loadedMaps.Find(item => item.center_Position.x == x && item.center_Position.y == y && item.center_Position.z == z) != null;
//    }

//    //    
//    public Map FindMap(int x, int y, int z)
//    {
//        // List.Find : item 변수 조건에 맞는 Map을 찾아 반환
//        return loadedMaps.Find(item => item.center_Position.x == x && item.center_Position.y == y && item.center_Position.z == z);
//    }

//    // 해당 Map에서 Player가 있는 방을 반환
//    public void OnPlayerEnterMap(Map room)
//    {
//        CameraFollow.Instance.currMap = room;

//        currMap = room;

//        for (int i = 0; i < loadedMaps.Count; i++)
//        {
//            if (room.parent_Position == loadedMaps[i].parent_Position)
//                loadedMaps[i].childMaps.minimapUpdate();
//        }
//    }

//}