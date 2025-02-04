using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRUIControls
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRPointer), true)]
    class VRPointerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                VRPointer src = target as VRPointer;
                GUILayout.Label("State: " + src.state);
            }
        }
    }
}
