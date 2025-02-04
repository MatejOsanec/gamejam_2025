using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[AddToToolDirectory(
    displayName: "Show render queue",
    description: "Allows close inspection of render queue bands, showing all materials within a range.",
    packageName: "HMLib package",
    maintainer: ToolMaintainer.TechArt,
    openToolFunctionName: nameof(ShowWindow),
    labelTypes: LabelType.Core | LabelType.TechArt)]
public class RenderQueuesWindow : EditorWindow {

        private Object _shader = (Shader) default;
        private List<Material> _materials = new List<Material>();
        private List<Shader> _ourShaders = new List<Shader>();
        private bool _showMaterials = true;
        private Shader _cachedShader = default;
        private Vector2 _scrollPos;
        private bool _allShaders = false;
        private bool _cachedAllShaders = false;
        private string _path = "Assets/";
        private string _cachedPath = "Assets/";

        private Vector2Int _minMaxQueue = new Vector2Int(1000,4500);

        [MenuItem("Tools/Show Render Queue")]
        private static void ShowWindow() {
            var window = GetWindow<RenderQueuesWindow>();
            window.titleContent = new GUIContent("RenderQueue");
            window.Show();
        }

        private void OnGUI() {

            if (_shader != null && _cachedShader != _shader) {
                _cachedShader = (Shader) _shader;
                FindMaterials();
            }

            if (_cachedAllShaders != _allShaders) {
                _cachedAllShaders = _allShaders;
                FindMaterials();
            }

            if (_cachedPath != _path) {
                _cachedPath = _path;
                FindMaterials();
            }

            _scrollPos = EditorGUILayout.BeginScrollView (_scrollPos, false, false);
            GUI.skin.button.wordWrap = true;

            _allShaders = EditorGUILayout.Toggle("All Shaders", _allShaders);

            if (!_allShaders) {
                EditorStyles.label.fontStyle = FontStyle.Bold;
                _shader = (Shader)EditorGUILayout.ObjectField("Shader:", _shader, typeof(Shader), false);
                EditorStyles.label.fontStyle = FontStyle.Normal;
            }

            _path = EditorGUILayout.TextField("Path", _path);

            _minMaxQueue = EditorGUILayout.Vector2IntField("RenderQueue Range:", _minMaxQueue);

            EditorGUILayout.Space(8);
            DisplayMaterials();
            EditorGUILayout.Space(8);

            EditorGUILayout.EndScrollView();
        }

        private void FindMaterials() {

            _materials.Clear();
            _ourShaders.Clear();

            var allMaterials = AssetDatabase.FindAssets("t: Material");

            if (_allShaders) {
                var allShaders = AssetDatabase.FindAssets("t: Shader");

                foreach (var guid in allShaders) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    if (path.StartsWith("Assets/Visuals/Shaders")) {
                        var shader = (Shader)AssetDatabase.LoadAssetAtPath(path, typeof(Shader));
                        _ourShaders.Add(shader);
                    }
                }
            }
            else {
                _ourShaders.Add((Shader)_shader);
            }

            foreach (var guid in allMaterials)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith(_path)) {
                    continue;
                }

                var material = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));

                if (material.shader == _shader || _allShaders && _ourShaders.Contains(material.shader)) {
                    _materials.Add(material);
                }
            }
        }

        private void DisplayMaterials() {

            if (_materials.Count > 0) {
                EditorStyles.label.fontStyle = FontStyle.Bold;
                _showMaterials = EditorGUILayout.BeginFoldoutHeaderGroup(_showMaterials, "Found Materials");
                EditorStyles.label.fontStyle = FontStyle.Normal;
                EditorGUI.indentLevel++;

                if (_showMaterials) { // Crazy, but you can't use another if inside this if
                    var currentMaterials = _materials.Where(m => m.renderQueue <= _minMaxQueue.y && m.renderQueue >= _minMaxQueue.x).OrderBy(m => m.renderQueue).ToList();

                    foreach (var mat in currentMaterials) {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        EditorGUILayout.LabelField($"{mat.renderQueue}", GUILayout.Width(50.0f));
                        EditorGUILayout.ObjectField(mat, typeof(Material), false);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndFoldoutHeaderGroup();

            } else {
                EditorGUI.indentLevel++;
                EditorGUILayout.SelectableLabel("No material found");
                EditorGUI.indentLevel--;
            }
        }
    }

