public abstract class FixedUpdateSmoothValue<T> {

    private T _currentSmoothedValue;
    private T _prevSmoothedValue;
    private readonly float _smooth;

    protected FixedUpdateSmoothValue(float smooth) {

        _smooth = smooth;
    }

    public void SetStartValue(T value) {

        _currentSmoothedValue = value;
        _prevSmoothedValue = value;
    }

    public void FixedUpdate(T value) {

        _prevSmoothedValue = _currentSmoothedValue;
        _currentSmoothedValue = Interpolate(_currentSmoothedValue, value, 1.0f / _smooth);
    }

    public T GetValue(float interpolationFactor) {

        return Interpolate(_prevSmoothedValue, _currentSmoothedValue, interpolationFactor);
    }

    protected abstract T Interpolate(T value0, T value1, float t);
}
