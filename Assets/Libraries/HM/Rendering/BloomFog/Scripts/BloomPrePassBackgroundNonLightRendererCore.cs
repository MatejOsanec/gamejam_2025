using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

[ExecuteAlways]
public abstract class BloomPrePassBackgroundNonLightRendererCore : BloomPrePassNonLightPass {

    [SerializeField] protected bool _keepDefaultRendering = default;
    [SerializeField] bool _useCustomMaterial = false;
    [SerializeField] [DrawIf("_useCustomMaterial", true)] [NullAllowed] Material _customMaterial = default;
    [SerializeField] bool _useCustomPropertyBlock = false;

    [DoesNotRequireDomainReloadInit]
    private static readonly int _worldSpaceCameraPosID = Shader.PropertyToID("_WorldSpaceCameraPos");

    private CommandBuffer _commandBuffer = default;

    [DoesNotRequireDomainReloadInit]
    static MaterialPropertyBlock _materialPropertyBlock;

    private MaterialPropertyBlock _customPropertyBlock = default;

#pragma warning disable 109
    public new abstract Renderer renderer { get; }
#pragma warning restore 109

    public bool useCustomMaterial => _useCustomMaterial;
    public Material customMaterial => _customMaterial;

    public void SetCustomPropertyBlock(MaterialPropertyBlock bloomPropertyBlock) {

        _customPropertyBlock = bloomPropertyBlock;
        _materialPropertyBlock ??= new();
    }

    protected virtual void InitIfNeeded() {

        if (renderer == null || !isActiveAndEnabled) {
            return;
        }

        if (!_keepDefaultRendering) {
            renderer.enabled = false;
        }

        if (_commandBuffer == null) {
            _commandBuffer = new CommandBuffer() { name = "BloomPrePassBackgroundNonLightRenderer" };
        }
    }

    protected virtual void Awake() {

        InitIfNeeded();
    }

    public override void Render(RenderTexture dest, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) {

        InitIfNeeded();
        Assert.IsNotNull(renderer, "Attempted to render Bloom Pre Pass Background while no renderer was defined");

        _commandBuffer.Clear();
        _commandBuffer.SetRenderTarget(dest);
        _commandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        _commandBuffer.SetGlobalVector(_worldSpaceCameraPosID, viewMatrix.GetColumn(3)); // The position of camera is not being sent to the shader so we do it manually

        if (_useCustomPropertyBlock && _customPropertyBlock != null) {
            renderer.GetPropertyBlock(_materialPropertyBlock);
            renderer.SetPropertyBlock(_customPropertyBlock);
        }

        // NOTE: This idiot will silently generate garbage if renderer = null. Tread carefully
        _commandBuffer.DrawRenderer(renderer, _useCustomMaterial && _customMaterial ? _customMaterial : renderer.sharedMaterial, submeshIndex: 0, shaderPass: 0);

        if (_useCustomPropertyBlock && _customPropertyBlock != null) {
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }

        Graphics.ExecuteCommandBuffer(_commandBuffer);
    }
}
