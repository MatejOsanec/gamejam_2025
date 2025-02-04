using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NullReferenceCheckerMenu {

    [MenuItem("Tools/Find Null References/In Loaded Scenes")]
    private static void FindNullReferencesInLoadedScenes() {

        FindNullReferencesInObjects(
            FindUnityObjectsHelper.LoadedScenes.GetAllMonoBehaviours,
            NullAllowed.Context.Everywhere,
            searchContext: "ScriptableObjects"
        );
    }

    [MenuItem("Tools/Find Null References/In ScriptableObjects")]
    private static void FindNullReferencesInScriptableObjects() {

        FindNullReferencesInObjects(
            FindUnityObjectsHelper.FindAllScriptableObjectsInProject,
            NullAllowed.Context.Everywhere,
            "ScriptableObjects"
        );
    }

    [MenuItem("Tools/Find Null References/In Edited Prefab")]
    private static void FindNullReferencesInEditedPrefab() {

        FindNullReferencesInObjects(
            FindUnityObjectsHelper.CurrentPrefab.GetAllMonoBehaviours,
            NullAllowed.Context.Prefab,
            "edited Prefab"
        );
    }

    [MenuItem("Tools/Find Null References/In Edited Prefab", isValidateFunction: true)]
    private static bool FindNullReferencesInEditedPrefab_Validate() {

        return FindUnityObjectsHelper.CurrentPrefab.GetContentRoot() != null;
    }

    [MenuItem("Tools/Find Null References/In Addressable Prefabs")]
    private static void FindNullReferencesInAddressablePrefabs() {

        FindNullReferencesInObjects(
            FindUnityObjectsHelper.AddressablePrefabs.GetAllMonoBehaviours,
            NullAllowed.Context.Everywhere,
            "Addressable Prefabs"
        );
    }

    [MenuItem("Tools/Find Null References/In Selection")]
    private static void FindNullReferencesInSelection() {

        var objects = new List<UnityEngine.Object>(Selection.objects);

        FindNullReferencesInObjects(
            () => objects,
            NullAllowed.Context.Everywhere,
            "Selection"
        );
    }

    private static void FindNullReferencesInObjects(
        Func<IEnumerable<UnityEngine.Object>> objectsFactory,
        NullAllowed.Context nullAllowedContext,
        string searchContext
    ) {

        EditorUtility.DisplayProgressBar(
            "Null References Search",
            $"Searching for null references in {searchContext}",
            0.0f
        );

        var nullReferenceDescriptions = NullReferencesChecker.FindNullReferencesInObjects(
            objectsFactory(),
            nullAllowedContext
        );

        if (nullReferenceDescriptions.Count > 0) {
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Null References");
            window.objectDescriptions = nullReferenceDescriptions.ToArray();
        }
        else {
            Selection.activeObject = null;
            Debug.Log($"All references in {searchContext} are OK.");
        }

        EditorUtility.ClearProgressBar();
    }
}
