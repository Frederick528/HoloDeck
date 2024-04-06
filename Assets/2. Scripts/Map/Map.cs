using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    //public List<GameObject> minimapMapWall;
    //public GameObject floorMap;
    //public bool visitedMap = false;

    //public GameObject currMap;
    public bool visited = false;

    //public IObjectPool<GameObject> MapPool { get; set; }
    public Button btn;
    public Image img;
    public Vector3Int array_Position;
    public List<Map> aroundMap = new();


    public void SeeMap(List<Vector3Int> direction4, List<Map> maps)
    {
        foreach (Vector3Int direction in direction4)
        {
            Map connectMap = maps.Find(x => x.array_Position == array_Position + direction);
            if (connectMap != null)
            {
                aroundMap.Add(connectMap);
                connectMap.gameObject.SetActive(true);
                //connectMap.img.color = Color.white;
                //connectMap.btn.interactable = true;
            }
        }
    }
    public void ClearMap()
    {
        foreach (Map map in aroundMap)
        {
            //map.img.color = Color.white;
            map.btn.interactable = true;
        }
    }

    //public void VisitiedMap(bool boolean, bool currBool)
    //{
    //    transform.gameObject.SetActive(boolean);
    //    if (currBool)
    //    {
    //        visited = true;
    //    }
    //}

    //public void VisitiedCurrMap()
    //{
    //    GetComponent<Image>().color = Color.white;
    //    visited = true;
    //}

    //public void MapRelease()
    //{
    //    MapPool.Release(this.gameObject);
    //}
}