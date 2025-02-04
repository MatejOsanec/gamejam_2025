using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class ComponentRefresherRegistry {

    private static IEnumerable<IComponentRefresher> _components;

    static ComponentRefresherRegistry() {

        SceneManager.sceneLoaded += HandleSceneManagerSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneManagerSceneUnloaded;
        EditorApplication.hierarchyChanged += HandleEditorApplicationHierarchyChanged;
    }

    public static void ForceRefreshComponents() {

        if (_components == null) {
            _components = GatherComponents();
        }

        foreach (var c in _components) {
            c.__Refresh();
        }

        SceneView.RepaintAll();
    }

    private static void HandleSceneManagerSceneLoaded(Scene scene, LoadSceneMode loadMode) {

        _components = null;
    }

    private static void HandleSceneManagerSceneUnloaded(Scene arg0) {

        _components = null;
    }

    private static void HandleEditorApplicationHierarchyChanged() {

        _components = null;
    }

    private static IEnumerable<IComponentRefresher> GatherComponents() {

        return FindUnityObjectsHelper.LoadedScenes.GetAllComponents<IComponentRefresher>(includeInactive: false).ToArray();
    }
}
