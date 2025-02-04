using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HMUI {

    public class EmptyBoxGraphic : Graphic {

        [SerializeField] float _depth = 1.0f;

        protected override void OnPopulateMesh(VertexHelper vh) {

            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            Vector2 v2Zero = Vector2.zero;
            
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y, _depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.x, v.w, _depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.z, v.w, _depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.z, v.y, _depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.x, v.y, -_depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.x, v.w, -_depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.z, v.w, -_depth), color32, v2Zero);
            vh.AddVert(new Vector3(v.z, v.y, -_depth), color32, v2Zero);
            
            vh.AddTriangle(0, 1, 1);
            vh.AddTriangle(2, 3, 3);
            vh.AddTriangle(4, 5, 5);
            vh.AddTriangle(6, 7, 7);
        }

        protected void OnDrawGizmosSelected() {
            
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(v.x, v.y, _depth), new Vector3(v.x, v.w, _depth));
            Gizmos.DrawLine(new Vector3(v.z, v.w, _depth), new Vector3(v.z, v.y, _depth));
            Gizmos.DrawLine(new Vector3(v.x, v.y, -_depth), new Vector3(v.x, v.w, -_depth));
            Gizmos.DrawLine(new Vector3(v.z, v.w, -_depth), new Vector3(v.z, v.y, -_depth));
        }
    }
}
