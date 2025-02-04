using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls.Editor {

    /**
        Holds a game object that could potentially be saved. It's useful in case you are working in view controller a few layers deeper in your hierarchy, you can easily find this class with `GetFirstAncestorOfType` and invoke a save event on the object you are monitoring.
        Maybe to extend, it could monitor the component for if unsaved prefab changes arise and then save it without having to send a signal.
    */
    public class SaveGameObjectElement : VisualElement {

        public new class UxmlFactory : UxmlFactory<SaveGameObjectElement, UxmlTraits> { }

        public event Action savedMonitoredObject;
        public bool shouldAutosave => _autosaveToggle.value;

        private VisualElement _rightSide = new();
        private Button _saveButton = new();
        private Button _warningButton = new Button();
        private VisualElement _warningIcon = new VisualElement();
        private Toggle _autosaveToggle = new();
        private GameObject _toMonitor;

        public SaveGameObjectElement() {

            this.style.justifyContent = Justify.SpaceBetween;
            this.style.flexDirection = FlexDirection.Row;
            _rightSide.style.flexDirection = FlexDirection.Row;
            _autosaveToggle.Query("unity-checkmark").First().style.marginRight = 5;
            _warningButton.AddToClassList("unity-button-with-icon");
            _warningButton.AddToClassList("warning-button");

            _saveButton.text = "Save";
            _warningButton.tooltip = "Not a prefab - Execute caution when saving.";
            _autosaveToggle.text = "Autosave";

            _saveButton.clicked += () => {
                ProcessMonitoredGameObject(InteractionMode.UserAction);
            };
            _warningButton.Hide();

            _warningButton.Add(_warningIcon);
            _rightSide.Add(_saveButton);
            _rightSide.Add(_warningButton);

            this.Add(_autosaveToggle);
            this.Add(_rightSide);
        }

        /// <summary>Allows you to monitor a GameObject. The save button text will be adjusted accordingly to whether it can save this GameObject as prefab instance root, or to the scene. </summary>
        public void MonitorGameObject(GameObject gameObject) {

            if (gameObject == null) {
                StopMonitoringGameObject();
                return;
            }

            _toMonitor = gameObject;
            bool isPrefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject) == gameObject;
            _saveButton.text = isPrefabRoot ? "Save Prefab" : "Save Scene";

            if (!isPrefabRoot) {
                _autosaveToggle.SetValueWithoutNotify(false);
                _warningButton.Show();
            }
            else {
                _warningButton.Hide();
            }
        }

        /// <summary> Clears the currently monitored gameobject and adjusts text accordingly. </summary>
        public void StopMonitoringGameObject() {

            _toMonitor = null;
            _saveButton.text = "Save";
        }

        /// <summary> Notify the element that the monitored game object should be saved. </summary>
        public void PropertyWasModified(InteractionMode interactionMode = InteractionMode.AutomatedAction) {

            if (!shouldAutosave) {
                return;
            }

            ProcessMonitoredGameObject(interactionMode);
        }

        private void ProcessMonitoredGameObject(InteractionMode interactionMode) {

            if (PrefabUtility.GetNearestPrefabInstanceRoot(_toMonitor) == _toMonitor) {
                PrefabUtility.ApplyPrefabInstance(_toMonitor, interactionMode);
            }
            else {
                bool saveScene = EditorUtility.DisplayDialog("Saving Scene", "Are you sure you want to save the scene? This could have unintended side effects.", "Save", "Abort");

                if (saveScene) {
                    if (!EditorSceneManager.SaveOpenScenes()) {
                        Debug.LogWarning("Failed to save open scenes");
                    }
                }
            }

            savedMonitoredObject?.Invoke();
        }
    }
}
