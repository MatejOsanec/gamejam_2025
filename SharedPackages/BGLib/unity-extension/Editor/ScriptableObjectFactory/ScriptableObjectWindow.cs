using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaber.UnityExtension.Editor;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectWindow : EditorWindow {

    private class TypeNamePairs {

        public readonly string[] names;
        public readonly List<Type> types;

        public TypeNamePairs(List<Type> types) {

            this.names = types.Select(x => x.FullName).ToArray();
            this.types = types;
        }
    }

    private List<Type> _allScriptableObjectTypes;
    private TypeNamePairs _filteredTypeNamePairs;

    private int _selectedIndex;
    private string _filter;
    private bool _initialized;
    private bool _closeAfterCreate = true;

    public static void Present() {

        var window = GetWindow<ScriptableObjectWindow>(
            utility: true,
            "Create a new ScriptableObject",
            focus: true
        );
        window.minSize = new Vector2(320.0f, 300.0f);
        window.maxSize = new Vector2(320.0f, 300.0f);
        window.ShowPopup();
    }

    protected void LazyInit(bool forced) {

        if (_initialized && !forced) {
            return;
        }

        _initialized = true;

        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        _allScriptableObjectTypes = new List<Type>();

        foreach (var assembly in allAssemblies) {
            // Get all classes derived from ScriptableObject
            var scriptableObjects =
                (from t in assembly.GetTypes() where t.IsSubclassOf(typeof(ScriptableObject)) select t);
            _allScriptableObjectTypes.AddRange(scriptableObjects);
        }

        _allScriptableObjectTypes.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
    }

    public void OnGUI() {

        LazyInit(forced: false);

        if (GUILayout.Button("Reload")) {
            LazyInit(forced: true);
        }

        GUILayout.Space(8.0f);
        GUILayout.Label("Filter");
        var newFilter = EditorGUILayout.TextField(_filter);
        if (newFilter != _filter) {

            var lowercaseNewFilter = newFilter.ToLower();
            if (String.IsNullOrEmpty(newFilter)) {
                _filteredTypeNamePairs = null;
            }
            else {
                var allScriptableObjectTypeNames =
                    _allScriptableObjectTypes.FindAll(x => x.FullName.ToLower().Contains(lowercaseNewFilter));
                _filteredTypeNamePairs = new TypeNamePairs(allScriptableObjectTypeNames);
            }
            _selectedIndex = 0;
            _filter = newFilter;
        }

        if (_filteredTypeNamePairs != null && _filteredTypeNamePairs.names.Length > 0) {

            GUILayout.Space(8.0f);
            GUILayout.Label("ScriptableObject Class");
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _filteredTypeNamePairs.names);

            GUILayout.Space(8.0f);

            if (GUILayout.Button("Create")) {

                var newScriptableObjectType = _filteredTypeNamePairs.types[_selectedIndex];
                var newScriptableObjectName = _filteredTypeNamePairs.names[_selectedIndex];

                //Strip SO part from name to follow our naming standards
                if (newScriptableObjectName.Substring(newScriptableObjectName.Length - 2) == "SO") {
                    newScriptableObjectName = newScriptableObjectName.Substring(0, newScriptableObjectName.Length - 2);
                }

                ScriptableObjectEditorExtensions.CreateScriptableObjectInSelectedProjectFolder(
                    newScriptableObjectType,
                    newScriptableObjectName
                );
                if (_closeAfterCreate) {
                    Close();
                }
            }
            _closeAfterCreate = GUILayout.Toggle(_closeAfterCreate, "Close after create");
        }
    }
}
