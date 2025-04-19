using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameUIManager : MonoBehaviour
{
    public static OutGameUIManager Instance;

    public enum CanvasName      // 순서가 Canvas 순서랑 일치해야 함.
    {
        Main,
        PowerUP,
        Character,
        Option,
        Sound
    }

    Dictionary<int, Transform> _canvasDict = new();

    Transform _canvas;

    [Header("Lobby")]
    TMP_Text _goodsText;

    void Awake()
    {
        _canvas = GameObject.Find("OutGameCanvases").GetComponent<Transform>();

        if (Instance != null)
        {
            Destroy(gameObject);
        }
        print('S');
        Instance = Instance != null ? Instance : this;

        for (int i = 0; i < /*CanvasList.Count*/_canvas.childCount; ++i)
        {
            //_canvasRaycaster.Add(_canvas.GetChild(i).GetComponent<GraphicRaycaster>());
            _canvasDict.Add(i, _canvas.GetChild(i));
        }
        Canvas(CanvasName.Option).SetParent(null);
        DontDestroyOnLoad(Canvas(CanvasName.Option));
        Canvas(CanvasName.Sound).SetParent(null);
        DontDestroyOnLoad(Canvas(CanvasName.Sound));
    }
    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // 꺼질 때
        {
            switch (canvasName)
            {
                case CanvasName.Main:
                    break;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            _canvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.Main:
                    break;
            }
        }

    }

    public Transform Canvas(CanvasName canvasName)
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
        if (GameManager.Instance.NowChapterLV == 0)
        {
            _canvasDict.Clear();
            _canvas = GameObject.Find("OutGameCanvases").GetComponent<Transform>();
            for (int i = 0; i < /*CanvasList.Count*/_canvas.childCount; ++i)
            {
                //_canvasRaycaster.Add(_canvas.GetChild(i).GetComponent<GraphicRaycaster>());
                _canvasDict.Add(i, _canvas.GetChild(i));
            }
            _goodsText.text = GameManager.Instance.Goods.Value.ToString();
        }
    }
}
