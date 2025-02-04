using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public abstract class BloomPrePassLight : MonoBehaviour {

    private const int kFloatSize = 4;
    private const int kVertexOffset = 0;
    private const int kVertexSize = 3 * kFloatSize;
    private const int kViewPosOffset = kVertexOffset + kVertexSize;
    private const int kViewPosSize = 3 * kFloatSize;
    private const int kColorOffset = kViewPosOffset + kViewPosSize;
    private const int kColorSize = 4 * kFloatSize;
    private const int kUvOffset = kColorOffset + kColorSize;
    private const int kUvSize = 3 * kFloatSize;
    private const int kVertexDataSize = kUvOffset + kUvSize;

    [StructLayout(LayoutKind.Explicit)]
    public struct VertexData {

        [FieldOffset(kVertexOffset)]
        public Vector3 vertex;
        [FieldOffset(kViewPosOffset)]
        public Vector3 viewPos;
        [FieldOffset(kColorOffset)]
        public Color color;
        [FieldOffset(kUvOffset)]
        public Vector3 uv;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct QuadData {

        [FieldOffset(0 * kVertexDataSize)]
        public VertexData vertex0;
        [FieldOffset(1 * kVertexDataSize)]
        public VertexData vertex1;
        [FieldOffset(2 * kVertexDataSize)]
        public VertexData vertex2;
        [FieldOffset(3 * kVertexDataSize)]
        public VertexData vertex3;
    }

    [SerializeField] BloomPrePassLightTypeSO _lightType = default;

    public static Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>> bloomLightsDict => _bloomLightsDict;
    public static List<LightsDataItem> lightsDataItems => _lightsDataItems;
    private static readonly Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>> _bloomLightsDict = new Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>();
    private static readonly List<LightsDataItem> _lightsDataItems = new List<LightsDataItem>();

    public class LightsDataItem {
        public readonly BloomPrePassLightTypeSO lightType;
        public readonly HashSet<BloomPrePassLight> lights;

        public LightsDataItem(BloomPrePassLightTypeSO lightType, HashSet<BloomPrePassLight> lights) {
            this.lightType = lightType;
            this.lights = lights;
        }
    }

    private BloomPrePassLightTypeSO _registeredWithLightType;
    private bool _isRegistered = false;
    private bool _isBeingDestroyed;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void NoDomainReloadInit() {

        _bloomLightsDict.Clear();
        _lightsDataItems.Clear();
    }
#endif

    protected virtual void OnEnable() {

        RegisterLight();
    }

    protected virtual void OnDisable() {

        UnregisterLight();
    }

    protected void OnDestroy() {

        _isBeingDestroyed = true;
        UnregisterLight();
    }

    private void RegisterLight() {

        if (_isRegistered || _isBeingDestroyed) {
            return;
        }

        DidRegisterLight();

        _isRegistered = true;

        _registeredWithLightType = _lightType;

        HashSet<BloomPrePassLight> lights = null;
        if (!_bloomLightsDict.TryGetValue(_registeredWithLightType, out lights)) {
            lights = new HashSet<BloomPrePassLight>();
            _bloomLightsDict[_registeredWithLightType] = lights;

            int i = 0;
            for (; i < _lightsDataItems.Count; i++) {
                if (_lightsDataItems[i].lightType.renderingPriority > _registeredWithLightType.renderingPriority) {
                    break;
                }
            }
            _lightsDataItems.Insert(i, new LightsDataItem(_registeredWithLightType, lights));
        }
        lights.Add(this);
    }

    private void UnregisterLight() {

        if (!_isRegistered) {
            return;
        }

        _isRegistered = false;

        if (_bloomLightsDict.TryGetValue(_registeredWithLightType, out HashSet<BloomPrePassLight> lights)) {
            lights.Remove(this);
        }
    }

    protected abstract void DidRegisterLight();
    public abstract void FillMeshData(ref int lightNum, QuadData[] lightQuads, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, float lineWidth);
    public abstract void Refresh();
}
