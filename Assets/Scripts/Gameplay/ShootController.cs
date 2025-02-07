using System;
using Core;
using UnityEngine;

namespace Gameplay
{

    public class ShootController : BeatmapCallbackListener
    {
        public float progress = 20;
        public int fireRateEventId = 20;

        public GameObject projectile;
        
        private float _lastTriggerTime;
        private float _shootingInterval = Mathf.Infinity; 
        private float[] _frequencySet;

        protected override void OnGameInit()
        {
            Locator.Callbacks.AddEventListener(fireRateEventId, EventTriggeredHandler);
            _frequencySet = new[] { 0f, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32};
        }

        private void EventTriggeredHandler(float value)
        {
            value = Mathf.Clamp01(value);
            int index = Mathf.FloorToInt(value * (_frequencySet.Length - 1));
            float selectedFrequency = _frequencySet[index];
            _shootingInterval = selectedFrequency == 0 ? Mathf.Infinity : 1f / selectedFrequency;
            Debug.Log($"_shootingInterval = {_shootingInterval} value: {value}");
        }

        private void Update()
        {
            if (projectile == null)
            {
                Debug.LogWarning("Target object is not assigned.");
                return;
            }

            progress = Locator.BeatModel.GetShapedBeatProgress(Waveform.Square, _shootingInterval);
            projectile.SetActive(progress > 0.5);
        }
    }
}