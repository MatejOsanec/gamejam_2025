using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace BGLib.JiraBridge {

    public class PanicButtonHandler : MonoBehaviour {

        public event Action panicButtonSequenceWasCompleted;

        private const int kCountNeeded = 5;
        private const double kMsUntilSequenceInvalid = 1000;

        private int currentCount = 0;
        private Stopwatch stopwatch = new();
        private bool lastKnownControllerPressState = false;
        private bool lastKnownKeyboardPressState = false;

        protected void Update() {

            // VR controller
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, devices);
            if (devices.Count > 0) {
                var device = devices.First();

                device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonPressed);
                if (secondaryButtonPressed == true && lastKnownControllerPressState == false) {
                    lastKnownControllerPressState = true;
                    return;
                }
                else if (secondaryButtonPressed == false && lastKnownControllerPressState == true) {
                    lastKnownControllerPressState = false;
                    ButtonReleased();
                    return;
                }
            }

            // Keyboard
            bool keyboardBtnPressed = Input.GetKey(KeyCode.PageUp);
            if (keyboardBtnPressed == true && lastKnownKeyboardPressState == false) {
                lastKnownKeyboardPressState = true;
                return;
            }
            else if (keyboardBtnPressed == false && lastKnownKeyboardPressState == true) {
                lastKnownKeyboardPressState = false;
                ButtonReleased();
                return;
            }

            if (stopwatch.ElapsedMilliseconds >= kMsUntilSequenceInvalid) {
                stopwatch.Stop();
                stopwatch.Reset();
                currentCount = 0;
            }
        }

        private void ButtonReleased() {

            currentCount++;
            stopwatch.Restart();

            if (currentCount >= kCountNeeded) {
                panicButtonSequenceWasCompleted?.Invoke();
                currentCount = 0;
                stopwatch.Stop();
            }
        }
    }
}
