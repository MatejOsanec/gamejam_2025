using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Assertions;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using Object = UnityEngine.Object;

public class ObjectBrowserEditorWindow : EditorWindow {

    public IList<UnityObjectWithDescription> objectDescriptions {
        private get => _objectDescriptions;
        set {
            _objectDescriptions = value;
            _idx = 0;
            Repaint();
            RefreshSelection();
        }
    }

    public IReadOnlyList<Object> objects {
        set {
            _objectDescriptions = new UnityObjectWithDescription[value.Count];
            for (int i = 0; i < value.Count; i++) {
                _objectDescriptions[i] = new UnityObjectWithDescription(value[i], DescriptionForObject(value[i]));
            }
        }
    }

    private IList<UnityObjectWithDescription> _objectDescriptions;

    private int _idx = 0;

    private static string DescriptionForObject(Object obj) {

        if (obj is GameObject go) {

            string description = go.name;

            while (go.transform.parent != null) {
                go = go.transform.parent.gameObject;
                description = $"{go.name}\\{description}";
            }
            return description;
        }
        else {
            return obj.name;
        }
    }

    private void OnGUI() {

        if (objectDescriptions == null || objectDescriptions.Count == 0) {
            return;
        }

        _idx = Mathf.Clamp(_idx, 0, _objectDescriptions.Count - 1);

        var item = objectDescriptions[_idx];
        
        while ((item == null || item.obj == null) && objectDescriptions.Count > 0) {
            
            objectDescriptions.RemoveAt(_idx);
            if (_idx >= objectDescriptions.Count) {
                _idx = objectDescriptions.Count - 1;
            }

            if (_idx < 0) {
                _idx = 0;
                item = null;
            }
            else {
                item = objectDescriptions[_idx];
            }
        }
        
        if (objectDescriptions.Count == 0) {
            GUILayout.Label("No objects found!");
            return;
        }
        
        GUI.skin.label.wordWrap = true;
        GUILayout.Label(item.obj.name + " " + (_idx + 1) + "/" + objectDescriptions.Count, GUILayout.Width(800), GUILayout.Height(40));
        GUILayout.Label(item.description, GUILayout.Width(800));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Prev", GUILayout.Width(100))) {
            _idx = Mathf.Clamp(_idx - 1, 0, objectDescriptions.Count - 1);
            RefreshSelection();
        }
        if (GUILayout.Button("Next", GUILayout.Width(100))) {
            _idx = Mathf.Clamp(_idx + 1, 0, objectDescriptions.Count - 1);
            RefreshSelection();
        }

        GUILayout.EndHorizontal();

        if (item.obj is Component) {
            Component component = objectDescriptions[_idx].obj as Component;
            if (EditorUtility.IsPersistent(component)) {
                Transform rootTransform = component.transform.root;
                if (rootTransform != null) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Prefab - InstanceID: " + component.GetInstanceID());
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(rootTransform.gameObject, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Select Prefab")) {
                        Selection.activeObject = rootTransform.gameObject;
                        EditorGUIUtility.PingObject(rootTransform.gameObject);
                    }
                }
            }
        }

    }

    private void RefreshSelection() {

        if (_idx >= 0 && _idx < objectDescriptions.Count) {
            Selection.activeObject = objectDescriptions[_idx].obj;
            EditorGUIUtility.PingObject(objectDescriptions[_idx].obj);
        }
    }
}