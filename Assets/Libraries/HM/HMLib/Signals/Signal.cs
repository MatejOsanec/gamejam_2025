using System.Collections.Generic;
using UnityEngine;

public class Signal : ScriptableObject {

#if UNITY_EDITOR
#pragma warning disable 414
    [SerializeField] [Multiline] [NullAllowed] string _description = default;
#pragma warning restore 414
#endif

    private event System.Action _event;

    public virtual void Raise() {

        _event?.Invoke();
    }

    public void Subscribe(System.Action foo) {

        _event -= foo;
        _event += foo;
    }

    public void Unsubscribe(System.Action foo) {

        _event -= foo;
    }
}
