using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class ResaveAllScenes : MonoBehaviour {

    [MenuItem("Tools/Re-save All Scenes")]
    public static void Run() {

        EditorUtility.DisplayProgressBar("Re-save All Scenes", "Re-saving all scenes...", 0.0f);

        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
            
            var editorBuildSettingsScene = EditorBuildSettings.scenes[i];            
            EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
            EditorSceneManager.SaveOpenScenes();
            EditorUtility.DisplayProgressBar("Re-save All Scenes", "Re-saving all scenes...", i / (float)(EditorBuildSettings.scenes.Length - 1));
        }
        EditorUtility.ClearProgressBar();        
    }
}
