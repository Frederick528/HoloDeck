using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Map : MonoBehaviour
{
    //public List<GameObject> minimapMapWall;
    //public GameObject floorMap;
    //public bool visitedMap = false;

    //public GameObject currMap;
    public bool visited = false;

    public IObjectPool<GameObject> MapPool { get; set; }

    //void Start()
    //{
    //    transform.gameObject.SetActive(false);
    //    //floorMap.GetComponentInChildren<MeshRenderer>().material = MapController.Instance.DefaultBackground;

    //    minimapWallset(false);
    //}

    public void VisitiedMap(bool boolean, bool currBool)
    {
        transform.gameObject.SetActive(boolean);
        if (currBool)
        {
            visited = true;
        }

        minimapWallset(true);
    }

    public void VisitiedCurrMap(bool boolean)
    {
        // 4. 현재 위치 밝게 처리
        //if (boolean)
        //    floorMap.GetComponentInChildren<MeshRenderer>().material = MapController.Instance.currMaterial;
        //else
        //{
        //    if (visited)
        //        floorMap.GetComponentInChildren<MeshRenderer>().material = MapController.Instance.VisitedBack;
        //    else
        //        floorMap.GetComponentInChildren<MeshRenderer>().material = MapController.Instance.DefaultBackground;
        //}
    }

    public void minimapWallset(bool boolean)
    {
        //if (visited || boolean)
        //    for (int i = 0; i < walls.Count; i++)
        //    {
        //        if (walls[i].isSetUp)
        //            walls[i].transform.gameObject.SetActive(boolean);
        //    }
        //else
        //    for (int i = 0; i < walls.Count; i++)
        //    {
        //        if (walls[i].isSetUp)
        //            walls[i].transform.gameObject.SetActive(boolean);
        //    }
    }

    public void MapRelease()
    {
        MapPool.Release(this.gameObject);
    }
}