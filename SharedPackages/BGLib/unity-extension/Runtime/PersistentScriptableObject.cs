using UnityEngine;

public class PersistentScriptableObject : ScriptableObject {

    protected virtual void OnEnable() {

        hideFlags |= HideFlags.DontUnloadUnusedAsset;
    }
}
