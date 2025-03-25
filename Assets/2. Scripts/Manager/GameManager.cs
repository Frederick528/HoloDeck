using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<GameObject> DontDestroyObjects = new();
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDontDestroy(GameObject gameObject)
    {
        DontDestroyObjects.Add(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyAllDontDestroyObjects()
    {
        foreach (GameObject obj in DontDestroyObjects)
        {
            Destroy(obj);
        }
        DontDestroyObjects.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
