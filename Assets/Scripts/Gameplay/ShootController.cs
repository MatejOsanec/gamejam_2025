using System;
using Core;
using UnityEngine;

namespace Gameplay
{

    public class ShootController : BeatmapCallbackListener
    {
        public Vector3 targetPosition; // The position you want the object to look at
        public float smoothSpeed = 2.0f;
        public int fireRateEventId = 20;

        public GameObject projectile;
        
        private float _lastTriggerTime;
        private float _shootingInterval = Mathf.Infinity; 
        private float _lastShotTime = -Mathf.Infinity;
        private float _defaultShotDuration = 0.2f;
        private float _shotDuration = 0.2f;

        protected override void OnGameInit()
        {
            Locator.Callbacks.AddEventListener(fireRateEventId, EventTriggeredHandler);    
        }

        private void EventTriggeredHandler(float value)
        {
            Debug.Log($"EVENT ID 20 value: {value}");
            // Calculate the shooting interval based on the event value
            // 0 = not shooting, 0.1 = 1 shot per second, 1 = 30 shots per second
            _shootingInterval = value > 0 ? 1f / (value * 30f) : Mathf.Infinity;
            _shotDuration = Mathf.Min(_shootingInterval / 2, _defaultShotDuration);
        }
        private void Update()
        {
            //aim at player
            Vector3 direction = targetPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);

            if (projectile == null)
            {
                Debug.LogWarning("Target object is not assigned.");
                return;
            }
            
            float currentTime = Time.time;
            
            if (currentTime >= _lastShotTime + _shootingInterval)
            {
                projectile.SetActive(true);
                _lastShotTime = currentTime;
            }
            
            if (currentTime >= _lastShotTime + _shotDuration)
            {
                projectile.SetActive(false);
            }
        }
    }
}