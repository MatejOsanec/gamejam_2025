using UnityEngine;

namespace VRUIControls {

    public class VRLaserPointer : MonoBehaviour {

#if BS_TOURS
        [SerializeField] Parametric3SliceSpriteController _laserController = default;
#else
        [SerializeField] MeshRenderer _renderer = default;
#endif

#if BS_TOURS
        protected void OnEnable() {

            _laserController.Refresh();
        }

        public void SetLength(float length) {

            _laserController.length = length;
        }
#else
        [DoesNotRequireDomainReloadInit]
        private static readonly int _fadeStartNormalizedDistanceId = Shader.PropertyToID("_FadeStartNormalizedDistance");

        [DoesNotRequireDomainReloadInit]
        private static MaterialPropertyBlock _materialPropertyBlock;

        public void SetLocalPosition(Vector3 position) {

            transform.localPosition = position;
        }

        public void SetLocalScale(Vector3 scale) {

            transform.localScale = scale;
        }

        public void SetFadeDistance(float distance) {

            if (_materialPropertyBlock == null) {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }
            _materialPropertyBlock.SetFloat(_fadeStartNormalizedDistanceId, distance);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }
#endif
    }
}
