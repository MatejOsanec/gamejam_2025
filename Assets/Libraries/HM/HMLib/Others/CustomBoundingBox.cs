using System;
using UnityEngine;
using System.Linq;

public class CustomBoundingBox : MonoBehaviour {

    [SerializeField] Vector3 _boundingBoxCenter = default;
    [SerializeField] Vector3 _boundingBoxSize = Vector3.one;

    #pragma warning disable 414
    [SerializeField] MeshRenderer _meshRenderer = default;
    #pragma warning restore 414

    protected void Awake() {

        _meshRenderer.localBounds = new Bounds(_boundingBoxCenter, _boundingBoxSize * 2.0f);
    }

#if UNITY_EDITOR

    private void OnValidate() {

        if (_meshRenderer == null) {
            _meshRenderer = GetComponent<MeshRenderer>();
            var localBounds = _meshRenderer.localBounds;
            _boundingBoxCenter = localBounds.center;
            _boundingBoxSize = localBounds.extents;
        }
    }

    private void OnDrawGizmosSelected() {

        if (!UnityEditor.Selection.gameObjects.Contains(transform.gameObject)) {
            return;
        }

        var meshFilterTransform = _meshRenderer.transform;
        var position = meshFilterTransform.position;
        var scale = meshFilterTransform.lossyScale;

        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawWireCube(Vector3.Scale(scale, _boundingBoxCenter) + position, Vector3.Scale(scale, _boundingBoxSize * 2.0f));

        if (_meshRenderer) {
            var bounds = _meshRenderer.bounds;
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2.0f);
        }
    }
#endif

}
