using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls {

    // <summary> Modified version of the original Unity Foldout, but replacing the normal label with a Text Field </summary>
    public class TextInputFoldout : Foldout {

        public new class UxmlFactory : UxmlFactory<TextInputFoldout, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits {

            UxmlStringAttributeDescription _textFieldText = new () { name = "text-field-text" };
            UxmlStringAttributeDescription _suffix = new() { name = "suffix" };
            UxmlBoolAttributeDescription _value = new() { name = "value", defaultValue = true };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {

                base.Init(ve, bag, cc);
                TextInputFoldout foldout = (TextInputFoldout)ve;
                foldout.suffix = _suffix.GetValueFromBag(bag, cc);
                foldout.textFieldText = _textFieldText.GetValueFromBag(bag, cc);
                foldout.SetValueWithoutNotify(_value.GetValueFromBag(bag, cc));
            }
        }

        public string suffix { get { return text; } set { text = value; } }
        public string textFieldText { get { return textField.value; } set { textField.value = value; } }

        protected TextField textField = new();

        public TextInputFoldout() : base() {

            VisualElement checkmark = this.Query<VisualElement>("unity-checkmark");
            textField.style.marginRight = 6;
            checkmark.style.marginRight = 0;
            checkmark.parent.Insert(1, textField);
        }

        public void RegisterTextFieldValueChangedCallback(EventCallback<ChangeEvent<string>> callback) {

            textField.RegisterValueChangedCallback(callback);
        }

        public void UnregisterTextFieldValueChangedCallback(EventCallback<ChangeEvent<string>> callback) {

            textField.UnregisterValueChangedCallback(callback);
        }
    }
}
