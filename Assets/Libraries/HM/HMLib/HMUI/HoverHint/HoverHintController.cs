using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMUI {

    public class HoverHintController : MonoBehaviour {

#if BS_TOURS
        [SerializeField] HoverHintPanel _hoverHintPanel = default;
#else
        [SerializeField] HoverHintPanel _hoverHintPanelPrefab = default;
#endif

        private const float kShowHintDelay = 0.6f;
        private const float kHideHintDelay = 0.3f;

        private bool _isHiding;
#if BS_TOURS
        private readonly List<HoverHint> _activeHints = new List<HoverHint>();
#else
        private HoverHintPanel _hoverHintPanel;
#endif

        protected void Awake() {

#if !BS_TOURS
            _hoverHintPanel = Instantiate(_hoverHintPanelPrefab, transform);
#endif
            _hoverHintPanel.Hide();
        }

        protected void OnApplicationFocus(bool hasFocus) {

            if (!hasFocus && _hoverHintPanel.isShown) {
                _hoverHintPanel.Hide();
            }
        }

        public void ShowHint(HoverHint hoverHint) {

            if (string.IsNullOrEmpty(hoverHint.text)) {
                return;
            }

            _isHiding = false;
#if BS_TOURS
            _activeHints.Add(hoverHint);
#endif
            ShowHintInternal(hoverHint);
        }

        private void ShowHintInternal(HoverHint hoverHint) {

            StopAllCoroutines();

            if (_hoverHintPanel.isShown) {
                SetupAndShowHintPanel(hoverHint);
            }
            else {
                StartCoroutine(ShowHintAfterDelay(hoverHint, kShowHintDelay));
            }
        }

        public void HideHint(HoverHint hoverHint) {

            if (_isHiding) {
                return;
            }
#if BS_TOURS
            _activeHints.Remove(hoverHint);

            if (_activeHints.Count == 0) {
                StopAllCoroutines();
                StartCoroutine(HideHintAfterDelay(kHideHintDelay));
            }
            else {
                ShowHintInternal(_activeHints[_activeHints.Count-1]);
            }
#else
            StopAllCoroutines();
            StartCoroutine(HideHintAfterDelay(kHideHintDelay));
#endif
        }

        public void HideHintInstant(HoverHint hoverHint) {

#if BS_TOURS
            _activeHints.Remove(hoverHint);
            if (_activeHints.Count == 0) {
                StopAllCoroutines();
                if (_hoverHintPanel.isShown) {
                    _hoverHintPanel.Hide();
                }
            }
            else {
                ShowHintInternal(_activeHints[_activeHints.Count-1]);
            }
#else
            StopAllCoroutines();
            if (_hoverHintPanel.isShown) {
                _hoverHintPanel.Hide();
            }
#endif
        }

        private IEnumerator ShowHintAfterDelay(HoverHint hoverHint, float delay) {

            yield return new WaitForSeconds(delay);
            if (hoverHint != null) {
                SetupAndShowHintPanel(hoverHint);
            }
        }

        private IEnumerator HideHintAfterDelay(float delay) {

            _isHiding = true;
            yield return new WaitForSeconds(delay);
            _hoverHintPanel.Hide();
            _isHiding = false;
        }

        private void SetupAndShowHintPanel(HoverHint hoverHint) {

            var rootTransform = (RectTransform)GetScreenTransformForHoverHint(hoverHint);

            Rect hoverHintRect = new Rect();
            hoverHintRect.size = hoverHint.size;
            hoverHintRect.position = rootTransform.InverseTransformPoint(hoverHint.worldCenter);
            hoverHintRect.position -= hoverHintRect.size * 0.5f;
            _hoverHintPanel.Show(hoverHint.text, parent: rootTransform, containerSize: rootTransform.rect.size, spawnRect: hoverHintRect);
        }

        private static Transform GetScreenTransformForHoverHint(HoverHint hoverHint) {

            var t = hoverHint.transform;
            while (t != null) {
                if (t.GetComponent<Canvas>() && t.GetComponent<ScreenBase>()) {
                    return t;
                }
                t = t.parent;
            }

            return hoverHint.transform;
        }
    }
}
