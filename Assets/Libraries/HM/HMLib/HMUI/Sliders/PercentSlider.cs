using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HMUI {

    public class PercentSlider : RangeValuesTextSlider {

        protected override string TextForValue(float value) {

            return string.Format("{0:F1}%", value * 100.0f);
        }
    }
}