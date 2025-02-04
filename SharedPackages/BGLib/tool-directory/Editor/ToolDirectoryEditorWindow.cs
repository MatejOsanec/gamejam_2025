using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BGLib.UiToolkitUtilities.Editor;
using UIToolkitUtilities.Controls.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ToolDirectoryEditorWindow : EditorWindow {

    private class ToolData {

        public ToolDefinition tool { get; }
        public bool isVisible { get; set; }

        public ToolData(ToolDefinition tool) {

            this.tool = tool;
        }
    }

    private class LabelData {

        public VisualElement opacityElement;
        public int count;
        public bool isEnabled = false;
    }

    private const string kQuickSearchPrefKey = "ToolDirectory_QuickSearchEnabled";

    [SerializeField] VisualTreeAsset _UXMLTree;
    [SerializeField] VisualTreeAsset _toolVisualEntryTree;
    [SerializeField] VisualTreeAsset _toolVisualEntrySmallTree;
    [SerializeField] VisualTreeAsset _labelFilterTree;

    [MenuItem("Beat Saber/Tool directory", priority = -20)]
    public static void ShowWindow() {

        ToolDirectoryEditorWindow wnd = GetWindow<ToolDirectoryEditorWindow>();
        wnd.titleContent = new GUIContent("Tool directory");
    }

    private VisualElement _toolsList;
    private readonly Dictionary<ToolData, VisualElement> _toolElements = new();
    private readonly Dictionary<LabelType, LabelData> _labelDatas = new();

    private string _filterText;
    private bool _quickSearchEnabled;

    public void CreateGUI() {

        List<ToolDefinition> toolDefinitions = FindTools();

        _quickSearchEnabled = EditorPrefs.GetBool(kQuickSearchPrefKey, defaultValue: false);

        TemplateContainer toolElementRoot = _UXMLTree.Instantiate();
        rootVisualElement.Add(toolElementRoot);

        _toolsList = rootVisualElement.Q<VisualElement>("ToolsList");

        var textFilterField = rootVisualElement.Q<TextField>("TextFilterField");
        textFilterField.RegisterCallback<ChangeEvent<string>>(
            e => {
                _filterText = e.newValue;
                ApplyFilters();
            }
        );

        var quickSearchToggle = rootVisualElement.Q<Toggle>("QuickSearchToggle");
        quickSearchToggle.value = _quickSearchEnabled;
        quickSearchToggle.RegisterValueChangedCallback(
            e => {
                _quickSearchEnabled = e.newValue;
                EditorPrefs.SetBool(kQuickSearchPrefKey, e.newValue);
                CreateToolVisualEntries(toolDefinitions, e.newValue);
            }
        );

        CreateToolVisualEntries(toolDefinitions, quickSearchToggle.value);

        CreateLabelFilterButtons();

        if (_quickSearchEnabled) {
            textFilterField.Focus();
        }

        textFilterField.RegisterCallback<KeyDownEvent>(
            e => {
                if (!_quickSearchEnabled) {
                    return;
                }

                if (e.keyCode == KeyCode.Escape) {
                    Close();
                    return;
                }

                if (e.keyCode != KeyCode.Return) {
                    return;
                }

                ToolData selectedTool = null;
                foreach (var (toolData, _) in _toolElements) {
                    if (!toolData.isVisible) {
                        continue;
                    }

                    selectedTool = toolData;
                    break;
                }

                selectedTool?.tool.openToolCallback();

                Close();
            }
        );
    }

    private void CreateLabelFilterButtons() {

        VisualElement labelFilterParent = rootVisualElement.Q<VisualElement>("LabelButtonGroup");
        foreach (var (labelType, labelData) in _labelDatas) {

            TemplateContainer newLabelFilter = _labelFilterTree.Instantiate();
            labelData.opacityElement = newLabelFilter.Q<VisualElement>("Label");
            LabelDefinitions.TryGetLabel(labelType, out var labelDefinition);
            ToolDirectoryToolLabels.SetLabelData(labelData.opacityElement, labelDefinition);
            newLabelFilter.Q<Label>("LabelCount").text = labelData.count.ToString();
            newLabelFilter.Q<Button>("ToggleButton").clicked += () => {
                labelData.isEnabled = !labelData.isEnabled;
                labelData.opacityElement.style.opacity = labelData.isEnabled ? 1 : 0.35f;
                ApplyFilters();
            };

            labelData.opacityElement.style.opacity = labelData.isEnabled ? 1 : 0.35f;
            labelFilterParent.Add(newLabelFilter);
        }
    }

    private void ApplyFilters() {

        bool isFilterTextEmpty = string.IsNullOrWhiteSpace(_filterText);
        bool isLabelFilterEnabled = _labelDatas.Any(labelData => labelData.Value.isEnabled);

        foreach (var (toolData, toolElement) in _toolElements) {
            var toolDefinition = toolData.tool;
            bool passTextFilter = true;
            if (!isFilterTextEmpty) {
                passTextFilter = toolDefinition.GetSearchableStrings()
                    .Any(t => t.Contains(_filterText, StringComparison.InvariantCultureIgnoreCase));
            }

            bool hasAnyLabelEnabled = false;
            if (isLabelFilterEnabled) {
                foreach (var (labelType, labelData) in _labelDatas) {
                    if (labelData.isEnabled && toolDefinition.labels.HasFlag(labelType)) {
                        hasAnyLabelEnabled = true;
                        break;
                    }
                }
            }
            else {
                hasAnyLabelEnabled = true;
            }

            bool showElement = passTextFilter && hasAnyLabelEnabled;
            toolData.isVisible = showElement;
            toolElement.style.display = showElement ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private List<ToolDefinition> FindTools() {

        _toolElements.Clear();
        _labelDatas.Clear();

        foreach (var labelType in (LabelType[])Enum.GetValues(typeof(LabelType))) {
            if (labelType == LabelType.None) {
                continue;
            }
            _labelDatas.Add(labelType, new LabelData());
        }

        var toolDefinitions = ToolDefinitionLoader.GetToolDefinitions();

        foreach (var toolDefinition in toolDefinitions) {
            if (toolDefinition.HasErrors) {

                StringBuilder stringBuilder = new($"Errors encountered while parsing tool {toolDefinition.displayName}:");
                for (var index = 0; index < toolDefinition.parseErrors.Count; index++) {
                    var toolDefinitionError = toolDefinition.parseErrors[index];
                    stringBuilder.AppendLine($"{index}: {toolDefinitionError}");
                }
                Debug.LogError(stringBuilder);

                continue;
            }

            var labelDefinitions = LabelDefinitions.GetLabels(toolDefinition.labels);
            foreach (var labelDefinition in labelDefinitions) {
                _labelDatas[labelDefinition.type].count++;
            }
        }

        return toolDefinitions;
    }

    private void CreateToolVisualEntries(List<ToolDefinition> toolDefinitions, bool quickSearch) {

        List<ToolData> toolDatas = toolDefinitions.OrderBy(definition => definition.labelSortOrder)
            .ThenBy(definition => definition.displayName)
            .Select(definition => new ToolData(definition)).ToList();

        _toolsList.Clear();
        _toolElements.Clear();

        foreach (var toolData in toolDatas) {
            var containerElement = quickSearch ?
                CreateToolVisualEntryQuickSearch(toolData) :
                CreateToolVisualEntry(toolData);

            if (containerElement == null) {
                continue;
            }

            _toolsList.Add(containerElement);
            _toolElements.Add(toolData, containerElement);
        }
    }

    private VisualElement CreateToolVisualEntry(ToolData toolData) {

        var toolDefinition = toolData.tool;

        var toolVisualEntry = _toolVisualEntryTree.Instantiate();
        var containerElement = toolVisualEntry.Q<VisualElement>("Container");

        toolVisualEntry.Q<Label>("ToolName").text = toolDefinition.displayName;
        toolVisualEntry.Q<Label>("PackageName").text = toolDefinition.packageName;
        toolVisualEntry.Q<Label>("Description").text = toolDefinition.description;
        toolVisualEntry.Q<ToolDirectoryToolLabels>("LabelContainer").SetLabels(toolDefinition.labels);

        LabelUtilities.SetClickableMaintainer(
            toolVisualEntry.Q<Label>("Maintainer"),
            toolDefinition.maintainer.name,
            toolDefinition.maintainer.link,
            toolDefinition.maintainer.linkIsEmail
        );

        var openToolButton = toolVisualEntry.Q<Button>("OpenToolButton");
        if (toolDefinition.openToolCallback == null) {
            openToolButton.style.display = DisplayStyle.None;
        }
        else {
            openToolButton.clicked += toolDefinition.openToolCallback;
        }

        var shortcutEditor = toolVisualEntry.Q<ShortcutEditor>("ShortcutEditor");
        shortcutEditor.SetShortcutPath(toolDefinition.menuItemPath);

        var buttonsParent = toolVisualEntry.Q<VisualElement>("Buttons");
        foreach (string linkString in toolDefinition.links) {

            // kLinkSeparatorCharacter existence is verified in creation of ToolDefinition
            string[] linkSplit = linkString.Split(ToolDefinitionLoader.kLinkSeparatorCharacter);
            string displayText = linkSplit[0];
            string link = string.Join(ToolDefinitionLoader.kLinkSeparatorCharacter, value: linkSplit, startIndex: 1, count: linkSplit.Length - 1);

            var linkButton = new Button(
                () => {
                    System.Diagnostics.Process.Start(link);
                }
            );

            linkButton.text = displayText;
            buttonsParent.Add(linkButton);
        }

        return containerElement;
    }

    private VisualElement CreateToolVisualEntryQuickSearch(ToolData toolData) {

        var toolDefinition = toolData.tool;

        if (toolDefinition.openToolCallback == null) {
            return null;
        }

        var toolVisualEntry = _toolVisualEntrySmallTree.Instantiate();
        var containerElement = toolVisualEntry.Q<VisualElement>("Container");

        toolVisualEntry.Q<Label>("ToolName").text = toolDefinition.displayName;
        toolVisualEntry.Q<ToolDirectoryToolLabels>("LabelContainer").SetLabels(toolDefinition.labels);
        toolVisualEntry.Q<Button>("OpenToolButton").clicked += toolDefinition.openToolCallback;

        return containerElement;
    }
}
