using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TextMeshProValidatorMenu {

    private delegate List<UnityObjectWithDescription> ValidationMethod(Object[] objects);

    [MenuItem("Tools/TextMeshPro Validation/Find Inconsistent TextMeshPro Font Size/In Loaded Scenes")]
    public static void FindInconsistentFontSizeInLoadedScenes() {

        FindInLoadedScenes(
            TextMeshProValidator.GetInconsistentTextMeshProFontSize,
            windowTitle: "Inconsistent Font Sizes",
            validLogMessage: "No inconsistent font size found."
        );
    }

    [MenuItem("Tools/TextMeshPro Validation/Find Inconsistent TextMeshPro Font Size/In Prefabs")]
    public static void FindInconsistentFontSizeInPrefabs() {

        FindInPrefabs(
            TextMeshProValidator.GetInconsistentTextMeshProFontSize,
            windowTitle: "Inconsistent Font Sizes",
            validLogMessage: "No inconsistent font size found."
        );
    }

    [MenuItem("Tools/TextMeshPro Validation/Find Inconsistent TextMeshPro Font Size/In Edited Prefab")]
    public static void FindInconsistentFontSizeInEditedPrefab() {

        FindInEditedPrefab(
            TextMeshProValidator.GetInconsistentTextMeshProFontSize,
            windowTitle: "Inconsistent Font Sizes",
            validLogMessage: "No inconsistent font size found."
        );
    }

    [MenuItem("Tools/TextMeshPro Validation/Invalid Alignment/In Loaded Scenes")]
    public static void FindInvalidAlignmentInLoadedScenes() {

        FindInLoadedScenes(
            TextMeshProValidator.GetInvalidTextMeshProAlignment,
            "Invalid Text Alignment",
            "No invalid alignment found"
        );
    }

    [MenuItem("Tools/TextMeshPro Validation/Invalid Alignment/In Prefabs")]
    public static void FindInvalidAlignmentInPrefabs() {

        FindInPrefabs(
            TextMeshProValidator.GetInvalidTextMeshProAlignment,
            "Invalid Text Alignment",
            "No invalid alignment found"
        );
    }

    [MenuItem("Tools/TextMeshPro Validation/Invalid Alignment/In Edited Prefab")]
    public static void FindInvalidAlignmentInEditedPrefab() {

        FindInEditedPrefab(
            TextMeshProValidator.GetInvalidTextMeshProAlignment,
            "Invalid Text Alignment",
            "No invalid alignment found"
        );
    }

    private static void FindInLoadedScenes(
        ValidationMethod validationMethod,
        string windowTitle,
        string validLogMessage
    ) {

        FindInObjects(
            FindUnityObjectsHelper.LoadedScenes.GetAllMonoBehaviours(),
            validationMethod,
            windowTitle,
            validLogMessage
        );
    }

    private static void FindInPrefabs(ValidationMethod validationMethod, string windowTitle, string validLogMessage) {

        FindInObjects(
            FindUnityObjectsHelper.AllPrefabs.GetAllComponents<TMP_Text>(),
            validationMethod,
            windowTitle,
            validLogMessage
        );
    }

    private static void FindInEditedPrefab(
        ValidationMethod validationMethod,
        string windowTitle,
        string validLogMessage
    ) {

        if (FindUnityObjectsHelper.CurrentPrefab.GetContentRoot() == null) {
            Debug.Log("No prefab currently being edited.");
            return;
        }

        FindInObjects(
            FindUnityObjectsHelper.CurrentPrefab.GetAllMonoBehaviours(),
            validationMethod,
            windowTitle,
            validLogMessage
        );
    }

    private static void FindInObjects(
        IEnumerable<UnityEngine.Object> objects,
        ValidationMethod validationMethod,
        string windowTitle,
        string validLogMessage
    ) {

        var results = validationMethod(objects.ToArray());

        if (results.Any()) {
            var window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>(windowTitle);
            window.objectDescriptions = results;
        }
        else {
            Debug.Log(validLogMessage);
        }
    }
}
