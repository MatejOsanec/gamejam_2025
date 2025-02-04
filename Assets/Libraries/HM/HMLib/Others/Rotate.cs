using UnityEngine;
using System.Collections;


public class Rotate : MonoBehaviour {

    public Vector3 _rotationVector = new Vector3(0.0f, 1.0f, 0.0f);
    public float _speed = 1.0f;

    public bool _randomize;
    [DrawIf("_randomize", true)] public Vector3 _randomMinMultiplier = new Vector3(-1.0f, -1.0f, -1.0f);
    [DrawIf("_randomize", true)] public Vector3 _randomMaxMultiplier = new Vector3(1.0f, 1.0f, 1.0f);

    private Transform _transform;
    private Vector3 _startRotationAngles;
    private Vector3 _randomizedMultiplier = Vector3.one;

    protected void Awake() {

        _transform = transform;
        _startRotationAngles = _transform.localEulerAngles;

        // Will rotate only if visible
        if (GetComponent<Renderer>()) {
            enabled = false;
        }
    }

    protected void OnBecameVisible() {

        enabled = true;
        Randomize();
    }

    protected void OnBecameInvisible() {

        enabled = false;
    }

    protected void Update() {

        if (_randomize) {
            var rotation = Vector3.Scale(_randomizedMultiplier, _rotationVector);
            _transform.localEulerAngles = _startRotationAngles + rotation * (_speed * Time.timeSinceLevelLoad);
        }
        else {
            _transform.localEulerAngles = _startRotationAngles + _rotationVector * (_speed * Time.timeSinceLevelLoad);
        }
    }

    protected void Randomize() {
        if (_randomize) {
            _randomizedMultiplier = new Vector3(Random.Range(_randomMinMultiplier.x, _randomMaxMultiplier.x), Random.Range(_randomMinMultiplier.y, _randomMaxMultiplier.y), Random.Range(_randomMinMultiplier.z, _randomMaxMultiplier.z));
        }
    }

}
