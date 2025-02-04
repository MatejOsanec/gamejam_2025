using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace HMUI {

    public class RangeValuesTextSlider : TextSlider {

        [SerializeField] float _minValue = 0.0f;
        [SerializeField] float _maxValue = 1.0f;
        [SerializeField] [NullAllowed] Button _decButton = default;
        [SerializeField] [NullAllowed] Button _incButton = default;

        public new bool interactable {
            set {
                _decButton.gameObject.SetActive(value);
                _incButton.gameObject.SetActive(value);
                base.interactable = value;
            }
        }

        public float minValue { get => _minValue; set { if (SetPropertyUtility.SetStruct(ref _minValue, value)) UpdateVisuals(); } }
        public float maxValue { get => _maxValue; set { if (SetPropertyUtility.SetStruct(ref _maxValue, value)) UpdateVisuals(); } }

        public float value { set { normalizedValue = NormalizeValue(value); } get => ConvertFromNormalizedValue(normalizedValue); }

        public event System.Action<RangeValuesTextSlider, float> valueDidChangeEvent;

        private ButtonBinder _buttonBinder;

        protected override void Awake() {

            base.Awake();

            normalizedValueDidChangeEvent += HandleNormalizedValueDidChange;
            if (_decButton != null && _incButton != null) {
                _buttonBinder = new ButtonBinder();
                _buttonBinder.AddBinding(_decButton, () => { SetNormalizedValue(normalizedValue - (numberOfSteps > 0 ? 1.0f / numberOfSteps : 0.1f)); });
                _buttonBinder.AddBinding(_incButton, () => { SetNormalizedValue(normalizedValue + (numberOfSteps > 0 ? 1.0f / numberOfSteps : 0.1f)); } );
            }
        }

        protected override void OnDestroy() {

            normalizedValueDidChangeEvent -= HandleNormalizedValueDidChange;

            _buttonBinder?.ClearBindings();

            base.OnDestroy();
        }

        private void HandleNormalizedValueDidChange(TextSlider slider, float normalizedValue) {

            valueDidChangeEvent?.Invoke(this, ConvertFromNormalizedValue(normalizedValue));
        }

        public float ConvertFromNormalizedValue(float normalizedValue) {
            return normalizedValue * (_maxValue - _minValue) + _minValue;
        }

        public float NormalizeValue(float rangeValue) {
            return (rangeValue - _minValue) / (_maxValue - _minValue);
        }

        protected override string TextForNormalizedValue(float normalizedValue) {

            return TextForValue(ConvertFromNormalizedValue(normalizedValue));
        }

        protected virtual string TextForValue(float value) {

            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
