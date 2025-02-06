using System;
using Core;
using UnityEngine;

namespace Gameplay
{

    public class ShootController : BeatmapCallbackListener
    {
        public GameObject targetObject;
        
        private float _lastTriggerTime;
        private float _shootingInterval = Mathf.Infinity; 
        private float _lastShotTime = -Mathf.Infinity;
        private float _defaultShotDuration = 0.2f;
        private float shotDuration = 0.2f;

        protected override void OnGameInit()
        {
            Locator.Callbacks.AddEventListener(20, EventTriggeredHandler);    
        }

        private void EventTriggeredHandler(float value)
        {
            Debug.Log($"EVENT ID 20 value: {value}");
            // Calculate the shooting interval based on the event value
            // 0 = not shooting, 0.1 = 1 shot per second, 1 = 30 shots per second
            _shootingInterval = value > 0 ? 1f / (value * 30f) : Mathf.Infinity;
            shotDuration = Mathf.Max(_shootingInterval / 2, _defaultShotDuration);
        }
        private void Update()
        {
            if (targetObject != null)
            {
                float currentTime = Time.time;
                // Check if it's time to shoot
                if (currentTime >= _lastShotTime + _shootingInterval)
                {
                    // Activate the GameObject for the shot duration
                    targetObject.SetActive(true);
                    _lastShotTime = currentTime;
                }
                // Deactivate the GameObject after the shot duration
                if (currentTime >= _lastShotTime + shotDuration)
                {
                    targetObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Target object is not assigned.");
            }
        }
    }
}