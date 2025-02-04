using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UnityScenesHelper {

    public static void SetActiveRootObjectsInScene(Scene scene, bool active) {

        var rootGameObjects = new List<GameObject>(scene.rootCount);
        scene.GetRootGameObjects(rootGameObjects);

        if (rootGameObjects.Count == 0) {
            return;
        }

        foreach (GameObject go in rootGameObjects) {
            go.SetActive(active);
        }
    }

    public static void GetComponentsInScene<T>(Scene scene, List<T> components, bool includeInactive = false) where T : Component
    {
        var gameObjects = new List<GameObject>();
        scene.GetRootGameObjects(gameObjects);
        foreach (var gameObject in gameObjects) {
            if (!includeInactive && !gameObject.activeInHierarchy) {
                continue;
            }

            components.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive));
        }
    }
}
