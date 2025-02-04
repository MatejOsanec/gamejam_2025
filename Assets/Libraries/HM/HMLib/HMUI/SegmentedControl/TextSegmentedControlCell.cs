using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HMUI {

	public class TextSegmentedControlCell : SegmentedControlCell {

		[SerializeField] protected TextMeshProUGUI _text = default;
		[SerializeField] GameObject _backgroundGameObject = default;

        public string text {
			set => _text.text = value;
			get => _text.text;
		}

        public float fontSize {
            set => _text.fontSize = value;
            get => _text.fontSize;
        }

        public bool hideBackgroundImage {
	        set => _backgroundGameObject.SetActive(!value);
        }

		public float preferredWidth => _text.preferredWidth;

        public bool enableWordWrapping {
            set => _text.enableWordWrapping = value;
            get => _text.enableWordWrapping;
        }

        public TextOverflowModes textOverflowMode {
            set => _text.overflowMode = value;
            get => _text.overflowMode;
        }
    }
}
