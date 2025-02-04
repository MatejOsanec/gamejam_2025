using UnityEditor;
using UnityEngine.UIElements;

public class ToolDirectoryToolLabels : VisualElement {

    private const string kLabelTemplatePath =
        "Packages/com.beatgames.bglib.tool-directory/Editor/ToolDirectoryToolLabel.uxml";

    public new class UxmlFactory : UxmlFactory<ToolDirectoryToolLabels> { }

    private readonly VisualTreeAsset _labelTreeAsset;

    public ToolDirectoryToolLabels() {

        _labelTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kLabelTemplatePath);

        style.flexDirection = FlexDirection.Row;
    }

    public void SetLabels(LabelType labels) {

        var labelDefinitions = LabelDefinitions.GetLabels(labels);
        foreach (var labelDefinition in labelDefinitions) {
            var newLabel = _labelTreeAsset.Instantiate();
            SetLabelData(newLabel, labelDefinition);
            Add(newLabel);
        }
    }

    public static void SetLabelData(VisualElement labelElement, LabelDefinition label) {

        labelElement.Q<Label>("Text").text = label.displayName;
        var backgroundElement = labelElement.Q<VisualElement>("Background");
        backgroundElement.style.backgroundColor = label.color;
        backgroundElement.tooltip = label.tooltip;
    }
}
