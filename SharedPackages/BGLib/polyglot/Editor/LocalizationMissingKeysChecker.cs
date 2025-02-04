using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LocalizationMissingKeysChecker {

    private const string kNotDefinedKey = "!Not Defined!";

    public delegate void FindMissingLocalizationKeysResultDelegate(IList<UnityObjectWithDescription> localizedReferenceDescriptions, string title);

    public static void FindMissingLocalizationKeysInLoadedScenes(FindMissingLocalizationKeysResultDelegate onComplete) {

        var monoBehaviours = FindUnityObjectsHelper.LoadedScenes.GetAllMonoBehaviours();
        IList<UnityObjectWithDescription> localizedReferenceDescriptions = LocalizationChecker.FindMissingLocalizationKeysInObjects(monoBehaviours,  keysToIgnore: null);

        onComplete?.Invoke(localizedReferenceDescriptions, "Missing Localization Keys");
    }

    public static void FindMissingLocalizationKeysInPrefabs(bool skipNotDefined, FindMissingLocalizationKeysResultDelegate onComplete) {

        EditorUtility.DisplayProgressBar("Missing Localization Keys References Search", "Searching for Missing Localization Keys References in Prefabs", 0.0f);
        var monoBehavioursInAllPrefabs = FindUnityObjectsHelper.AllPrefabs.GetAllMonoBehaviours();
        var ignoredKeys = new HashSet<string>();
        if (skipNotDefined) {
            ignoredKeys.Add(kNotDefinedKey);
        }
        var localizedReferenceDescriptions = LocalizationChecker.FindMissingLocalizationKeysInObjects(monoBehavioursInAllPrefabs, ignoredKeys);

        onComplete?.Invoke(localizedReferenceDescriptions, "Missing Localization Keys");
        EditorUtility.ClearProgressBar();
    }

    public static void FindMissingLocalizationKeysInEditedPrefabs(FindMissingLocalizationKeysResultDelegate onComplete) {

        if (FindUnityObjectsHelper.CurrentPrefab.GetContentRoot() == null) {
            Debug.Log("No prefab currently being edited.");
            return;
        }

        EditorUtility.DisplayProgressBar("Missing Localization Keys References Search", "Searching for Missing Localization Keys References in Prefabs", 0.0f);
        var monoBehaviours = FindUnityObjectsHelper.CurrentPrefab.GetAllMonoBehaviours();
        var localizedReferenceDescriptions = LocalizationChecker.FindMissingLocalizationKeysInObjects(monoBehaviours, keysToIgnore: null);
        onComplete?.Invoke(localizedReferenceDescriptions, "Missing Localization Keys");
        EditorUtility.ClearProgressBar();
    }

    public static void FindMissingLocalizationKeysInAllScripts(FindMissingLocalizationKeysResultDelegate onComplete) {

        var localizedReferenceDescriptions = LocalizationChecker.FindMissingLocalizationKeysInScripts();

        onComplete?.Invoke(localizedReferenceDescriptions, "Missing Localization Keys Scripts");
    }

    public static void FindMissingLocalizationKeysInAllScriptableObjects(FindMissingLocalizationKeysResultDelegate onComplete) {

        var localizedReferenceDescriptions = LocalizationChecker.FindMissingLocalizationKeysInScriptableObjects();

        onComplete?.Invoke(localizedReferenceDescriptions, "Missing Localization Keys in Scriptable Objects");
    }
}
