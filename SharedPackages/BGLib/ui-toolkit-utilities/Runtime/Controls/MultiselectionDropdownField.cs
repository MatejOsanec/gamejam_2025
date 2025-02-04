using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls {

    public class MultiSelectionDropdownField : DropdownField {

        private const string kNothingKeyword = "Nothing";
        private const string kEverythingKeyword = "Everything";
        private const string kMixedKeyword = "Mixed...";

        private readonly List<string> _selectedChoices = new();
        private GenericDropdownMenu _wrapper;
        public IReadOnlyList<string> selectedChoices => _selectedChoices;
        public event Action<IReadOnlyList<string>> selectedChoicesChanged;

        public new class UxmlFactory : UxmlFactory<MultiSelectionDropdownField, UxmlTraits> { }

        public MultiSelectionDropdownField() {

            // HACK: We're bypassing the default pointer down event here because we want to display our own
            var eventCallback = (EventCallback<PointerDownEvent>)Delegate.CreateDelegate(
              typeof(EventCallback<PointerDownEvent>), this,
              "OnPointerDownEvent", false, true);
            UnregisterCallback(eventCallback);
            RegisterCallback<PointerDownEvent>(HandlePointerDownEvent);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
            => UpdateValueBasedOnChoices();

        public override string value {

            get => base.value;
            set {
                _selectedChoices.Clear();

                if (choices.Contains(value)) {
                    _selectedChoices.Add(value);
                }

                UpdateValueBasedOnChoices();
                SendChangeEvent();
            }
        }

        private void HandlePointerDownEvent(PointerDownEvent evt) {

            _wrapper = new GenericDropdownMenu();
            _wrapper.AddItem(kNothingKeyword, _selectedChoices.Count == 0, SelectNothing);
            _wrapper.AddItem(kEverythingKeyword, _selectedChoices.Count == choices.Count, SelectEverything);
            foreach (var choice in choices) {
                _wrapper.AddItem(choice, _selectedChoices.Contains(choice), UpdateItem, choice);
            }

            _wrapper.DropDown(worldBound, this);
        }

        public void SelectSelection(IEnumerable<string> selection) {

            _selectedChoices.Clear();
            foreach (var choice in selection) {
                if (!choices.Contains(choice)) {
                    continue;
                }

                _selectedChoices.Add(choice);
            }

            UpdateValueBasedOnChoices();
            SendChangeEvent();
        }

        private void SelectNothing() {

            _selectedChoices.Clear();
            _wrapper.UpdateItem(kNothingKeyword, true);
            _wrapper.UpdateItem(kEverythingKeyword, false);
            foreach (var choice in choices) {
                _wrapper.UpdateItem(choice, false);
            }

            UpdateValueBasedOnChoices();
            SendChangeEvent();
        }

        private void SelectEverything() {

            _selectedChoices.Clear();
            _selectedChoices.AddRange(choices);
            _wrapper.UpdateItem(kNothingKeyword, false);
            _wrapper.UpdateItem(kEverythingKeyword, true);
            foreach (var choice in choices) {
                _wrapper.UpdateItem(choice, true);
            }

            UpdateValueBasedOnChoices();
            SendChangeEvent();
        }

        private void UpdateItem(object userData) {

            var choice = (string)userData;
            if (_selectedChoices.Contains(choice)) {
                _selectedChoices.Remove(choice);
            }
            else {
                _selectedChoices.Add(choice);
            }

            _wrapper.UpdateItem(kNothingKeyword, _selectedChoices.Count == 0);
            _wrapper.UpdateItem(kEverythingKeyword, _selectedChoices.Count == choices.Count);
            _wrapper.UpdateItem(choice, _selectedChoices.Contains(choice));
            UpdateValueBasedOnChoices();
            SendChangeEvent();
        }

        private void UpdateValueBasedOnChoices() {

            if (_selectedChoices.Count == 0) {
                SetValueWithoutNotify(kNothingKeyword);
            }
            else if (_selectedChoices.Count == choices.Count) {
                SetValueWithoutNotify(kEverythingKeyword);
            }
            else if (_selectedChoices.Count == 1) {
                SetValueWithoutNotify(_selectedChoices.First());
            }
            else {
                SetValueWithoutNotify(kMixedKeyword);
            }
        }

        private void SendChangeEvent() {

            using (ChangeEvent<string> pooled = ChangeEvent<string>.GetPooled(string.Empty, string.Empty)) {
                pooled.target = this;
                SendEvent(pooled);
            }
            selectedChoicesChanged?.Invoke(selectedChoices);
        }
    }
}
