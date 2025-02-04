using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class MissingRequiredComponentsCheckerMenu {

    [MenuItem("Tools/Find Missing Required Components/In Loaded Scenes")]
    public static void FindMissingRequiredComponentsInLoadedScenes() {

        EditorCoroutineUtility.StartCoroutineOwnerless(FindMissingScriptsInPrefabsCoroutine());
    }

    [MenuItem("Tools/Find Missing Required Components/In Prefabs")]
    public static void FindMissingRequiredComponentsInPrefabs() {

        EditorCoroutineUtility.StartCoroutineOwnerless(FindMissingRequiredComponentsInPrefabsCoroutine());
    }

    private static IEnumerator FindMissingScriptsInPrefabsCoroutine() {

        EditorUtility.DisplayProgressBar("Required Components in Scenes", "Searching...", 0.1f);

        yield return null;

        var gameObjects = FindUnityObjectsHelper.LoadedScenes.GetRootGameObjects();
        var gameObjectsWithMissingRequiredComponents = FindMissingRequiredComponents.FindMissingRequiredComponentsInGameObjectsAndChildren(gameObjects);

        EditorUtility.DisplayProgressBar("Required Components in Scenes", "Searching...", 1.0f);

        yield return null;

        EditorUtility.ClearProgressBar();

        yield return null;

        if (gameObjectsWithMissingRequiredComponents.Count > 0) {
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Null References");
            window.objectDescriptions = gameObjectsWithMissingRequiredComponents.ToArray();
        }
        else {
            Selection.activeObject = null;
            Debug.Log("All required components in active scenes are present.");
        }
    }

    private static IEnumerator FindMissingRequiredComponentsInPrefabsCoroutine() {

        EditorUtility.DisplayProgressBar("Required Components in Prefabs", "Searching...", 0.1f);

        yield return null;

        var gameObjects = FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects();
        var gameObjectsWithMissingRequiredComponents = FindMissingRequiredComponents.FindMissingRequiredComponentsInGameObjects(gameObjects);

        yield return null;

        EditorUtility.ClearProgressBar();

        yield return null;

        if (gameObjectsWithMissingRequiredComponents.Count > 0) {
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Null References");
            window.objectDescriptions = gameObjectsWithMissingRequiredComponents.ToArray();
        }
        else {
            Selection.activeObject = null;
            Debug.Log("All required components in prefabs are present.");
        }

        EditorUtility.ClearProgressBar();
    }
}
