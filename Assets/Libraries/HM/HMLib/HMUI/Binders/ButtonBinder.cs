using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HMUI {

    public class ButtonBinder {

        private List<Tuple<Button, UnityAction>> _bindings;

        public ButtonBinder() {

            Init();
        }

        public ButtonBinder(Button button, System.Action action) {

            Init();
            AddBinding(button, action);
        }

        public ButtonBinder(List<Tuple<Button, System.Action>> bindingData) {

            Init();
            AddBindings(bindingData);
        }

        private void Init() {

            _bindings = new List<Tuple<Button, UnityAction>>();
        }

        public void AddBindings(List<Tuple<Button, System.Action>> bindingData) {

            foreach (var tuple in bindingData) {

                AddBinding(tuple.Item1, tuple.Item2);
            }
        }

        public void AddBinding(Button button, System.Action action) {

            if (button == null) {
                Debug.LogWarning("[ButtonBinder] Attempted to bind an action to a null button. Did you assign the reference in the editor?");
                return;
            }

            var onClickAction = new UnityAction(action);
            button.onClick.AddListener(onClickAction);
            _bindings.Add(button, onClickAction);
        }

        public void ClearBindings() {

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var button = binding.Item1;
                if (button != null) {
                    button.onClick.RemoveListener(binding.Item2);
                }
            }

            _bindings.Clear();
        }
    }
}
