using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class FindUnityObjects {

    [MenuItem("Tools/Find Objects/Mesh Renderers in Loaded Scene")]
    private static void FindAllMeshRenderersInLoadedScenes() {

        var gameObjects = FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects();
        List<GameObject> filteredGameObjets = FindGameObjectsWithMeshRenderer(gameObjects);

        if (filteredGameObjets.Count > 0) {
            Selection.objects = filteredGameObjets.ToArray<UnityEngine.Object>();
        }
        else {
            Selection.activeGameObject = null;
        }
    }

    [MenuItem("Tools/Find Objects/Mesh Renderers in Prefabs")]
    private static void FindAllMeshRenderersInPrefabs() {

        var gameObjects = FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects();
        List<GameObject> filteredGameObjets = FindGameObjectsWithMeshRenderer(gameObjects);

        ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Object Browser");
        window.objects = filteredGameObjets.ToArray();
    }

    [MenuItem("Tools/Find Objects/Dirty Objects in Loaded Scene")]
    private static void FindDirtyObjectsInLoadedScene() {

        var gameObjects = FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects();
        var filteredGameObjets = gameObjects.Where(EditorUtility.IsDirty).ToArray();

        if (filteredGameObjets.Length > 0) {
            foreach (var filteredGameObjet in filteredGameObjets) {
                foreach (var component in filteredGameObjet.GetComponents<Component>()) {
                    if (EditorUtility.IsDirty(component)) {
                        Debug.Log(component.GetType());
                    }
                }
            }
            Selection.objects = filteredGameObjets;
        }
        else {
            Selection.activeGameObject = null;
        }
    }

    private static List<GameObject> FindGameObjectsWithMeshRenderer(IEnumerable<GameObject> gameObjects) {

        List<GameObject> filteredGameObjets = new List<GameObject>();
        foreach (GameObject go in gameObjects) {
            if (go.GetComponent<MeshRenderer>()) {
                filteredGameObjets.Add(go);
            }
        }

        return filteredGameObjets;
    }
}
