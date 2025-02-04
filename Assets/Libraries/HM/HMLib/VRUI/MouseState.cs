using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VRUIControls {

    public class MouseState {

        private List<ButtonState> _trackedButtons = new List<ButtonState>();

        public bool AnyPressesThisFrame() {

            for (int i = 0; i < _trackedButtons.Count; i++) {
                if (_trackedButtons[i].eventData.PressedThisFrame())
                    return true;
            }
            return false;
        }

        public bool AnyReleasesThisFrame() {

            for (int i = 0; i < _trackedButtons.Count; i++) {
                if (_trackedButtons[i].eventData.ReleasedThisFrame())
                    return true;
            }
            return false;
        }

        public ButtonState GetButtonState(PointerEventData.InputButton button) {

            ButtonState tracked = null;
            for (int i = 0; i < _trackedButtons.Count; i++) {
                if (_trackedButtons[i].button == button) {
                    tracked = _trackedButtons[i];
                    break;
                }
            }

            if (tracked == null) {
                tracked = new ButtonState { button = button, eventData = new MouseButtonEventData(), pressedValue = 0 };
                _trackedButtons.Add(tracked);
            }
            return tracked;
        }

        public void SetButtonState(PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton, PointerEventData data) {
            
            var toModify = GetButtonState(button);
            toModify.eventData.buttonState = stateForMouseButton;
            toModify.eventData.buttonData = data;
        }
    }

}