using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ColorLibrary {

    public class SimpleColorTabEditor : IColorLibraryTab {

        string _filterText = string.Empty;

        private GUIStyle headerGUIStyle => _headerGUIStyle ?? (_headerGUIStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 });
        private GUIStyle headerBackgroundStyle => _headerBackgroundStyle ?? (_headerBackgroundStyle = new GUIStyle { padding = new RectOffset(0, 10, 0, 10) });
        private GUIStyle oddBackgroundStyle => _oddBackgroundStyle ?? (_oddBackgroundStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.grayTexture } });

        private GUIStyle _headerGUIStyle;
        private GUIStyle _headerBackgroundStyle;
        private GUIStyle _oddBackgroundStyle;

        private Dictionary<string, CacheDataWithSerializedProperty> _cachedColorObjects;
        private IEnumerable<string> _filteredColorObjects;

        private Vector2 _scrollPosition;

        public void Initialize() {

            RefreshCache();
        }

        public void OnGUI() {

            FilterGUI();
            HeaderGUI();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            int i = 0;
            foreach (var colorObject in _filteredColorObjects) {
                RowGUI(_cachedColorObjects[colorObject], i % 2 == 0);
                i++;
            }
            EditorGUILayout.EndScrollView();
        }

        public void RefreshCache() {

            _headerGUIStyle = null;
            _headerBackgroundStyle = null;
            _oddBackgroundStyle = null;

            var colorSerializedObjects = AssetDatabase.FindAssets($"t:{typeof(SimpleColorSO)}").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ColorSO>).OrderBy(color => color.name)
                .Select(color => new SerializedObject(color));

            _cachedColorObjects = new Dictionary<string, CacheDataWithSerializedProperty>();
            foreach (var serializedObject in colorSerializedObjects) {
                var colorName = serializedObject.FindProperty("m_Name").stringValue;
                _cachedColorObjects[colorName] = new CacheDataWithSerializedProperty(serializedObject, colorName, serializedObject.FindProperty("_color"));
            }
            FilterColorObjects();
        }

        private void FilterGUI() {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter");
            var prevFilter = _filterText;
            _filterText = EditorGUILayout.TextField(_filterText);
            if (prevFilter != _filterText) {
                FilterColorObjects();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void HeaderGUI() {

            EditorGUILayout.BeginHorizontal(headerBackgroundStyle);
            EditorGUILayout.LabelField("Name", headerGUIStyle);
            EditorGUILayout.LabelField("Color", headerGUIStyle);
            EditorGUILayout.EndHorizontal();
        }

        private void RowGUI(CacheDataWithSerializedProperty data, bool isOdd) {

            data.serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.BeginHorizontal(isOdd ? oddBackgroundStyle : GUIStyle.none);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(data.name);
            EditorGUILayout.PropertyField(data.serializedProperty, GUIContent.none);

            if (EditorGUI.EndChangeCheck()) {
                ComponentRefresherRegistry.ForceRefreshComponents();
            }

            EditorGUILayout.EndHorizontal();

            if (data.serializedObject.hasModifiedProperties) {
                data.serializedObject.ApplyModifiedProperties();
            }
        }

        private void FilterColorObjects() {

            if (_cachedColorObjects == null) {
                RefreshCache();
            }

            _filteredColorObjects = _cachedColorObjects.Where(data => data.Key.ToLower().Contains(_filterText.ToLower())).Select(data => data.Key);
        }
    }
}
