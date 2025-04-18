using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGameUIManager : MonoBehaviour
{
    public static OutGameUIManager Instance;
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
}
