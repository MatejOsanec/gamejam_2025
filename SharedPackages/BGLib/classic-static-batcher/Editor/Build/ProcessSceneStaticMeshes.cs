using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

///<summary>
/// This class being present inserts another build step in processing the scene
/// When entering play mode, or when building the game, we'll freshly batch all meshes for each scene on which this is possible.
///</summary>
class ProcessSceneStaticMeshes : IProcessSceneWithReport {

    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report) {

        var shouldPerformStaticBatching = EditorPrefs.GetBool(StaticBatcherConstants.kStaticBatchingKey, defaultValue: true);
        // "report" is null, when OnProcessScene is called when changing Play Mode in Editor
        if (report == null && !shouldPerformStaticBatching) {
            return;
        }

        StaticMeshBatchingUtility batchingUtility = new StaticMeshBatchingUtility(scene);
        if (batchingUtility.batchMap.Count <= 0) {
            return;
        }
        
        Debug.Log($"Performing static batching on {scene.name}");
        batchingUtility.Batch();
        AssetDatabase.SaveAssets();
    }
}
