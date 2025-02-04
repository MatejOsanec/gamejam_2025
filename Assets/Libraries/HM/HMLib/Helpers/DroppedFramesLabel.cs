using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedFramesLabel : MonoBehaviour {

    [SerializeField] TMPro.TextMeshProUGUI _text = default;
    [SerializeField] int _expectedFrameRate = 90;
    [SerializeField] int _resetInterval = 5;

    int _totalNumberOfDroppedFrames = 0;
    float _syncedFrameTime;
    float _intervalTime;
    float _maxFrameTimeInInterval;
    int _frameCountInInterval;

    protected void Start() {

        _syncedFrameTime = 1.0f / _expectedFrameRate;
        _intervalTime = 0.0f;

        _text.text = "0";
    }

    protected void Update() {
    
        _frameCountInInterval++;
        _maxFrameTimeInInterval = Mathf.Max(_maxFrameTimeInInterval, Time.unscaledDeltaTime);

        _intervalTime += Time.unscaledDeltaTime;
        if (_intervalTime >= _resetInterval) {
            
            int droppedFrames = _resetInterval * _expectedFrameRate - _frameCountInInterval;
            if (droppedFrames < 0) {
                droppedFrames = 0;
            }
            _totalNumberOfDroppedFrames += droppedFrames;

            RefreshText();

            _frameCountInInterval = 0;
            _intervalTime = 0.0f;
            _maxFrameTimeInInterval = 0.0f;
        }
    }

    private void RefreshText() {
        _text.text = string.Format("Dropped: {0}\nFT: {1} : {2}", _totalNumberOfDroppedFrames, Mathf.CeilToInt(_maxFrameTimeInInterval / _syncedFrameTime), _maxFrameTimeInInterval);
    }
}

