using UnityEngine;
using UnityEngine.Scripting;

public class DisableGCWhileEnabled : MonoBehaviour {

#if !UNITY_EDITOR
    protected void OnEnable() {
        
        GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
    }

    protected void OnDisable() {

        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
    }
#endif
}
