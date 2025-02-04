namespace BGLib.UnityExtension.Editor {

    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class MultiObjectAssetUtility {

        private const string kUnpackMultiObjectAsset = "Assets/Unpack Multi-object Asset";
        private const string kRemoveSubObjectItemName = "Assets/Remove selected sub object";

        [MenuItem(kUnpackMultiObjectAsset)]
        private static void UnpackMultiObjectAsset() {

            var selection = Selection.activeObject;
            if (!AssetDatabase.IsMainAsset(selection)) {
                throw new InvalidOperationException(
                    "Selection object is a sub asset. Select the root object of a multi object asset."
                );
            }
            var selectionPath = AssetDatabase.GetAssetPath(selection);
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(selectionPath);
            if (subAssets.Length == 0) {
                Debug.LogWarning("The selected asset does not have any sub assets.");
                return;
            }
            var selectionDirectory = Path.GetDirectoryName(selectionPath) ?? string.Empty;
            foreach (var subAsset in subAssets) {
                string newPath = Path.Combine(selectionDirectory, subAsset.name + ".asset");
                if (ExistAssetOnPath(newPath)) {
                    newPath = Path.Combine(selectionDirectory, subAsset.name + "(subobject).asset");
                }
                var clone = Object.Instantiate(subAsset);
                AssetDatabase.CreateAsset(clone, newPath);
            }
            AssetDatabase.Refresh();
        }

        private static bool ExistAssetOnPath(string path) {
            
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
        }

        [MenuItem(kUnpackMultiObjectAsset, isValidateFunction: true)]
        private static bool ValidateUnpackMultiObjectAsset() {

            if (Selection.count != 1) {
                return false;
            }
            var selected = Selection.activeObject;
            return selected != null && AssetDatabase.IsMainAsset(selected);
        }

        [MenuItem(kRemoveSubObjectItemName)]
        private static void RemoveSubObject() {

            var selection = Selection.activeObject;
            if (!AssetDatabase.IsSubAsset(selection)) {
                throw new InvalidOperationException(
                    "Selected asset must be a sub object that you want to remove from the main asset."
                );
            }
            var selectionPath = AssetDatabase.GetAssetPath(selection);
            AssetDatabase.RemoveObjectFromAsset(selection);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(selectionPath);
            EditorUtility.SetDirty(mainAsset);
            AssetDatabase.SaveAssetIfDirty(mainAsset);
            AssetDatabase.Refresh();
        }

        [MenuItem(kRemoveSubObjectItemName, isValidateFunction: true)]
        private static bool ValidateRemoveSubObject() {
            
            if (Selection.count != 1) {
                return false;
            }
            var selected = Selection.activeObject;
            return selected != null && AssetDatabase.IsSubAsset(selected);
        }
    }
}
