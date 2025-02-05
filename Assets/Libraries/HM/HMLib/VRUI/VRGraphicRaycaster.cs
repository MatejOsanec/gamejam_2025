using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HMUI;


namespace VRUIControls {

    [RequireComponent(typeof(Canvas))]
    public class VRGraphicRaycaster : BaseRaycaster {

        [SerializeField] LayerMask _blockingMask = -1;

       readonly PhysicsRaycasterWithCache _physicsRaycaster = default;

        public override Camera eventCamera => null;

        private Canvas _canvas;
        private readonly List<VRGraphicRaycastResult> _raycastResults = new List<VRGraphicRaycastResult>();
        private readonly CurvedCanvasSettingsHelper _curvedCanvasSettingsHelper = new CurvedCanvasSettingsHelper();

        private const float kPhysics3DRaycastDistance = 6.0f;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void NoDomainReloadInit() {

            // Not sure why it was not happening before, maybe Unity version, but with domain reload disabled
            // this static instance is sometimes retained between Editor Playmode sessions, containing destroyed GOs
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled) {
                Type type = typeof(GraphicRegistry);
                System.Reflection.FieldInfo field = type.GetField("s_Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                field.SetValue(null, null);
            }
        }
#endif

        protected override void OnEnable() {

            base.OnEnable();
            _curvedCanvasSettingsHelper.Reset();
            _canvas = GetComponent<Canvas>();
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {

            var curvedCanvasSettings = _curvedCanvasSettingsHelper.GetCurvedCanvasSettings(_canvas);
            float curvedUIRadius = curvedCanvasSettings == null ? 0 : curvedCanvasSettings.radius;
            float hitDistance = float.MaxValue;

            var ray = new Ray(eventData.pointerCurrentRaycast.worldPosition, eventData.pointerCurrentRaycast.worldNormal);

            // Physics hits (to block the ray)
            {
                // 3D hits
                {
                    if (_physicsRaycaster.Raycast(ray, out var hit, kPhysics3DRaycastDistance, _blockingMask)) {
                        hitDistance = hit.distance;
                        var castResult = new RaycastResult {
                            gameObject = hit.collider.gameObject,
                            module = this,
                            distance = hitDistance,
                            screenPosition = new Vector2(float.MaxValue, float.MaxValue),
                            worldPosition = hit.point,
                            index = resultAppendList.Count,
                            depth = 0,
                            sortingLayer = _canvas.sortingLayerID,
                            sortingOrder = _canvas.sortingOrder
                        };
                        resultAppendList.Add(castResult);
                    }
                }
                // 2D hits
                {
                    var hit = Physics2D.Raycast(ray.origin, ray.direction, eventData.pointerCurrentRaycast.depth, _blockingMask);
                    if (hit.collider != null) {
                        float currentHit = hit.fraction * eventData.pointerCurrentRaycast.depth;
                        if (currentHit < hitDistance) {
                            hitDistance = currentHit;
                        }
                    }
                }
            }

            RaycastCanvas(_canvas, ray, hitDistance, curvedUIRadius, _raycastResults);

            for (int index = 0; index < _raycastResults.Count; index++) {
                var go = _raycastResults[index].graphic.gameObject;

                var castResult = new RaycastResult {
                    gameObject = go,
                    module = this,
                    distance = _raycastResults[index].distance,
                    screenPosition = _raycastResults[index].insideRootCanvasPosition,
                    worldPosition = _raycastResults[index].position,
                    index = resultAppendList.Count,
                    depth = _raycastResults[index].graphic.depth,
                    sortingLayer = _canvas.sortingLayerID,
                    sortingOrder = _canvas.sortingOrder
                };
                resultAppendList.Add(castResult);
            }
        }

        struct VRGraphicRaycastResult {

            public readonly Graphic graphic;
            public readonly float distance;
            public readonly Vector3 position;
            public readonly Vector2 insideRootCanvasPosition;

            public VRGraphicRaycastResult(Graphic graphic, float distance, Vector3 position, Vector2 insideRootCanvasPosition) {

                this.graphic = graphic;
                this.distance = distance;
                this.position = position;
                this.insideRootCanvasPosition = insideRootCanvasPosition;
            }
        }

        
        private static readonly float[] _ray2DCircleIntersectionDistances = new float[2];

        private static void RaycastCanvas(Canvas canvas, Ray ray, float hitDistance, float curvedUIRadius, List<VRGraphicRaycastResult> results) {

            results.Clear();

            var canvasTransform = (RectTransform)canvas.transform;
            var canvasPosition = canvasTransform.position;
            var canvasForward = canvasTransform.forward;

            var distance = 0.0f;
            Vector3 onCanvasWorldPosition;
            Vector3 worldPosition;

            // Flat UI
            if (Math.Abs(curvedUIRadius) < 0.1f) {

                // http://geomalgorithms.com/a06-_intersect-2.html
                distance = (Vector3.Dot(canvasForward, canvasPosition - ray.origin) / Vector3.Dot(canvasForward, ray.direction));
                if (distance < 0 || distance >= hitDistance) {
                    return;
                }

                worldPosition = ray.GetPoint(distance);
                onCanvasWorldPosition = worldPosition;
            }
            // Curved UI
            else {
                var rootCanvasTransform = (RectTransform)canvas.rootCanvas.transform;
                var rootCanvasPosition = rootCanvasTransform.position;
                var rootCanvasRotation = rootCanvasTransform.rotation;
                var localSpaceCurvedUIRadius = curvedUIRadius * rootCanvasTransform.lossyScale.z;
                var localCurvedCanvasCircleCenter = new Vector2(0.0f, -localSpaceCurvedUIRadius);

                var localRay = new Ray {
                    origin = Quaternion.Inverse(rootCanvasRotation) * (ray.origin - rootCanvasPosition),
                    direction = rootCanvasTransform.InverseTransformDirection(ray.direction)
                };
                var ray2D = new Ray2D(new Vector2(localRay.origin.x, localRay.origin.z), new Vector2(localRay.direction.x, localRay.direction.z));

                // Interaction is possible only from inside of the circle
                if (ray2D.CircleIntersections(localCurvedCanvasCircleCenter, localSpaceCurvedUIRadius, _ray2DCircleIntersectionDistances) != 1) {
                    return;
                }

                var circleIntersection2D = ray2D.GetPoint(_ray2DCircleIntersectionDistances[0]);
                var rayToIntersection2D = (circleIntersection2D - ray2D.origin);
                float localPosY = rayToIntersection2D.magnitude * localRay.direction.y / Mathf.Sqrt(localRay.direction.x * localRay.direction.x + localRay.direction.z * localRay.direction.z) + localRay.origin.y;
                var localPosition = new Vector3(circleIntersection2D.x, localPosY, circleIntersection2D.y);

                worldPosition = (rootCanvasRotation * localPosition) + rootCanvasPosition;
                distance = (worldPosition - ray.origin).magnitude;

                var localCanvasForward = Vector3.forward;
                float alpha = -Vector2.SignedAngle(new Vector2(localCanvasForward.x, localCanvasForward.z), circleIntersection2D - localCurvedCanvasCircleCenter);
                var onCanvasLocalPosition = new Vector3(alpha * Mathf.Deg2Rad * localSpaceCurvedUIRadius, localPosition.y, localPosition.z);
                onCanvasWorldPosition = (rootCanvasRotation * onCanvasLocalPosition) + rootCanvasPosition;

                var canvasRectTransform = (RectTransform)canvas.transform;
                if (!canvasRectTransform.rect.Contains(canvasRectTransform.InverseTransformPoint(onCanvasWorldPosition))) {
                    return;
                }
            }

            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            for (int i = 0; i < graphics.Count; i++) {

                var graphic = graphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget) {
                    continue;
                }

                var graphicRecTransform = (RectTransform)graphic.transform;
                var localPosition = graphicRecTransform.InverseTransformPoint(onCanvasWorldPosition);
                var graphicRect = graphicRecTransform.rect;

                if (!graphicRect.Contains(localPosition)) {
                    continue;
                }

                results.Add(new VRGraphicRaycastResult(graphic: graphic, distance: distance, position: worldPosition, insideRootCanvasPosition: onCanvasWorldPosition));
            }

            results.Sort((g1, g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
        }
    }
}
