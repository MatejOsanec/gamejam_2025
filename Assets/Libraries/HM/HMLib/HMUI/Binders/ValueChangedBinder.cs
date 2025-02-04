using System.Collections.Generic;
using System;

namespace HMUI {

    public interface IValueChanger<T> {
        event Action<T> valueChangedEvent;
    }
    
    public class ValueChangedBinder<T> {

        private List<Tuple<IValueChanger<T>, Action<T>>> _bindings;

        public ValueChangedBinder() {

            Init();
        }

        public ValueChangedBinder(IValueChanger<T> valueChanger, Action<T> action) {

            Init();
            AddBinding(valueChanger, action);
        }

        public ValueChangedBinder(List<Tuple<IValueChanger<T>, Action<T>>> bindingData) {

            Init();
            AddBindings(bindingData);
        }

        private void Init() {

            _bindings = new List<Tuple<IValueChanger<T>, Action<T>>>();
        }

        public void AddBindings(List<Tuple<IValueChanger<T>, Action<T>>> bindingData) {

            foreach (var (item1, item2) in bindingData) {

                AddBinding(item1, item2);
            }
        }

        public void AddBinding(IValueChanger<T> valueChanger, Action<T> action) {
            
            valueChanger.valueChangedEvent += action;
            _bindings.Add(valueChanger, action);
        }

        public void ClearBindings() {

            if (_bindings == null) {
                return;
            }

            foreach (var (valueChanger, action) in _bindings) {
                if (valueChanger != null) {
                    valueChanger.valueChangedEvent -= action;
                }
            }

            _bindings.Clear();
        }
    }
}
