using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class FixNegativeScaling {

    [MenuItem ("Tools/Fix Negative Scaling")]
    private static void FixSelectedNegativeScaling() {
        
        List<Transform> selectedTransforms = Selection.transforms.ToList();
        List<Transform> transformsToProcess = new List<Transform>();

        foreach (var transform in selectedTransforms) {
            GetAllChildren(transform, ref transformsToProcess);
        }

        foreach (var transform in transformsToProcess) {
            var originalScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), Mathf.Abs(originalScale.y), Mathf.Abs(originalScale.z));
            EditorUtility.SetDirty(transform);
        }
    }

    private static void GetAllChildren(Transform parent, ref List <Transform> transforms) {

        transforms.Add(parent);
        
        for (int i = 0; i < parent.childCount; i++) {
            GetAllChildren(parent.GetChild(i), ref transforms);
        }
    }
}
