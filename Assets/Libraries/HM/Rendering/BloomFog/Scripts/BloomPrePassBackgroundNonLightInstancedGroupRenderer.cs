using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BloomPrePassBackgroundNonLightInstancedGroupRenderer: BloomPrePassNonLightPass {

    [SerializeField] BloomPrePassBackgroundNonLightRenderer[] _renderers = default;

    [Space]
    [SerializeField] SupportedProperty[] _supportedProperties = default;

    [Serializable]
    public class SupportedProperty {

        public PropertyType propertyType;
        public string propertyName;
        [NonSerialized] public int propertyId;
    }

    public enum PropertyType {
        Float,
        Vector,
        Color,
        Matrix4x4
    }

    
    private static readonly int _worldSpaceCameraPosID = Shader.PropertyToID("_WorldSpaceCameraPos");

    private const string kInternalMatricesCachingId = "INTERNAL_MATRICES";

    private readonly Dictionary<string, float[]> _reusableFloatArrays = new Dictionary<string, float[]>();
    private readonly Dictionary<string, Vector4[]> _reusableVectorArrays = new Dictionary<string, Vector4[]>();
    private readonly Dictionary<string, Matrix4x4[]> _reusableMatrixArrays = new Dictionary<string, Matrix4x4[]>();

    private int _reusableArraysSize = 0;
    private CommandBuffer _commandBuffer = default;
    private MaterialPropertyBlock _reusableSetMaterialPropertyBlock;
    private MaterialPropertyBlock _reusableGetMaterialPropertyBlock;

    protected void Awake() {

        InitIfNeeded();
    }

#if UNITY_EDITOR
    protected override void OnEnable() {

        base.OnEnable();
        InitIfNeeded();
    }

    protected override void OnValidate() {

        InitIfNeeded();
    }
#endif

    private void InitIfNeeded() {

        if (_commandBuffer == null) {
            _commandBuffer = new CommandBuffer() { name = "BloomPrePassBackgroundNonLightInstancedRenderer" };
        }

        foreach (var supportedProperty in _supportedProperties) {
            supportedProperty.propertyId = Shader.PropertyToID(supportedProperty.propertyName);
        }

        foreach (var currentRenderer in _renderers) {
            currentRenderer.isPartOfInstancedRendering = true;
        }

        if (_reusableArraysSize != _renderers.Length) {
            _reusableFloatArrays.Clear();
            _reusableVectorArrays.Clear();
            _reusableMatrixArrays.Clear();
            _reusableArraysSize = _renderers.Length;
            _reusableSetMaterialPropertyBlock = new MaterialPropertyBlock();
            _reusableGetMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        if (_reusableGetMaterialPropertyBlock == null) {
            _reusableSetMaterialPropertyBlock = new MaterialPropertyBlock();
            _reusableGetMaterialPropertyBlock = new MaterialPropertyBlock();
        }
    }

    public override void Render(RenderTexture dest, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) {

#if UNITY_EDITOR
        InitIfNeeded();
#endif
        if (_renderers.Length == 0) {
            return;
        }

        var mesh = _renderers[0].meshFilter.sharedMesh;
        var material = _renderers[0].useCustomMaterial ? _renderers[0].customMaterial : _renderers[0].renderer.sharedMaterial;

        _commandBuffer.Clear();
        _commandBuffer.SetRenderTarget(dest);
        _commandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        _commandBuffer.SetGlobalVector(_worldSpaceCameraPosID, viewMatrix.GetColumn(3)); // The position of camera is not being sent to the shader so we do it manually

        if (_renderers.Length == 1) {
            Debug.LogWarning("Using BloomPrePassBackgroundNonLightInstancedRenderingSystem to render single Renderer, this add extra overhead with no benefit");
            _commandBuffer.DrawRenderer(_renderers[0].renderer, material, submeshIndex: 0, shaderPass: 0);
        }
        else {
            Matrix4x4[] matrices = GetCachedMatrixArray(kInternalMatricesCachingId);

            for (int i = 0; i < _renderers.Length; i++) {
                var currentRenderer = _renderers[i];
                if (currentRenderer.isActiveAndEnabled) {
                    matrices[i] = currentRenderer.cachedTransform.localToWorldMatrix;
                }
                else {
                    matrices[i] = Matrix4x4.zero;
                }

#if UNITY_EDITOR
                Assert.IsTrue(mesh == currentRenderer.meshFilter.sharedMesh);
                Assert.IsTrue(material == currentRenderer.useCustomMaterial ? currentRenderer.customMaterial : currentRenderer.renderer.sharedMaterial);
                Assert.IsTrue(executionTimeType == currentRenderer.executionTimeType);
                Assert.IsTrue(currentRenderer.renderer.sharedMaterials.Length == 1);
#endif
                currentRenderer.renderer.GetPropertyBlock(_reusableGetMaterialPropertyBlock);

                foreach (var property in _supportedProperties) {
                    switch (property.propertyType) {
                        case PropertyType.Vector: {
                            var array = GetCachedVectorArray(property.propertyName);
                            array[i] = _reusableGetMaterialPropertyBlock.GetVector(property.propertyId);
                            break;
                        }
                        case PropertyType.Color: {
                            var array = GetCachedVectorArray(property.propertyName);
                            array[i] = _reusableGetMaterialPropertyBlock.GetColor(property.propertyId);
                            break;
                        }
                        case PropertyType.Matrix4x4: {
                            var array = GetCachedMatrixArray(property.propertyName);
                            array[i] = _reusableGetMaterialPropertyBlock.GetMatrix(property.propertyId);
                            break;
                        }
                        case PropertyType.Float: {
                            var array = GetCachedFloatArray(property.propertyName);
                            array[i] = _reusableGetMaterialPropertyBlock.GetFloat(property.propertyId);
                            break;
                        }
                    }
                }
            }

            foreach (var property in _supportedProperties) {
                switch (property.propertyType) {
                    case PropertyType.Vector:
                    case PropertyType.Color: {
                        var array = GetCachedVectorArray(property.propertyName);
                        _reusableSetMaterialPropertyBlock.SetVectorArray(property.propertyId, array);
                        break;
                    }
                    case PropertyType.Matrix4x4: {
                        var array = GetCachedMatrixArray(property.propertyName);
                        _reusableSetMaterialPropertyBlock.SetMatrixArray(property.propertyId, array);
                        break;
                    }
                    case PropertyType.Float: {
                        var array = GetCachedFloatArray(property.propertyName);
                        _reusableSetMaterialPropertyBlock.SetFloatArray(property.propertyId, array);
                        break;
                    }
                }
            }

            _commandBuffer.DrawMeshInstanced(mesh, submeshIndex: 0, material, shaderPass: 0, matrices, count: _renderers.Length, _reusableSetMaterialPropertyBlock);
        }

        Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    private Matrix4x4[] GetCachedMatrixArray(string propertyName) {

        if (_reusableMatrixArrays.TryGetValue(propertyName, out var array)) {
            return array;
        }
        array = _reusableMatrixArrays[propertyName] = new Matrix4x4[_renderers.Length];
        return array;
    }

    private float[] GetCachedFloatArray(string propertyName) {

        if (_reusableFloatArrays.TryGetValue(propertyName, out var array)) {
            return array;
        }
        array = _reusableFloatArrays[propertyName] = new float[_renderers.Length];
        return array;
    }

    private Vector4[] GetCachedVectorArray(string propertyName) {

        if (_reusableVectorArrays.TryGetValue(propertyName, out var array)) {
            return array;
        }
        array = _reusableVectorArrays[propertyName] = new Vector4[_renderers.Length];
        return array;
    }

    [ContextMenu("AutoFill Renderers")]
    private void AutoFillRenderers() {
        _renderers = GetComponentsInChildren<BloomPrePassBackgroundNonLightRenderer>();
    }

}
