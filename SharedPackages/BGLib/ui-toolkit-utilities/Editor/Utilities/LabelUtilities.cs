
using UnityEngine.UIElements;

namespace BGLib.UiToolkitUtilities.Editor {

    public static class LabelUtilities {

        public static void SetClickableMaintainer(Label label, string text, string link, bool isEmail) {

            if (isEmail) {
                text = $"{text} - {link}";
                link = $"mailto:{link}";
            }
            label.text = $"<a href=\"{link}\">{text}</a>";
        }
    }
}
