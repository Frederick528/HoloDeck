using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance { get; private set; }

    [SerializeField] List<GameObject> gameUi;
    [SerializeField] List<GameObject> battleUi;
    [SerializeField] TextMeshProUGUI topHealthText;     // TMP텍스트로 변경가능성있음
    [SerializeField] TextMeshProUGUI topCoinText;
    [SerializeField] TextMeshProUGUI turnEndButtonText;
    [SerializeField] GameObject map;


    private void Awake() => instance = this;

    public void SetupGameUi(bool state)
    {
        foreach (GameObject gameObject in gameUi)
        {
            gameObject.SetActive(state);
        }
    }
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
    public void LookMap()
    {
        map.SetActive(!map.activeSelf);
    }

    public void SetHealth(int curHp, int maxHp)
    {
        topHealthText.text = $"{curHp} / {maxHp}";
    }

    public void SetCoin(int coin)
    {
        topCoinText.text = coin.ToString();
    }
}
