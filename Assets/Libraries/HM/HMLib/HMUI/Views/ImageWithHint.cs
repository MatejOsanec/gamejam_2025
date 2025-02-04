using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class ImageWithHint : MonoBehaviour {

        [SerializeField] UnityEngine.UI.Image _image = default;
        [SerializeField] HoverHint _hoverHint = default;

        public Sprite sprite {
            set => _image.sprite = value;
            get => _image.sprite;
        }

        public string hintText {
            set => _hoverHint.text = value;
        }

        public Color imageColor {
            get => _image.color;
            set => _image.color = value;
        }
    }
}
