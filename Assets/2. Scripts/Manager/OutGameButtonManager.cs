using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameButtonManager : MonoBehaviour
{
    public static OutGameButtonManager Instance { get; private set; }
    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
    }

    //public void ESC()
    //{
    //    GameManager.Instance.ESC();
    //}

    public void ChangeScene(int idx)
    {
        GameManager.Instance.ChangeScene(idx).Forget();
    }
    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }

    [VisibleEnum(typeof(OutGameUIManager.CanvasName))]
    public void OnCanvas(int canvasNameIdx)
    {
        OutGameUIManager.Instance.SetActiveCanvas((OutGameUIManager.CanvasName)canvasNameIdx, true);
    }
    [VisibleEnum(typeof(OutGameUIManager.CanvasName))]
    public void OffCanvas(int canvasNameIdx)
    {
        OutGameUIManager.Instance.SetActiveCanvas((OutGameUIManager.CanvasName)canvasNameIdx, false);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }
}
