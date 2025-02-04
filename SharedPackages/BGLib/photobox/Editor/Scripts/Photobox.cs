using System;
using System.Collections.Generic;
using System.IO;
using BGLib.JsonExtension;
using BGLib.UnityExtension.Editor;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BGLib.Photobox.Editor {

    [AddToToolDirectory(
        displayName: "Photobox",
        description: "Allows you to make screenshots of an object with many effects and setup being done for you.",
        packageName: "Photobox package",
        maintainer: ToolMaintainer.DannyDeBruijne,
        openToolFunctionName: null,
        labelTypes: LabelType.Core)]
    public class Photobox<TSettings> where TSettings : class, IPhotoboxSettings {

        private const string kTempMaterialName = "Photobox Temp Material";

        public TSettings settings => _settings;

        protected TSettings _settings = null!;

        protected Camera? _camera;
        protected RenderTexture? _renderTexture;
        protected MainEffectController? _mainEffectController;
        protected Transform? _containerTransform;
        protected GameObject? _cameraObject;

        private readonly int _colorPass = Shader.PropertyToID("_ColorPass");
        private readonly int _depthTexture = Shader.PropertyToID("_DepthTexture");
        private readonly int _lightingPass = Shader.PropertyToID("_LightingPass");
        private readonly string _settingsPath;
        private readonly Dictionary<Renderer, Material?[]> _originalMaterials = new ();

        public Photobox(string settingsPath) {

            _settingsPath = settingsPath;

            LoadSettings();
        }

        private void LoadSettings() {

            string dir = Path.GetDirectoryName(_settingsPath)!;
            if (Directory.Exists(dir) && File.Exists(_settingsPath)) {
                string json = File.ReadAllText(_settingsPath);
                var loadedSettings = JsonConvert.DeserializeObject<TSettings>(json);
                if (loadedSettings != null) {
                    _settings = loadedSettings;
                    return;
                }
            }

            Debug.LogWarning("Creating new photobox settings file. Check if this is intentional.");
            _settings = Activator.CreateInstance<TSettings>();
        }

        public void SaveSettings() {

            string json = JsonConvert.SerializeObject(_settings, JsonSettings.readableWithDefault);

            string dir = Path.GetDirectoryName(_settingsPath)!;
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_settingsPath, json);
        }

        public Camera SetupCameraRig(
            Vector3 cameraOffset,
            float jibLength,
            bool orthographicCamera,
            Camera? overrideCameraPrefab = null,
            float? fov = null,
            float? orthographicSize = null
        ) {

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var anchor = new GameObject("Anchor");
            _containerTransform = new GameObject("Container").transform;
            _containerTransform.localPosition = cameraOffset;

            if (overrideCameraPrefab != null) {
                _cameraObject = Object.Instantiate(overrideCameraPrefab).gameObject;
                _camera = _cameraObject.GetComponent<Camera>();
                _mainEffectController = _cameraObject.GetComponent<MainEffectController>();
            }
            else {
                _cameraObject = new GameObject("Camera", typeof(Camera));

                // Setup the camera
                _camera = _cameraObject.GetComponent<Camera>();

                // MainFX
                _cameraObject.AddComponent<ImageEffectController>();
                _mainEffectController = _cameraObject.AddComponent<MainEffectController>();
                _mainEffectController.InitEditor(
                    FindUnityObjectsHelper.FindObjectByType<MainEffectContainerSO>(),
                    FindUnityObjectsHelper.FindObjectByType<FloatSO>()
                );
            }

            _cameraObject.transform.SetParent(_containerTransform);

            Vector3 cameraPosition = Vector3.forward * -1 * jibLength;
            _camera.transform.localPosition = cameraPosition;
            _camera.backgroundColor = new Color(0, 0, 0, 0);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.orthographic = orthographicCamera;
            if (fov != null) {
                _camera.fieldOfView = (float)fov;
            }
            if (orthographicSize != null) {
                _camera.orthographicSize = (float)orthographicSize;
            }

            foreach (var ssc in Object.FindObjectsOfType<Parametric3SliceSpriteController>()) {
                ssc.InitEditor();
                ssc.Refresh();
            }

            // Update the RenderTexture
            if (_renderTexture != null) {
                _renderTexture.Release();
            }

            _renderTexture = new(
                _settings.thumbnailResolution.x,
                _settings.thumbnailResolution.y,
                depth: 32,
                RenderTextureFormat.ARGB32
            );
            _renderTexture.Create();
            _camera.targetTexture = _renderTexture;

            return _camera;
        }

        public Texture2D TakePicture(Material compositionMaterial, Texture2D? lightingPass = null) {

            if (_renderTexture == null) {
                throw new InvalidOperationException("Render Texture is not set, setup camera rig first");
            }

            UnityEngine.RenderTexture.active = _renderTexture;

            // first, get a normal rgb texture. We can't use transparency directly as that is bloom!
            Texture2D color = RenderTexture();

            // Use a special shader to render a depth texture
            Shader depthTextureShader =
                AssetDatabaseExtensions.LoadAssetAtGUID<Shader>(_settings.depthTextureShaderGUID);
            Texture2D depth = RenderTexture(depthTextureShader);

            // Blit the composition material, stitches the two together by shader.
            Texture2D colorWithAlpha = BlitCompositeTexture(compositionMaterial, color, depth, lightingPass);

            UnityEngine.RenderTexture.active = null;
            return colorWithAlpha;
        }

        public Texture2D TakePictureSimple() {

            if (_renderTexture == null) {
                throw new InvalidOperationException("Render Texture is not set, setup camera rig first");
            }

            UnityEngine.RenderTexture.active = _renderTexture;

            // first, get a normal rgb texture. We can't use transparency directly as that is bloom!
            Texture2D color = RenderTexture(useTransparency: false);

            UnityEngine.RenderTexture.active = null;
            return color;
        }

        private Texture2D RenderTexture(Shader? replacementShader = null, bool useTransparency = true) {

            if (_camera == null) {
                throw new InvalidOperationException("Camera is not set, setup camera rig first");
            }

            if (replacementShader != null) {
                _camera.SetReplacementShader(replacementShader, "RenderType");
            }

            Texture2D result = new Texture2D(
                _settings.thumbnailResolution.x,
                _settings.thumbnailResolution.y,
                useTransparency ? TextureFormat.RGBA32 : TextureFormat.RGB48,
                false
            );
            _camera.Render();
            result.ReadPixels(new Rect(Vector2.zero, _settings.thumbnailResolution), 0, 0);
            result.Apply();

            if (replacementShader != null) {
                _camera.ResetReplacementShader();
            }

            return result;
        }

        private Texture2D BlitCompositeTexture(
            Material material,
            Texture2D color,
            Texture2D depth,
            Texture2D? lighting = null
        ) {

            material.SetTexture(_colorPass, color);
            material.SetTexture(_depthTexture, depth);
            if (lighting != null) {
                material.SetTexture(_lightingPass, lighting);
            }

            Graphics.Blit(null, _renderTexture, material);
            Texture2D result = new Texture2D(
                _settings.thumbnailResolution.x,
                _settings.thumbnailResolution.y,
                TextureFormat.RGBA32,
                false
            );
            result.ReadPixels(new Rect(Vector2.zero, _settings.thumbnailResolution), 0, 0);
            result.Apply();

            return result;
        }

        /// <summary>
        /// Loop helper that will create instances for editor use as needed and recycle ones already present. <br/>
        /// Only use on Renderers that are instanced and not directly on assets so you don't write references to disposable materials.
        /// </summary>
        /// <param name="renderer">renderer to run forEach on</param>
        /// <param name="uniqueCopy">whether these operations should run on an unique copy of the original material or on one which could already have some changes</param>
        /// <param name="forEach">inline function with material parameter</param>
        protected void ForEachUniqueMaterial(Renderer renderer, bool uniqueCopy, Action<Material> forEach) {

            List<Material> newSharedMaterials = new(renderer.sharedMaterials.Length);
            if (!_originalMaterials.ContainsKey(renderer)) {
                _originalMaterials.Add(renderer, new Material[renderer.sharedMaterials.Length]);
            }

            for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                Material? sharedMaterial = renderer.sharedMaterials[i];
                if (sharedMaterial == null) {
                    continue;
                }

                // cache the original material so we can retrieve it later if desired
                if (sharedMaterial.name != kTempMaterialName) {
                    _originalMaterials[renderer][i] = sharedMaterial;
                }

                // retrieve original material from the cache
                if (uniqueCopy && sharedMaterial.name == kTempMaterialName) {
                    sharedMaterial = _originalMaterials[renderer][i];
                }

                if (sharedMaterial == null) {
                    continue;
                }

                // create a new temp material, or recycle the previous temp material.
                if (sharedMaterial.name != kTempMaterialName) {
                    Material newInstance = new Material(sharedMaterial);
                    newInstance.name = kTempMaterialName;
                    newSharedMaterials.Add(newInstance);
                    forEach?.Invoke(newInstance);
                }
                else {
                    newSharedMaterials.Add(sharedMaterial);
                    forEach?.Invoke(sharedMaterial);
                }
            }

            renderer.SetSharedMaterials(newSharedMaterials);
        }
    }
}
