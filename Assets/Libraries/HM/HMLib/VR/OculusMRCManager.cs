#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID
#define BS_WINDOWS_OR_ANDROID // Avoid compilation issues with Mac and Linux
#endif

using UnityEngine;
#if BS_OCULUS_VR
using System;
using Zenject;

//TODO: MOVE_TO_OCULUS_SPECIFIC_ASSEMBLY
// This will require a big refactor because there is a reference to the MonoBehaviour in the ExternalCameras prefab
public class OculusMRCManager : MonoBehaviour, OVRMixedRealityCaptureConfiguration {

    [Inject] readonly IVRPlatformHelper _vrPlatformHelper = default!;

    private Func<GameObject, GameObject> _instantiateMixedRealityBackgroundCameraGameObject;
    private Func<GameObject, GameObject> _instantiateMixedRealityForegroundCameraGameObject;
#if BS_WINDOWS_OR_ANDROID
#pragma warning disable 0618
    public bool enableMixedReality { get; set; }
    public LayerMask extraHiddenLayers { get; set; }
    public LayerMask extraVisibleLayers { get; set; }
    public bool dynamicCullingMask { get; set; }
    public OVRManager.CompositionMethod compositionMethod { get; set; } = OVRManager.CompositionMethod.External;
    public Color externalCompositionBackdropColorRift { get; set; } = new Color(0, 1, 0, 0);
    public Color externalCompositionBackdropColorQuest { get; set; } = new Color(0, 0, 0, 0);
    public OVRManager.CameraDevice capturingCameraDevice { get; set; }
    public bool flipCameraFrameHorizontally { get; set; }
    public bool flipCameraFrameVertically { get; set; }
    public float handPoseStateLatency { get; set; }
    public float sandwichCompositionRenderLatency { get; set; }
    public int sandwichCompositionBufferedFrames { get; set; } = 8;
    public Color chromaKeyColor { get; set; } = Color.green;
    public float chromaKeySimilarity { get; set; } = 0.60f;
    public float chromaKeySmoothRange { get; set; } = 0.03f;
    public float chromaKeySpillRange { get; set; } = 0.06f;
    public bool useDynamicLighting { get; set; }
    public OVRManager.DepthQuality depthQuality { get; set; } = OVRManager.DepthQuality.Medium;
    public float dynamicLightingSmoothFactor { get; set; } = 8.0f;
    public float dynamicLightingDepthVariationClampingValue { get; set; } = 0.001f;
    public OVRManager.VirtualGreenScreenType virtualGreenScreenType { get; set; }
    public float virtualGreenScreenTopY { get; set; } = 10.0f;
    public float virtualGreenScreenBottomY { get; set; } = -10.0f;
    public bool virtualGreenScreenApplyDepthCulling { get; set; }
    public float virtualGreenScreenDepthTolerance { get; set; } = 0.2f;
    public OVRManager.MrcActivationMode mrcActivationMode { get; set; }
#pragma warning restore 0618

    // Force this to return our InstantiateMixedRealityCameraGameObject method

    private OVRManager.InstantiateMrcCameraDelegate _instantiateMixedRealityCameraGameObject;
    public OVRManager.InstantiateMrcCameraDelegate instantiateMixedRealityCameraGameObject {
        get {
            // Cache the method as a delegate to avoid conversion every frame
            if (_instantiateMixedRealityCameraGameObject == null) {
                _instantiateMixedRealityCameraGameObject = InstantiateMixedRealityCameraGameObject;
            }
            return _instantiateMixedRealityCameraGameObject;
        }
        set {}
    }
#endif

#if BS_OCULUS_PLATFORM && BS_WINDOWS_OR_ANDROID
    protected void Update() {

        OVRManager.StaticUpdateMixedRealityCapture(this, gameObject, OVRManager.TrackingOrigin.FloorLevel);
    }

    protected void OnDestroy() {

        OVRManager.StaticShutdownMixedRealityCapture(this);
    }
#endif

    public void Init(Func<GameObject, GameObject> instantiateMixedRealityBackgroundCameraGameObject, System.Func<GameObject, GameObject> instantiateMixedRealityForegroundCameraGameObject) {

        if (_vrPlatformHelper.vrPlatformSDK != VRPlatformSDK.Oculus) {
            enabled = false;
            return;
        }

        enabled = true;
        _instantiateMixedRealityBackgroundCameraGameObject = instantiateMixedRealityBackgroundCameraGameObject;
        _instantiateMixedRealityForegroundCameraGameObject = instantiateMixedRealityForegroundCameraGameObject;

#if BS_OCULUS_PLATFORM && BS_WINDOWS_OR_ANDROID
        Debug.Log("Init OculusMRCManager");
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        ProcessCommandLineSettings();
#endif
        OVRManager.StaticInitializeMixedRealityCapture(this);
#endif
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private void ProcessCommandLineSettings() {

        bool createMrcConfig = false;
        bool loadMrcConfig = false;

        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            switch (args[i].ToLower()) {

                case "-mixedreality":
                    enableMixedReality = true;
                    break;
                case "-directcomposition":
#pragma warning disable 0618
                    compositionMethod = OVRManager.CompositionMethod.Direct;
#pragma warning restore 0618
                    break;
                case "-externalcomposition":
                    compositionMethod = OVRManager.CompositionMethod.External;
                    break;
                case "-create_mrc_config":
                    createMrcConfig = true;
                    break;
                case "-load_mrc_config":
                    loadMrcConfig = true;
                    break;
            }
        }

        if (loadMrcConfig || createMrcConfig) {
            OVRMixedRealityCaptureSettings mrcSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
            mrcSettings.ReadFrom(this);
            if (loadMrcConfig) {
                mrcSettings.CombineWithConfigurationFile();
                mrcSettings.ApplyTo(this);
            }
            if (createMrcConfig) {
                mrcSettings.WriteToConfigurationFile();
            }
            Destroy(mrcSettings);
        }
    }
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID
    private GameObject InstantiateMixedRealityCameraGameObject(GameObject mainCameraGameObject, OVRManager.MrcCameraType cameraType) {

        switch (cameraType) {
            case OVRManager.MrcCameraType.Background:
                return _instantiateMixedRealityBackgroundCameraGameObject?.Invoke(mainCameraGameObject);
            case OVRManager.MrcCameraType.Foreground:
                return _instantiateMixedRealityForegroundCameraGameObject?.Invoke(mainCameraGameObject);
            // We don't really have other option so just do the default for OVRManager.MrcCameraType.Normal (is used with direct composition, which we dont use?).
            default:
                return _instantiateMixedRealityBackgroundCameraGameObject?.Invoke(mainCameraGameObject);
        }
    }
#endif
}
#else
public class OculusMRCManager : MonoBehaviour { }
#endif
