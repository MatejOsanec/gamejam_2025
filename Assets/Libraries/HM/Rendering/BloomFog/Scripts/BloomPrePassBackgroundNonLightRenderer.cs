using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BloomPrePassBackgroundNonLightRenderer : BloomPrePassBackgroundNonLightRendererCore {

    [SerializeField] Renderer _renderer = default;
    [SerializeField] MeshFilter _meshFilter = default;

    public override Renderer renderer => _renderer;
    public MeshFilter meshFilter => _meshFilter;
    public Transform cachedTransform => _cachedTransform;

    public bool isPartOfInstancedRendering {
        set {
            if (value) {
                Unregister();
            }
            else {
                Register();
            }

            _isPartOfInstancedRendering = value;
        }
    }
    private bool _isPartOfInstancedRendering = false;
    private Transform _cachedTransform;

    protected override void Awake() {

        base.Awake();
        _cachedTransform = transform;
    }

    protected override void OnEnable() {

        if (!_isPartOfInstancedRendering) {
            base.OnEnable();
        }
    }

    protected override void OnValidate() {

        if (isActiveAndEnabled && !_isPartOfInstancedRendering) {
            Register();
        }
        else {
            Unregister();
        }
    }

    public void SetRenderer(Renderer renderer) {

        _renderer = renderer;
    }

    protected override void InitIfNeeded() {

        if (!_isPartOfInstancedRendering) {
            base.InitIfNeeded();
            return;
        }

        if (renderer == null) {
            return;
        }

        if (!_keepDefaultRendering) {
            renderer.enabled = false;
        }
    }
}
