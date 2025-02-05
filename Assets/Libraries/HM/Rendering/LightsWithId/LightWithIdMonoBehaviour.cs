using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public abstract class LightWithIdMonoBehaviour : MonoBehaviour, ILightWithId {

    [SerializeField] int _ID = -1;

   LightWithIdManager _lightManager = default;

    public int lightId => _ID;
    public bool isRegistered => _isRegistered;

    private bool _isRegistered = false;

    public void __SetIsRegistered() => _isRegistered = true;
    public void __SetIsUnRegistered() => _isRegistered = false;

    public abstract void ColorWasSet(Color color);

    protected virtual void OnEnable() {

        RegisterLight();
    }

    protected virtual void Start() {

        RegisterLight();
    }

    protected virtual void OnDisable() {

        if (_lightManager != null) {
            _lightManager.UnregisterLight(this);
        }
    }

    private void RegisterLight() {

        if (_lightManager == null) {

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                _lightManager = FindObjectOfType<LightWithIdManager>();
                if (_lightManager == null) {
                    return;
                }
            }
            else {
                return;
            }
#else
            return;
#endif
        }

        _lightManager.RegisterLight(this);
    }

    public void SetLightId(int newLightId) {

        if (_lightManager == null) {
            return;
        }

        _lightManager.UnregisterLight(this);
        _ID = newLightId;
        _lightManager.RegisterLight(this);
    }

#if UNITY_EDITOR
    public void __EditorSetLightId(int newLightId) {

        _ID = newLightId;
    }

    void OnDrawGizmos() {

        Handles.Label(transform.position, _ID.ToString());
    }
#endif
}
