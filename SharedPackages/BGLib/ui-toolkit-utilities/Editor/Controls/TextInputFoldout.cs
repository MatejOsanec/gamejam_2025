using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls.Editor {

    public class TextInputFoldout : UIToolkitUtilities.Controls.TextInputFoldout {

        public new class UxmlFactory : UxmlFactory<TextInputFoldout, UxmlTraits> { }

        public TextInputFoldout() : base () {}

        public void BindPropertyToTextField(SerializedProperty property) {

            this.textField.BindProperty(property);
        }
    }
}
