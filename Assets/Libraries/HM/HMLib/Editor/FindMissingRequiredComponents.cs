using UnityEngine;
using System.Collections.Generic;

public static class FindMissingRequiredComponents {

    public static List<UnityObjectWithDescription> FindMissingRequiredComponentsInGameObjectsAndChildren(IEnumerable<GameObject> gameObjects) {

        var gameObjectsWithMissingRequiredComponents = new List<UnityObjectWithDescription>();

        foreach (GameObject go in gameObjects) {
            FindMissingRequiredComponentsInGameObjectAndChildren(go, ref gameObjectsWithMissingRequiredComponents);
        }

        return gameObjectsWithMissingRequiredComponents;
    }

    public static List<UnityObjectWithDescription> FindMissingRequiredComponentsInGameObjects(IEnumerable<GameObject> gameObjects) {

        var gameObjectsWithMissingRequiredComponents = new List<UnityObjectWithDescription>();

        foreach (GameObject go in gameObjects) {
            FindMissingRequiredComponentsInGameObject(go, ref gameObjectsWithMissingRequiredComponents);
        }

        return gameObjectsWithMissingRequiredComponents;
    }

    private static void FindMissingRequiredComponentsInGameObjectAndChildren(GameObject go, ref List<UnityObjectWithDescription> gameObjectsWithMissingRequiredComponents) {

        FindMissingRequiredComponentsInGameObject(go, ref gameObjectsWithMissingRequiredComponents);

        foreach (Transform child in go.transform) {
            FindMissingRequiredComponentsInGameObjectAndChildren(child.gameObject, ref gameObjectsWithMissingRequiredComponents);
        }
    }

    private static void FindMissingRequiredComponentsInGameObject(GameObject go, ref List<UnityObjectWithDescription> gameObjectsWithMissingRequiredComponents) {

        var components = go.GetComponents<Component>();

        foreach (var component in components) {

            if (component is null) {
                var str = $"{go.transform.GetPath()}: one of components is missing";
                gameObjectsWithMissingRequiredComponents.Add(new UnityObjectWithDescription(go, str));
                continue;
            }
            var attributes = component.GetType().GetCustomAttributes(inherit: true);
            foreach (var attribute in attributes) {
                if (attribute is RequireComponent requireComponentAttribute) {
                    bool exists = false;
                    foreach (var c in components) {
                        if (requireComponentAttribute.m_Type0.IsInstanceOfType(c)) {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists) {
                        var str = $"{go.transform.GetPath()} . {component.name} missing: {requireComponentAttribute.m_Type0.Name}";
                        if (requireComponentAttribute.m_Type1 != null) {
                            str += $" {requireComponentAttribute.m_Type1.Name}";
                        }
                        if (requireComponentAttribute.m_Type2 != null) {
                            str += $" {requireComponentAttribute.m_Type2.Name}";
                        }
                        gameObjectsWithMissingRequiredComponents.Add(new UnityObjectWithDescription(go, str));
                    }
                }
            }
        }
    }
}
