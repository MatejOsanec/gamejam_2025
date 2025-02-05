using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

///<summary>
/// Window that can combine meshes defined with StaticBatchableMesh Component into one for performance optimizations.
/// When a scene is selected, it will automatically add a GameObject to your scene with the StaticBatchComponentContainer. When batching, that will hold all StaticBatchComponents
///</summary>
public class StaticBatcherWindow : EditorWindow {

    StaticMeshBatchingUtility batchingUtility = null;
    Vector2 _scrollPositionLeft = Vector2.zero;
    Vector2 _scrollPositionRight = Vector2.zero;
    bool _drawRightPane = false;

    [MenuItem("Tools/Static Batcher")]
    protected static void ShowWindow() {

        var window = GetWindow<StaticBatcherWindow>();
        window.titleContent = new GUIContent("Static Batcher");
        window.Show();
    }

    protected void OnGUI() {

        if (batchingUtility != null && !batchingUtility.selectedScene.isLoaded) {
            batchingUtility = null;
        }

        DrawHeader();

        EditorGUILayout.BeginHorizontal();
        try {
            DrawLeftPane();

            if(_drawRightPane) {
                DrawRightPane();
            }
        }
        finally {
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawHeader() {

        // Dropdown
        int countLoaded = SceneManager.sceneCount;
        Scene[] loadedScenes = new Scene[countLoaded];
        GenericMenu dropdownContent = new GenericMenu();
        for (int i = 0; i < countLoaded; i++) {
            loadedScenes[i] = SceneManager.GetSceneAt(i);
            if(batchingUtility == null) {
                batchingUtility = new StaticMeshBatchingUtility(loadedScenes[i]);
            }

            dropdownContent.AddItem(new GUIContent(loadedScenes[i].name), loadedScenes[i] == batchingUtility.selectedScene, OnSceneSelected, loadedScenes[i]);
        }
        if (EditorGUILayout.DropdownButton(new GUIContent(batchingUtility.selectedScene.name), FocusType.Passive)) {
            dropdownContent.ShowAsContext();
        }

        EditorGUILayout.ObjectField("Container", batchingUtility.componentContainer, typeof(StaticBatchedMeshContainer), true);

        // Batching buttons
        EditorGUILayout.BeginHorizontal();
        try {
            if (GUILayout.Button("Refresh List", EditorStyles.miniButtonLeft)) {
                batchingUtility.Reload();
            }
            if (GUILayout.Button("Batch", EditorStyles.miniButtonMid)) {
                batchingUtility.Reload();
                batchingUtility.Batch();
            }
            if (GUILayout.Button("Unbatch", EditorStyles.miniButtonRight)) {
                batchingUtility.Reload();
                batchingUtility.Unbatch();
            }
        }
        finally {
            EditorGUILayout.EndHorizontal();
        }

        // Toggle right pane
        _drawRightPane = GUILayout.Toggle(_drawRightPane, "Show GameObjects part of unity's static batching, which are not part of this system yet.", EditorStyles.toggle);
        GUILayout.Space(20);
    }

    private void DrawLeftPane() {

        // List entries that are part of our static batching system.
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Batchable Meshes (sorted by material)", EditorStyles.boldLabel);
        _scrollPositionLeft = EditorGUILayout.BeginScrollView(_scrollPositionLeft, false, true);
        try {
            foreach(var material in batchingUtility.batchMap.Keys) {
                GUILayout.Label(material.name, EditorStyles.boldLabel);
                foreach (var mesh in batchingUtility.batchMap[material]) {
                    if(GUILayout.Button(mesh.name, EditorStyles.miniButton)) {
                        Selection.activeGameObject = mesh;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
                GUILayout.Label("");
            }
        }
        finally {
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawRightPane() {

        // List entries that are not yet part of our static batching system.
        EditorGUILayout.BeginVertical();
        GUILayout.Label("'Missing' Static Meshes", EditorStyles.boldLabel);
        _scrollPositionRight = EditorGUILayout.BeginScrollView(_scrollPositionRight, false, true);
        try {
            var rootGOs = batchingUtility.selectedScene.GetRootGameObjects();
            List<MeshFilter> unityStaticMeshes = new List<MeshFilter>();
            foreach (var gameObject in rootGOs) {
                unityStaticMeshes.AddRange(gameObject.GetComponentsInChildren<MeshFilter>(true));
            }
            for (int i = unityStaticMeshes.Count-1; i >= 0; i--) {
                if (!unityStaticMeshes[i].gameObject.activeSelf) {
                    unityStaticMeshes.RemoveAt(i);
                    continue;
                }

                if (unityStaticMeshes[i].gameObject.GetComponent<StaticBatchableMesh>() != null) {
                    unityStaticMeshes.RemoveAt(i);
                    continue;
                }

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(unityStaticMeshes[i].gameObject);
                if (!flags.HasFlag(StaticEditorFlags.BatchingStatic)) {
                    unityStaticMeshes.RemoveAt(i);
                    continue;
                }
            }

            foreach (var mesh in unityStaticMeshes) {
                if(GUILayout.Button(mesh.gameObject.name, EditorStyles.miniButton)) {
                    Selection.activeGameObject = mesh.gameObject;
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            }

        }
        finally {
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    // Reloads all data
    private void OnSceneSelected(object scene) {

        batchingUtility.SetScene((Scene) scene);
    }
}
