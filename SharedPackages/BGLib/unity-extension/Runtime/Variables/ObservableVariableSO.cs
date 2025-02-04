public class ObservableVariableSO<T> : PersistentScriptableObject, IValue<T>, IObservableChange {

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

    public static implicit operator T(ObservableVariableSO<T> obj) {
        return obj.value;
    }
}
