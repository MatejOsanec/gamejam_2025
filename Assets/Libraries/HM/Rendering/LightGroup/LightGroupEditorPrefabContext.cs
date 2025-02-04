using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightGroupEditorPrefabContext : MonoBehaviour {
    
    public List<LightGroupSO> lightGroups;

#if UNITY_EDITOR
    private void Reset() {

        if (!PrefabUtility.IsPartOfPrefabInstance(transform)) {
            var confirmed = EditorUtility.DisplayDialog("LightGroupEditorPrefabContext", "Can't add LightGroupPrefabContext to this GameObject as it's not part of a prefab", "Confirm");
            if (confirmed) {
                DestroyImmediate(this);
                return;
            }
        }

        if (PrefabUtility.GetNearestPrefabInstanceRoot(gameObject) != gameObject) {
            var confirmed = EditorUtility.DisplayDialog("LightGroupEditorPrefabContext", "Can't add LightGroupPrefabContext to this GameObject as it's not the instance root", "Confirm");
            if (confirmed) {
                DestroyImmediate(this);
                return;
            }
        }

        lightGroups = new List<LightGroupSO>();
    }
#endif
}
