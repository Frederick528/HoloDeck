using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    #region Public Fields
    [Tooltip("The prefab of arrow head")]
    public GameObject ArrowHeadPrefab;

    [Tooltip("The prefab of arrow node")]
    public GameObject ArrowNodePrefab;

    [Tooltip("The number of arrow nodes")]
    public int ArrowNodeNum;

    [Tooltip("The scale multiplier for arrow nodes")]
    public float scaleFactor = 1f;
    #endregion
    
    #region Private Fields
    private Transform origin;
    private List<Transform> arrowNodes = new();
    private List<Vector2> controlPoints = new();
    private readonly List<Vector2> controlPointFactors = new List<Vector2> { new Vector2(-0.3f, 0.8f), new Vector2(0.1f, 1.4f) };
    #endregion

    #region Private Methods

    private void Awake()
    {
        this.origin = this.GetComponent<Transform>();

        for (int i = 0; i < this.ArrowNodeNum; ++i)
        {
            this.arrowNodes.Add(Instantiate(this.ArrowNodePrefab, this.transform).GetComponent<Transform>());
        }

        this.arrowNodes.Add(Instantiate(this.ArrowHeadPrefab, this.transform).GetComponent<Transform>());

        this.arrowNodes.ForEach(a => a.GetComponent<Transform>().position = new Vector2(-0.5f, -0.5f));

        for (int i = 0; i < 4; ++i)
        {
            this.controlPoints.Add(Vector2.zero);
        }
    }

    private void Update()
    {
        
        this.controlPoints[0] = new Vector2(this.origin.position.x, this.origin.position.y - 1.5f);

        this.controlPoints[3] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.controlPoints[1] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[0];
        this.controlPoints[2] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[1];

        for (int i = 0; i < this.arrowNodes.Count; ++i)
        {
            //var t = Mathf.Log(1f * i / (this.arrowNodes.Count - 1) + 1f, 2f);
            var t = Mathf.Pow(2, i/(this.arrowNodes.Count - 1f)) - 1f;
            this.arrowNodes[i].position = 
                Mathf.Pow(1 - t, 3) * this.controlPoints[0] +
                3 * Mathf.Pow(1 - t, 2) * t * this.controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * this.controlPoints[2] +
                Mathf.Pow(t, 3) * this.controlPoints[3];
            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, this.arrowNodes[i].position - this.arrowNodes[i - 1].position));
                this.arrowNodes[i].rotation = Quaternion.Euler(euler);
            }
            
            var scale = this.scaleFactor * (1f - 0.06f * (this.arrowNodes.Count - 1 - i));
            this.arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }

        this.arrowNodes[0].transform.rotation = this.arrowNodes[1].transform.rotation;
    }
    #endregion
}
