using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities {

    public class GenericDropdownMenu {

        private readonly UnityEngine.UIElements.GenericDropdownMenu _dropdownMenu;

        public GenericDropdownMenu()
            => _dropdownMenu = new UnityEngine.UIElements.GenericDropdownMenu();

        public void AddItem(string itemName, bool isChecked, Action action)
            => _dropdownMenu.AddItem(itemName, isChecked, action);

        public void AddItem(string itemName, bool isChecked, Action<object> action, object data)
            => _dropdownMenu.AddItem(itemName, isChecked, action, data);

        public void AddDisabledItem(string itemName, bool isChecked)
            => _dropdownMenu.AddDisabledItem(itemName, isChecked);

        public void AddSeparator(string path)
            => _dropdownMenu.AddSeparator(path);

        public void DropDown(Rect position, VisualElement targetElement = null, bool anchored = false)
            => _dropdownMenu.DropDown(position, targetElement, anchored);

        public void UpdateItem(string itemName, bool isChecked)
            => _dropdownMenu.UpdateItem(itemName, isChecked);
    }
}
