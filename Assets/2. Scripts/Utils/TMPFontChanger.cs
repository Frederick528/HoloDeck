using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TMPFontChanger : MonoBehaviour
{
    [SerializeField] public TMP_FontAsset FontAsset;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TMPFontChanger))]
public class TMPFontChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Change Font!"))
        {
            TMP_FontAsset fontAsset = ((TMPFontChanger)target).FontAsset;

            foreach (TextMeshPro textMeshPro3D in FindObjectsOfType<TextMeshPro>(true))
            {
                textMeshPro3D.font = fontAsset;
            }
            foreach (TextMeshProUGUI textMeshProUi in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                textMeshProUi.font = fontAsset;
            }
        }
    }
}
#endif