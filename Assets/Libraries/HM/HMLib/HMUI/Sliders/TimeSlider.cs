namespace HMUI {

    using UnityEngine;
    using System;

    public class TimeSlider : RangeValuesTextSlider {

        [SerializeField] TimeType _timeType = default;

        public enum TimeType {
            Default,
            Milliseconds,
            Normalized
        }

        private bool _valuesValid;
        private float _lowerValue;
        private float _upperValue;

        public void SetBounds(bool valuesValid, float lowerValue, float upperValue) {

            _valuesValid = valuesValid;
            _lowerValue = lowerValue;
            _upperValue = upperValue;
        }

        protected override string TextForValue(float value) {

            if (float.IsNaN(value) || !float.IsFinite(value)) {
                return string.Empty;
            }

            switch (_timeType) {
                case TimeType.Milliseconds:
                    return $"{Mathf.RoundToInt(value * 1000.0f)} ms";
                case TimeType.Normalized:
                    return _valuesValid ?
                        FormatTimeSpan(TimeSpan.FromSeconds(Mathf.Lerp(_lowerValue, _upperValue, value))) :
                        "-- s";
            }

            return FormatTimeSpan(TimeSpan.FromSeconds(value));
        }

        private static string FormatTimeSpan(TimeSpan ts) {

            return ts.Minutes > 0 ?
                $"{ts.Minutes} m {ts.Seconds} s" :
                $"{ts.Seconds} s";
        }
    }
}
