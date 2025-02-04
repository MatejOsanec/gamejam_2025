using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using BGLib.UnityExtension.Editor;

[CustomEditor(typeof(LightGroupElementsSpawner)), CanEditMultipleObjects]
public class LightGroupElementsSpawnerEditor : Editor {

    private LightGroup _lightGroup;
    private SerializedProperty _lightPrefabProperty;
    private SerializedProperty _useAlternatePrefabProperty;
    private SerializedProperty _alternateLightPrefabProperty;
    private GameObject _lightPrefabInstance;
    private GameObject _alternateLightPrefabInstance;
    private LightGroupElementsSpawner _targetComponent;

    protected void OnEnable() {

        _targetComponent = target as LightGroupElementsSpawner;
        _lightGroup = _targetComponent.gameObject.GetComponent<LightGroup>();
        _lightPrefabProperty = serializedObject.FindProperty(nameof(LightGroupElementsSpawner._lightPrefab));
        _useAlternatePrefabProperty =
            serializedObject.FindProperty(nameof(LightGroupElementsSpawner._useAlternatePrefab));
        _alternateLightPrefabProperty =
            serializedObject.FindProperty(nameof(LightGroupElementsSpawner._alternateLightPrefab));
        if (_lightGroup != null) {
            _lightGroup.respawnEvent += Refresh;
        }
    }

    public override void OnInspectorGUI() {

        EditorGUILayout.HelpBox(
            "This component is manually invoked from the Light Group Component - use with care!",
            MessageType.Warning
        );
        base.OnInspectorGUI();
    }

    protected void OnDisable() {

        if (_lightGroup != null) {
            _lightGroup.respawnEvent -= Refresh;
        }
    }

    protected void OnValidate() {

        if (Application.isPlaying) {
            return;
        }

        ValidatePrefabToSpawn(_lightPrefabProperty.objectReferenceValue as GameObject);

        if (_useAlternatePrefabProperty.boolValue) {
            ValidatePrefabToSpawn(_alternateLightPrefabProperty.objectReferenceValue as GameObject);
        }
    }

    protected void Refresh(GameObject rootGameObject) {

        if (PrefabUtilityExtension.IsPartOfVariantPrefab(this)) {
            return;
        }
        try {
            Assert.IsNotNull(_lightPrefabProperty);
            bool foundLightPrefabGO = _lightPrefabProperty.TryGetGameObject(out _lightPrefabInstance);
            Assert.IsTrue(
                foundLightPrefabGO,
                "LightGroupElementsSpawner: Attempted to refresh, but a light prefab was not defined."
            );
            Assert.IsNotNull(_useAlternatePrefabProperty);
            Assert.IsNotNull(_alternateLightPrefabProperty);
            if (_useAlternatePrefabProperty.boolValue) {
                bool foundAlternateLightPrefabGO = _alternateLightPrefabProperty.TryGetGameObject(out _alternateLightPrefabInstance);
                Assert.IsTrue(
                    foundAlternateLightPrefabGO,
                    "LightGroupElementsSpawner: Attempted to refresh, but alternate light prefab was not defined."
                );
            }
            int numberOfElements = _lightGroup.numberOfElements * _lightGroup.sameIdElements;
            int childCount = rootGameObject.transform.childCount;

            // Spawn again if either number of elements or one of the first two (to account for alternation) prefabs don't match
            if (childCount == numberOfElements &&
                rootGameObject.transform.GetChild(0).gameObject.name == GetPrefabToSpawn(0).name &&
                rootGameObject.transform.GetChild(1).gameObject.name == GetPrefabToSpawn(1).name) {
                return;
            }
            // Delete all children game objects
            for (int i = childCount - 1; i >= 0; i--) {
                DestroyImmediate(rootGameObject.transform.GetChild(i).gameObject);
            }

            // Spawn new elements
            for (int i = 0; i < numberOfElements; i++) {
                PrefabUtility.InstantiatePrefab(GetPrefabToSpawn(i), rootGameObject.transform);

            }
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }
    
    private GameObject GetPrefabToSpawn(int index) {

        if (_useAlternatePrefabProperty.boolValue) {
            return index % 2 == 0 ? _lightPrefabInstance : _alternateLightPrefabInstance;
        }
        return _lightPrefabInstance;
    }

    private void ValidatePrefabToSpawn(GameObject prefab) {

        var lightColorGroupParent = prefab.GetComponentInChildren<LightColorGroupParent>();
        var lightGroupRotationXTransform = prefab.GetComponentInChildren<LightGroupRotationXTransform>();
        var lightGroupRotationYTransform = prefab.GetComponentInChildren<LightGroupRotationYTransform>();
        var lightGroupRotationZTransform = prefab.GetComponentInChildren<LightGroupRotationZTransform>();
        var lightGroupTranslationXTransform = prefab.GetComponentInChildren<LightGroupTranslationXTransform>();
        var lightGroupTranslationYTransform = prefab.GetComponentInChildren<LightGroupTranslationYTransform>();
        var lightGroupTranslationZTransform = prefab.GetComponentInChildren<LightGroupTranslationZTransform>();

        Assert.IsFalse(
            lightColorGroupParent == null && lightGroupRotationXTransform == null &&
            lightGroupRotationYTransform == null && lightGroupRotationZTransform == null &&
            lightGroupTranslationXTransform == null && lightGroupTranslationYTransform == null &&
            lightGroupTranslationZTransform == null,
            $"{_targetComponent.gameObject.name}'s prefab for light group element spawner needs at least one of following components: LightColorGroupParent, LightGroup Translation, Rotation Transform"
        );
    }

}
