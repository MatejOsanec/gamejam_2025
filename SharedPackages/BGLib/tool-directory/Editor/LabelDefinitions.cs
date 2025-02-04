using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum LabelType {
    None = 0,
    Essential = 1 << 0,
    Core = 1 << 1,
    Tours = 1 << 2,
    Art = 1 << 3,
    Design = 1 << 4,
    Engineering = 1 << 5,
    TechArt = 1 << 6,
}

/// <summary>
/// Contains all data for a label, derived from ScriptableObject so we can bind to it in the UI.
/// </summary>
public struct LabelDefinition {

    public LabelType type;
    public string displayName;
    public Color color;
    public string tooltip;
}

public static class LabelDefinitions {

    private static readonly Dictionary<LabelType, LabelDefinition> _labelDefinitions = new LabelDefinition[] {
        new () {
            type = LabelType.None,
            displayName = "None",
            tooltip = "No label was assigned.",
            color = new Color(0.5f, 0.5f, 0.5f)
        },
        new () {
            type = LabelType.Essential,
            displayName = "Essential",
            tooltip = "This is likely a core part of everyone's workflow.",
            color = new Color(0.8f, 0.8f, 0.8f)
        },
        new () {
            type = LabelType.Core,
            displayName = "Core",
            tooltip = "Available everywhere.",
            color = new Color(0.9f, 0.7f, 0.7f)
        },
        new () {
            type = LabelType.Tours,
            displayName = "Tours",
            tooltip = "Available only on tours.",
            color = new Color(0.7f, 0.9f, 0.9f)
        },
        new () {
            type = LabelType.Art,
            displayName = "Art",
            tooltip = "Relevant for artists.",
            color = new Color(0.8f, 0.8f, 0.5f)
        },
        new () {
            type = LabelType.Design,
            displayName = "Design",
            tooltip = "Relevant for designers.",
            color = new Color(0.5f, 0.5f, 0.8f)
        },
        new () {
            type = LabelType.Engineering,
            displayName = "Tech",
            tooltip = "Relevant for engineers and other technical users.",
            color = new Color(0.5f, 0.8f, 0.5f)
        },
        new () {
            type = LabelType.TechArt,
            displayName = "Tech art",
            tooltip = "Relevant for tech artists.",
            color = new Color(0.6f, 0.7f, 0.1f)
        }
    }.ToDictionary(definition => definition.type);

    public static bool TryGetLabel(LabelType labelType, out LabelDefinition labelDefinition) => _labelDefinitions.TryGetValue(labelType, out labelDefinition);

    public static IEnumerable<LabelDefinition> GetLabels(LabelType labelTypes) {

        foreach (var labelType in (LabelType[])Enum.GetValues(typeof(LabelType))) {
            if (labelType == LabelType.None) {
                continue;
            }

            if (labelTypes.HasFlag(labelType) && TryGetLabel(labelType, out var labelDefinition)) {
                yield return labelDefinition;
            }
        }
    }
}
