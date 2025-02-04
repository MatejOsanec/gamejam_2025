using BGLib.UnityExtension.Editor;
using UnityEngine;
using UnityEditor;

// Custom Editor for Light Group Components
// Allows to edit variables on the SO directly and immediately see changes applied in the scene.
[CustomEditor(typeof(LightGroup)), CanEditMultipleObjects]
public class LightGroupEditor : Editor {

    bool _enableInlineSOEditing;

    public override void OnInspectorGUI() {

        // First, show default serialized properties before appending our own elements.
        base.OnInspectorGUI();

        LightGroup component = (LightGroup)serializedObject.targetObject;
        if (component.lightGroupSO == null) {
            return;
        }

        _enableInlineSOEditing = EditorGUILayout.BeginFoldoutHeaderGroup(_enableInlineSOEditing, "Inline Scriptable Object Editing");
        if (_enableInlineSOEditing) {
            DrawInlineLightGroupSOEditing(component);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        DrawActions(component);
    }

    private void DrawInlineLightGroupSOEditing(LightGroup component) {

        bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(component);
        bool isVariantPrefab = PrefabUtilityExtension.IsPartOfVariantPrefab(component);

        if (isPrefab) {
            EditorGUILayout.HelpBox("Changes made here get directly applied to the SO and prefab", MessageType.Info);
            EditorGUILayout.Separator();
        }

        if (isVariantPrefab) {
            EditorGUILayout.HelpBox("You are editing a variant prefab\nSome values were automatically corrected to match the light group defined on the parent prefab.", MessageType.Info);

            var sourceObject = PrefabUtility.GetCorrespondingObjectFromOriginalSource(component);
            if (sourceObject != null && (sourceObject.numberOfElements != component.numberOfElements || sourceObject.sameIdElements != component.sameIdElements)) {
                component.lightGroupSO.UpdateParametersEditorTime(numberOfElements: sourceObject.numberOfElements, sameIdElements: sourceObject.sameIdElements);
            }
        }
        
        LightGroupSOEditor soEditor = (LightGroupSOEditor) CreateEditor(component.lightGroupSO);
        soEditor.DrawEditor(false, isVariantPrefab);
    }

    private void DrawActions(LightGroup component) {

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (!PrefabUtilityExtension.IsPartOfVariantPrefab(component)) {
            if (GUILayout.Button("Respawn Lights (Don't use on variant prefabs)", EditorStyles.miniButton)) {
                component.RespawnLights();
            }
        }

        if (GUILayout.Button("Update Light Group (Doesn't Apply Changes)", EditorStyles.miniButton)) {
            component.RefreshContent(true);
        }

        if (GUILayout.Button("Update Light Group (Directly saves to context if prefab)", EditorStyles.miniButton)) {
            component.RefreshContent();
        }
    }
}