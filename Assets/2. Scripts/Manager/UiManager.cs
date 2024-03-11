using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance { get; private set; }

    [SerializeField] GameObject top;
    [SerializeField] TextMeshProUGUI topHealthText;     // TMP텍스트로 변경가능성있음


    private void Awake() => instance = this;

    public void SetupTop(bool state)
    {
        if (state)
            top.SetActive(true);
        else
            top.SetActive(false);
    }

    public void AddHPText(int value, int crtHealth, int maxHealth)
    {
        topHealthText.text = string.Format("{0}/{1}", crtHealth + value, maxHealth);
    }
}
