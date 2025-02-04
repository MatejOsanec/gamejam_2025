using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class ButtonSpriteSwapCapsLockState : MonoBehaviour {

        [SerializeField] private Sprite _lowercaseSprite;
        [SerializeField] private Sprite _uppercaseOnceSprite;
        [SerializeField] private Sprite _uppercaseSprite;
        [SerializeField] private Color _lowercaseColor = Color.white;
        [SerializeField] private Color _uppercaseOnceColor = Color.white;
        [SerializeField] private Color _uppercaseColor = Color.white;

        [Space]
        [SerializeField] private UIKeyboard _keyboard;
        [SerializeField] private Image[] _images;


        protected void OnEnable() {

            UpdateSprites(_keyboard.capsLockState);
            _keyboard.capsLockStateChangedEvent += OnCapsLockStateChanged;
        }

        protected void OnDisable() {

            _keyboard.capsLockStateChangedEvent -= OnCapsLockStateChanged;
        }

        private void OnCapsLockStateChanged(CapsLockState capsLockState) {

            UpdateSprites(capsLockState);
        }

        public void UpdateSprites(CapsLockState capsLockState) {

            var sprite = capsLockState switch {
                CapsLockState.Lowercase => _lowercaseSprite,
                CapsLockState.UppercaseOnce => _uppercaseOnceSprite,
                CapsLockState.Uppercase => _uppercaseSprite,
                _ => _lowercaseSprite
            };
            var color = capsLockState switch {
                CapsLockState.Lowercase => _lowercaseColor,
                CapsLockState.UppercaseOnce => _uppercaseOnceColor,
                CapsLockState.Uppercase => _uppercaseColor,
                _ => _lowercaseColor
            };
            if (sprite != null) {
                foreach (var image in _images) {
                    image.sprite = sprite;
                    image.color = color;
                }
            }
        }
    }
}
