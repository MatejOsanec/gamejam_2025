using UnityEditor;
using UnityEngine;

public class GUIDSearchEditorWindow : EditorWindow {

    string guid = "";
    string path = "";

    [MenuItem("Tools/Find Objects/Find Assets by GUID")]
    static void CreateWindow() {

        GUIDSearchEditorWindow window = (GUIDSearchEditorWindow)EditorWindow.GetWindowWithRect(typeof(GUIDSearchEditorWindow), new Rect(0, 0, 400, 120));
    }

    protected void OnGUI() {

        GUILayout.Label("Enter guid");
        guid = GUILayout.TextField(guid);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Get Asset Path", GUILayout.Width(120))) {
            path = GetAssetPath(guid);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label(path);
    }

    static string GetAssetPath(string guid) {

        string p = AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log(p);
        if (p.Length == 0) {
            p = "not found";
        }
        return p;
    }
}
