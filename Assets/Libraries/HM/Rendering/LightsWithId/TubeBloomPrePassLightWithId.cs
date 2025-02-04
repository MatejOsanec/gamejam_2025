using UnityEngine;

public class TubeBloomPrePassLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] TubeBloomPrePassLight _tubeBloomPrePassLight = default;
    [SerializeField] bool _setOnlyOnce = false;
    [SerializeField] bool _setColorOnly = false;

    public Color color => _tubeBloomPrePassLight.color;

#if UNITY_EDITOR
    public event System.Action didValidateEvent;
#endif

    public override void ColorWasSet(Color color) {

        if (_setColorOnly) {
            color.a = _tubeBloomPrePassLight.color.a;
        }

        _tubeBloomPrePassLight.color = color;

        if (_setOnlyOnce) {
            enabled = false;
        }
    }

#if UNITY_EDITOR
    protected void OnValidate() {
        didValidateEvent?.Invoke();
    }
#endif
}
