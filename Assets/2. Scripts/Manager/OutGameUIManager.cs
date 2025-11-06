using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public Dictionary<string, Action> CurOpenUI = new();
    public List<string> OpenUIOrder = new();

    Dictionary<int, RectTransform> _canvasDict = new();

    Transform _canvasTr;

    Image _fadeImage;

    GameObject _fastCheck;

    (RectTransform, bool)[] _optionContent;

    ScrollRect _optionScrollRect;

    //[SerializeField]
    //private TMP_Dropdown resolutionDropdown; // 인스펙터에서 연결할 UI 드롭다운

    private Resolution[] resolutions; // 컴퓨터가 지원하는 해상도 목록 저장


    void Awake()
    {
        if (GameManager.Instance.OutGameRootObj != null)
        {
            _canvasTr = GameObject.Find("OutGame").transform.Find("OutGameCanvases");
        }
        else
        {
            _canvasTr = GameObject.Find("OutGameCanvases").transform;
        }

        if (Instance != null)
        {
            //Destroy(_canvasTr.gameObject);
            return;
        }
        //Instance = Instance != null ? Instance : this;
        Instance = this;

        for (int i = 0; i < _canvasTr.childCount; ++i)
        {
            _canvasDict.Add(i, _canvasTr.GetChild(i) as RectTransform);
        }
        if (GameManager.Instance.OutGameRootObj == null)
        {
            //print(GameManager.Instance.OutGameRootObj);
            _canvasTr.gameObject.name = "OutGameCanvasesDontDestroy";
            DontDestroyOnLoad(_canvasTr);
        }

        _fadeImage = _canvasDict[(int)CanvasName.Fade].GetComponentInChildren<Image>();

        ContentSizeFitter[] csf = _canvasDict[(int)CanvasName.Option].GetComponentsInChildren<ContentSizeFitter>(true);
        _optionContent = new (RectTransform, bool)[csf.Length];
        for (int i = 0; i < csf.Length; ++i)
        {
            _optionContent[i].Item1 = csf[i].transform as RectTransform;
            _optionContent[i].Item2 = false;
        }
        _optionScrollRect = _canvasDict[(int)CanvasName.Option].GetComponentInChildren<ScrollRect>();
        _fastCheck = FindTransform.ContinueFindChildUIByName(_optionContent[0].Item1, "Check").gameObject;

        SettingCanResolution();
    }
    public void SettingCanResolution()
    {
        TMP_Dropdown resolutionDropdown;
        if (!FindTransform.ContinueFindChildUIByName(_optionContent[1].Item1, "Resolution").gameObject.TryGetComponent<TMP_Dropdown>(out resolutionDropdown))
            return;
        // 1. 컴퓨터가 지원하는 모든 해상도 가져오기
        resolutions = Screen.resolutions;

        // 2. 드롭다운의 기존 옵션 모두 삭제
        resolutionDropdown.ClearOptions();

        // 3. 해상도 목록 가공 및 드롭다운 옵션으로 추가
        HashSet<string> options = new HashSet<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            // "너비 x 높이" 형태의 문자열 생성
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // 현재 게임의 해상도와 일치하는 옵션을 찾으면 인덱스 저장
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options.ToList()); // 가공된 목록을 드롭다운에 추가
        resolutionDropdown.value = currentResolutionIndex; // 현재 해상도를 기본값으로 설정
        resolutionDropdown.RefreshShownValue(); // 드롭다운 UI 갱신

        // 드롭다운 값이 변경될 때 호출될 함수 연결
        resolutionDropdown.onValueChanged.AddListener((resolutionIndex) =>
        {
            Resolution selectedResolution = resolutions[resolutionIndex];
            GameManager.Instance.ResolutionSetting(Camera.main, selectedResolution.width, selectedResolution.height);
        });
    }

    public void SetActiveCanvas(CanvasName canvasName, bool state, int idx = -1)
    {
        if (!state)     // 꺼질 때
        {
            switch (canvasName)     // ESC 체크용. 각각 쓰기 귀찮아서 그냥 한 곳에 모음.
            {
                case CanvasName.Option:
                    RemoveOpenUIOrder(canvasName.ToString());
                    break;
            }
            //if (curUI.Remove(() => SetActiveCanvas(canvasName, state, idx)))
            //{

            //}
            switch (canvasName)
            {
                case CanvasName.Option:
                    //RemoveOpenUIOrder(canvasName.ToString());
                    GameManager.Instance.ESC(state);
                    break;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(false);
        }
        else
        {
            switch (canvasName)     // ESC 체크용. 각각 쓰기 귀찮아서 그냥 한 곳에 모음.
            {
                case CanvasName.Option:
                    if (idx == -1)
                        idx = 0;
                    AddOpenUIOreder(canvasName.ToString(), () => SetActiveCanvas(canvasName, !state, idx));
                    break;
            }

            _canvasDict[(int)canvasName].gameObject.SetActive(true);

            switch (canvasName)
            {
                case CanvasName.Option:
                    if (idx == -1)
                        idx = 0;
                    //AddOpenUIOreder(canvasName.ToString(), () => SetActiveCanvas(canvasName, !state, idx));
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

    public void CloseUIByOrder()
    {
        CurOpenUI[OpenUIOrder[^1]]();
    }

    public void AddOpenUIOreder(string key, Action action)
    {
        if (CurOpenUI.ContainsKey(key))
        {
            return;
        }
        CurOpenUI.Add(key, action);
        OpenUIOrder.Add(key);
    }

    public void RemoveOpenUIOrder(string key)
    {
        CurOpenUI.Remove(key);
        OpenUIOrder.Remove(key);
    }

    public Transform OutGameCanvas(CanvasName canvasName)
    {
        return _canvasDict[(int)canvasName];
    }

    public void CheckFastMode()
    {
        _fastCheck.SetActive(GameManager.Instance.GetFast());
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
        _fadeImage.color = new Color(0, 0, 0, 1f);
        await _fadeImage.DOFade(0f, fadeDuration);
        //float time = 0f;
        //while (time < fadeDuration)
        //{
        //    time += Time.deltaTime;
        //    _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, time / fadeDuration));
        //    await UniTask.Yield();
        //}

        //_fadeImage.color = new Color(0, 0, 0, 0f); // 완전히 밝아짐
        SetActiveCanvas(CanvasName.Fade, false);
    }
    public async UniTask FadeOut(float fadeDuration)
    {
        SetActiveCanvas(CanvasName.Fade, true);
        _fadeImage.color = new Color(0, 0, 0, 0f);
        await _fadeImage.DOFade(1f, fadeDuration);
        //float time = 0f;
        //while (time < fadeDuration)
        //{
        //    time += Time.deltaTime;
        //    _fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, time / fadeDuration));
        //    await UniTask.Yield();
        //}
    }

}
