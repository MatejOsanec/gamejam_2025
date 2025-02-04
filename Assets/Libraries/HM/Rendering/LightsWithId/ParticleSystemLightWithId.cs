using UnityEngine;

[ExecuteAlways]
public class ParticleSystemLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] ParticleSystem _particleSystem = default;
    [SerializeField] bool setOnlyOnce = false;
    [SerializeField] bool _setColorOnly = false;
    [SerializeField][DrawIf("_setColorOnly", false)] float _intensity = 1.0f;
    [SerializeField][DrawIf("_setColorOnly", false)] float _minAlpha = 0.0f;

    public Color color => _mainModule.startColor.color;

    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.Particle[] _particles;

    protected void Awake() {

        _mainModule = _particleSystem.main;
        _particles = new ParticleSystem.Particle[_mainModule.maxParticles];
    }

    public override void ColorWasSet(Color color) {

        // To support lightmapping scene force color set
#if UNITY_EDITOR
        if (_particles == null) {
            _mainModule = _particleSystem.main;
            _particles = new ParticleSystem.Particle[_mainModule.maxParticles];
        }
#endif

        color.a = _setColorOnly ? _mainModule.startColor.color.a : Mathf.Max(_minAlpha, color.a * _intensity);

        _mainModule.startColor = new ParticleSystem.MinMaxGradient(color);

        _particleSystem.GetParticles(_particles, _particles.Length);

        for (int i = 0; i < _particleSystem.particleCount; i++) {
            _particles[i].startColor = color;
        }

        _particleSystem.SetParticles(_particles, _particleSystem.particleCount);

        if (setOnlyOnce) {
            enabled = false;
        }
    }
}
