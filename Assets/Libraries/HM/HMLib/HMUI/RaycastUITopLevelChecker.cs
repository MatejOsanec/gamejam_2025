using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HMUI {

    public class RaycastUITopLevelChecker : MonoBehaviour {

        private List<RaycastResult> results = new List<RaycastResult>();

        private Canvas _canvas;

        protected void Awake() {

            var canvases = GetComponentsInParent<Canvas>();
            Assert.IsTrue(canvases.Length > 0);
            _canvas = canvases[canvases.Length - 1];
        }

        public bool isOnTop {

            get {
                var rectTransform = transform as RectTransform;
                RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, rectTransform.TransformPoint(rectTransform.rect.center));
                var eventSystem = EventSystem.current;
                var pointerEventData = new PointerEventData(eventSystem);
                pointerEventData.position = new Vector2(0.0f, 0.0f);

                // Raycast through all raycasters in the scene.
                EventSystem.current.RaycastAll(pointerEventData, results);
                return (results.Count > 0 && GameObject.ReferenceEquals(results[0].gameObject, gameObject));
            }
        }
    }
}