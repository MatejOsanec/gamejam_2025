using System;
using System.Collections.Generic;

// TODO: Optimize by not removing first element when spawning item
public class SimpleMemoryPool<T> where T : class {

    public List<T> items => _activeElements.items;

    private readonly LazyCopyHashSet<T> _activeElements;
    private readonly List<T> _inactiveElements;
    private readonly Func<T> _createNewItemFunc;

    public SimpleMemoryPool(int startCapacity, Func<T> createNewItemFunc) {

        _createNewItemFunc = createNewItemFunc;
        _activeElements = new LazyCopyHashSet<T>(startCapacity);
        _inactiveElements = new List<T>(startCapacity);
        for (int i = 0; i < startCapacity; i++) {
            _inactiveElements.Add(_createNewItemFunc());
        }
    }

    public T Spawn() {

        T item = null;
        if (_inactiveElements.Count > 0) {
            item = _inactiveElements[0];
            _inactiveElements.RemoveAt(0);
        }
        else {
            item = _createNewItemFunc();
        }

        _activeElements.Add(item);
        return item;
    }

    public void Despawn(T item) {

        _activeElements.Remove(item);
        _inactiveElements.Add(item);
    }
}
