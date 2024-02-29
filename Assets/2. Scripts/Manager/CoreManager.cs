using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager instance;
    public GameCore Core;
    private void Awake()
    {
        instance ??= this;
    }


}
