namespace BGLib.UnityExtension.Editor {
    
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Relative of UnityEngine.PrefabUtility, this class contains additional and related calls to make working with prefabs in editor time easier.
    /// </summary>
    public static class PrefabUtilityExtension {

        /// <summary>
        /// UnityEngine's version of this call does not detect whether you're editing a variant prefab in the PrefabStage.<br/>
        /// By using GetCorrespondingObjectFromOriginalSource and comparing the results, we can work around this issue.
        /// </summary>
        public static bool IsPartOfVariantPrefab(Object componentOrGameObject) {
            
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null) {
                return UnityEditor.PrefabUtility.IsPartOfVariantPrefab(componentOrGameObject);
            }

            var sourceObject = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(componentOrGameObject);
            return sourceObject != null && sourceObject != componentOrGameObject;
        }

        /// <summary>
        /// Simple check to see if this gameObject is loaded in a normal scene or in a prefab preview scene
        /// </summary>
        public static bool IsGameObjectLoadedInPrefabPreviewScene(GameObject go) {
            
            return go.scene.path == AssetDatabase.GetAssetPath(EditorSettings.prefabUIEnvironment) ||
                go.scene.path == AssetDatabase.GetAssetPath(EditorSettings.prefabRegularEnvironment);
        }
    }
}