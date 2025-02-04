using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

namespace HMUI {

    public class HoverHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] string _text = default;

        [Inject] readonly HoverHintController _hoverHintController = default;

        public string text { get => _text; set => _text = value; }

        public Vector2 size => ((RectTransform)transform).rect.size;

        public Vector3 worldCenter {
            get {
                ((RectTransform)transform).GetWorldCorners(_worldCornersTemp);
                Vector3 center = Vector3.zero;
                for (int i = 0; i < 4; i++) {
                    center += _worldCornersTemp[i];
                }
                return center * 0.25f;
            }
        }

        private readonly Vector3[] _worldCornersTemp = new Vector3[4];

        public void OnPointerEnter(PointerEventData eventData) {

            _hoverHintController.ShowHint(this);
        }

        public void OnPointerExit(PointerEventData eventData) {

            if (eventData.currentInputModule == null || eventData.currentInputModule.enabled == false) {
                _hoverHintController.HideHintInstant(this);
            }
            else {
                _hoverHintController.HideHint(this);
            }
        }

        protected void OnDisable() {

            if (_hoverHintController != null) {
                _hoverHintController.HideHintInstant(this);
            }
        }
    }

}
