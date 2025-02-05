using UnityEngine;

public sealed class BloomPrePassLightsUpdateSystem : MonoBehaviour {

    
    public static bool disableUpdateAlways = false;

    private void LateUpdate() {

        var allLights = BloomPrePassLight.bloomLightsDict;
        foreach (var kvp in allLights) {
            foreach (var prePassLight in kvp.Value) {
                prePassLight.Refresh();
            }
        }
    }
}
