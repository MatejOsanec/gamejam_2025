using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HMUI {

    public class ToggleBinder {

        private List<Tuple<Toggle, UnityAction<bool>>> _bindings;

        private bool _enabled = true;

        public ToggleBinder() {

            Init();
        }

        public ToggleBinder(List<Tuple<Toggle, Action<bool>>> bindingData) {

            Init();
            AddBindings(bindingData);
        }

        private void Init() {

            _bindings = new List<Tuple<Toggle, UnityAction<bool>>>();
        }

        public void AddBindings(List<Tuple<Toggle, Action<bool>>> bindingData) {

            foreach (var tuple in bindingData) {

                AddBinding(tuple.Item1, tuple.Item2);
            }
        }

        public void AddBinding(Toggle toggle, Action<bool> action) {

            var onValueChangedAction = new UnityAction<bool>(action);
            toggle.onValueChanged.AddListener(onValueChangedAction);
            _bindings.Add(toggle, onValueChangedAction);
        }

        public void AddBinding(Toggle toggle, bool enabled, Action action) {

            var onValueChangedAction = new UnityAction<bool>((b) => {
                if (b == enabled) {
                    action.Invoke();
                }
            });

            toggle.onValueChanged.AddListener(onValueChangedAction);
            _bindings.Add(toggle, onValueChangedAction);
        }

        public void ClearBindings() {

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var toggle = binding.Item1;
                if (toggle != null) {
                    toggle.onValueChanged.RemoveListener(binding.Item2);
                }
            }

            _bindings.Clear();
        }

        public void Disable() {

            if (!_enabled) {
                return;
            }

            _enabled = false;

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var toggle = binding.Item1;
                if (toggle != null) {
                    toggle.onValueChanged.RemoveListener(binding.Item2);
                }
            }
        }

        public void Enable() {

            if (_enabled) {
                return;
            }

            _enabled = true;

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var toggle = binding.Item1;
                if (toggle != null) {
                    toggle.onValueChanged.AddListener(binding.Item2);
                }
            }
        }
    }
}
