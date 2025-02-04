using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VRUIControls {

    public class ButtonState {

        private PointerEventData.InputButton _button = PointerEventData.InputButton.Left;

        private MouseButtonEventData _eventData;
        private float _pressedValue = 0;

        public MouseButtonEventData eventData {
            get { return _eventData; }
            set { _eventData = value; }
        }

        public PointerEventData.InputButton button {
            get { return _button; }
            set { _button = value; }
        }

        public float pressedValue {
            get { return _pressedValue; }
            set { _pressedValue = value; }
        }
    }
}