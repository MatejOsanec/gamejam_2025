namespace BGLib.ShaderInspector {

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class ShaderInspector : ShaderGUI {

        protected abstract IReadOnlyList<Element> GetRootElements();

        public const float kGlobalLeftOffset = -15.0f;

        private IReadOnlyList<Element> _rootElements;

        // Reusable collections
        private readonly List<InfoBox> _reusableErrorInfoBoxes = new List<InfoBox>();
        private readonly List<InfoBox> _reusableWarningInfoBoxes = new List<InfoBox>();
        private readonly HashSet<MaterialProperty> _reusableUsedMaterialPropertiesSet = new HashSet<MaterialProperty>();

        private readonly PresetsData _presetsData = new PresetsData();
        private readonly HelpBoxPressData _helpBoxPressData = new HelpBoxPressData();
        private readonly Dictionary<Element, Element> _childToParentDictionary = new Dictionary<Element, Element>();

        public class PresetsData {

            private readonly Dictionary<MaterialProperty, string> _reusableDisabledMaterialPropertiesToPresetDisablingItDictionary = new Dictionary<MaterialProperty, string>();

            // If any presets can theoretically cause a cycle, this will be true
            public bool hasCyclicPreset => cyclicPresetsHashSet.Count > 0;
            // Cached error info box if hasCyclicPreset is true
            public InfoBox cyclicPresetInfoBox;
            // Used to find cyclic presets ahead of time - will contain latest found presets cycle
            public readonly HashSet<PresetDropdown> cyclicPresetsHashSet = new HashSet<PresetDropdown>();
            public readonly Dictionary<string, PresetDropdown> propertyNameToPresetDropdownMap = new Dictionary<string, PresetDropdown>();

            public void MarkDisabledMaterialProperty(MaterialProperty presetProperty, string disablingPresetDisplayName) {

                if (_reusableDisabledMaterialPropertiesToPresetDisablingItDictionary.TryGetValue(presetProperty, out var alreadyExistingDisablingPresetName) &&
                    disablingPresetDisplayName != alreadyExistingDisablingPresetName) {
                    Debug.LogWarning($"2 or more presets ({disablingPresetDisplayName}, {alreadyExistingDisablingPresetName}) are trying to set the same property \"{presetProperty.displayName}\"");
                }
                _reusableDisabledMaterialPropertiesToPresetDisablingItDictionary[presetProperty] = disablingPresetDisplayName;
            }

            public bool IsDisabledByPresets(MaterialProperty materialProperty, out string disablingPresetName) {

                return _reusableDisabledMaterialPropertiesToPresetDisablingItDictionary.TryGetValue(materialProperty, out disablingPresetName);
            }

            public void ClearReusable() {

                _reusableDisabledMaterialPropertiesToPresetDisablingItDictionary.Clear();
            }
        }

        public class HelpBoxPressData {

            public enum State {

                None,
                ExpandingFoldouts,
                GettingScrollPosition,
                WaitingForScroll
            }

            public State state;
            public InfoBox pressedInfoBox;

            public void Reset() {

                state = State.None;
                pressedInfoBox = null;
            }
        }

        private bool _unusedPropertiesFoldout;
        private bool _errorInfoBoxesFoldout;
        private bool _warningInfoBoxesFoldout;
        private string _searchString;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {

            // This is a very hacky way how to offset the whole inspector to the left (lower the left margin)
            // I haven't found a better way to achieve this
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(kGlobalLeftOffset);
            EditorGUILayout.BeginVertical();

            // Most of these needs to happen in this order, for example PreDrawingDataCollectAndMark is using search string from DrawSearchField
            InitIfNeeded(properties);
            if (_presetsData.hasCyclicPreset) {
                _presetsData.cyclicPresetInfoBox.OnGUI(materialEditor, properties, searchString: string.Empty, presetsData: _presetsData, parentDisabled: false);
                return;
            }
            ClearReusableCollections();

            GUILayout.Space(5);
            DrawSearchField();
            GUILayout.Space(5);

            PreDrawingDataCollect(properties);
            DrawErrorAndWarningInfoBoxes(materialEditor, properties);
            DrawRootElements(materialEditor, properties);
            DrawUnusedProperties(materialEditor, properties);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (_helpBoxPressData.state == HelpBoxPressData.State.ExpandingFoldouts) {
                materialEditor.Repaint();
                _helpBoxPressData.state = HelpBoxPressData.State.GettingScrollPosition;
            }
            else if (_helpBoxPressData.state == HelpBoxPressData.State.GettingScrollPosition) {
                // Basically one frame pause, don't ask me why, Unity internals
                _helpBoxPressData.state = HelpBoxPressData.State.WaitingForScroll;
            }
            else if (_helpBoxPressData.state == HelpBoxPressData.State.WaitingForScroll) {
                var scrollTo = _helpBoxPressData.pressedInfoBox.lastRectDrawnInto;
                ScrollToPosition(scrollTo.position);
                _helpBoxPressData.Reset();
            }
        }

        private void InitIfNeeded(MaterialProperty[] properties) {

            if (_rootElements != null) {
                return;
            }

            _rootElements = GetRootElements();

            // Make a map of all presets
            // Make a map of which element is which parent
            foreach (var rootElement in _rootElements) {
                foreach (var (element, _, parentElement) in rootElement.EnumerateSelfAndChildElementsRecursively(isParentActive: true, properties, parentElement: null)) {
                    if (element is PresetDropdown presetDropdown) {
                        _presetsData.propertyNameToPresetDropdownMap[presetDropdown.propertyName] = presetDropdown;
                    }
                    _childToParentDictionary[element] = parentElement;
                }
            }

            ValidateCyclicPresets(properties);
        }

        private void ClearReusableCollections() {

            _reusableUsedMaterialPropertiesSet.Clear();
            _reusableErrorInfoBoxes.Clear();
            _reusableWarningInfoBoxes.Clear();
            _presetsData.ClearReusable();
        }

        private void DrawSearchField() {

            using (new EditorGUILayout.HorizontalScope()) {
                _searchString = EditorGUILayout.TextField("Search", _searchString);
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_searchString))) {
                    if (GUILayout.Button("X", GUILayout.Width(20))) {
                        _searchString= string.Empty;
                        GUI.FocusControl(null);
                    }
                }
            }
        }

        private void PreDrawingDataCollect(MaterialProperty[] properties) {

            // Gathering data before drawing
            foreach (var rootElement in _rootElements) {
                foreach (var (element, isActive, parentElement) in rootElement.EnumerateSelfAndChildElementsRecursively(isParentActive: true, properties, parentElement: null)) {

                    // Collecting unused properties
                    element.MarkUsedMaterialPropertiesSelfOnly(_reusableUsedMaterialPropertiesSet, properties);

                    // Collecting errors and warnings
                    if (isActive && element is InfoBox infoBox) {
                        switch (infoBox.messageType) {
                            case MessageType.Error:
                                _reusableErrorInfoBoxes.Add(infoBox);
                                break;
                            case MessageType.Warning:
                                _reusableWarningInfoBoxes.Add(infoBox);
                                break;
                        }
                    }
                    if (element is PresetDropdown presetDropdown) {
                        presetDropdown.MarkDisabledMaterialProperties(_presetsData, properties);
                    }
                    _childToParentDictionary[element] = parentElement;
                }
            }
        }

        private void ValidateCyclicPresets(MaterialProperty[] properties) {

            var hasCyclicPreset = FindCyclicPresets(_rootElements, properties, _presetsData);
            if (!hasCyclicPreset) {
                _presetsData.cyclicPresetsHashSet.Clear();
            }
            else {
                var stringBuilder = new StringBuilder();
                stringBuilder.Clear();
                stringBuilder.AppendLine("Found a possible preset cycle, please fix the cycle.");
                stringBuilder.AppendLine("In case you need this cycle to be defined like this, contact Martin Novák.");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Following presets can cause the cycle:");
                stringBuilder.AppendLine();
                foreach (var preset in _presetsData.cyclicPresetsHashSet) {
                    stringBuilder.AppendLine($"{preset.displayName} ({preset.propertyName})");
                }
                while (stringBuilder.Length > 0 && (stringBuilder[^1] == '\n' || stringBuilder[^1] == '\r')) {
                    stringBuilder.Length--;
                }
                _presetsData.cyclicPresetInfoBox = new InfoBox(stringBuilder.ToString(), MessageType.Error);
            }
        }

        private void DrawErrorAndWarningInfoBoxes(MaterialEditor materialEditor, MaterialProperty[] properties) {

            DrawInfoBoxFoldout(
                materialEditor,
                properties,
                _reusableErrorInfoBoxes,
                _presetsData,
                "Errors",
                ref _errorInfoBoxesFoldout,
                _searchString,
                new Color(1.0f, 0.0f, 0.0f, 0.05f)
            );
            DrawInfoBoxFoldout(
                materialEditor,
                properties,
                _reusableWarningInfoBoxes,
                _presetsData,
                "Warnings",
                ref _warningInfoBoxesFoldout,
                _searchString,
                new Color(1.0f, 1.0f, 0.0f, 0.05f)
            );
        }

        private void DrawInfoBoxFoldout(
            MaterialEditor materialEditor,
            MaterialProperty[] materialProperties,
            IReadOnlyList<InfoBox> infoBoxes,
            PresetsData presetsData,
            string title,
            ref bool foldout,
            string searchString,
            Color backgroundColor
        ) {

            if (infoBoxes.Count == 0) {
                return;
            }

            using var category = new ShaderInspectorLayout.Category(
                name: title,
                tooltip: null,
                featuresCount: infoBoxes.Count,
                showMixed: false,
                foldout: foldout,
                backgroundColor: backgroundColor
            );

            foldout = category.foldout;
            if (!foldout) {
                return;
            }
            foreach (var infoBox in infoBoxes) {
                infoBox.OnGUI(materialEditor, materialProperties, searchString, presetsData, false);
                // Workaround for handling clicks on InfoBox
                Rect helpBoxRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && helpBoxRect.Contains(Event.current.mousePosition)) {
                    _helpBoxPressData.pressedInfoBox = infoBox;
                    _helpBoxPressData.state = HelpBoxPressData.State.ExpandingFoldouts;

                    // Expand all elements from root to the info box
                    Element element = infoBox;
                    while (element != null) {
                        element.ForceExpand();
                        element = _childToParentDictionary[element];
                    }
                }
            }
        }

        private void DrawRootElements(MaterialEditor materialEditor, MaterialProperty[] properties) {

            // Draw Defined Elements
            foreach (var rootElement in _rootElements) {
                rootElement.OnGUI(
                    materialEditor,
                    properties,
                    _searchString,
                    _presetsData,
                    false
                );
            }
        }

        private void DrawUnusedProperties(MaterialEditor materialEditor, MaterialProperty[] properties) {

            // Draw Unused Properties
            materialEditor.SetDefaultGUIWidths();
            var unusedPropertiesCount = properties.Count(property => !_reusableUsedMaterialPropertiesSet.Contains(property));

            if (unusedPropertiesCount == 0) {
                return;
            }

            using (var category = new ShaderInspectorLayout.Category(
                       name: "Properties w/o GUI",
                       tooltip: "Properties that does not have their custom drawing declared in ShaderInspector.cs",
                       featuresCount: unusedPropertiesCount,
                       showMixed: false,
                       foldout: _unusedPropertiesFoldout
                   )) {

                _unusedPropertiesFoldout = category.foldout;
                if (_unusedPropertiesFoldout) {
                    foreach (var materialProperty in properties) {
                        if (_reusableUsedMaterialPropertiesSet.Contains(materialProperty)) {
                            continue;
                        }

                        materialEditor.ShaderProperty(materialProperty, materialProperty.displayName);
                    }
                }
            }
        }

        // Collection of cyclic presets will be output into presetsData.reusableHashSetToFindCyclicPresets if return value is true
        private static bool FindCyclicPresets(
            IReadOnlyList<Element> rootElements,
            MaterialProperty[] properties,
            PresetsData presetsData
        ) {

            foreach (var rootElement in rootElements) {
                foreach (var (element, _, _) in rootElement.EnumerateSelfAndChildElementsRecursively(isParentActive: true, properties, parentElement: null)) {
                    if (element is PresetDropdown presetDropdown) {
                        presetsData.cyclicPresetsHashSet.Clear();
                        if (CheckIfPresetIsCyclic(presetDropdown, properties, presetsData)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool CheckIfPresetIsCyclic(
            PresetDropdown presetDropdown,
            MaterialProperty[] properties,
            PresetsData presetsData
        ) {

            if (presetsData.cyclicPresetsHashSet.Contains(presetDropdown)) {
                return true;
            }
            presetsData.cyclicPresetsHashSet.Add(presetDropdown);
            foreach (var option in presetDropdown.options) {
                if (option.presetValues == null) {
                    continue;
                }

                foreach (var presetValue in option.presetValues) {
                    if (presetValue is not PresetDropdown.PresetPropertyPreset propertyPreset) {
                        continue;
                    }

                    var hasCycle = CheckIfPresetIsCyclic(
                        presetsData.propertyNameToPresetDropdownMap[propertyPreset.propertyName],
                        properties,
                        presetsData
                    );
                    if (hasCycle) {
                        return true;
                    }
                }
            }

            presetsData.cyclicPresetsHashSet.Remove(presetDropdown);
            return false;
        }

        public void KeywordChangedFromPreset(string propertyName, string keyword, bool isEnabled, MaterialProperty[] properties) {

            foreach (var rootElement in _rootElements) {
                foreach (var (element, _, _) in rootElement.EnumerateSelfAndChildElementsRecursively(isParentActive: true, properties, parentElement: null)) {
                    if (element is KeywordDropdown keywordDropdown) {
                        keywordDropdown.KeywordChangedExternally(propertyName, keyword, isEnabled, properties);
                    }
                }
            }
        }

        public void ForceApplyAllPresets(
            MaterialEditor materialEditor,
            MaterialProperty[] properties,
            MaterialProperty requestingPresetProperty
        ) {
            foreach (var rootElement in _rootElements) {
                foreach (var (element, _, _) in rootElement.EnumerateSelfAndChildElementsRecursively(isParentActive: true, properties, parentElement: null)) {
                    if (element is PresetDropdown presetDropdown) {
                        MaterialProperty property = FindProperty(presetDropdown.propertyName, properties);
                        // Do not re-apply self, it would report it above as cycle
                        if (property == requestingPresetProperty) {
                            continue;
                        }
                        presetDropdown.ApplySelectedPreset(materialEditor, properties);
                    }
                }
            }
        }

        // Exposing ShaderGUI protected method
        public new static MaterialProperty FindProperty(
            string propertyName,
            MaterialProperty[] properties,
            bool propertyIsMandatory = false
        ) {

            return ShaderGUI.FindProperty(propertyName, properties, propertyIsMandatory);
        }

        private void ScrollToPosition(Vector2 position) {

            var editorWindow = EditorWindow.focusedWindow;
            var type = editorWindow.GetType();
            var scrollViewField = type.GetField("m_ScrollView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollView = (ScrollView)scrollViewField.GetValue(editorWindow);
            scrollView.scrollOffset = position;
        }
    }
}
