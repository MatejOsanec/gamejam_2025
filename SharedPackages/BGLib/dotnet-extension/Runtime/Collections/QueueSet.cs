using System.Collections.Generic;

public class QueueSet<T> {

    private readonly LinkedList<T> _linkedList = new LinkedList<T>();
    private readonly HashSet<T> _set = new HashSet<T>();

    public int Count => _set.Count;

    public void Enqueue(T item) {

        if (_set.Contains(item)) {
            return;
        }

        _linkedList.AddLast(item);
        _set.Add(item);
    }

    public T Dequeue() {

        var node = _linkedList.First;

        _linkedList.RemoveFirst();
        _set.Remove(node.Value);

        return node.Value;
    }

    public void Clear() {

        _linkedList.Clear();
        _set.Clear();
    }
}
