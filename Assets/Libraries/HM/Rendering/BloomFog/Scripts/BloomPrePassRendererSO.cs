using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class BloomPrePassRendererSO : PersistentScriptableObject {

    [SerializeField] BloomFogSO _bloomFog = default;

    [Space]
    [SerializeField] PreallocationData[] _preallocationData = default;

    [System.Serializable]
    private class PreallocationData {
        public BloomPrePassLightTypeSO lightType = default;
        public int preallocateCount = 10;
    }


    private class LightsRenderingData {

        public Mesh mesh;
        public BloomPrePassLight.QuadData[] lightQuads;
        public SubMeshDescriptor subMeshDescriptor;
    }

    private readonly Dictionary<BloomPrePassLightTypeSO, LightsRenderingData> _lightsRenderingData = new Dictionary<BloomPrePassLightTypeSO, LightsRenderingData>();
    private CommandBuffer _commandBuffer;

    [DoesNotRequireDomainReloadInit]
    private static readonly int _vertexTransformMatrixID = Shader.PropertyToID("_VertexTransformMatrix");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _bloomPrePassTextureID = Shader.PropertyToID("_BloomPrePassTexture");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _stereoCameraEyeOffsetID = Shader.PropertyToID("_StereoCameraEyeOffset");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _customFogTextureToScreenRatioID = Shader.PropertyToID("_CustomFogTextureToScreenRatio");

    private bool _initialized;
    private Texture2D _blackTexture;
    private RenderTexture _lowestResBloomTexture;

    protected override void OnEnable() {

        base.OnEnable();
        Init();
    }

    protected void OnDisable() {

        Cleanup();
    }

    public void Init() {

        if (_initialized) {
#if UNITY_EDITOR
            Cleanup();
#else
            return;
#endif
        }

        _initialized = true;

        foreach (var preallocationDataItem in _preallocationData) {
            if (!_lightsRenderingData.TryGetValue(preallocationDataItem.lightType, out var lightsRenderingData)) {
                lightsRenderingData = new LightsRenderingData();
                _lightsRenderingData[preallocationDataItem.lightType] = lightsRenderingData;
            }
            PrepareLightsMeshRendering(preallocationDataItem.lightType, lightsRenderingData, preallocationDataItem.preallocateCount);
        }

        _blackTexture = Texture2D.blackTexture;
    }

    private void Cleanup() {

        if (!_initialized) {
            return;
        }

        foreach (var item in _lightsRenderingData) {
            EssentialHelpers.SafeDestroy(item.Value.mesh);
        }
        _lightsRenderingData.Clear();

        _initialized = false;
    }

    public void RenderAndSetData(
        Vector3 cameraPos,
        Matrix4x4 projectionMatrix,
        Matrix4x4 viewMatrix,
        float stereoCameraEyeOffset,
        IBloomPrePassParams bloomPrePassParams,
        RenderTexture dest,
        out Vector2 textureToScreenRatio,
        out ToneMapping toneMapping
    ) {

        _bloomFog.UpdateShaderParams();

        // Computing texture to screen ratio based on target FOV and current projection matrix FOV.
        textureToScreenRatio.x = Mathf.Clamp01(1.0f / (Mathf.Tan(bloomPrePassParams.fov.x * 0.5f * Mathf.Deg2Rad) * projectionMatrix.m00));
        textureToScreenRatio.y = Mathf.Clamp01(1.0f / (Mathf.Tan(bloomPrePassParams.fov.y * 0.5f * Mathf.Deg2Rad) * projectionMatrix.m11));
        projectionMatrix.m00 *= textureToScreenRatio.x;
        projectionMatrix.m02 *= textureToScreenRatio.x;
        projectionMatrix.m11 *= textureToScreenRatio.y;
        projectionMatrix.m12 *= textureToScreenRatio.y;

        // If some of the renderers requires fog, we provide the black texture.
        EnableBloomFog();
        SetDataToShaders(stereoCameraEyeOffset: 0.0f, textureToScreenRatio: Vector2.one, _blackTexture, bloomPrePassParams.toneMapping);

        // Render lights.
        var lightsTexture = RenderTexture.GetTemporary(bloomPrePassParams.textureWidth, bloomPrePassParams.textureHeight, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Linear);
        try {
            Graphics.SetRenderTarget(lightsTexture);
            GL.Clear(clearDepth: true, clearColor: true, backgroundColor: Color.black);
            RenderAllLights(viewMatrix, projectionMatrix, bloomPrePassParams.linesWidth);

            if (!SystemInfo.usesReversedZBuffer) {
                projectionMatrix.m11 *= -1.0f;
                projectionMatrix.m12 *= -1.0f;
            }

            // Before blur passes
            var beforeBlurPasses = BloomPrePassNonLightPass.bloomPrePassBeforeBlurList;
            foreach (var beforeBlurPass in beforeBlurPasses) {
                beforeBlurPass.Render(lightsTexture, viewMatrix, projectionMatrix);
            }

            // Bloom Blur
            dest.DiscardContents();
            bloomPrePassParams.textureEffect.Render(lightsTexture, dest);
            toneMapping = bloomPrePassParams.toneMapping;
        }
        finally {
            RenderTexture.ReleaseTemporary(lightsTexture);
        }

        // After blur passes
        toneMapping.SetShaderKeyword(); // after blur passes can already use tone mapping
        var afterBlurPasses = BloomPrePassNonLightPass.bloomPrePassAfterBlurList;
        foreach (var afterBlurPass in afterBlurPasses) {
            afterBlurPass.Render(dest, viewMatrix, projectionMatrix);
        }
    }

    public static void SetDataToShaders(float stereoCameraEyeOffset, Vector2 textureToScreenRatio, Texture bloomFogTexture, ToneMapping toneMapping) {

        // Set data for rendering with bloom fog.
        Shader.SetGlobalTexture(_bloomPrePassTextureID, bloomFogTexture);
        Shader.SetGlobalFloat(_stereoCameraEyeOffsetID, stereoCameraEyeOffset);
        Shader.SetGlobalVector(_customFogTextureToScreenRatioID, textureToScreenRatio);
        toneMapping.SetShaderKeyword();
    }

    public void SetCustomStereoCameraEyeOffset(float stereoCameraEyeOffset) {

        Shader.SetGlobalFloat(_stereoCameraEyeOffsetID, stereoCameraEyeOffset);
    }

    public RenderTexture CreateBloomPrePassRenderTextureIfNeeded(RenderTexture renderTexture, IBloomPrePassParams bloomPrePassParams) {

        if (renderTexture != null && renderTexture.width == bloomPrePassParams.textureWidth && renderTexture.height == bloomPrePassParams.textureHeight) {
            return renderTexture;
        }

        if (renderTexture != null) {
            renderTexture.Release();
            EssentialHelpers.SafeDestroy(renderTexture);
        }

        renderTexture = new RenderTexture(bloomPrePassParams.textureWidth, bloomPrePassParams.textureHeight, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Linear);
        renderTexture.name = "BloomRenderTexture";
        return renderTexture;
    }

    public void EnableBloomFog() {

        _bloomFog.bloomFogEnabled = true;
    }

    public void DisableBloomFog() {

        _bloomFog.bloomFogEnabled = false;
    }

    public void UpdateBloomFogParams() {

        _bloomFog.UpdateShaderParams();
    }

    public void GetCameraParams(Camera camera, out Matrix4x4 projectionMatrix, out Matrix4x4 viewMatrix, out float stereoCameraEyeOffset) {

        viewMatrix = camera.worldToCameraMatrix;

        // Stereo camera.
        if (camera.stereoEnabled) {
            Matrix4x4 leftEyeProjectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 rightEyeProjectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            stereoCameraEyeOffset = -(leftEyeProjectionMatrix.m02 - rightEyeProjectionMatrix.m02) * 0.25f;
            projectionMatrix = MatrixLerp(leftEyeProjectionMatrix, rightEyeProjectionMatrix, 0.5f);
        }
        // Non stereo cameras.
        else {
            stereoCameraEyeOffset = 0.0f;
            projectionMatrix = camera.projectionMatrix;
        }
    }

    private void RenderAllLights(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, float linesWidth) {

        var lightsDataItems = BloomPrePassLight.lightsDataItems;
        if (lightsDataItems == null) {
            return;
        }

        if (_commandBuffer == null) {
            _commandBuffer = new CommandBuffer() {
                name = "Bloom Pre Pass Render"
            };
        }
        else {
            _commandBuffer.Clear();
        }

        foreach (var lightDataItem in lightsDataItems) {

            var lightType = lightDataItem.lightType;
            var lights = lightDataItem.lights;

            int lightsCount = lights.Count;

            if (!_lightsRenderingData.TryGetValue(lightType, out var lightsRenderingData)) {
                Debug.LogWarning($"Did not preallocate mesh for {lightType}, generating on the fly.");
                lightsRenderingData = new LightsRenderingData();
                _lightsRenderingData[lightType] = lightsRenderingData;
            }

            PrepareLightsMeshRendering(lightType, lightsRenderingData, lightsCount);

            _commandBuffer.DrawMesh(lightsRenderingData.mesh, Matrix4x4.identity, lightType.material);

            lightType.material.SetMatrix(_vertexTransformMatrixID, Matrix4x4.Ortho(0, 1, 1, 0, -1, 1));

            int renderedLightCount = 0;
            foreach (var light in lights) {
                light.FillMeshData(ref renderedLightCount, lightsRenderingData.lightQuads, viewMatrix, projectionMatrix, linesWidth);
            }

            // Don't render rest of the vertices
            lightsRenderingData.subMeshDescriptor.indexCount = 6 * renderedLightCount;
            lightsRenderingData.mesh.SetSubMesh(index: 0, lightsRenderingData.subMeshDescriptor,
                // skip validation for speed
                flags: MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);

            lightsRenderingData.mesh.SetVertexBufferData(lightsRenderingData.lightQuads, dataStart: 0, meshBufferStart: 0, lightsRenderingData.lightQuads.Length,
                // skip validation for speed
                flags: MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
        }

        Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    private void PrepareLightsMeshRendering(BloomPrePassLightTypeSO lightType, LightsRenderingData data, int numberOfLights) {

        if (data.lightQuads == null || data.lightQuads.Length < numberOfLights) {
            // round our number of lights to the next highest 64 to avoid reallocation
            const int kLightBlockSize = 64;
            numberOfLights = ((numberOfLights + kLightBlockSize - 1) / kLightBlockSize) * kLightBlockSize;
            data.lightQuads = new BloomPrePassLight.QuadData[numberOfLights];
        }

        if (data.mesh && data.mesh.vertexCount >= numberOfLights * 4) {
            return;
        }

        if (data.mesh) {
            Debug.LogWarning($"Reallocating BloomPrePass mesh for {lightType}. " +
                             $"Current Mesh supports {data.mesh.vertexCount / 4} lights, " +
                             $"but need to support {numberOfLights}");
            data.mesh.Clear();
        }
        else {
            data.mesh = new Mesh();
            data.mesh.name = "BloomPrePassRenderer";
            data.mesh.MarkDynamic();
        }

        // verify we aren't allocating more lights than we can address with uint16 index buffer.
        // If this does happen, change triangles back to an int array and set the format to UInt32
        const int maxLightCount = ushort.MaxValue / 4;
        Assert.IsTrue(numberOfLights <= maxLightCount, $"Cannot have more than {maxLightCount} lights, " +
                                                       $"but tried to allocate {numberOfLights}");

        var triangles = new ushort[numberOfLights * 2 * 3];

        for (int i = 0; i < numberOfLights; i++) {

            // First triangle
            triangles[i * 6] = (ushort)(i * 4);
            triangles[i * 6 + 1] = (ushort)(i * 4 + 1);
            triangles[i * 6 + 2] = (ushort)(i * 4 + 2);

            // Second triangle
            triangles[i * 6 + 3] = (ushort)(i * 4 + 2);
            triangles[i * 6 + 4] = (ushort)(i * 4 + 3);
            triangles[i * 6 + 5] = (ushort)(i * 4);

        }

        data.mesh.SetVertexBufferParams(4 * data.lightQuads.Length,
            new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 3));
        data.mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt16);
        data.mesh.SetIndexBufferData(triangles, dataStart: 0, meshBufferStart: 0, triangles.Length);
    }

    private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float t) {

        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            ret[i] = Mathf.Lerp(from[i], to[i], t);
        return ret;
    }
}
