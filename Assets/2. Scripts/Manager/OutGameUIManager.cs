using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameUIManager : MonoBehaviour
{
    public static OutGameUIManager Instance;

    public enum CanvasName
    {
        Option,
        Sound
    }

    Dictionary<int, Transform> _canvasDict = new();

    Transform _canvas;


    void Awake()
    {
        _canvas = GameObject.Find("OutGameCanvases").GetComponent<Transform>();
        
        if (Instance != null)
        {
            Destroy(_canvas.gameObject);
            return;
        }
        //Instance = Instance != null ? Instance : this;
        Instance = this;
        
        _canvas.gameObject.name = "OutGameCanvasesDontDestroy";
        for (int i = 0; i < _canvas.childCount; ++i)
        {
            _canvasDict.Add(i, _canvas.GetChild(i));
        }
        DontDestroyOnLoad(_canvas);

    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // ²¨Áú ¶§
        {
            switch (canvasName)
            {
                case CanvasName.Option:
                    break;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            _canvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.Option:
                    break;
            }
        }

    }

    public Transform OutGameCanvas(CanvasName canvasName)
    {
        return _canvasDict[(int)canvasName];
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
