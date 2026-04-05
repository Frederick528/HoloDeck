using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnMouseEnter
{
    bool BoolOnMouseEnter();
    public Material OutlineMaterial { get; set; }
    public Color BaseColor { get; set; }
    public Color HoverColor { get; set; }
    public float BaseThickness { get; set; }
    public float HoverThickness { get; set; }
}
