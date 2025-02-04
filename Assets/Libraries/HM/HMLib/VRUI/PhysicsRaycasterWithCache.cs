using System.Collections.Generic;
using UnityEngine;

namespace VRUIControls {

    public class PhysicsRaycasterWithCache {

        private readonly struct CachedRaycast {

            public readonly bool wasHit;
            public readonly Ray ray;
            public readonly RaycastHit hitInfo;
            public readonly float maxDistance;
            public readonly int layerMask;

            public CachedRaycast(bool wasHit, Ray ray, RaycastHit hitInfo, float maxDistance, int layerMask) {

                this.wasHit = wasHit;
                this.ray = ray;
                this.hitInfo = hitInfo;
                this.maxDistance = maxDistance;
                this.layerMask = layerMask;
            }
        }

        private readonly List<CachedRaycast> _cachedRaycasts = new List<CachedRaycast>();
        private int _lastFrameCount = -1;

        public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask) {

            if (Time.frameCount != _lastFrameCount) {
                _lastFrameCount = Time.frameCount;
                _cachedRaycasts.Clear();
            }

            foreach (var cachedRaycast in _cachedRaycasts) {
                if (cachedRaycast.ray.origin == ray.origin && cachedRaycast.ray.direction == ray.direction && Mathf.Approximately(cachedRaycast.maxDistance, maxDistance) && cachedRaycast.layerMask == layerMask) {
                    hitInfo = cachedRaycast.hitInfo;
                    return cachedRaycast.wasHit;
                }
            }

            var wasHit = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
            _cachedRaycasts.Add(new CachedRaycast(wasHit, ray, hitInfo, maxDistance, layerMask));
            return wasHit;
        }
    }
}
