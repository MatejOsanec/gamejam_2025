namespace BGLib.UnityExtension.Editor.BezierCurves {

    using BGLib.UnityExtension.BezierCurves;
    using UnityEditor;
    using UnityEngine;

    public class BaseBezierCurveEditor<T> : Editor where T : BaseBezierCurve {

        /// <summary>
        /// Draws Curve and returns true is was able
        /// </summary>
        /// <param name="curve">Gives out curve if wss possible to draw it</param>
        /// <returns></returns>
        protected bool InternalOnSceneGUI(out T curve) {

            curve = target as T;

            if (curve == null || !curve.isReady) {
                return false;
            }

            var data = curve.GetBezierCurveData();
            Handles.DrawBezier(
                data.startPoint,
                data.endPoint,
                data.startControlPoint,
                data.endControlPoint,
                curve.lineColor,
                Texture2D.normalTexture,
                curve.lineWidth
            );

            return true;
        }
    }

    [CustomEditor(typeof(GenericBezierCurve))]
    public class GenericBezierCurveEditor : BaseBezierCurveEditor<GenericBezierCurve> {

        public void OnSceneGUI() {

            if (!InternalOnSceneGUI(out var curve)) {
                return;
            }

            var data = curve.GetBezierCurveData();

            data.startPoint = Handles.PositionHandle(data.startPoint, Quaternion.identity);
            data.endPoint = Handles.PositionHandle(data.endPoint, Quaternion.identity);

            data.startControlPoint = Handles.PositionHandle(data.startControlPoint, Quaternion.identity);
            data.endControlPoint = Handles.PositionHandle(data.endControlPoint, Quaternion.identity);
            curve.SetBezierCurveData(data);
        }
    }

    [CustomEditor(typeof(ObjectBasedBezierCurve))]
    public class ObjectBasedBezierCurveEditor : BaseBezierCurveEditor<ObjectBasedBezierCurve> {

        public void OnSceneGUI() {

            if (!InternalOnSceneGUI(out var curve)) {
                return;
            }

            var data = curve.GetBezierCurveData();

            data.startControlPoint = Handles.PositionHandle(data.startControlPoint, Quaternion.identity);
            data.endControlPoint = Handles.PositionHandle(data.endControlPoint, Quaternion.identity);
            curve.SetBezierCurveData(data);
        }
    }
}
