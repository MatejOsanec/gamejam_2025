using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class Parametric3SliceSpriteController : MonoBehaviour {

    [SerializeField] float _widthMultiplier = 1.0f;

    public float width = 0.5f;
    public float length = 1.0f;
    public float center = 0.5f;
    public Color color;
    public float alphaMultiplier = 1.0f;
    public float minAlpha = 0.0f;

    [Space]
    [Min(0)] public float alphaStart = 1.0f;
    [Min(0)] public float alphaEnd = 1.0f;
    [Min(0)] public float widthStart = 1.0f;
    [Min(0)] public float widthEnd = 1.0f;

    public bool useCollision { get; set; }
    public float collisionLength { get; set; } = float.MaxValue;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    // https://docs.unity3d.com/Manual/script-Serialization.html Hot reloading section
    // Unity tries to restore private field's value through the domain reload to get good dev experience.
    // This causes _isInitialized to remain true after domain reload while _materialPropertyBlock is static and is set
    // to null during domain reload, creating null pointer exception while calling Refresh in Unity Editor Edit Mode.
    // NonSerialized attribute seems to tell Unity to not do this = fix this issue
    [NonSerialized] private bool _isInitialized = false;

    private const float kMaxWidth = 10.0f;
    private const float kMaxLength = 2500.0f;

    [DoesNotRequireDomainReloadInit]
    private static readonly int _colorID = Shader.PropertyToID("_Color");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _sizeParamsID = Shader.PropertyToID("_SizeParams");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _alphaStartID = Shader.PropertyToID("_AlphaStart");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _alphaEndID = Shader.PropertyToID("_AlphaEnd");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _widthStartID = Shader.PropertyToID("_StartWidth");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _widthEndID = Shader.PropertyToID("_EndWidth");

    private static MaterialPropertyBlock _materialPropertyBlock;
    private static Mesh _mesh;
    private static int _instanceCount = 0;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _mesh = null;
        _instanceCount = 0;
        _materialPropertyBlock = null;
    }

    public void InitEditor() {

        Awake();
        _meshRenderer.enabled = true;
    }
#endif

    protected void Awake() {

        InitIfNeeded();

        _meshRenderer.enabled = false;

        if (_mesh == null) {
            _mesh = _meshFilter.sharedMesh;
            if (_mesh == null) {
                _mesh = CreateMesh();
            }
        }

        _instanceCount++;
    }

    protected void Start() {

        _meshFilter.sharedMesh = _mesh;
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

    protected void OnDestroy() {

        _instanceCount--;
        if (_instanceCount < 0) {
            _instanceCount = 0;
        }

        if (_instanceCount <= 0) {
            EssentialHelpers.SafeDestroy(_mesh);
        }
    }

    public void InitIfNeeded() {

        if (_isInitialized) {
            return;
        }
        _isInitialized = true;

        _meshFilter = GetComponent<MeshFilter>();
        Assert.IsNotNull(_meshFilter);

        _meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsNotNull(_meshRenderer);

        if (_materialPropertyBlock == null) {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
    }

#if UNITY_EDITOR
    private void OnValidate() {

        if (!gameObject.scene.IsValid()) {
            return;
        }

        width = Mathf.Clamp(width, 0.0f, kMaxWidth);
        length = Mathf.Clamp(length, 0.0f, kMaxLength);

        InitIfNeeded();
        Refresh();
    }
#endif

    private Mesh CreateMesh() {

        var mesh = new Mesh();
        mesh.name = "Dynamic3SliceSprite";

        Vector3[] vertices = {
            new Vector3(-1.0f, 0.0f, 0.0f), // 0
            new Vector3(1.0f, 0.0f, 0.0f), // 1
            new Vector3(-1.0f, 0.0f, 0.0f), // 2
            new Vector3(1.0f, 0.0f, 0.0f), // 3
            new Vector3(-1.0f, 1.0f, 0.0f), // 4
            new Vector3(1.0f, 1.0f, 0.0f), // 5
            new Vector3(-1.0f, 1.0f, 0.0f), // 6
            new Vector3(1.0f, 1.0f, 0.0f), // 7
        };

        int[] triangles = {
            0, 2, 1,
            1, 2, 3,
            2, 4, 3,
            3, 4, 5,
            4, 6, 5,
            5, 6, 7,
        };

        Vector2[] uv = {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.25f),
            new Vector2(1.0f, 0.25f),
            new Vector2(0.0f, 0.75f),
            new Vector2(1.0f, 0.75f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.bounds = new Bounds(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(kMaxWidth, kMaxLength * 2.0f, 0.1f));
        mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;

        return mesh;
    }

    public void Refresh() {

        if (!_isInitialized) {
            return;
        }

        var processedColor = this.color;
        processedColor.a *= alphaMultiplier;
        if (processedColor.a < minAlpha) {
            processedColor.a = minAlpha;
        }

        //color.a = Mathf.GammaToLinearSpace(color.a);

        var calculatedLength = useCollision ? Mathf.Min(collisionLength, length) : length;
        var interpolatedAlphaEnd = Mathf.Lerp(alphaStart, alphaEnd, Mathf.InverseLerp(0.0f, length, calculatedLength));

        _materialPropertyBlock.SetColor(_colorID, processedColor);
        _materialPropertyBlock.SetFloat(_alphaStartID, alphaStart);
        _materialPropertyBlock.SetFloat(_alphaEndID, interpolatedAlphaEnd);
        _materialPropertyBlock.SetFloat(_widthStartID, widthStart);
        _materialPropertyBlock.SetFloat(_widthEndID, widthEnd);
        _materialPropertyBlock.SetVector(_sizeParamsID, new Vector4(width * _widthMultiplier, calculatedLength, center, width * 2.0f * _widthMultiplier));

        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
