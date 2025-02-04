using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BGLib.HierarchyIcons.Editor {

    public class HierarchyDataContainer : IDisposable {

        private readonly HashSet<int> _allComponents;
        private readonly Dictionary<int, HashSet<GameObject>> _componentParents;
        private readonly Type _type;
        private readonly System.Reflection.MethodInfo _findComponentsOfType;

        public HierarchyDataContainer(Type t) {

            _allComponents = new HashSet<int>();
            _componentParents = new Dictionary<int, HashSet<GameObject>>();
            _type = t;

            // Reflection because there's only a generic method and not one like UnityEngine.Object.FindObjectsOfType(type);
            _findComponentsOfType = typeof(PrefabStage).GetMethod("FindComponentsOfType").MakeGenericMethod(_type);

            EditorSceneManager.sceneOpened += SceneOpenedCallback;
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        public bool IsComponent(int id) {

            return _allComponents.Contains(id);
        }

        public bool IsParentOfComponent(int id, out GameObject[] children) {

            var exists = _componentParents.TryGetValue(id, out var childrenHashSet);
            children = childrenHashSet?.ToArray();
            return exists;
        }

        private void HierarchyChanged() {

            RecalculateDependencies();
        }

        private void SceneOpenedCallback(Scene scene, OpenSceneMode mode) {

            RecalculateDependencies();
        }

        private void RecalculateDependencies() {

            _allComponents.Clear();
            _componentParents.Clear();

            var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            Transform root = null;
            if (currentPrefabStage != null && currentPrefabStage.prefabContentsRoot != null) {
                root = currentPrefabStage.prefabContentsRoot.transform;
            }

            object components = null;
            if (currentPrefabStage == null) {
                components = UnityEngine.Object.FindObjectsOfType(_type, true);
            }
            else {
                // Reflection because there's only a generic method and not one like UnityEngine.Object.FindObjectsOfType(type);
                components = _findComponentsOfType.Invoke(currentPrefabStage, new object[] { });
            }

            foreach (var component in components as MonoBehaviour[]) {
                RegisterComponent(component, root);
            }
        }

        private void RegisterComponent(MonoBehaviour component, Transform root) {

            var gameObject = component.gameObject;

            _allComponents.Add(gameObject.GetInstanceID());

            if (gameObject.transform.parent == null) {
                return;
            }
            RegisterComponentParent(gameObject.transform.parent, gameObject, root);
        }

        private void RegisterComponentParent(Transform parent, GameObject child, Transform root) {

            var parentId = parent.gameObject.GetInstanceID();

            if (parent == root) {
                return;
            }

            if (!_componentParents.TryGetValue(parentId, out var children)) {
                children = new HashSet<GameObject>();
                _componentParents.Add(parentId, children);
            }

            children.Add(child);

            if (parent.parent == null) {
                return;
            }

            RegisterComponentParent(parent.parent, child, root);
        }

        public void Dispose() {

            _allComponents.Clear();
            _componentParents.Clear();

            EditorSceneManager.sceneOpened -= SceneOpenedCallback;
            EditorApplication.hierarchyChanged -= HierarchyChanged;
        }
    }
}
