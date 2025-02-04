using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightGroupSO : PersistentScriptableObject, ILightGroup {

    [SerializeField][Tooltip("Automatically updated based on file name")]
    string _groupName;

    [SerializeField][TextArea][Tooltip("Only used for own descriptive purposes")]
    string _groupDescription;

    [SerializeField][Min(0)]
    int _groupId;

    [SerializeField][Min(0)]
    int _startLightId;

    [SerializeField][Min(0)]
    int _numberOfElements;

    [SerializeField][Min(1)]
    int _sameIdElements = 1;

    [SerializeField] bool _ignoreLightGroupEffectManager = false;

    public string groupName => _groupName;
    public int groupId => _groupId;
    public int startLightId => _startLightId;
    public int numberOfElements => _numberOfElements;
    public int sameIdElements => _sameIdElements;
    public bool ignoreLightGroupEffectManager => _ignoreLightGroupEffectManager;


#if UNITY_EDITOR
    public void UpdateParametersEditorTime(
        string groupName = null,
        int? groupId = null,
        int? startLightId = null,
        int? numberOfElements = null,
        int? sameIdElements = null,
        bool? ignoreLightGroupEffectManager = null,
        bool? skipSave = null
    ) {
        if (groupName != null) {
            _groupName = groupName;
        }
        if (groupId != null) {
            _groupId = groupId.Value;
        }
        if (startLightId != null) {
            _startLightId = startLightId.Value;
        }
        if (numberOfElements != null) {
            _numberOfElements = numberOfElements.Value;
        }
        if (sameIdElements != null) {
            _sameIdElements = sameIdElements.Value;
        }
        if (ignoreLightGroupEffectManager != null) {
            _ignoreLightGroupEffectManager = ignoreLightGroupEffectManager.Value;
        }

        // todo support undo-redo?
        if (skipSave == null  || skipSave == false) {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    } 
#endif
}
