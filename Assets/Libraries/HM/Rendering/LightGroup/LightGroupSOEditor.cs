#if UNITY_EDITOR
using UnityEditor;
using System;
using System.IO;

// Custom editor for Light Group Scriptable Object
// Gives some extra data to make it easier to make sweeping changes across light groups.
[CanEditMultipleObjects]
[CustomEditor(typeof(LightGroupSO))]
public class LightGroupSOEditor : Editor {

    SerializedProperty _groupName;
    SerializedProperty _groupDescription;
    LightGroupSO _targetLightGroup;

    public event Action onPropertyChanged;

    protected void OnEnable() {

        _groupName = serializedObject.FindProperty("_groupName");
        _groupDescription = serializedObject.FindProperty("_groupDescription");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // Automatically update group name to be the file name of this SO.
        _targetLightGroup = (LightGroupSO)serializedObject.targetObject;
        string objPath = AssetDatabase.GetAssetPath(_targetLightGroup);
        string fileName = Path.GetFileNameWithoutExtension(objPath);
        if(_targetLightGroup.groupName != fileName) {
            _targetLightGroup.UpdateParametersEditorTime(groupName: fileName);
        }

        EditorGUILayout.HelpBox("Changes made here DO NOT automatically propagate to the Light Group Component in scenes/prefabs. Run `Refresh Light Group` on components using this SO.", MessageType.Warning);
        EditorGUILayout.Separator();

        // Custom inspector fields   
        EditorGUILayout.LabelField("Light Group", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("These are obligatory fields", EditorStyles.miniLabel);
        DrawEditor(showIdentifyingInformation: true, isVariantPrefab: false);
    }

    /// <summary>
    /// Renders the input fields, and statistics. Provide your own header for the first set of fields.<br/>
    /// Before calling, bind to onPropertyChanged if you want an event if any of the properties were changed.
    /// </summary>
    public void DrawEditor(bool showIdentifyingInformation, bool isVariantPrefab = true) {

        if(_targetLightGroup == null) {
            _targetLightGroup = (LightGroupSO)serializedObject.targetObject;
        }
        
        // Custom inspector fields  
        RenderVariantPrefabUnsafeParameters(isVariantPrefab);

        int newStartLightId = EditorGUILayout.DelayedIntField("Start Light Id", _targetLightGroup.startLightId);
        if (newStartLightId != _targetLightGroup.startLightId) {
            _targetLightGroup.UpdateParametersEditorTime(startLightId: newStartLightId);
            onPropertyChanged?.Invoke();
        }
        
        int newGroupId = EditorGUILayout.DelayedIntField(label: "Group Id", _targetLightGroup.groupId);
        if (newGroupId != _targetLightGroup.groupId) {
            _targetLightGroup.UpdateParametersEditorTime(groupId: newGroupId);
            onPropertyChanged?.Invoke();
        }

        bool newIgnoreLightGroupEffectManager = EditorGUILayout.ToggleLeft("Ignore Light Group Effect Manager", _targetLightGroup.ignoreLightGroupEffectManager);
        if (newIgnoreLightGroupEffectManager != _targetLightGroup.ignoreLightGroupEffectManager) {
            _targetLightGroup.UpdateParametersEditorTime(ignoreLightGroupEffectManager: newIgnoreLightGroupEffectManager);
            onPropertyChanged?.Invoke();
        }

        EditorGUILayout.Separator();

        if(showIdentifyingInformation) {
            EditorGUILayout.LabelField("Identifying information", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("These are non-obligatory fields", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Group Name", _groupName.stringValue);
            EditorGUILayout.PropertyField(_groupDescription);
            EditorGUILayout.Separator();
        }

        EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Last light Id: " + (_targetLightGroup.startLightId + _targetLightGroup.numberOfElements - 1).ToString());
        EditorGUILayout.LabelField("Total light instances: " + (_targetLightGroup.numberOfElements * _targetLightGroup.sameIdElements).ToString());
    }

    public void RenderVariantPrefabUnsafeParameters(bool isVariantPrefab) {

        if (isVariantPrefab == false) {
            int newNumberOfElements = EditorGUILayout.DelayedIntField("Number of elements", _targetLightGroup.numberOfElements);
            if (newNumberOfElements != _targetLightGroup.numberOfElements) {
                _targetLightGroup.UpdateParametersEditorTime(numberOfElements: newNumberOfElements);
                onPropertyChanged?.Invoke();
            }

            int newSameIdElements = EditorGUILayout.DelayedIntField("Elements with same Id", _targetLightGroup.sameIdElements);
            if (newSameIdElements != _targetLightGroup.sameIdElements) {
                _targetLightGroup.UpdateParametersEditorTime(sameIdElements: newSameIdElements);
                onPropertyChanged?.Invoke();
            }
        }
        else {
            EditorGUILayout.LabelField("Number of elements", _targetLightGroup.numberOfElements.ToString());
            EditorGUILayout.LabelField("Elements with same Id", _targetLightGroup.sameIdElements.ToString());
        }
    }
}

#endif
