using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BloomPrePassBackgroundParticleSystemRenderer : BloomPrePassBackgroundNonLightRendererCore {

    [SerializeField] ParticleSystem _particleSystem = default;

    public override Renderer renderer {
        get {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                if (_renderer == null && _particleSystem != null) {
                    _renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                }
            }
#endif
            return _renderer;
        }
    }

    private Renderer _renderer;

    protected override void Awake() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
#endif
            _renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
#if UNITY_EDITOR
        }
#endif

        // This must be called after we have _renderer
        base.Awake();
    }
}
