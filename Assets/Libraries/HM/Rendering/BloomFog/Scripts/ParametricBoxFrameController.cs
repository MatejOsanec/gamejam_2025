using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class ParametricBoxFrameController : MonoBehaviour {

    public float width = 1.0f;
    public float height = 1.0f;
    public float length = 1.0f;
    public float edgeSize = 0.1f;
    public Color color;

    public Vector3 localPosition {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

    [SerializeField] MeshRenderer _meshRenderer = default;
    [SerializeField] MaterialPropertyBlockController _materialPropertyBlockController = default;

    
    private static readonly int _colorID = Shader.PropertyToID("_Color");

    
    private static readonly int _sizeParamsID = Shader.PropertyToID("_SizeParams");

    protected void Awake() {

        _meshRenderer.enabled = false;
    }

    protected void OnEnable() {

        Refresh();
        _meshRenderer.enabled = true;
    }

    protected void OnDisable() {

        _meshRenderer.enabled = false;
    }

#if UNITY_EDITOR
    private void OnValidate() {

        if (!gameObject.scene.IsValid()) {
            return;
        }

        Refresh();
    }
#endif

    public void Refresh() {

        var v = new Vector4(width * 0.5f, height * 0.5f, length * 0.5f, edgeSize * 0.5f);

        transform.localScale = v;

        var mpb = _materialPropertyBlockController.materialPropertyBlock;

        mpb.SetColor(_colorID, color);
        mpb.SetVector(_sizeParamsID, v);

        _materialPropertyBlockController.ApplyChanges();
    }
}
