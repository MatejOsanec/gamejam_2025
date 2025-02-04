using UnityEngine;

///<summary>
/// Add this component to meshes to mark it for static batching.
///</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StaticBatchableMesh : MonoBehaviour { }