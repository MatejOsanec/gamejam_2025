using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

namespace HMUI {

    public class ColorGradientSlider : TextSlider, IPointerUpHandler {

        [SerializeField] string _textPrefix = default;

        [Space]
        [SerializeField] Color _color0 = default;
        [SerializeField] Color _color1 = default;
        [SerializeField] ImageViewBase[] _gradientImages = default;
        [SerializeField] Color _darkColor = default;
        [SerializeField] Color _lightColor = default;

        public event System.Action<ColorGradientSlider, Color, ColorChangeUIEventType> colorDidChangeEvent;

        [DoesNotRequireDomainReloadInit]
        private static readonly StringBuilder _stringBuilder = new StringBuilder(16);

        protected override void Awake() {

            base.Awake();

            this.numberOfSteps = 256;

            normalizedValueDidChangeEvent += HandleNormalizedValueDidChange;
        }

        protected override void OnDestroy() {

            normalizedValueDidChangeEvent -= HandleNormalizedValueDidChange;
            base.OnDestroy();
        }

        public void SetColors(Color color0, Color color1) {

            _color0 = color0;
            _color1 = color1;

            UpdateVisuals();
        }

        protected override void UpdateVisuals() {

            base.UpdateVisuals();

            var color = Color.Lerp(_color0, _color1, normalizedValue);
            if (color.grayscale > 0.7f) {
                handleColor = _darkColor;
                valueTextColor = _darkColor;
            }
            else {
                handleColor = _lightColor;
                valueTextColor = _lightColor;
            }

            foreach (var gradientImage in _gradientImages) {
                gradientImage.color0 = _color0;
                gradientImage.color1 = _color1;
            }
        }

        protected override string TextForNormalizedValue(float normalizedValue) {

            _stringBuilder.Clear();
            _stringBuilder.Append(_textPrefix);
            _stringBuilder.Append(Mathf.RoundToInt(normalizedValue * 255.0f));
            return _stringBuilder.ToString();
        }

        private void HandleNormalizedValueDidChange(TextSlider slider, float normalizedValue) {

            colorDidChangeEvent?.Invoke(this, Color.Lerp(_color0, _color1, normalizedValue), ColorChangeUIEventType.Drag);
        }

        public override void OnPointerUp(PointerEventData eventData) {

            base.OnPointerUp(eventData);

            colorDidChangeEvent?.Invoke(this, Color.Lerp(_color0, _color1, normalizedValue), ColorChangeUIEventType.PointerUp);
        }
    }
}
