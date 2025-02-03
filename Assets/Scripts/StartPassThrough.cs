using System.Collections;
using UnityEngine;

/// Going to VR is way faster than starting up MR each time, this allows to quickly switch between MR and VR
public class StartPassThrough : MonoBehaviour {

    [SerializeField] bool _enablePassthrough = true;
    [SerializeField] Camera _mainCamera;

    protected IEnumerator Start() {

        if (_enablePassthrough) {
            _mainCamera.backgroundColor = new Color(0,0,0,0);
        }
        // Hacky way to make it work with domain reload disabled
        OVRManager.instance.isInsightPassthroughEnabled = false;
        if (_enablePassthrough) {
            yield return null;
            OVRManager.instance.isInsightPassthroughEnabled = true;
        }
    }
}
