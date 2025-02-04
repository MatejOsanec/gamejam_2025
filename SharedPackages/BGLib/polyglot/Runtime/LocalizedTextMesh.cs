using UnityEngine;

namespace BGLib.Polyglot {

    [AddComponentMenu("Mesh/Localized TextMesh")]
    [RequireComponent(typeof(TextMesh))]
    public class LocalizedTextMesh : MonoBehaviour, ILocalize {

        [Tooltip("The TextMesh component to localize")]
        [SerializeField] TextMesh text = default!;

        [Tooltip("The key to localize with")]
        [SerializeField] [LocalizationKey] string key = default!;

        public string Key => key;

        public void Reset() {

            text = GetComponent<TextMesh>();
        }

        public void Start() {
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        public void OnLocalize(LocalizationModel localization) {

            var flags = text.hideFlags;
            text.hideFlags = HideFlags.DontSave;
            text.text = localization.Get(key);

            var direction = localization.selectedLanguageDirection;

            if (IsOppositeDirection(text.alignment, direction)) {
                switch (text.alignment) {
                    case TextAlignment.Left:
                        text.alignment = TextAlignment.Right;
                        break;
                    case TextAlignment.Right:
                        text.alignment = TextAlignment.Left;
                        break;
                }
            }
            text.hideFlags = flags;
        }

        private bool IsOppositeDirection(TextAlignment alignment, LanguageDirection direction) {
            return (direction == LanguageDirection.LeftToRight && IsAlignmentRight(alignment)) ||
                   (direction == LanguageDirection.RightToLeft && IsAlignmentLeft(alignment));
        }

        private bool IsAlignmentRight(TextAlignment alignment) {
            return alignment == TextAlignment.Right;
        }

        private bool IsAlignmentLeft(TextAlignment alignment) {
            return alignment == TextAlignment.Left;
        }
    }
}
