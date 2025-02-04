using System;
using UnityEngine;

[SelectionBase]
public class TubeBloomPrePassLight : BloomPrePassLight {

    [SerializeField] BoolSO _mainEffectPostProcessEnabled = default;
    [SerializeField] float _width = 0.5f;
    [SerializeField] bool _overrideChildrenLength = true;
    [SerializeField] float _length = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _center = 0.5f;
    [SerializeField] Color _color = default;
    [SerializeField] float _colorAlphaMultiplier = 1.0f;
    [SerializeField] float _bloomFogIntensityMultiplier = 1.0f;
    [SerializeField] float _fakeBloomIntensityMultiplier = 1.0f;
    [SerializeField] float _boostToWhite = 0.0f;
    [SerializeField] [Min(1.0f)] float _lightWidthMultiplier = 1.0f;
    [SerializeField] bool _addWidthToLength = false;
    [SerializeField] bool _thickenWithDistance = false;
    [SerializeField, DrawIf("_thickenWithDistance", true)] AnimationCurve _thickenCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField, DrawIf("_thickenWithDistance", true)] float _minDistance = 30.0f;
    [SerializeField, DrawIf("_thickenWithDistance", true)] float _maxDistance = 200.0f;
    [SerializeField, DrawIf("_thickenWithDistance", true)] float _minWidthMultiplier = 1.0f;
    [SerializeField, DrawIf("_thickenWithDistance", true)] float _maxWidthMultiplier = 10.0f;
    [SerializeField] bool _disableRenderersOnZeroAlpha = false;

    [Space]
    [SerializeField] float _bakedGlowWidthScale = 1.0f;
    [SerializeField] bool _forceUseBakedGlow = false;

    [Space]
    [SerializeField] bool _multiplyLengthByAlpha = false;
    [SerializeField] [DrawIf("_multiplyLengthByAlpha", true)] AnimationCurve _alphaToLengthCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField] [DrawIf("_multiplyLengthByAlpha", true)] AnimationCurve _alphaToLengthBloomFogCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

    [Space] [Tooltip("Use when this light is updated with animations")]
    [SerializeField] bool _updateAlways = default;

    [Space]
    [SerializeField] bool _limitAlpha = false;
    [SerializeField] [DrawIf("_limitAlpha", true)] float _minAlpha = 0.0f;
    [SerializeField] [DrawIf("_limitAlpha", true)] float _maxAlpha = 1.0f;

    [Space]
    [SerializeField] bool _overrideChildrenAlpha = true;
    [SerializeField, Min(0.0f)] float _startAlpha = 1.0f;
    [SerializeField, Min(0.0f)] float _endAlpha = 1.0f;

    [Space]
    [SerializeField] bool _overrideChildrenWidth = false;
    [SerializeField, Min(1.0f)] float _startWidth = 1.0f;
    [SerializeField, Min(1.0f)] float _endWidth = 1.0f;

    [Space]
    [SerializeField] [NullAllowed] ParametricBoxController _parametricBoxController = default;
    [SerializeField] [NullAllowed] Parametric3SliceSpriteController _dynamic3SliceSprite = default;

    private bool _initialized = false;
    private bool _enabledRenderers = false;
    private bool _parametricBoxControllerOnceParInitialized = false;
    private bool _bakedGlowOnceParInitialized = false;
    private bool _isParametricBoxControllerValid = false;
    private bool _isDynamic3SliceSpriteValid = false;

    public event Action didRefreshEvent;
    public bool enabledRenderers => _enabledRenderers;
    public float colorAlphaMultiplier => _colorAlphaMultiplier;
    public float center => _center;

    public bool useCollision {
        get => _useCollision;
        set {
            _useCollision = value;

            InitIfNeeded();

            if (_isParametricBoxControllerValid) {
                _parametricBoxController.useCollision = _useCollision;
            }
            if (_isDynamic3SliceSpriteValid) {
                _dynamic3SliceSprite.useCollision = _useCollision;
            }
        }
    }
    public float collisionLength {
        get => _collisionLength;
        set {
            _collisionLength = value;
            _isDirty = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Refresh();
            }
#endif
        }
    }
    public float collisionEndAlpha =>
        (
            useCollision ?
                Mathf.Lerp(
                    _startAlpha,
                    _endAlpha,
                    Mathf.InverseLerp(0.0f, _length, calculatedCollisionLength)
                ) :
                _endAlpha
        ) * _multiplyLengthByAlphaMultiplier;
    private float calculatedCollisionLength => useCollision ? Mathf.Min(collisionLength, _length) : _length;
    private bool _useCollision;
    private float _collisionLength = float.MaxValue;

    private float _multiplyLengthByAlphaBloomFogMultiplier = 1.0f;
    private float _multiplyLengthByAlphaMultiplier = 1.0f;

    private bool _isDirty = true;

    private void MarkDirty() {

        _isDirty = true;
    }

    public float length {
        get => _length;
        set {
            _length = value;
            _isDirty = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Refresh();
            }
#endif
        }
    }

    public float width {
        get => _width;
        set {
            _width = value;
            _isDirty = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Refresh();
            }
#endif
        }
    }

    public float startAlpha {
        get => _startAlpha;
        set {
            _startAlpha = value;
            _isDirty = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Refresh();
            }
#endif
        }
    }

    public float lightWidthMultiplier { get => _lightWidthMultiplier; set => _lightWidthMultiplier = value; }
    public float bloomFogIntensityMultiplier { get => _bloomFogIntensityMultiplier; set => _bloomFogIntensityMultiplier = value; }

    public Color color {
        set {
            _color = value;

            InitIfNeeded();

            _isDirty |= _isParametricBoxControllerValid;
            _isDirty |= _isDynamic3SliceSpriteValid & (!_mainEffectPostProcessEnabled | _forceUseBakedGlow);
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                Refresh();
            }
#endif
        }
        get => _color;
    }

    private Transform _transform;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected new static void NoDomainReloadInit() {

        BloomPrePassLight.NoDomainReloadInit();
    }
#endif

    protected void Awake() {

        collisionLength = _length;
        InitIfNeeded();
    }

    private void InitIfNeeded() {

        if (_initialized) {
            return;
        }
        _isParametricBoxControllerValid = _parametricBoxController != null;
        _isDynamic3SliceSpriteValid = _dynamic3SliceSprite != null;
        if (_isParametricBoxControllerValid) {
            _parametricBoxController!.InitIfNeeded();
        }
        if (_isDynamic3SliceSpriteValid) {
            _dynamic3SliceSprite!.InitIfNeeded();
        }
        _initialized = true;
    }

    protected override void OnEnable() {

        base.OnEnable();
        Refresh();
    }

#if UNITY_EDITOR
    private void OnValidate() {

        if (!gameObject.scene.IsValid() || !enabled) {
            return;
        }
        MarkDirty();
        Refresh();
    }
#endif

    protected override void DidRegisterLight() {

        _transform = transform;
    }

    private bool NeedsRefresh() {

        bool needsRefresh = _isDirty;
        needsRefresh |= _updateAlways && !BloomPrePassLightsUpdateSystem.disableUpdateAlways;
        return needsRefresh;
    }

    public override void Refresh() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
#endif
            if (!NeedsRefresh()) {
                return;
            }
#if UNITY_EDITOR
        }
#endif

        InitIfNeeded();
        var shouldUseBakedGlow = !_mainEffectPostProcessEnabled || _forceUseBakedGlow;
        // Caching computation
        var shouldRenderersBeEnabled = !_disableRenderersOnZeroAlpha || _color.a > 0.01f;

        if (_enabledRenderers != shouldRenderersBeEnabled) {
            _enabledRenderers = shouldRenderersBeEnabled;

            if (_isParametricBoxControllerValid) {
                _parametricBoxController.enabled = shouldRenderersBeEnabled;
            }
            if (_isDynamic3SliceSpriteValid) {
                _dynamic3SliceSprite.enabled = shouldRenderersBeEnabled && shouldUseBakedGlow;
            }
        }

        if (shouldRenderersBeEnabled) {
            float thickenedWithDistanceMultiplier = 1.0f;
            if (_thickenWithDistance) {
                float distanceProgress = Mathf.InverseLerp(_minDistance, _maxDistance, transform.position.z);
                thickenedWithDistanceMultiplier = Mathf.Lerp(_minWidthMultiplier, _maxWidthMultiplier, _thickenCurve.Evaluate(distanceProgress));
            }

            float lengthMultiplier = 1.0f;
            if (_multiplyLengthByAlpha) {
                _multiplyLengthByAlphaMultiplier = _alphaToLengthCurve.Evaluate(_color.a);
                _multiplyLengthByAlphaBloomFogMultiplier = _alphaToLengthBloomFogCurve.Evaluate(_color.a);
                lengthMultiplier = _multiplyLengthByAlphaMultiplier;
            }
            var minAlpha = _limitAlpha ? _minAlpha : 0.0f;

            if (_isParametricBoxControllerValid) {

                // These parameters do not have runtime setters, apply only once
                if (!_parametricBoxControllerOnceParInitialized) {
                    _parametricBoxController.heightCenter = _center;
                    _parametricBoxController.alphaMultiplier = _colorAlphaMultiplier;
                    _parametricBoxController.minAlpha = minAlpha;

                    if (_overrideChildrenWidth) {
                        _parametricBoxController.widthStart = _startWidth;
                        _parametricBoxController.widthEnd = _endWidth;
                    }

                    _parametricBoxControllerOnceParInitialized = true;
                }

                if (_overrideChildrenAlpha) {
                    _parametricBoxController.alphaStart = _startAlpha;
                    _parametricBoxController.alphaEnd = _endAlpha;
                }

                float parametricBoxWidth = _thickenWithDistance ? _width * thickenedWithDistanceMultiplier : _width;
                _parametricBoxController.width = parametricBoxWidth;
                if (_overrideChildrenLength) {
                    _parametricBoxController.height = (_length + (_addWidthToLength ? _width : 0.0f)) * lengthMultiplier;
                }
                _parametricBoxController.length = parametricBoxWidth;
                _parametricBoxController.color = _color;
                if (_useCollision) {
                    _parametricBoxController.collisionHeight = collisionLength;
                }
                _parametricBoxController.Refresh();
            }

            if (_isDynamic3SliceSpriteValid && shouldUseBakedGlow) {

                // These parameters do not have runtime setters, apply only once
                if (!_bakedGlowOnceParInitialized) {
                    _dynamic3SliceSprite.center = _center;
                    _dynamic3SliceSprite.minAlpha = minAlpha;
                    _dynamic3SliceSprite.alphaMultiplier = _colorAlphaMultiplier * _fakeBloomIntensityMultiplier;

                    if (_overrideChildrenWidth) {
                        _dynamic3SliceSprite.widthStart = _startWidth;
                        _dynamic3SliceSprite.widthEnd = _endWidth;
                    }

                    _bakedGlowOnceParInitialized = true;
                }

                if (_overrideChildrenAlpha) {
                    _dynamic3SliceSprite.alphaStart = _startAlpha;
                    _dynamic3SliceSprite.alphaEnd = _endAlpha;
                }

                float dynamicSliceSpriteWidth = _thickenWithDistance ? _width * _bakedGlowWidthScale * thickenedWithDistanceMultiplier : _width * _bakedGlowWidthScale;
                _dynamic3SliceSprite.width = dynamicSliceSpriteWidth;
                if (_overrideChildrenLength) {
                    _dynamic3SliceSprite.length = (_length + (_addWidthToLength ? dynamicSliceSpriteWidth : 0.0f)) * lengthMultiplier;
                }
                _dynamic3SliceSprite.color = _color;

                if (_useCollision) {
                    _dynamic3SliceSprite.collisionLength = collisionLength;
                }
                _dynamic3SliceSprite.Refresh();
            }
        }

        didRefreshEvent?.Invoke();

        _isDirty = false;
    }

    public override void FillMeshData(ref int lightNum, QuadData[] lightQuads, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, float lineWidth) {

        // Fill in our vertex data, by component, in order
        ref var quad = ref lightQuads[lightNum];
        lightNum++;

        if (_disableRenderersOnZeroAlpha && _color.a < 0.01f) {
            ZeroQuad(ref quad);
            return;
        }

        var calculatedLength = calculatedCollisionLength;

        float fromPoint = -calculatedLength * _multiplyLengthByAlphaBloomFogMultiplier * _center;
        float toPoint = calculatedLength * _multiplyLengthByAlphaBloomFogMultiplier * (1.0f - _center);
        var localToWorldMatrix = _transform.localToWorldMatrix;
        Vector3 fromPointWorldPos = localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.0f, fromPoint, 0.0f));
        Vector3 toPointWorldPos = localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.0f, toPoint, 0.0f));
        Vector3 fromPointViewPos = viewMatrix.MultiplyPoint3x4(fromPointWorldPos);
        Vector3 toPointViewPos = viewMatrix.MultiplyPoint3x4(toPointWorldPos);
        Vector4 fromPointClipPos = projectionMatrix * new Vector4(fromPointViewPos.x, fromPointViewPos.y, fromPointViewPos.z, 1.0f);
        Vector4 toPointClipPos = projectionMatrix * new Vector4(toPointViewPos.x, toPointViewPos.y, toPointViewPos.z, 1.0f);

        // clip left
        bool fromPointInside = fromPointClipPos.x >= -fromPointClipPos.w;
        bool toPointInside = toPointClipPos.x >= -toPointClipPos.w;
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (-fromPointClipPos.w - fromPointClipPos.x) / (toPointClipPos.x - fromPointClipPos.x + toPointClipPos.w - fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }

        // clip right
        fromPointInside = fromPointClipPos.x <= fromPointClipPos.w;
        toPointInside = toPointClipPos.x <= toPointClipPos.w;
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (fromPointClipPos.w - fromPointClipPos.x) / (toPointClipPos.x - fromPointClipPos.x - toPointClipPos.w + fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }

        // clip bottom
        fromPointInside = fromPointClipPos.y >= -fromPointClipPos.w;
        toPointInside = toPointClipPos.y >= -toPointClipPos.w;
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (-fromPointClipPos.w - fromPointClipPos.y) / (toPointClipPos.y - fromPointClipPos.y + toPointClipPos.w - fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }

        // clip top
        fromPointInside = fromPointClipPos.y <= fromPointClipPos.w;
        toPointInside = toPointClipPos.y <= toPointClipPos.w;
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (fromPointClipPos.w - fromPointClipPos.y) / (toPointClipPos.y - fromPointClipPos.y - toPointClipPos.w + fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }

        // clip far
        fromPointInside = fromPointClipPos.z <= fromPointClipPos.w;
        toPointInside = toPointClipPos.z <= toPointClipPos.w;
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (fromPointClipPos.w - fromPointClipPos.z) / (toPointClipPos.z - fromPointClipPos.z - toPointClipPos.w + fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }
        const float clipOffset = 0.0001f;

        // clip near
        fromPointInside = fromPointClipPos.z >= (-fromPointClipPos.w - clipOffset);
        toPointInside = toPointClipPos.z >= (-toPointClipPos.w - clipOffset);
        if (!fromPointInside && !toPointInside) {
            ZeroQuad(ref quad);
            return;
        }
        else if (fromPointInside != toPointInside) {
            float t = (-fromPointClipPos.w - fromPointClipPos.z) / (toPointClipPos.z - fromPointClipPos.z + toPointClipPos.w - fromPointClipPos.w);
            ClipPoints(ref fromPointClipPos, ref toPointClipPos, ref fromPointViewPos, ref toPointViewPos, fromPointInside, t);
        }

        // Originally these were Vectors and Color structs, but unrolling them all to
        // floats speeds up this function by nearly 2x
        float fromPointVertexPosX = fromPointClipPos.x / fromPointClipPos.w * 0.5f + 0.5f;
        float fromPointVertexPosY = fromPointClipPos.y / fromPointClipPos.w * 0.5f + 0.5f;
        float toPointVertexPosX = toPointClipPos.x / toPointClipPos.w * 0.5f + 0.5f;
        float toPointVertexPosY = toPointClipPos.y / toPointClipPos.w * 0.5f + 0.5f;

        float dirX = toPointVertexPosX - fromPointVertexPosX;
        float dirY = toPointVertexPosY - fromPointVertexPosY;

        // Normalization of direction vector. We care only about x and y.
        float dirLength = Mathf.Sqrt(dirX * dirX + dirY * dirY);
        if (dirLength == 0.0f) {
            dirLength = 0.000001f;
        }

        dirX /= dirLength;
        dirY /= dirLength;

        const float offset = 0.015625f;
        float offsetDirX = dirX * offset;
        float offsetDirY = dirY * offset;
        toPointVertexPosX += offsetDirX;
        toPointVertexPosY += offsetDirY;
        fromPointVertexPosX -= offsetDirX;
        fromPointVertexPosY -= offsetDirY;

        float normMultiplier = lineWidth * _lightWidthMultiplier;

        float normalX = -dirY * normMultiplier;
        float normalY = dirX * normMultiplier;

        float startNormalX = normalX * _startWidth;
        float startNormalY = normalY * _startWidth;
        float endNormalX = normalX * _endWidth;
        float endNormalY = normalY * _endWidth;

        float colorR = _color.r + _boostToWhite;
        float colorG = _color.g + _boostToWhite;
        float colorB = _color.b + _boostToWhite;
        float colorA = _color.a * _bloomFogIntensityMultiplier;

        if (_limitAlpha) {
            colorA = Mathf.Clamp(colorA, _minAlpha, _maxAlpha);
        }

        colorA = Mathf.LinearToGammaSpace(colorA);

        float startColorR = _startAlpha * colorR;
        float startColorG = _startAlpha * colorG;
        float startColorB = _startAlpha * colorB;
        float startColorA = _startAlpha * colorA;

        var interpolatedEndAlpha = collisionEndAlpha;

        float endColorR = interpolatedEndAlpha * colorR;
        float endColorG = interpolatedEndAlpha * colorG;
        float endColorB = interpolatedEndAlpha * colorB;
        float endColorA = interpolatedEndAlpha * colorA;

        // Vertex 0
        quad.vertex0.vertex.x = fromPointVertexPosX - startNormalX;
        quad.vertex0.vertex.y = fromPointVertexPosY - startNormalY;
        quad.vertex0.vertex.z = 0.0f;

        quad.vertex0.viewPos.x = fromPointViewPos.x;
        quad.vertex0.viewPos.y = fromPointViewPos.y;
        quad.vertex0.viewPos.z = fromPointViewPos.z;

        quad.vertex0.color.r = startColorR;
        quad.vertex0.color.g = startColorG;
        quad.vertex0.color.b = startColorB;
        quad.vertex0.color.a = startColorA;

        quad.vertex0.uv.x = 0.0f;
        quad.vertex0.uv.y = 0.0f;
        quad.vertex0.uv.z = _startWidth;

        // Vertex 1
        quad.vertex1.vertex.x = fromPointVertexPosX + startNormalX;
        quad.vertex1.vertex.y = fromPointVertexPosY + startNormalY;
        quad.vertex1.vertex.z = 0.0f;

        quad.vertex1.viewPos.x = fromPointViewPos.x;
        quad.vertex1.viewPos.y = fromPointViewPos.y;
        quad.vertex1.viewPos.z = fromPointViewPos.z;

        quad.vertex1.color.r = startColorR;
        quad.vertex1.color.g = startColorG;
        quad.vertex1.color.b = startColorB;
        quad.vertex1.color.a = startColorA;

        quad.vertex1.uv.x = _startWidth;
        quad.vertex1.uv.y = 0.0f;
        quad.vertex1.uv.z = _startWidth;

        // Vertex 2
        quad.vertex2.vertex.x = toPointVertexPosX + endNormalX;
        quad.vertex2.vertex.y = toPointVertexPosY + endNormalY;
        quad.vertex2.vertex.z = 0.0f;

        quad.vertex2.viewPos.x = toPointViewPos.x;
        quad.vertex2.viewPos.y = toPointViewPos.y;
        quad.vertex2.viewPos.z = toPointViewPos.z;

        quad.vertex2.color.r = endColorR;
        quad.vertex2.color.g = endColorG;
        quad.vertex2.color.b = endColorB;
        quad.vertex2.color.a = endColorA;

        quad.vertex2.uv.x = _endWidth;
        quad.vertex2.uv.y = 1.0f;
        quad.vertex2.uv.z = _endWidth;

        // Vertex 3
        quad.vertex3.vertex.x = toPointVertexPosX - endNormalX;
        quad.vertex3.vertex.y = toPointVertexPosY - endNormalY;
        quad.vertex3.vertex.z = 0.0f;

        quad.vertex3.viewPos.x = toPointViewPos.x;
        quad.vertex3.viewPos.y = toPointViewPos.y;
        quad.vertex3.viewPos.z = toPointViewPos.z;

        quad.vertex3.color.r = endColorR;
        quad.vertex3.color.g = endColorG;
        quad.vertex3.color.b = endColorB;
        quad.vertex3.color.a = endColorA;

        quad.vertex3.uv.x = 0.0f;
        quad.vertex3.uv.y = 1.0f;
        quad.vertex3.uv.z = _endWidth;
    }

    private static void ClipPoints(ref Vector4 fromPointClipPos, ref Vector4 toPointClipPos, ref Vector3 fromPointViewPos, ref Vector3 toPointViewPos, bool fromPointInside, float t) {

        Vector4 clippedPointClipPos = fromPointClipPos + (toPointClipPos - fromPointClipPos) * t;
        Vector3 clippedPointViewPos = fromPointViewPos + (toPointViewPos - fromPointViewPos) * t;
        if (fromPointInside) {
            toPointClipPos = clippedPointClipPos;
            toPointViewPos = clippedPointViewPos;
        }
        else {
            fromPointClipPos = clippedPointClipPos;
            fromPointViewPos = clippedPointViewPos;
        }
    }

    private static void ZeroQuad(ref QuadData quad) {

        quad.vertex0.vertex = Vector3.zero;
        quad.vertex1.vertex = Vector3.zero;
        quad.vertex2.vertex = Vector3.zero;
        quad.vertex3.vertex = Vector3.zero;
    }

    protected void OnDrawGizmos() {

        Gizmos.color = _color;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(
            new Vector3(0.0f, -(calculatedCollisionLength - 0.01f) * (_center - 0.5f), 0.0f),
            new Vector3(_width - 0.01f, calculatedCollisionLength - 0.01f, _width - 0.01f)
        );
    }

#if UNITY_EDITOR
    protected void LateUpdate() {

        if (!Application.isPlaying && _updateAlways) {
            Refresh();
        }
    }
#endif
}
