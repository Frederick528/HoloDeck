using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueChange : MonoBehaviour
{
    private Slider _slider;

    void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(SliderValue);
    }

    public void SliderValue(float value)
    {
        _slider.value =  Mathf.Round(value);
    }
}
