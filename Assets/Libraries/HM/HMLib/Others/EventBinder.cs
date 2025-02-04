using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBinder {

    private List<System.Action> _unsubscribes = new List<System.Action>();

    public void Bind(System.Action subscribe, System.Action unsubscribe) {

        subscribe();
        _unsubscribes.Add(unsubscribe);
    }

    public void ClearAllBindings() {

        foreach (var unsubscribe in _unsubscribes) {
            unsubscribe();
        }
        _unsubscribes.Clear();
    }
}
