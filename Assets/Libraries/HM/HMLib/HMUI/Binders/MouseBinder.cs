using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace HMUI {

    public class MouseBinder {

        public enum MouseEventType {
            ButtonDown,
            ButtonUp,
            ButtonPress
        }

        public enum ButtonType {
            Primary = 0,
            Secondary = 1,
            Middle = 2
        }
        public bool enabled { get; set; }

        private List<UnityAction<float>> _scrollBindings;
        private List<(ButtonType buttonType, MouseEventType mouseEventType, UnityAction action)> _buttonBindings;

        public MouseBinder() {

            Init();
        }

        private void Init() {

            enabled = true;
            _scrollBindings = new List<UnityAction<float>>();
            _buttonBindings = new List<(ButtonType, MouseEventType, UnityAction)>();
        }

        public void AddScrollBindings(List<UnityAction<float>> bindingData) {

            foreach (var binding in bindingData) {
                AddScrollBinding(binding);
            }
        }

        public void AddScrollBinding(UnityAction<float> action) {

            _scrollBindings.Add(action);
        }

        public void RemoveScrollBinding(UnityAction<float> action) {

            _scrollBindings.Remove(action);
        }

        public void AddButtonBindings(List<Tuple<ButtonType, MouseEventType, UnityAction>> bindingData) {

            foreach (var tuple in bindingData) {
                AddButtonBinding(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        public void AddButtonBinding(ButtonType buttonType, MouseEventType keyBindingType, UnityAction action) {

            _buttonBindings.Add((buttonType, keyBindingType, action));
        }

        public void RemoveButtonBinding(ButtonType buttonType, MouseEventType keyBindingType, UnityAction action) {

            _buttonBindings.Remove((buttonType, keyBindingType, action));
        }

        public void ClearBindings() {

            _buttonBindings?.Clear();
            _scrollBindings?.Clear();
        }

        public void ManualUpdate() {

            if (!enabled) {
                return;
            }

            var yScrollDelta = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(yScrollDelta, 0)) {
                foreach (var binding in _scrollBindings) {
                    binding.Invoke(yScrollDelta);
                }
            }

            foreach (var binding in _buttonBindings) {
                switch (binding.mouseEventType) {
                    case MouseEventType.ButtonDown:
                        if(Input.GetMouseButtonDown((int)binding.buttonType)) {
                            binding.action.Invoke();
                        }
                        break;
                    case MouseEventType.ButtonUp:
                        if (Input.GetMouseButtonUp((int)binding.buttonType)) {
                            binding.action.Invoke();
                        }
                        break;
                    case MouseEventType.ButtonPress:
                        if(Input.GetMouseButton((int)binding.buttonType)) {
                            binding.action.Invoke();
                        }
                        break;
                }
            }
        }
    }
}
