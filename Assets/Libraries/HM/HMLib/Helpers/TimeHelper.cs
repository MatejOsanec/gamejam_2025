using UnityEditor;
using UnityEngine;

public class TimeHelper : MonoBehaviour {

    public static float time {
        get {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                return (float)EditorApplication.timeSinceStartup;
            }
#endif
            return _time;
        }
        private set {
            _time = value;
        }
    }

    public static int frameCount {
        get {
            return Time.frameCount - TimeHelper._baseFrameCount;
        }
    }

    public static float deltaTime { get; private set; }
    public static float fixedDeltaTime { get; private set; }
    public static float interpolationFactor { get; private set; }

    private static readonly int _timeHelperPropertyID = Shader.PropertyToID("_TimeHelperOffset");

    private static float _time;
    private static int _baseFrameCount;

    private float _accumulator;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _time = 0;
        _baseFrameCount = 0;
    }
#endif

    protected void Awake() {

        fixedDeltaTime = Time.fixedDeltaTime;
        _accumulator += fixedDeltaTime;
        __SetTime(0.0f);
    }

    protected void FixedUpdate() {

        fixedDeltaTime = Time.fixedDeltaTime;
        _accumulator -= fixedDeltaTime;
    }

    protected void Update() {

        deltaTime = Time.deltaTime;
        _accumulator += deltaTime;
        time += deltaTime;
        interpolationFactor = _accumulator / fixedDeltaTime;
    }

    public static void __SetTime(float time) {

        TimeHelper.time = time;
        _baseFrameCount = Time.frameCount;
        var offset = time - Time.timeSinceLevelLoad;
        Shader.SetGlobalVector(_timeHelperPropertyID, new Vector4(offset * 0.05f, offset, offset * 2.0f, offset * 3.0f));
    }
}
