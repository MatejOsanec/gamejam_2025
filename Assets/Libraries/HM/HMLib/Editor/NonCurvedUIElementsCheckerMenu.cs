using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NonCurvedUIElementsCheckerMenu {

    [MenuItem("Tools/Find Non Curved UI Elements/In Loaded Scenes")]
    public static void FindInLoadedScenes() {

        var gameObjects = FindUnityObjectsHelper.LoadedScenes.GetRootGameObjects().ToArray();
        FindInGameObjectsAndChildren(gameObjects);
    }

    [MenuItem("Tools/Find Non Curved UI Elements/In Edited Prefabs")]
    public static void FindInEditedPrefab() {

        var gameObjects = FindUnityObjectsHelper.CurrentPrefab.GetRootGameObjects().ToArray();
        FindInGameObjectsAndChildren(gameObjects);
    }

    [MenuItem("Tools/Find Non Curved UI Elements/In Edited Prefabs", isValidateFunction: true)]
    public static bool FindInEditedPrefabValidation() {

        return FindUnityObjectsHelper.CurrentPrefab.GetContentRoot() != null;
    }

    [MenuItem("Tools/Find Non Curved UI Elements/In Addressable Prefabs")]
    public static void FindInAddressablePrefabs() {

        var gameObjects = FindUnityObjectsHelper.AddressablePrefabs.GetRootGameObjects().ToArray();
        FindInGameObjectsAndChildren(gameObjects);
    }

    private static void FindInGameObjectsAndChildren(GameObject[] gameObjects) {

        var results = new List<UnityObjectWithDescription>();
        results.AddRange(ComponentFinder.FindExactComponentsInGameObjectsAndChildren<Image>(gameObjects));
        results.AddRange(ComponentFinder.FindExactComponentsInGameObjectsAndChildren<TextMeshProUGUI>(gameObjects));

        if (results.Any()) {
            var window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Non Curved Images and Texts");
            window.objectDescriptions = results;
        }
        else {
            Debug.Log("No non curved images or texts found.");
        }
    }

    [MenuItem("Tools/Find Non Curved UI Elements/Canvases Without UV2 in Loaded Scenes")]
    public static void CanvasesWithoutUV2InLoadedScenes() {

        var gameObjects = FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects();

        var results = FindComponents.FindComponentsInGameObjects(typeof(Canvas), gameObjects, ignoreSubclasses: false);
        results = results.Where(x => !((Canvas)x.obj).additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord2)).ToList();

        if (results.Any()) {
            var window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Canvases without UV2");
            window.objectDescriptions = results;
        }
        else {
            Debug.Log("No Canvases without UV2 found.");
        }
    }

    [MenuItem("Tools/Find Non Curved UI Elements/Canvases Without UV2 in Prefabs")]
    public static void CanvasesWithoutUV2InPrefabs() {

        var gameObjects = FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects();

        var results = FindComponents.FindComponentsInGameObjects(typeof(Canvas), gameObjects, ignoreSubclasses: false);
        Debug.Log(results.Count);
        results = results.Where(x => !((Canvas)x.obj).additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord2)).ToList();

        if (results.Any()) {
            var window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Canvases without UV2");
            window.objectDescriptions = results;
        }
        else {
            Debug.Log("No Canvases without UV2 found in prefabs.");
        }
    }
}
