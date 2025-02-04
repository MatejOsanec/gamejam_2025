using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HMUI {
    
    public class InputFieldViewChangeBinder {
    
        private List<Tuple<InputFieldView, UnityAction<InputFieldView>>> _bindings;
        
        private bool _enabled = true;

        public InputFieldViewChangeBinder() {
            
            Init();
        }

        private void Init() {
            
            _bindings = new List<Tuple<InputFieldView, UnityAction<InputFieldView>>>();
        }

        public void AddBindings(List<Tuple<InputFieldView, System.Action<InputFieldView>>> bindings) {

            foreach (var t in bindings) {
                
                AddBinding(t.Item1, t.Item2);    
            }
        }

        public void AddBinding(InputFieldView inputField, System.Action<InputFieldView> action) {
            
            var onValueChangedAction = new UnityAction<InputFieldView>(action);
            inputField.onValueChanged.AddListener(onValueChangedAction);
            _bindings.Add(inputField, onValueChangedAction);
        }

        public void ClearBindings() {

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var input = binding.Item1;
                if (input != null) {
                    input.onValueChanged.RemoveListener(binding.Item2);
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
                var input = binding.Item1;
                if (input != null) {
                    input.onValueChanged.RemoveListener(binding.Item2);
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
                var input = binding.Item1;
                if (input != null) {
                    input.onValueChanged.AddListener(binding.Item2);
                }
            }
        }
    }
}
