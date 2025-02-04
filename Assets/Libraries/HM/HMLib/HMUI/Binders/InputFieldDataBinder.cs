using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HMUI {

    public class InputFieldDataBinder {

        private List<Tuple<InputField, IObservableChange, UnityAction<string>, System.Action>> _bindings;

        public InputFieldDataBinder() {

            _bindings = new List<Tuple<InputField, IObservableChange, UnityAction<string>, System.Action>>();
        }

        public void AddBindings<T0, T1>(List<Tuple<InputField, T0, Func<string, T1>, Func<T1, string>>> bindingData) where T0 : IObservableChange, IValue<T1> {

            foreach (var tuple in bindingData) {

                var inputField = tuple.Item1;
                var valueItem = tuple.Item2;
                var toValueConvertor = tuple.Item3;
                var toStringConvertor = tuple.Item4;

                var onEndEditAction = new UnityAction<string>(
                    (string value) => {
                        var newValue = toValueConvertor(value);
                        if (newValue.Equals(valueItem.value)) {
                            inputField.text = toStringConvertor(newValue);
                        }
                        else {
                            valueItem.value = newValue;
                        }
                    }
                );

                System.Action variableChangedHandler = () => { inputField.text = toStringConvertor(valueItem.value); };

                inputField.onEndEdit.AddListener(onEndEditAction);
                valueItem.didChangeEvent += variableChangedHandler;

                _bindings.Add(tuple.Item1, tuple.Item2, onEndEditAction, variableChangedHandler);

                variableChangedHandler();
            }
        }

        public void AddStringBindings<T>(List<Tuple<InputField, T>> bindingData) where T : IObservableChange, IValue<string> {

            var newBindingData = new List<Tuple<InputField, T, Func<string, string>, Func<string, string>>>();
            Func<string, string> convertor = (string value) => { return value; };
            foreach (var binding in bindingData) {
                newBindingData.Add(binding.Item1, binding.Item2, convertor, convertor);
            }
            AddBindings(newBindingData);
        }

        public void ClearBindings() {

            if (_bindings == null) {
                return;
            }

            foreach (var binding in _bindings) {
                var inputField = binding.Item1;
                var variable = binding.Item2;
                if (inputField != null) {
                    inputField.onEndEdit.RemoveListener(binding.Item3);
                }
                if (variable != null) {
                    variable.didChangeEvent -= binding.Item4;
                }
            }

            _bindings.Clear();
        }
    }
}