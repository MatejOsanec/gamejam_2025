using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissingScriptsCheckerMenu {

    [MenuItem("Tools/Find Missing Scripts/In Loaded Scenes")]
    public static void FindMissingScriptsInLoadedScenes() {

        FindMissingScriptsInGameObjects(
            FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects()
        );
    }

    [MenuItem("Tools/Find Missing Scripts/In Prefabs")]
    public static void FindMissingScriptsInPrefabs() {

        FindMissingScriptsInGameObjects(
            FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects()
        );
    }

    [MenuItem("Tools/Find Missing Scripts/In Edited Prefab")]
    public static void FindMissingScriptsInEditedPrefab() {

        FindMissingScriptsInGameObjects(
            FindUnityObjectsHelper.CurrentPrefab.GetAllGameObjects()
        );
    }

    [MenuItem("Tools/Find Missing Scripts/In Edited Prefab", isValidateFunction: true)]
    public static bool FindMissingScriptsInEditedPrefabValidation() {

        return FindUnityObjectsHelper.CurrentPrefab.GetContentRoot() != null;
    }

    private static void FindMissingScriptsInGameObjects(IEnumerable<GameObject> gameObjects) {

        var gameObjectsWithMissingComponents = FindMissingScripts.FindMissingComponentsInGameObjects(gameObjects);

        if (gameObjectsWithMissingComponents.Count > 0) {
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Missing scripts");
            window.objectDescriptions = gameObjectsWithMissingComponents.ToArray();
        }
        else {
            Selection.activeObject = null;
            Debug.Log("No missing scripts found.");
        }
    }
}
