using System.Collections.Generic;

public interface ILazyCopyHashSet<in T> {

    void Add(T item);
    void Remove(T item);
}

// This class is used when you need to modify the collection during the iteration
public class LazyCopyHashSet<T> : ILazyCopyHashSet<T> {

    public List<T> items {
        get {
            if (_dirty) {
                _itemsCopy.Clear();
                foreach (var listener in _items) {
                    _itemsCopy.Add(listener);
                }
                _dirty = false;
            }
            return _itemsCopy;
        }
    }

    private readonly List<T> _itemsCopy;
    private readonly HashSet<T> _items;
    private bool _dirty;

    public LazyCopyHashSet() : this(capacity: 10) { }

    public LazyCopyHashSet(int capacity) {

        _itemsCopy = new List<T>(capacity);
        _items = new HashSet<T>(capacity);
        _dirty = false;
    }

    public void Add(T item) {

        _dirty = true;
        _items.Add(item);
    }

    public void Remove(T item) {

        _dirty = true;
        _items.Remove(item);
    }

    public void Clear() {

        _dirty = true;
        _items.Clear();
    }
}
