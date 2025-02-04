using UnityEngine;

public class RandomAnimationStartTime : MonoBehaviour {

    [SerializeField] Animation _animation;

    protected void Start() {


        foreach (AnimationState state in _animation) {
            state.normalizedTime = Random.Range(0.0f, 1.0f);
        }
        _animation.Play();
    }
}
