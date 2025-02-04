using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

public static class FindComponents {

    public static IEnumerable<UnityObjectWithDescription> FindComponentsInGameObjectsAndChildren<T>(
        this IEnumerable<GameObject> gameObjects,
        bool ignoreSubclasses
    ) where T : Component {

        return gameObjects.SelectMany(go => go.FindComponentsInGameObjectAndChildren<T>(ignoreSubclasses));
    }

    public static IEnumerable<UnityObjectWithDescription> FindComponentsInGameObjectAndChildren<T>(
        this GameObject gameObject,
        bool ignoreSubclasses
    ) where T : Component {

        IEnumerable<T> components = gameObject.GetComponentsInChildren<T>(includeInactive: true);
        if (ignoreSubclasses) {
            components = components.Where(component => component.GetType() == typeof(T));
        }
        return components.Select(
            component => new UnityObjectWithDescription(
                component,
                $"{component.gameObject.name} {component.GetType().Name}"
            )
        );
    }

    public static IList<UnityObjectWithDescription> FindComponentsInGameObjects(
        Type componentType,
        IEnumerable<GameObject> gameObjects,
        bool ignoreSubclasses
    ) {

        var results = new List<UnityObjectWithDescription>();
        foreach (var go in gameObjects) {
            results.AddRange(FindComponentsInGameObject(componentType, go, ignoreSubclasses));
        }

        return results;
    }

    public static IList<UnityObjectWithDescription> FindComponentsInGameObject(
        Type componentType,
        GameObject gameObject,
        bool ignoreSubclasses
    ) {

        var components = gameObject.GetComponents(componentType);
        var results = new List<UnityObjectWithDescription>();

        foreach (var component in components) {
            if (ignoreSubclasses) {
                if (component.GetType() == componentType) {
                    results.Add(
                        new UnityObjectWithDescription(
                            component,
                            $"{component.gameObject.name} {component.GetType().Name}"
                        )
                    );
                }
            }
            else {
                if (component.GetType().IsAssignableFrom(componentType)) {
                    results.Add(
                        new UnityObjectWithDescription(
                            component,
                            $"{component.gameObject.name} {component.GetType().Name}"
                        )
                    );
                }
            }
        }

        return results;
    }
}
