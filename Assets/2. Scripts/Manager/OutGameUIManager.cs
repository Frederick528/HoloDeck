using Cysharp.Threading.Tasks;
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
        Sound,
        Fade
    }

    Dictionary<int, RectTransform> _canvasDict = new();

    Transform _canvasTr;

    Image _fadeImage;

    (RectTransform, bool)[] _optionContent;

    ScrollRect _optionScrollRect;


    void Awake()
    {
        _canvasTr = GameObject.Find("OutGameCanvases").transform;
        
        if (Instance != null)
        {
            Destroy(_canvasTr.gameObject);
            return;
        }
        //Instance = Instance != null ? Instance : this;
        Instance = this;
        
        _canvasTr.gameObject.name = "OutGameCanvasesDontDestroy";
        for (int i = 0; i < _canvasTr.childCount; ++i)
        {
            _canvasDict.Add(i, _canvasTr.GetChild(i) as RectTransform);
        }
        DontDestroyOnLoad(_canvasTr);

        _fadeImage = _canvasDict[(int)CanvasName.Fade].GetComponentInChildren<Image>();

        ContentSizeFitter[] csf = _canvasDict[(int)CanvasName.Option].GetComponentsInChildren<ContentSizeFitter>(true);
        _optionContent = new (RectTransform, bool)[csf.Length];
        for (int i = 0; i < csf.Length; ++i)
        {
            _optionContent[i].Item1 = csf[i].transform as RectTransform;
            _optionContent[i].Item2 = false;
        }
        _optionScrollRect = _canvasDict[(int)CanvasName.Option].GetComponentInChildren<ScrollRect>();

    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // ²¨Áú ¶§
        {
            switch (canvasName)
            {
                case CanvasName.Option:
                    GameManager.Instance.ESC(state);
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
                    if (idx == -1)
                        idx = 0;
                    GameManager.Instance.ESC(state);
                    SetActiveOptionWindow(idx);
                    break;
            }
        }

    }

    public void SetActiveOptionWindow(int idx)
    {
        _optionScrollRect.content = _optionContent[idx].Item1;
        _optionScrollRect.viewport = _optionContent[idx].Item1.parent as RectTransform;
        for (int i = 0; i < _optionContent.Length; ++i)
        {
            if (i == idx && !_optionContent[i].Item2)
            {
                _optionScrollRect.normalizedPosition = new Vector2(0, 1);

                _optionContent[i].Item1.parent.gameObject.SetActive(true);
                _optionContent[i].Item2 = true;
            }
            else if (i != idx && _optionContent[i].Item2)
            {
                _optionContent[i].Item1.parent.gameObject.SetActive(false);
                _optionContent[i].Item2 = false;
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

    public async UniTask FadeIn(float fadeDuration)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, time / fadeDuration));
            await UniTask.Yield();
        }

        _fadeImage.color = new Color(0, 0, 0, 0f); // ¿ÏÀüÈ÷ ¹à¾ÆÁü
        SetActiveCanvas(CanvasName.Fade, false);
    }
    public async UniTask FadeOut(float fadeDuration)
    {
        SetActiveCanvas(CanvasName.Fade, true);
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, time / fadeDuration));
            await UniTask.Yield();
        }
    }

}
