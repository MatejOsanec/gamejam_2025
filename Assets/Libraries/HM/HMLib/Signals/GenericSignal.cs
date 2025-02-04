using System.Collections.Generic;
using UnityEngine;

public class GenericSignal<T> : Signal {

    private System.Action<T> _floatEvent;

    public override void Raise() {

        base.Raise();

        if (_floatEvent != null) {
            _floatEvent(default(T));
        }
    }

    public void Raise(T f) {

        base.Raise();

        if (_floatEvent != null) {
            _floatEvent(f);
        }        
    }

    public void Subscribe(System.Action<T> foo) {

        _floatEvent -= foo;
        _floatEvent += foo;
    }

    public void Unsubscribe(System.Action<T> foo) {

        _floatEvent -= foo;
    }
}