using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all data for a tool
/// </summary>
public struct ToolDefinition {

    private const int kMaxLabels = 5;

    public string displayName;
    public string description;
    public string packageName;
    public ToolMaintainerData maintainer;
    public Action openToolCallback;
    public LabelType labels;
    public string menuItemPath;
    public string[] links;
    public int labelSortOrder;
    public List<string> parseErrors;

    public bool HasErrors => parseErrors.Count != 0;

    public ToolDefinition(LabelType labelTypes) : this() {

        labels = labelTypes;
        var labelList = LabelDefinitions.GetLabels(labelTypes);

        var sortedLabels = labelList.OrderBy(label => (int)label.type).ToArray();
        for (var index = 0; index < sortedLabels.Length; index++) {
            var label = sortedLabels[index];
            int labelTypeMultiplier = (kMaxLabels - index);
            labelSortOrder = (int)label.type * labelTypeMultiplier + (labelSortOrder * 100);
        }
    }

    public IEnumerable<string> GetSearchableStrings() {

        yield return displayName;
        yield return description;
        yield return packageName;
        yield return maintainer.name;
    }
}
