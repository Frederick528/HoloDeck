using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance { get; private set; }

    [SerializeField] List<GameObject> battleUi;
    [SerializeField] TextMeshProUGUI topHealthText;     // TMP텍스트로 변경가능성있음
    [SerializeField] TextMeshProUGUI turnEndButtonText;


    private void Awake() => instance = this;

    public void SetupBattleUi(bool state)
    {
        foreach (GameObject gameObject in battleUi)
        {
            gameObject.SetActive(state);
        }
    }

    public void ChangeTurnButtonText(bool turn)
    {
        if (turn)
            turnEndButtonText.text = "Turn End";
        else
            turnEndButtonText.text = "Enemy's Turn";
    }
}
