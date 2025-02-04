namespace BGLib.UnityExtension.Editor.BezierCurves {

    using BGLib.UnityExtension.BezierCurves;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(CurveAnimation), editorForChildClasses: true)]
    public class CurveAnimationEditor : Editor {

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            var curveAnim = target as CurveAnimation;

            if (curveAnim == null) {
                return;
            }

            var prevValue = GUI.enabled;
            GUI.enabled = !curveAnim.isPlaying;
            if (GUILayout.Button("Animate")) {
                if (!EditorApplication.isPlaying) {
                    curveAnim.StartAnimation();
                }
                else {
                    EditorCoroutineUtility.StartCoroutine(curveAnim.Animate(withDelay: false), this);
                }
            }
            GUI.enabled = prevValue;
        }
    }
}
