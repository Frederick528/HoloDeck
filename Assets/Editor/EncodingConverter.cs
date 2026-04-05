using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

public class SelectedFolderConverter : EditorWindow
{
    [MenuItem("Assets/Convert to UTF-8 BOM (Selected)", false, 100)]
    public static void ConvertSelectedFolder()
    {
        Object[] selectedObjects = Selection.GetFiltered<Object>(SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("error.");
            return;
        }

        int convertedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (ConvertToUTF8(file)) convertedCount++;
                }
            }
            else if (path.EndsWith(".cs"))
            {
                if (ConvertToUTF8(path)) convertedCount++;
            }
        }

        EditorUtility.DisplayDialog("변경", $"{convertedCount} change UTF-8 BOM.", "확인");
        AssetDatabase.Refresh();
    }

    private static bool ConvertToUTF8(string filePath)
    {

        try
        {
            byte[] buffer = File.ReadAllBytes(filePath);

            // 1. 이미 UTF-8 BOM(EF BB BF)인지 확인
            if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return false;
            }

            // 2. EUC-KR(CP949와 호환)로 읽기 시도
            // Provider 등록 없이 문자열 이름으로 시도합니다.
            Encoding eucKr = Encoding.GetEncoding("euc-kr");
            string content = File.ReadAllText(filePath, eucKr);

            // 3. UTF-8 BOM으로 다시 저장
            // new UTF8Encoding(true)는 BOM을 포함하라는 뜻입니다.
            File.WriteAllText(filePath, content, new UTF8Encoding(true));

            Debug.Log($"[성공] {Path.GetFileName(filePath)} 변환 완료");
            return true;
        }
        catch (System.ArgumentException)
        {
            // 만약 "euc-kr" 이름도 못 찾는다면 시스템 기본값으로 시도
            Debug.LogWarning($"{filePath}: euc-kr을 찾을 수 없어 기본값으로 시도합니다.");
            string content = File.ReadAllText(filePath, Encoding.Default);
            File.WriteAllText(filePath, content, new UTF8Encoding(true));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[에러] {filePath}: {e.Message}");
            return false;
        }
    }
}