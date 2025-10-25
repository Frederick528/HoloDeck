using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Action OpenBox;
    private void OnMouseDown()
    {
        OpenBox?.Invoke();
    }

}
