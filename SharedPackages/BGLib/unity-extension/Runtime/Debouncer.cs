namespace BGLib.UnityExtension {

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Debouncer<T>: IDisposable {

        private readonly Action<T> _callback;
        private readonly float _debounceDelaySeconds;
        private readonly Queue<T> _debounceQueue;

        private float _nextCallbackTime;

        public Debouncer(Action<T> callback, float debounceDelaySeconds) {

            _callback = callback;
            _debounceDelaySeconds = debounceDelaySeconds;
            _debounceQueue = new();
            _nextCallbackTime = 0;
        }

        public void Enqueue(T value) {

            var currentTime = GetCurrentTime();

            _nextCallbackTime = currentTime + _debounceDelaySeconds;
            _debounceQueue.Enqueue(value);
        }

        public void Tick() {

            if (_debounceQueue.Count == 0) {
                return;
            }

            var currentTime = GetCurrentTime();
            if (currentTime > _nextCallbackTime) {
                _callback(_debounceQueue.Dequeue());
                _nextCallbackTime = currentTime + _debounceDelaySeconds;
            }
        }

        public void Dispose() {

            while (_debounceQueue.TryDequeue(out var value)) {
                _callback(value);
            }
        }

        private float GetCurrentTime() {

            return Time.unscaledTime;
        }
    }
}
