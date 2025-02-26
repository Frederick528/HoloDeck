using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public Arrow ArrowCursor;

    private void Awake() => Instance = this;
    private void Start()
    {
        ArrowCursor = FindObjectOfType<Arrow>(true);
        //UIManager.Instance.SetupGameUi(true);
        //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    }
    /// <summary>
    /// Arrow커서의 활성화 여부와 위치를 설정합니다.
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="arrowIdx">Arrow의 위치를 의미(0 = Card, 1 = Active, 2 = Potion)</param>
    public void SetActiveArrowCursor(bool isOn, int arrowIdx)
    {
        if (isOn)
        {
            UIManager.Instance.SetActiveCanvas(UIManager.CanvasName.Map, false);
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.InGame, false);
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, false);
        }
        else
        {
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.InGame, true);
            UIManager.Instance.SetCanvasRaycast(UIManager.CanvasName.Battle, true);
        }
        ArrowCursor.ArrowIndex = arrowIdx;
        ArrowCursor.SetStartArrow();
        ArrowCursor.gameObject.SetActive(isOn);
        Cursor.visible = !isOn;
    }
}
