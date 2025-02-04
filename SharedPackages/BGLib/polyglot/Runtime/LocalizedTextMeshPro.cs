namespace BGLib.Polyglot {

    using UnityEngine;
    using TMPro;

    [AddComponentMenu("UI/Localized TextMesh Pro", 13)]
    [RequireComponent(typeof(TextMeshPro))]
    public class LocalizedTextMeshPro : LocalizedTextComponent<TextMeshPro> {

        protected override void SetText(TextMeshPro text, string value) {

            text.text = value;
        }

        protected override void UpdateAlignment(TextMeshPro text, LanguageDirection direction) {

            if (IsOppositeDirection(text.alignment, direction)) {
                text.alignment = text.alignment switch {
                    TextAlignmentOptions.TopLeft => TextAlignmentOptions.TopRight,
                    TextAlignmentOptions.TopRight => TextAlignmentOptions.TopLeft,
                    TextAlignmentOptions.Left => TextAlignmentOptions.Right,
                    TextAlignmentOptions.Right => TextAlignmentOptions.Left,
                    TextAlignmentOptions.BottomLeft => TextAlignmentOptions.BottomRight,
                    TextAlignmentOptions.BottomRight => TextAlignmentOptions.BottomLeft,
                    _ => text.alignment
                };
            }
        }

        private static bool IsOppositeDirection(TextAlignmentOptions alignment, LanguageDirection direction) {

            return (direction == LanguageDirection.LeftToRight && IsAlignmentRight(alignment)) ||
                   (direction == LanguageDirection.RightToLeft && IsAlignmentLeft(alignment));
        }

        private static bool IsAlignmentRight(TextAlignmentOptions alignment) {

            return alignment is TextAlignmentOptions.BottomRight or TextAlignmentOptions.Right
                or TextAlignmentOptions.TopRight;
        }

        private static bool IsAlignmentLeft(TextAlignmentOptions alignment) {

            return alignment is TextAlignmentOptions.BottomLeft or TextAlignmentOptions.Left
                or TextAlignmentOptions.TopLeft;
        }
    }
}
