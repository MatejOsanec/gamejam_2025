using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class LightManager : MonoBehaviour {

    [DoesNotRequireDomainReloadInit] private static readonly int _directionalLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");
    [DoesNotRequireDomainReloadInit] private static readonly int _directionalLightPositionID = Shader.PropertyToID("_DirectionalLightPositions");
    [DoesNotRequireDomainReloadInit] private static readonly int _directionalLightRadiiID = Shader.PropertyToID("_DirectionalLightRadii");
    [DoesNotRequireDomainReloadInit] private static readonly int _directionalLightColorsID = Shader.PropertyToID("_DirectionalLightColors");

    [DoesNotRequireDomainReloadInit] private static readonly int _pointLightPositionsID = Shader.PropertyToID("_PointLightPositions");
    [DoesNotRequireDomainReloadInit] private static readonly int _pointLightColorsID = Shader.PropertyToID("_PointLightColors");

    private readonly Vector4[] _directionalLightDirections = new Vector4[DirectionalLight.kMaxLights];
    private readonly Vector4[] _directionalLightColors = new Vector4[DirectionalLight.kMaxLights];
    private readonly Vector4[] _directionalLightPositions = new Vector4[DirectionalLight.kMaxLights];
    private readonly float[] _directionalLightRadii = new float[DirectionalLight.kMaxLights];

    private readonly Vector4[] _pointLightPositions = new Vector4[PointLight.kMaxLights];
    private readonly Vector4[] _pointLightColors = new Vector4[PointLight.kMaxLights];

    // What is the frame number the light directions were computed.
    private int lastRefreshFrameNum = -1;

    protected void OnEnable() {

        Camera.onPreRender += OnCameraPreRender;
    }

    protected void OnDisable() {

        Camera.onPreRender -= OnCameraPreRender;
    }

    private void OnCameraPreRender(Camera camera) {

        if (camera.cullingMask != (camera.cullingMask | 1 << gameObject.layer)) {
            return;
        }

#if UNITY_EDITOR
        if (!LightsEnabledForCamera(camera)) {
            _directionalLightColors[0] = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            _directionalLightDirections[0] = (new Vector4(1.0f, 0.5f, 0.3f, 0.0f)).normalized;
            _directionalLightPositions[0] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _directionalLightRadii[0] = 100000;

            _directionalLightColors[1] = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            _directionalLightDirections[1] = (new Vector4(-1.0f, 0.0f, -0.5f, 0.0f)).normalized;
            _directionalLightPositions[1] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _directionalLightRadii[1] = 100000;

            // Reset colors
            for (int i = 2; i < DirectionalLight.kMaxLights; i++) {
                _directionalLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }

            for (int i = 0; i < PointLight.kMaxLights; i++) {
                _pointLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }

            Shader.SetGlobalVectorArray(_directionalLightDirectionsID, _directionalLightDirections);
            Shader.SetGlobalVectorArray(_directionalLightColorsID, _directionalLightColors);
            Shader.SetGlobalVectorArray(_directionalLightPositionID, _directionalLightPositions);
            Shader.SetGlobalFloatArray(_directionalLightRadiiID, _directionalLightRadii);

            Shader.SetGlobalVectorArray(_pointLightColorsID, _pointLightColors);

            Shader.EnableKeyword("UNITY_LIGHTING_OFF");
            return;
        }
        else {
            Shader.DisableKeyword("UNITY_LIGHTING_OFF");
        }

        if (Application.isPlaying) {
#endif

            if (lastRefreshFrameNum == Time.frameCount) {
                return;
            }

            lastRefreshFrameNum = Time.frameCount;

#if UNITY_EDITOR
        }
#endif

        // Directional Lights
        var directionalLights = DirectionalLight.lights;

        for (int i = 0; i < DirectionalLight.kMaxLights; i++) {
            if (i < directionalLights.Count && directionalLights[i].isActiveAndEnabled) {
                var directionalLight = directionalLights[i];
                var lightTransform = directionalLight.transform;
                _directionalLightPositions[i] = lightTransform.position;
                _directionalLightDirections[i] = -lightTransform.forward;
                _directionalLightColors[i] = (directionalLight.color * directionalLight.intensity).linear;
                _directionalLightRadii[i] = directionalLight.radius;
            }
            else {
                _directionalLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                _directionalLightRadii[i] = 100;
            }
        }

        Shader.SetGlobalVectorArray(_directionalLightDirectionsID, _directionalLightDirections);
        Shader.SetGlobalVectorArray(_directionalLightPositionID, _directionalLightPositions);
        Shader.SetGlobalFloatArray(_directionalLightRadiiID, _directionalLightRadii);
        Shader.SetGlobalVectorArray(_directionalLightColorsID, _directionalLightColors);

        // Point Lights
        var pointLights = PointLight.lights;
        for (int i = 0; i < PointLight.kMaxLights; i++) {
            if (i < pointLights.Count && pointLights[i].isActiveAndEnabled) {
                var pointLight = pointLights[i];
                _pointLightPositions[i] = pointLight.transform.position;
                _pointLightColors[i] = (pointLight.color * pointLight.intensity).linear;
            }
            else {
                _pointLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }
        }

        Shader.SetGlobalVectorArray(_pointLightPositionsID, _pointLightPositions);
        Shader.SetGlobalVectorArray(_pointLightColorsID, _pointLightColors);
    }

    protected void OnDestroy() {

        ResetColors();
    }

    private void ResetColors() {

        // Reset colors
        for (int i = 0; i < DirectionalLight.kMaxLights; i++) {
            _directionalLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }

        for (int i = 0; i < PointLight.kMaxLights; i++) {
            _pointLightColors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }

        Shader.SetGlobalVectorArray(_directionalLightDirectionsID, _directionalLightDirections);
        Shader.SetGlobalVectorArray(_directionalLightColorsID, _directionalLightColors);
        Shader.SetGlobalVectorArray(_pointLightColorsID, _pointLightColors);
    }

#if UNITY_EDITOR
    private static bool LightsEnabledForCamera(Camera camera) {

        var currentDrawingSceneView = SceneView.currentDrawingSceneView;
        return currentDrawingSceneView == null || (currentDrawingSceneView != null && currentDrawingSceneView.camera == camera && currentDrawingSceneView.sceneLighting);
    }
#endif
}
