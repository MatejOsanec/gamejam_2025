using UnityEngine;
using UnityEngine.Rendering;

// Add this component to gameobject and it will receive OnWillRenderObject calls for all cameras rendering this gameobject's layer.
[ExecuteInEditMode]
public class OnWillRenderObjectTrigger : MonoBehaviour {

    [SerializeField] [NullAllowed] Shader _overrideShader = default;
    [SerializeField] int _renderQueue = 0;

    private Material _material = null;
    private Mesh _mesh = null;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    protected void OnEnable() {

        if (_material == null) {

            _material = new Material(_overrideShader != null ? _overrideShader : Shader.Find("Diffuse"));
            _material.renderQueue = _renderQueue;
        }

        if (_mesh == null) {
            _mesh = new Mesh();
            _mesh.name = "Huge Mesh";
            _mesh.hideFlags = HideFlags.HideAndDontSave;
            // Create big bounds for this mesh, so it is always rendered.
            _mesh.bounds = new Bounds(Vector3.zero, new Vector3(9999999.0f, 9999999.0f, 9999999.0f));
        }

        // Create Mesh Renderer and Mesh so OnWillRenderObject is called for all cameras.
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null) {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        _meshRenderer.hideFlags = 0;
        _meshRenderer.sharedMaterial = _material;
        _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        _meshRenderer.allowOcclusionWhenDynamic = false;
        _meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        _meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null) {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        _meshFilter.hideFlags = 0;
        _meshFilter.sharedMesh = _mesh;
    }

    protected void OnDisable() {

        EssentialHelpers.SafeDestroy(_material);
        _material = null;
        EssentialHelpers.SafeDestroy(_mesh);
        _mesh = null;
    }
}
