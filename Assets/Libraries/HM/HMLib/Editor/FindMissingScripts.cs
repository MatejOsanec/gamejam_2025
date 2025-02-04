using UnityEngine;
using System.Collections.Generic;

public static class FindMissingScripts {

    public static List<UnityObjectWithDescription> FindMissingComponentsInGameObjectsAndChildren(IEnumerable<GameObject> gameObjects) {

        var gameObjectsWithMissingComponents = new List<UnityObjectWithDescription>();

        foreach (GameObject go in gameObjects) {
            FindMissingComponentsInGOAndChildren(go, ref gameObjectsWithMissingComponents);
        }

        return gameObjectsWithMissingComponents;
    }

    public static List<UnityObjectWithDescription> FindMissingComponentsInGameObjects(IEnumerable<GameObject> gameObjects) {

        var gameObjectsWithMissingComponents = new List<UnityObjectWithDescription>();

        foreach (GameObject go in gameObjects) {
            FindMissingComponentsInGO(go, ref gameObjectsWithMissingComponents);
        }

        return gameObjectsWithMissingComponents;
    }

    private static void FindMissingComponentsInGOAndChildren(GameObject go, ref List<UnityObjectWithDescription> gameObjectsWithMissingComponents) {

        FindMissingComponentsInGO(go, ref gameObjectsWithMissingComponents);

        foreach (Transform child in go.transform) {
            FindMissingComponentsInGOAndChildren(child.gameObject, ref gameObjectsWithMissingComponents);
        }
    }

    private static void FindMissingComponentsInGO(GameObject go, ref List<UnityObjectWithDescription> gameObjectsWithMissingComponents) {

        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++) {
            if (components[i] == null) {
                gameObjectsWithMissingComponents.Add(new UnityObjectWithDescription(go, go.transform.GetPath()));
                break;
            }
        }
    }
}
