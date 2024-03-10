using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    [SerializeField] GameObject Top;
    [SerializeField] TextMeshProUGUI TopHealthText;


    private void Awake() => instance = this;

    public void SetupTop(bool state)
    {
        if (state)
            Top.SetActive(true);
        else
            Top.SetActive(false);
    }

    public void AddHPText(int value, int crtHealth, int maxHealth)
    {
        TopHealthText.text = string.Format("{0}/{1}", crtHealth + value, maxHealth);
    }
}
