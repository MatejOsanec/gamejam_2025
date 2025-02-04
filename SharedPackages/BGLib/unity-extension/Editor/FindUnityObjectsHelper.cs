using System;
using System.Collections.Generic;
using System.Linq;
using BGLib.UnityExtension.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.AddressableAssets;
using Object = UnityEngine.Object;

public static class FindUnityObjectsHelper {

    public static readonly string[] kAssetsOnlyFolders = { "Assets" };
    public static readonly string[] kPackagesOnlyFolders = { "Packages" };

    public static List<T> GetComponentsInScene<T>(Scene scene, bool includeInactive = false) {

        var components = new List<T>();
        foreach (var rootGameObject in scene.GetRootGameObjects()) {
            components.AddRange(rootGameObject.GetComponentsInChildren<T>(includeInactive));
        }

        return components;
    }

    public static T GetComponentInScene<T>(Scene scene, bool includeInactive = false) where T : class {

        foreach (var rootGameObject in scene.GetRootGameObjects()) {
            var component = rootGameObject.GetComponentInChildren<T>(includeInactive);
            if (component != null) {
                return component;
            }
        }

        return null;
    }

    public static class LoadedScenes {

        public static IEnumerable<GameObject> GetRootGameObjects() {

            var tmpGoList = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                SceneManager.GetSceneAt(i).GetRootGameObjects(tmpGoList);
                foreach (var go in tmpGoList) {
                    yield return go;
                }
            }
        }

        public static IReadOnlyList<GameObject> GetAllGameObjects() {

            return Object.FindObjectsOfType<GameObject>(includeInactive: true);
        }

        public static IReadOnlyList<MonoBehaviour> GetAllMonoBehaviours() {

            return Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true);
        }

        public static IEnumerable<T> GetAllComponents<T>(bool includeInactive = false) {

            return GetRootGameObjects().GetComponentsInChildren<T>(includeInactive);
        }
    }

    public static class AllPrefabs {

        public static IReadOnlyCollection<GameObject> GetAllGameObjects() {

            return AssetDatabaseExtensions.FindAssetEntries<GameObject>("t:Prefab")
                .Where(assetEntry => assetEntry.isBeatAsset)
                .LoadAll()
                .ToArray();
        }

        public static IEnumerable<MonoBehaviour> GetAllMonoBehaviours() {

            return GetAllComponents<MonoBehaviour>();
        }

        public static IEnumerable<T> GetAllComponents<T>() {

            return GetAllGameObjects().GetComponents<T>();
        }
    }

    public static class AddressablePrefabs {

        public static IEnumerable<GameObject> GetRootGameObjects() {

            var addressableGroups = AddressableAssetSettingsDefaultObject.Settings.groups;
            foreach (var addressableGroup in addressableGroups) {
                if (addressableGroup == null) {
                    continue;
                }
                foreach (var entry in addressableGroup.entries) {
                    if (entry.MainAsset is GameObject gameObject) {
                        yield return gameObject;
                    }
                }
            }
        }

        public static IEnumerable<MonoBehaviour> GetAllMonoBehaviours() {

            return GetAllComponents<MonoBehaviour>();
        }

        public static IEnumerable<T> GetAllComponents<T>() {

            return GetRootGameObjects().GetComponentsInChildren<T>();
        }
    }

    public static class CurrentPrefab {

        public static GameObject GetContentRoot() {

            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null ? prefabStage.prefabContentsRoot : null;
        }

        public static IEnumerable<GameObject> GetRootGameObjects() {

            var gameObject = GetContentRoot();
            if (gameObject != null) {
                yield return gameObject;
            }
        }

        public static IEnumerable<GameObject> GetAllGameObjects() {

            return GetRootGameObjects().GetGameObjectsInChildren();
        }

        public static IEnumerable<MonoBehaviour> GetAllMonoBehaviours() {

            return GetRootGameObjects().GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
        }
    }

    private static IEnumerable<T> GetComponentsInChildren<T>(
        this IEnumerable<GameObject> source,
        bool includeInactive = false
    ) {

        var tmpComponentList = new List<T>();
        foreach (var go in source) {
            go.GetComponentsInChildren(includeInactive, tmpComponentList);
            foreach (var component in tmpComponentList) {
                yield return component;
            }
        }
    }

    private static IEnumerable<GameObject> GetGameObjectsInChildren(this IEnumerable<GameObject> source) {

        var tmpComponentList = new List<Transform>();
        foreach (var go in source) {
            go.GetComponentsInChildren(includeInactive: true, tmpComponentList);
            foreach (var component in tmpComponentList) {
                yield return component.gameObject;
            }
        }
    }

    private static IEnumerable<T> GetComponents<T>(this IEnumerable<GameObject> source) {

        var tmpComponentList = new List<T>();
        foreach (var go in source) {
            go.GetComponents<T>(tmpComponentList);
            foreach (var component in tmpComponentList) {
                yield return component;
            }
        }
    }

    public static IEnumerable<ScriptableObject> FindAllScriptableObjectsInProject() {

        return AssetDatabaseExtensions.FindAssetEntries<ScriptableObject>().LoadAll();
    }

    public static T FindObjectWithUniqueName<T>(string assetName, string[] searchInFolders = null)
        where T : Object {

        try {
            return AssetDatabaseExtensions.FindAssetEntriesWithName<T>(assetName, searchInFolders)
                .LoadAll()
                .Single();
        }
        catch (InvalidOperationException innerException) {
            throw new InvalidOperationException(
                $"Failed to find exactly one object called '{assetName}' of type {typeof(T).Name}",
                innerException
            );
        }
    }

    public static T FindObjectWithUniqueNameOrDefault<T>(string assetName, string[] searchInFolders = null)
        where T : Object {

        try {
            return AssetDatabaseExtensions.FindAssetEntriesWithName<T>(assetName, searchInFolders)
                .LoadAll()
                .SingleOrDefault();
        }
        catch (InvalidOperationException innerException) {
            throw new InvalidOperationException(
                $"Failed to find a single object called '{assetName}' of type {typeof(T).Name}",
                innerException
            );
        }
    }

    public static T FindObjectByType<T>(string[] searchInFolders = null) where T : Object {

        return AssetDatabaseExtensions.FindAssetEntries<T>(searchInFolders).Single().Load();
    }

    public static IEnumerable<T> FindAllObjectsByType<T>(string[] searchInFolders = null) where T : Object {

        return AssetDatabaseExtensions.FindAssetEntries<T>(searchInFolders).LoadAll();
    }

    public static bool HasScene(string sceneName) {

        return AssetDatabaseExtensions.FindAssetEntries<SceneAsset>(sceneName).Any();
    }

    public static string FindScenePath(string sceneName) {

        try {
            return AssetDatabaseExtensions.FindAssetEntriesWithName<SceneAsset>(sceneName).Single().path;
        }
        catch (InvalidOperationException innerException) {
            throw new InvalidOperationException(
                $"Failed to find a single scene called '{sceneName}'",
                innerException
            );
        }
    }
}
