using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservableVariable<T> : IValue<T>, IObservableChange {

    public event System.Action didChangeEvent;

    public T value {
        set {
            if (_value != null && _value.Equals(value)) {
                return;
            }
            _value = value;
            didChangeEvent?.Invoke();            
        }
        get => _value;
    }

    private T _value;

    public static implicit operator T(ObservableVariable<T> obj) {
        return obj.value;
    }
}
