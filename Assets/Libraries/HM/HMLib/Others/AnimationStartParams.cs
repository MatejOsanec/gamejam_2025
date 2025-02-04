using UnityEngine;

public class AnimationStartParams : MonoBehaviour {

    [SerializeField] float _timeOffset = 0.0f;
    [SerializeField] float _speed = 1.0f;
    [SerializeField] Animation _animation = default;

    protected void Start() {

        foreach (AnimationState state in _animation) {
            state.time = _timeOffset;
            state.speed = _speed;
        }
    }
}
