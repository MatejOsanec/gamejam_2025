using System;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentFinder {

    public static List<UnityObjectWithDescription> FindExactComponentsInGameObjectsAndChildren<T>(IEnumerable<GameObject> gameObjects, string ignoreObjectsWithTag = null) where T : Component {

        var results = new List<UnityObjectWithDescription>();
        var componentType = typeof(T);

        var uniqueGameObjects = new HashSet<GameObject>(gameObjects);
        var rootPath = new List<string>(32);
        foreach (var gameObject in uniqueGameObjects) {
            if (ignoreObjectsWithTag != null && gameObject.CompareTag(ignoreObjectsWithTag)) {
                continue;
            }

            FindExactComponentsInObjectAndChildren(componentType, gameObject, ignoreObjectsWithTag, rootPath, results);
        }

        return results;
    }

    private static void FindExactComponentsInObjectAndChildren(Type componentType, GameObject gameObject, string ignoreObjectsWithTag, List<string> rootPath, ICollection<UnityObjectWithDescription> results) {

        rootPath.Add(gameObject.name);

        var components = gameObject.GetComponents(componentType);
        foreach (var component in components) {
            if (componentType == component.GetType()) {
                results.Add(new UnityObjectWithDescription(gameObject, string.Join("/", rootPath)));
                break;
            }
        }

        foreach (Transform childTransform in gameObject.transform) {
            if (ignoreObjectsWithTag != null && childTransform.CompareTag(ignoreObjectsWithTag)) {
                continue;
            }

            FindExactComponentsInObjectAndChildren(componentType, childTransform.gameObject, ignoreObjectsWithTag, rootPath, results);
        }

        rootPath.RemoveAt(rootPath.Count - 1);
    }
}
