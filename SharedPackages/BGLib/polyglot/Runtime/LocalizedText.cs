namespace BGLib.Polyglot {

    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("UI/Localized Text", 11)]
    [RequireComponent(typeof(Text))]
    public class LocalizedText : LocalizedTextComponent<Text> {

        protected override void SetText(Text text, string value) {

            if (text == null) {
                Debug.LogWarning("Missing Text Component on " + name, gameObject);
                return;
            }
            text.text = value;
        }

        protected override void UpdateAlignment(Text text, LanguageDirection direction) {

            if (IsOppositeDirection(text.alignment, direction)) {
                text.alignment = text.alignment switch {
                    TextAnchor.UpperLeft => TextAnchor.UpperLeft,
                    TextAnchor.UpperRight => TextAnchor.UpperRight,
                    TextAnchor.MiddleLeft => TextAnchor.MiddleRight,
                    TextAnchor.MiddleRight => TextAnchor.MiddleLeft,
                    TextAnchor.LowerLeft => TextAnchor.LowerRight,
                    TextAnchor.LowerRight => TextAnchor.LowerLeft,
                    _ => text.alignment
                };
            }
        }

        private bool IsOppositeDirection(TextAnchor alignment, LanguageDirection direction) {
            return (direction == LanguageDirection.LeftToRight && IsAlignmentRight(alignment)) ||
                   (direction == LanguageDirection.RightToLeft && IsAlignmentLeft(alignment));
        }

        private static bool IsAlignmentRight(TextAnchor alignment) {
            return alignment is TextAnchor.LowerRight or TextAnchor.MiddleRight or TextAnchor.UpperRight;
        }

        private static bool IsAlignmentLeft(TextAnchor alignment) {
            return alignment is TextAnchor.LowerLeft or TextAnchor.MiddleLeft or TextAnchor.UpperLeft;
        }
    }
}
