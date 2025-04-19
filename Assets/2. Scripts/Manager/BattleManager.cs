using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public Arrow ArrowCursor = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ArrowCursor = FindObjectOfType<Arrow>(true);
            GameManager.Instance.AddInGameDontDestroy(ArrowCursor.gameObject);
            //DontDestroyOnLoad(ArrowCursor);
        }
    }
    //private void Start()
    //{
    //    ArrowCursor = FindObjectOfType<Arrow>(true);
    //    DontDestroyOnLoad(ArrowCursor);
    //    //InGameUIManager.Instance.SetupGameUi(true);
    //    //SoundManager.Instance.Play("Sounds/Bgm/StoryBgm", Sound.Bgm, 0.2f);
    //}
    /// <summary>
    /// Arrow커서의 활성화 여부와 위치를 설정합니다.
    /// </summary>
    /// <param name="isOn"></param>
    /// <param name="arrowIdx">Arrow의 위치를 의미(0 = Card, 1 = Active, 2 = Potion)</param>
    public void SetActiveArrowCursor(bool isOn, int arrowIdx)
    {
        //if (isOn)
        //{
        //    InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Map, false);
        //    InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, false);
        //    InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, false);
        //}
        //else
        //{
        //    InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.InGame, true);
        //    InGameUIManager.Instance.SetCanvasRaycast(InGameUIManager.CanvasName.Battle, true);
        //}
        ArrowCursor.ArrowIndex = arrowIdx;
        ArrowCursor.SetStartArrow();
        ArrowCursor.gameObject.SetActive(isOn);
        Cursor.visible = !isOn;
    }       
}
