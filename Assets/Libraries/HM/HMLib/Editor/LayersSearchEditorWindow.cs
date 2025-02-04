using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

public class LayersSearchEditorWindow : EditorWindow {

    private const int kLayerCount = 32;

    [MenuItem("Tools/Search Layers")]
    public static void ShowWindow() {

        EditorWindow.GetWindow(typeof(LayersSearchEditorWindow));
    }

    private int _selectedLayerIndex = 0;
    private Dictionary<int, LayerInfo> _layerInfos;
    private List<string> _scenesWithGameObjectsWithLayer;

    void OnGUI() {

        string[] layerNames = GetLayerNames();
        var newSelectedLayerIndex = EditorGUILayout.Popup("Layer", _selectedLayerIndex, layerNames);
        if (newSelectedLayerIndex != _selectedLayerIndex) {
            _scenesWithGameObjectsWithLayer = null;
            _selectedLayerIndex = newSelectedLayerIndex;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Search in Prefabs")) {
            var gameObjects = FilterGameObjectsByLayer(FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects(), _selectedLayerIndex);
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Object Browser");
            window.objects = gameObjects;
        }

        if (GUILayout.Button("Search in Loaded Scenes")) {
            var gameObjects = FilterGameObjectsByLayer(FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects(), _selectedLayerIndex);
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Object Browser");
            window.objects = gameObjects;
        }

        if (GUILayout.Button("Find Scenes for Layer")) {
            EditorUtility.DisplayProgressBar("Finding Scenes with Layer", "", 0.1f);
            _layerInfos = null;
            _scenesWithGameObjectsWithLayer = GetScenesWithGameObjectsWithLayer(_selectedLayerIndex);
            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("Find Cameras with Layer in Prefabs")) {
            EditorUtility.DisplayProgressBar("Finding Cameras with Layer in Prefabs", "", 0.1f);
            var cameras = FindUnityObjectsHelper.AllPrefabs.GetAllComponents<Camera>();
            EditorUtility.ClearProgressBar();
            ObjectBrowserEditorWindow window = EditorWindow.GetWindow<ObjectBrowserEditorWindow>("Object Browser");
            window.objects = FilterCamerasByLayer(cameras, _selectedLayerIndex);
        }

        GUILayout.Space(16.0f);
        if (GUILayout.Button("Generate Statistics")) {
            EditorUtility.DisplayProgressBar("Generating Statistics", "", 0.1f);
            _scenesWithGameObjectsWithLayer = null;
            _layerInfos = GenerateLayerStatistics();
            EditorUtility.ClearProgressBar();
        }

        GUILayout.Space(16.0f);

        if (_layerInfos != null) {
            GUILayout.Label("ALL LAYERS INFO");
            foreach (var layerInfo in _layerInfos.Values) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(layerInfo.layerName, GUILayout.Width(240.0f));
                GUILayout.Label("In Prefabs:", GUILayout.Width(80.0f));
                GUILayout.Label(layerInfo.numberOfGameObjectsInPrefabs.ToString(), GUILayout.Width(50.0f));
                GUILayout.Label("In Scenes:", GUILayout.Width(80.0f));
                GUILayout.Label(layerInfo.numberOfGameObjectsInScenes.ToString(), GUILayout.Width(50.0f));
                GUILayout.EndHorizontal();
            }
        }

        if (_scenesWithGameObjectsWithLayer != null) {
            GUILayout.Label("SCENES WITH LAYER");
            foreach (var sceneName in _scenesWithGameObjectsWithLayer) {
                GUILayout.Label(sceneName);
            }
        }
    }

    private class LayerInfo {
        public int layer;
        public string layerName;
        public int numberOfGameObjectsInScenes;
        public int numberOfGameObjectsInPrefabs;
    }

    private Dictionary<int, LayerInfo> GenerateLayerStatistics() {

        var layerInfos = new Dictionary<int, LayerInfo>(kLayerCount);

        for (int layer = 0; layer < kLayerCount; layer++) {

            var layerInfo = new LayerInfo();
            layerInfo.layer = layer;
            layerInfo.layerName = LayerMask.LayerToName(layer);
            if (layerInfo.layerName == "") {
                continue;
            }
            layerInfos.Add(layer, layerInfo);
        }

        // Prefabs
        var allGameObjectsInPrefabs = FindUnityObjectsHelper.AllPrefabs.GetAllGameObjects();
        foreach (var go in allGameObjectsInPrefabs) {
            layerInfos[go.layer].numberOfGameObjectsInPrefabs++;
        }

        // Scenes
        foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes) {

            // Skip disabled scenes.
            if (!editorBuildSettingsScene.enabled) {
                continue;
            }

            var sceneName = Path.GetFileNameWithoutExtension(editorBuildSettingsScene.path);
            EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
            var activeScene = EditorSceneManager.GetActiveScene();

            var allGameObjectsInScene = FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects();
            foreach (var go in allGameObjectsInScene) {
                layerInfos[go.layer].numberOfGameObjectsInScenes++;
            }
        }

        return layerInfos;
    }

    private List<string> GetScenesWithGameObjectsWithLayer(int layer) {

        var result = new List<string>();

        foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes) {

            // Skip disabled scenes.
            if (!editorBuildSettingsScene.enabled) {
                continue;
            }

            var sceneName = Path.GetFileNameWithoutExtension(editorBuildSettingsScene.path);
            EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
            var activeScene = EditorSceneManager.GetActiveScene();

            var allGameObjectsInScene = FindUnityObjectsHelper.LoadedScenes.GetAllGameObjects();
            foreach (var go in allGameObjectsInScene) {
                if (go.layer == layer) {
                    result.Add(sceneName);
                    break;
                }
            }
        }

        return result;
    }

    private List<GameObject> FilterGameObjectsByLayer(IEnumerable<GameObject> gameObjects, int layer) {

        var filteredGameObjects = new List<GameObject>();
        foreach (var gameObject in gameObjects) {
            if (gameObject.layer == layer) {
                filteredGameObjects.Add(gameObject);
            }
        }
        return filteredGameObjects;
    }

    private List<GameObject> FilterCamerasByLayer(IEnumerable<Camera> cameras, int layer) {

        var filteredGameObjects = new List<GameObject>();
        foreach (var camera in cameras) {
            if ((camera.cullingMask & (1 << layer)) != 0) {
                filteredGameObjects.Add(camera.gameObject);
            }
        }
        return filteredGameObjects;
    }

    private string[] GetLayerNames() {

        var layerNames = new string[kLayerCount];
        for (int layer = 0; layer < kLayerCount; layer++) {
            layerNames[layer] = LayerMask.LayerToName(layer);
        }
        return layerNames;
    }
}
