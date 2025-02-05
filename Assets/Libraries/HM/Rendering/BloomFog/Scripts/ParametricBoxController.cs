using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class ParametricBoxController : MonoBehaviour {

    public float width = 1.0f;
    public float height = 1.0f;
    public float length = 1.0f;
    [Range(0.0f, 1.0f)] public float heightCenter = 0.5f;
    public Color color;
    public float alphaMultiplier = 1.0f;
    public float minAlpha = 0.0f;

    [Space]
    [Range(0, 1)] public float alphaStart = 1.0f;
    [Range(0, 1)] public float alphaEnd = 1.0f;
    public float widthStart = 1.0f;
    public float widthEnd = 1.0f;

    public bool useCollision { get; set; }
    public float collisionHeight { get; set; } = float.MaxValue;

    [Space]
    [SerializeField] MeshRenderer _meshRenderer;

    private Transform _transform;
    // https://docs.unity3d.com/Manual/script-Serialization.html Hot reloading section
    // Unity tries to restore private field's value through the domain reload to get good dev experience.
    // This causes _isInitialized to remain true after domain reload while _materialPropertyBlock is static and is set
    // to null during domain reload, creating null pointer exception while calling Refresh in Unity Editor Edit Mode.
    // NonSerialized attribute seems to tell Unity to not do this = fix this issue
    [NonSerialized] private bool _isInitialized;

    private static MaterialPropertyBlock _materialPropertyBlock;

    
    private static readonly int _colorID = Shader.PropertyToID("_Color");

    
    private static readonly int _alphaStartID = Shader.PropertyToID("_AlphaStart");

    
    private static readonly int _alphaEndID = Shader.PropertyToID("_AlphaEnd");

    
    private static readonly int _widthStartID = Shader.PropertyToID("_StartWidth");

    
    private static readonly int _widthEndID = Shader.PropertyToID("_EndWidth");

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _materialPropertyBlock = null;
    }
#endif

    protected void Awake() {

        InitIfNeeded();

        _meshRenderer.enabled = false;
    }

    protected void OnEnable() {

        _meshRenderer.enabled = true;
#if UNITY_EDITOR
        // Do not refresh during normal runtime, to avoid duplicate calls to Refresh (this and TubeBloomPrePassLight)
        Refresh();
#endif
    }

    protected void OnDisable() {

        _meshRenderer.enabled = false;
    }

    public void InitIfNeeded() {

        if (_isInitialized) {
            return;
        }

        _transform = transform;
        if (_materialPropertyBlock == null) {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        _isInitialized = true;
    }

#if UNITY_EDITOR
    private void OnValidate() {

        if (!gameObject.scene.IsValid()) {
            return;
        }

        if (_meshRenderer == null) {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        Refresh();
    }
#endif

    public void Refresh() {

        if (!_isInitialized) {
            return;
        }

        var calculatedHeight = useCollision ? Mathf.Min(collisionHeight, height) : height;

        _transform.localScale = new Vector3(width * 0.5f, calculatedHeight * 0.5f, length * 0.5f);
        _transform.localPosition = new Vector3(0.0f, (0.5f - heightCenter) * calculatedHeight, 0.0f);

        var color = this.color;
        color.a *= alphaMultiplier;
        if (color.a < minAlpha) {
            color.a = minAlpha;
        }

        var interpolatedAlphaEnd = Mathf.Lerp(alphaStart, alphaEnd, Mathf.InverseLerp(0.0f, height, calculatedHeight));

        _materialPropertyBlock.SetColor(_colorID, color);
        _materialPropertyBlock.SetFloat(_alphaStartID, alphaStart);
        _materialPropertyBlock.SetFloat(_alphaEndID, interpolatedAlphaEnd);
        _materialPropertyBlock.SetFloat(_widthStartID, widthStart);
        _materialPropertyBlock.SetFloat(_widthEndID, widthEnd);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
