using UnityEngine;

[ExecuteAlways]
public class ParticleSystemLightWithIds : RuntimeLightWithIds {

    [SerializeField] ParticleSystem _particleSystem = default;
    [SerializeField] bool _setOnlyOnce = false;
    [SerializeField] bool _setColorOnly = false;
    [SerializeField][DrawIf("_setColorOnly", false)] float _minAlpha = 0.0f;

    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.Particle[] _particles;

    protected override void Awake() {

        base.Awake();

        _mainModule = _particleSystem.main;
        _particles = new ParticleSystem.Particle[_mainModule.maxParticles];
    }

    protected override void ColorWasSet(Color color) {

        color.a = _setColorOnly ? _mainModule.startColor.color.a : Mathf.Max(_minAlpha, color.a);

        _mainModule.startColor = new ParticleSystem.MinMaxGradient(color);

        _particleSystem.GetParticles(_particles, _particles.Length);

        for (int i = 0; i < _particleSystem.particleCount; i++) {
            _particles[i].startColor = color;
        }

        _particleSystem.SetParticles(_particles, _particleSystem.particleCount);

        if (_setOnlyOnce) {
            enabled = false;
        }
    }
}
