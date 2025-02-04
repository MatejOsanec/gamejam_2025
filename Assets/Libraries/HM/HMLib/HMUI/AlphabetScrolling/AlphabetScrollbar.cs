using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class AlphabetScrollbar : Interactable, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] TableView _tableView = default;

        [Space]
        [SerializeField] float _characterHeight = 1.0f;
        [SerializeField] Color _normalColor = new Color(0.0f, 0.0f, 0.0f, 0.25f);

        [Space]
        [SerializeField] TextMeshProUGUI _textPrefab = default;
        [SerializeField] TextMeshProUGUI[] _prealocatedTexts = default;
        [SerializeField] Image _highlightImage = default;

        private IReadOnlyList<AlphabetScrollInfo.Data> _characterScrollData = default;
        private List<TextMeshProUGUI> _texts;
        private int _highlightedCharacterIndex = -1;
        private bool _pointerIsDown;

        protected void Awake() {

            _highlightImage.enabled = false;
        }

        public void SetData(IReadOnlyList<AlphabetScrollInfo.Data> characterScrollData) {

            if (_texts == null) {
                _texts = new List<TextMeshProUGUI>(_prealocatedTexts);
            }

            _characterScrollData = characterScrollData ?? Array.Empty<AlphabetScrollInfo.Data>();

            // Setup texts.
            for (int i = 0; i < _characterScrollData.Count && i < _texts.Count; i++) {
                InitText(_texts[i], _characterScrollData[i].character);
            }

            PrepareTransforms();
        }

        // Unity UI callbacks
        public void OnPointerDown(PointerEventData eventData) {

            _pointerIsDown = true;
            int characterIdx = GetPointerCharacterIndex(eventData);
            _tableView.ScrollToCellWithIdx(idx: _characterScrollData[characterIdx].cellIdx, TableView.ScrollPositionType.Beginning, animated: true);
        }

        public void OnPointerUp(PointerEventData eventData) {
            _pointerIsDown = false;
        }

        public void OnPointerEnter(PointerEventData eventData) {

            StartCoroutine(PointerMoveInsideCoroutine(eventData));
        }

        public void OnPointerExit(PointerEventData eventData) {

            _highlightedCharacterIndex = -1;
            RefreshHighlight();
            StopAllCoroutines();
        }

        // Helpers
        private void PrepareTransforms() {

            _highlightImage.rectTransform.sizeDelta = new Vector2(0.0f, _characterHeight);

            // Create new texts if needed.
            for (int i = _texts.Count; i < _characterScrollData.Count; i++) {

                var text = Instantiate(_textPrefab, transform);
                InitText(text, _characterScrollData[i].character);
                _texts.Add(text);
            }

            var rectTransform = (RectTransform)transform;
            float posX = -(rectTransform.pivot.x - 0.5f) * rectTransform.rect.size.x;
            float posY = (_characterScrollData.Count - 1) * _characterHeight * 0.5f;

            // Layout and configure texts.
            for (int i = 0; i < _characterScrollData.Count; i++) {
                _texts[i].rectTransform.localPosition = new Vector2(posX, posY);
                _texts[i].enabled = true;
                posY -= _characterHeight;
            }

            // Disable unneeded texts.
            for (int i = _characterScrollData.Count; i < _texts.Count; i++) {
                _texts[i].enabled = false;
            }
        }

        private void RefreshHighlight() {

            if (_highlightedCharacterIndex < 0) {
                _highlightImage.enabled = false;
                return;
            }

            _highlightImage.enabled = true;
            _highlightImage.rectTransform.localPosition = new Vector3(_highlightImage.rectTransform.localPosition.x, (_characterScrollData.Count - 1) * _characterHeight * 0.5f - _highlightedCharacterIndex * _characterHeight);
        }

        private IEnumerator PointerMoveInsideCoroutine(PointerEventData eventData) {

            while (true) {

                var hoverCharacterIndex = GetPointerCharacterIndex(eventData);

                if (hoverCharacterIndex != _highlightedCharacterIndex) {
                    _highlightedCharacterIndex = hoverCharacterIndex;
                    if (_pointerIsDown) {
                        _tableView.ScrollToCellWithIdx(idx: _characterScrollData[hoverCharacterIndex].cellIdx, TableView.ScrollPositionType.Beginning, animated: true);
                    }
                    RefreshHighlight();
                }

                yield return null;
            }
        }

        private int GetPointerCharacterIndex(PointerEventData eventData) {

            var rectTransform = (RectTransform)transform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePos)) {
                float startY = (_characterScrollData.Count - 1) * _characterHeight * 0.5f;
                int index = Mathf.RoundToInt(-(localMousePos.y - startY) / _characterHeight);
                return Mathf.Clamp(index, 0, _characterScrollData.Count - 1);
            }

            return -1;
        }

        private void InitText(TextMeshProUGUI text, char character) {

            text.color = _normalColor;
            text.text = character.ToString();

            var rectTransform = text.rectTransform;

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
            rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
        }
    }
}
