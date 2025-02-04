using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using BGLib.AppFlow.Editor;
using UnityEngine.SceneManagement;

public class SceneInfoCreator : MonoBehaviour {

    [MenuItem("Tools/Create Scene Info for current scene")]
    public static void CreateSceneInfo() {

        var scene = SceneManager.GetActiveScene();

        SceneInfo sceneInfo = ScriptableObject.CreateInstance<SceneInfo>();
        sceneInfo.SetSceneData(scene.name);

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(GetSelectedPathOrFallback(), scene.name + "SceneInfo.asset"));

        AssetDatabase.CreateAsset(sceneInfo, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = sceneInfo;
    }

    public static string GetSelectedPathOrFallback() {

        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}
