using System;
using Tweening;
using UnityEngine;
using Zenject;

public class ResizablePanel: MonoBehaviour {
    
    [SerializeField] RectTransform _rectTransform = default;
    
    [Inject] readonly TimeTweeningManager _tweeningManager = default;

    private Vector2Tween _resizeTween;

    protected void OnDestroy() {

        if (_tweeningManager != null) {
            _tweeningManager.KillAllTweens(owner:this);
        }
    }

    public void Resize(Vector2 size, float duration) {
        
        _resizeTween = _resizeTween ??  new Vector2Tween(fromValue: Vector2.zero, toValue: Vector2.zero, SetSize, duration: 0.0f, EaseType.InOutSine);
        
        if (duration <= 0.0f) {
            _resizeTween.Kill();
            SetSize(size);
        }
        else {
            _resizeTween.duration = duration;
            _resizeTween.fromValue = _rectTransform.sizeDelta;
            _resizeTween.toValue = size;
            _tweeningManager.RestartTween(_resizeTween, owner: this);    
        }
    }

    private void SetSize(Vector2 size) {
        
        _rectTransform.sizeDelta = size;
    }
}
