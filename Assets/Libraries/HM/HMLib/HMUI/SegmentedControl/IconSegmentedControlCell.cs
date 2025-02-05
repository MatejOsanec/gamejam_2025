using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HMUI {

    public class IconSegmentedControlCell : SegmentedControlCell {

        [SerializeField] UnityEngine.UI.Image _icon = default;
        [SerializeField] HoverHint _hoverHint = default;
        [SerializeField] GameObject _backgroundGameObject = default;

        public Sprite sprite {
            set => _icon.sprite = value;
            get => _icon.sprite;
        }

        public string hintText {
            set => _hoverHint.text = value;
        }

        public float iconSize {
            set => _icon.rectTransform.sizeDelta = new Vector2(value, value);
        }

        public bool hideBackgroundImage {
            set {
                if (_backgroundGameObject != null) {
                    _backgroundGameObject.SetActive(!value);
                }
            }
        }
    }
}
