using System;

public class BinaryHeap<T> where T : IComparable<T> {

    private T[] _data;
    private int _tail;

    public BinaryHeap() : this(capacity: 10) {}

    public BinaryHeap(int capacity) {

        _data = new T[capacity];
        _tail = 0;
    }

    public void Insert(T item) {

        _tail++;

        // Grow if needed
        if (_tail >= _data.Length) {
            var newData = new T[_data.Length * 2];
            Array.Copy(_data, newData, _data.Length);
            _data = newData;
        }

        _data[_tail] = item;

        // Move up
        int idx = _tail;
        while (true) {

            if (idx <= 1) {
                return;
            }

            var parentIdx = idx / 2;

            // Swap
            if (_data[parentIdx].CompareTo(_data[idx]) > 0) {
                (_data[parentIdx], _data[idx]) = (_data[idx], _data[parentIdx]);
            }
            else {
                return;
            }

            idx = parentIdx;
        }
    }

    public bool RemoveMin(out T output) {

        if (_tail < 1) {
            output = default(T);
            return false;
        }

        output = _data[1];
        _data[1] = _data[_tail];
        _tail--;

        int idx = 1;

        while (true) {

            int left = idx * 2;
            int right = (idx * 2) + 1;

            if (left > _tail) {
                return true;
            }

            // If there is only left child of this node, then do a comparison and return
            if (left == _tail) {
                if (_data[left].CompareTo(_data[idx]) < 0) {
                    (_data[idx], _data[left]) = (_data[left], _data[idx]);
                }
                return true;
            }

            // If both children are there
            int smallestChild = _data[left].CompareTo(_data[right]) < 0 ? left : right;

            // If Parent is greater than smallest child, then swap
            if (_data[smallestChild].CompareTo(_data[idx]) < 0) {
                (_data[idx], _data[smallestChild]) = (_data[smallestChild], _data[idx]);
            }

            idx = smallestChild;
        }
    }
}
