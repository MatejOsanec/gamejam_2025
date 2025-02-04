using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class CombineSelectedMeshes : MonoBehaviour {

    [MenuItem ("Tools/Combine selected meshes")]
    static void CombineMeshes() {

        Material material = null;                

        // Create list of selected mesh filters
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        GameObject[] selectedGameObjects = Selection.gameObjects;

        Transform parent = selectedGameObjects.Length > 0 ? selectedGameObjects[0].transform.parent : null;

        foreach (GameObject go in selectedGameObjects) {
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

            if (meshRenderer != null) {
                if (material == null) {
                    material = meshRenderer.sharedMaterial;
                }
                else if (material != meshRenderer.sharedMaterial) {
                    Debug.LogWarning("Can't combine meshes with different material");
                    return;
                }
            }

            if (meshFilter != null) {
                meshFilters.Add(meshFilter);
            }                
        }
            
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        GameObject newGO = new GameObject("CombinedMesh");
        newGO.transform.SetParent(parent, true);
        MeshFilter newGOMeshFilter = newGO.AddComponent<MeshFilter>();
        newGOMeshFilter.sharedMesh = new Mesh();
        newGOMeshFilter.sharedMesh.name = "CombinedMesh";
        newGOMeshFilter.sharedMesh.CombineMeshes(combine);

        MeshRenderer newGOMeshRenderer = newGO.AddComponent<MeshRenderer>();
        newGOMeshRenderer.receiveShadows = false;
        newGOMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		newGOMeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        newGOMeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        newGOMeshRenderer.sharedMaterial = material;

        Debug.Log("Meshes combined (vertices: " + newGOMeshFilter.sharedMesh.vertices.Length + ", triangles: " + newGOMeshFilter.sharedMesh.triangles.Length + ")");
    }
}
