using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildMouseHandler : MonoBehaviour
{
    public Entity ParentEntity;

    void OnMouseExit()
    {
        ParentEntity.OnChildMouseExit();
    }

}
