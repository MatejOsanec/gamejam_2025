using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Light Group Component
public class LightGroup : MonoBehaviour {

    [SerializeField] LightGroupSO _lightGroupSO;

    public LightGroupSO lightGroupSO => _lightGroupSO;
    public int numberOfElements => lightGroupSO ? lightGroupSO.numberOfElements : 0;
    public int startLightId => lightGroupSO ? lightGroupSO.startLightId : 0;
    public int groupId => lightGroupSO ? lightGroupSO.groupId : -1;
    public int sameIdElements => lightGroupSO ? lightGroupSO.sameIdElements : 0;
    public bool ignoreLightGroupEffectManager => lightGroupSO && lightGroupSO.ignoreLightGroupEffectManager;

   LightWithIdManager _lightWithIdManager = default;

#pragma warning disable CS0067
    public event System.Action<GameObject> respawnEvent;
    public event System.Action<GameObject> didRefreshContentEvent;
#pragma warning restore CS0067

#if UNITY_EDITOR
    protected void OnValidate() {

        _lightWithIdManager = FindObjectOfType<LightWithIdManager>();

        if (lightGroupSO == null) {
            string logMessage = $"[{this.gameObject.name}] Define a Light Group SO, or remove the Light Group Component from the Game Object.";
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
            if (path != null && path.Length > 0) {
                logMessage += "\n" + path;
            }
            Debug.LogError(logMessage);

            return;
        }
    }

    public void RefreshContent(bool dontSaveIfPrefab = false) {

        if (Application.isPlaying) {
            return;
        }

        GameObject prefabContext = GetPrefabContext();
        if (prefabContext != null && dontSaveIfPrefab == false) {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabContext);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path)) {
                var lightGroupInPrefab = FindCurrentLightGroupInPrefabInstance(editingScope.prefabContentsRoot);
                if (lightGroupInPrefab == null) {
                    Debug.LogWarning($"[{_lightGroupSO.name}] Light Group Component not found in prefab instance. Not Updating lights.");
                    return;
                }

                lightGroupInPrefab.didRefreshContentEvent?.Invoke(lightGroupInPrefab.gameObject);
            }
        } else {
            didRefreshContentEvent?.Invoke(gameObject);
            EditorUtility.SetDirty(gameObject);
        }
    }

    ///<summary>
    /// Only triggers a respawn of all the lights through Light Group Element Spawner
    ///</summary>
    public void RespawnLights() {

        if (Application.isPlaying) {
            return;
        }

        if (PrefabUtility.IsPartOfPrefabInstance(transform)) {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath)) {
                var lightGroupInPrefab = FindCurrentLightGroupInPrefabInstance(editingScope.prefabContentsRoot);
                if (lightGroupInPrefab == null) {
                    Debug.LogWarning($"[{_lightGroupSO.name}] Light Group Component not found in prefab instance. Not respawning lights.");
                    return;
                }

                lightGroupInPrefab.respawnEvent?.Invoke(lightGroupInPrefab.gameObject);
            }
        }
        else {
            respawnEvent?.Invoke(gameObject);
            EditorUtility.SetDirty(gameObject);
        }
    }

    public GameObject GetPrefabContext() {

        if (!PrefabUtility.IsPartOfPrefabInstance(gameObject)) {
            return null;
        }

        Transform current = transform;
        while (current != null) {
            // This component is _always_ on prefab root, so no need to verify this anymore.
            LightGroupEditorPrefabContext context = current.GetComponent<LightGroupEditorPrefabContext>();
            if (context != null && context.lightGroups.Contains(lightGroupSO)) {
                return current.gameObject;
            }

            current = current.parent;
        }

        return PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
    }

    ///<summary>
    /// Find the Light Group Component in the prefab instance that matches the current Light Group Component, uses GroupId to compare.
    ///</summary>
    private LightGroup FindCurrentLightGroupInPrefabInstance(GameObject gameObject) {

        var lightGroups = gameObject.GetComponentsInChildren<LightGroup>();
        foreach (LightGroup lightGroup in lightGroups) {
            if (lightGroup.lightGroupSO.groupId == groupId) {
                return lightGroup;
            }
        }

        return null;
    }
#endif

    public void SetColor(Color color) {

        for (int lightId = startLightId; lightId < startLightId + numberOfElements; lightId++) {
            _lightWithIdManager.SetColorForId(lightId, color);
        }
    }

}
