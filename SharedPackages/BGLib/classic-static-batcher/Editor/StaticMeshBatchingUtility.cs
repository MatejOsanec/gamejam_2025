using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

///<summary>
/// Back-end for both StaticBatcherWindow and ProcessSceneStaticMeshes
///</summary>
public class StaticMeshBatchingUtility {

    public Scene selectedScene { get; private set; }
    public StaticBatchedMeshContainer componentContainer { get; private set; }
    public Dictionary<Material, List<GameObject>> batchMap { get; private set; }
    readonly string kMeshSubDirectory = "BatchedMeshes";

    public StaticMeshBatchingUtility(Scene scene) {

        selectedScene = scene;
        Initialize();
    }

    ///<summary>
    /// Combines the meshes into one and disables the unused meshes
    ///</summary>
    public void Batch() {

        if (componentContainer == null) {
            return;
        }

        Unbatch();

        foreach (var materialListPair in batchMap) {
            CombineInstance[] combineInstances = new CombineInstance[materialListPair.Value.Count];
            Material m = materialListPair.Key;

            // Disable all meshes, add them to the combineInstance
            for (int i = 0; i < materialListPair.Value.Count; i++) {
                GameObject gameObject = materialListPair.Value[i];
                combineInstances[i].mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
                combineInstances[i].transform = gameObject.transform.localToWorldMatrix;
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            GameObject staticBatch = new GameObject(m.name);
            staticBatch.layer = LayerMask.NameToLayer("Environment");
            MeshFilter meshFilter = staticBatch.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = staticBatch.AddComponent<MeshRenderer>();
            staticBatch.transform.parent = componentContainer.transform;

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = m.name;
            combinedMesh.CombineMeshes(combineInstances);
            meshFilter.sharedMesh = combinedMesh;
            meshRenderer.sharedMaterial = m;

            SaveMeshToDisk(combinedMesh, m.name);
        }
    }

    ///<summary>
    /// Attempt to undo what Batch() potentially did
    ///</summary>
    public void Unbatch() {

        if (componentContainer == null) {
            return;
        }

        // Ensure all meshes are removed from the scene
        for (var i = componentContainer.transform.childCount - 1; i >= 0; i--) {
            Object.DestroyImmediate(componentContainer.transform.GetChild(i).gameObject);
        }

        // Re-enable all original meshes.
        foreach (var materialListPair in batchMap) {
            foreach (GameObject gameObject in materialListPair.Value) {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        DestroyMeshesOnDisk();
    }

    public void Reload() {

        Initialize();
    }

    public void SetScene(Scene scene) {

        selectedScene = scene;
        Initialize();
    }

    private void Initialize() {

        batchMap = new Dictionary<Material, List<GameObject>>();

        // Find all components we need to use
        var rootGOs = selectedScene.GetRootGameObjects();
        List<StaticBatchableMesh> batchableMeshes = new List<StaticBatchableMesh>();
        foreach (var gameObject in rootGOs) {
            batchableMeshes.AddRange(gameObject.GetComponentsInChildren<StaticBatchableMesh>(true));
            if(componentContainer == null) {
                componentContainer = gameObject.GetComponentInChildren<StaticBatchedMeshContainer>();
            }
        }

        if (batchableMeshes.Count <= 0) {
            return;
        }

        // Create container if none exists
        if (componentContainer == null) {
            GameObject containerGameObject = new GameObject("Static Batch Component Container");
            componentContainer = containerGameObject.AddComponent<StaticBatchedMeshContainer>();
            if(rootGOs.Length > 0) {
                containerGameObject.transform.parent = rootGOs[0].transform;
            }

            Undo.RegisterCreatedObjectUndo(containerGameObject, "Create Static Batch Component Container");
        }

        // Parse the batchable meshes into a map keyed on material.
        foreach (var batchableMesh in batchableMeshes) {
            if (!batchableMesh.gameObject.activeSelf) {
                continue;
            }
            
            MeshRenderer renderer = batchableMesh.GetComponent<MeshRenderer>();

            if(renderer.sharedMaterial == null) {
                continue;
            }

            if(!batchMap.ContainsKey(renderer.sharedMaterial)) {
                batchMap.Add(renderer.sharedMaterial, new List<GameObject>());
            }

            batchMap[renderer.sharedMaterial].Add(batchableMesh.gameObject);
        }
    }
    
    private void SaveMeshToDisk(Mesh meshToSave, string fileName) {

        MeshUtility.Optimize(meshToSave);

        var rootDir = GetRootDir();
        var meshDir = Path.Combine(rootDir, kMeshSubDirectory);
        if (!AssetDatabase.IsValidFolder(meshDir)) {
            AssetDatabase.CreateFolder(rootDir, kMeshSubDirectory);
        }

        AssetDatabase.CreateAsset(meshToSave, Path.Combine(meshDir, fileName + ".asset"));
        AssetDatabase.SaveAssets();
    }

    private void DestroyMeshesOnDisk() {

        var rootDir = GetRootDir();
        var meshDir = Path.Combine(rootDir, kMeshSubDirectory);

        if (!AssetDatabase.IsValidFolder(meshDir)) {
            return;
        }

        try {
            AssetDatabase.StartAssetEditing();

            var assets = AssetDatabase.FindAssets("t:Mesh", new string[] { meshDir });
            foreach (var guid in assets) {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            }
            AssetDatabase.DeleteAsset(meshDir);
        }
        finally {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }

    private string GetRootDir() {
        
        return Path.GetDirectoryName(Path.GetDirectoryName(selectedScene.path));
    }
}
