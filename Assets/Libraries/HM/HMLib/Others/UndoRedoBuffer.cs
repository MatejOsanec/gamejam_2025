using System.Collections.Generic;

public class UndoRedoBuffer<T> where T : class {

    private List<T> _data;
    private int _capacity;
    private int _cursor;

    public UndoRedoBuffer(int capacity) {

        _capacity = capacity;
        _data = new List<T>(capacity + 1);
    }

    public void Add(T item) {

        // Maintain capacity.
        if (_data.Count >= _capacity) {
            _data.RemoveAt(_data.Count - 1);
        }

        // Delete redo states.
        if (_cursor > 0) {
            for (int i = 0; i < _cursor; i++) {
                _data.RemoveAt(0);
            }
        }

        _data.Insert(0, item);
        _cursor = 0;
    }

    public T Undo() {
        
        if (_cursor + 1 >= _data.Count) {
            return null;
        }

        _cursor++;
        return _data[_cursor];
    }

    public T Redo() {

        if (_cursor == 0) {
            return null;
        }

        _cursor--;
        return _data[_cursor];
    }

    public void Clear() {

        _data.Clear();
        _cursor = 0;
    }
}