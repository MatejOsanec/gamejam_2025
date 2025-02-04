using ModestTree;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasRenderer))]
public class CurvedCanvasSettings : Graphic {

    [SerializeField] float _radius = 175.0f;
    #pragma warning disable 414
    [SerializeField] bool _useFlatInEditMode = default;
    #pragma warning restore 414

    public const float kMaxElementWidth = 10.0f;

#if UNITY_EDITOR
    public bool useFlatInEditMode {
        get => _useFlatInEditMode;
        set => _useFlatInEditMode = value;
    }
#endif

    public float radius {
        get {
#if UNITY_EDITOR
            if (!Application.isPlaying && useFlatInEditMode) {
                return 100000.0f;
            }
#endif
            return _radius;
        }
    }

    public void SetRadius(float value) {

        _radius = value;
        RebuildAndSetup(transform);
    }

    protected override void Start() {

        base.Start();

        Assert.IsNotNull(GetComponent<CanvasRenderer>());
        raycastTarget = false;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
    }

#if UNITY_EDITOR
    protected override void OnValidate() {

        base.OnValidate();
        raycastTarget = false;
        GetComponent<Canvas>().additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
        RebuildAndSetup(transform);
    }
#endif

    // We do this only to fake the bounding box of canvas
    protected override void OnPopulateMesh(VertexHelper vh) {

        var rect = rectTransform.rect;
        var z = transform.position.z;

        vh.Clear();

        vh.AddVert(position:TransformPointFromCanvasTo3D(rect.min), new Color32(), new Vector2());
        vh.AddVert(position:TransformPointFromCanvasTo3D(rect.max), new Color32(), new Vector2());
        vh.AddVert(position:TransformPointFromCanvasTo3D(rect.center), new Color32(), new Vector2());

        vh.AddTriangle(0, 1, 2);
    }

    public Vector3 TransformPointFromCanvasTo3D(Vector2 point) {

        return new Vector3(Mathf.Sin(point.x / radius) * radius, point.y, Mathf.Cos(point.x / radius) * radius - radius);
    }

    private static void RebuildAndSetup(Transform t) {

        var graphic = t.GetComponent<Graphic>();
        if (graphic != null) {
            graphic.SetAllDirty();
        }

        for (int i = 0; i < t.childCount; i++) {
            RebuildAndSetup(t.GetChild(i));
        }
    }
}
}
