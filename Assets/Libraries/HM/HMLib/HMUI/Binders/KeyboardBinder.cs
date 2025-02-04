using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace HMUI {

    public class KeyboardBinder {

        public enum KeyBindingType {
            KeyDown,
            KeyUp,
            KeyPress,
        }

        public bool enabled { get; set;}

        private bool _shouldClearBindings;
        private List<(KeyCode, KeyBindingType, UnityAction<bool>)> _newBindings = new();
        private List<(KeyCode, KeyBindingType, UnityAction<bool>)> _bindings = new();

        public KeyboardBinder() {

            Init();
        }

        public KeyboardBinder(KeyCode keycode, KeyBindingType keyBindingType, Action<bool> action) {

            Init();
            AddBinding(keycode, keyBindingType, action);
        }

        public KeyboardBinder(List<Tuple<KeyCode, KeyBindingType ,Action<bool>>> bindingData) {

            Init();
            AddBindings(bindingData);
        }

        private void Init() {

            enabled = true;
        }

        public void AddBindings(List<Tuple<KeyCode, KeyBindingType, Action<bool>>> bindingData) {

            foreach (var tuple in bindingData) {
                AddBinding(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        public void AddBinding(KeyCode keyCode, KeyBindingType keyBindingType, Action<bool> action) {

            var onPressAction = new UnityAction<bool>(action);
            _bindings.Add((keyCode, keyBindingType, onPressAction));
        }

        public void ClearBindings() {

            // _shouldClearBindings = true;
            _bindings.Clear();
        }

        public void ManualUpdate() {

            if (!enabled) {
                return;
            }

            /*if (_shouldClearBindings) {
                _bindings.Clear();
            }

            if (_newBindings.Count > 0) {
                _bindings = _newBindings;
                _newBindings = new List<(KeyCode, KeyBindingType, UnityAction<bool>)>();
            }*/

            foreach (var (keyCode, keyBindingType, action) in _bindings.ToArray()) {
                if (keyBindingType != KeyBindingType.KeyUp && Input.GetKeyDown(keyCode)) {
                   action.Invoke(true);
                }
                else if(keyBindingType != KeyBindingType.KeyDown && Input.GetKeyUp(keyCode)) {
                    action.Invoke(false);
                }
            }
        }
    }
}
