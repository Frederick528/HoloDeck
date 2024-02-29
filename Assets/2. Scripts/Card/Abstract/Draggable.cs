using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Draggable : MonoBehaviour
{
    protected static Vector3 _defPos;
    private static Vector3 _crntPos;

    public int level;
    public const int MaxLevel = 6;
    private bool _isInitialized = false;


    protected virtual void OnMouseDown()
    {
        _defPos = this.transform.position;
    }
    public abstract void OnMouseUp();

    protected virtual void OnMouseDrag()
    {
        float distance = Camera.main.WorldToScreenPoint(transform.position).z;

        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        _crntPos = Camera.main.ScreenToWorldPoint(mousePos);
        this.transform.position = _crntPos;
    }

    //public void Init(int level)
    //{
    //    this.level = level;
    //    _isInitialized = true;
    //}

    //private void initCheck()
    //{
    //    if (!_isInitialized) throw new Exception("this Mergeable Object has not Initialized.");
    //}

    //// 실제로 합쳤을 때 실행될 메소드
    //protected virtual void OnMergeEnter()
    //{
    //    initCheck();
    //}


    //// 합쳤을 때 동작할 내용 구현
    //protected abstract void OnMerge(GameObject t1, GameObject t2);
    ////protected abstract void OnMerge(IEnumerable<Mergeable> mergeable);

}
