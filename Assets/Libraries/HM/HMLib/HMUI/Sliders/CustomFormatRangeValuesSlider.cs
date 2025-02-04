using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace HMUI {

    public class CustomFormatRangeValuesSlider : RangeValuesTextSlider {

        [SerializeField] string _formatString = default;

        protected override string TextForValue(float value) {

            return string.Format(_formatString, value);
        }
    }

}