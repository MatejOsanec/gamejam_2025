namespace BeatSaber.UnityExtension.Editor {

    using System;
    using UnityEditor;
    using UnityEditor.ProjectWindowCallback;
    using UnityEngine;

    public static class ScriptableObjectEditorExtensions {

        public static T CreateScriptableObjectInSelectedProjectFolder<T>(string name) where T : ScriptableObject {

            return (T) CreateScriptableObjectInSelectedProjectFolder(typeof(T), name);
        }

        public static ScriptableObject CreateScriptableObjectInSelectedProjectFolder(Type type, string name) {

            var asset = ScriptableObject.CreateInstance(type);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                ScriptableObject.CreateInstance<EndNameEdit>(),
                $"{name}.asset",
                AssetPreview.GetMiniThumbnail(asset),
                null
            );
            return asset;
        }
    }

    public class EndNameEdit : EndNameEditAction {

        public override void Action(int instanceId, string pathName, string resourceFile) {
            AssetDatabase.CreateAsset(
                EditorUtility.InstanceIDToObject(instanceId),
                AssetDatabase.GenerateUniqueAssetPath(pathName)
            );
        }
    }
}
