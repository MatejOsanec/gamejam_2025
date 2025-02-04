using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class TextMeshProValidator {

    public static List<UnityObjectWithDescription> GetInconsistentTextMeshProFontSize(Object[] objects) {

        var results = new List<UnityObjectWithDescription>();
        foreach (var o in objects) {
            if (o is TMP_Text text) {
                if (text.enableAutoSizing) {
                    SerializedObject serializedObject = new SerializedObject(text);
                    SerializedProperty serializedProperty = serializedObject.FindProperty("m_fontSizeBase");

                    var maxSize = text.fontSizeMax;
                    var nonAutoSizingFontSize = serializedProperty.floatValue;

                    if (!Mathf.Approximately(maxSize, nonAutoSizingFontSize)) {
                        results.Add(new UnityObjectWithDescription(o, $"Serialized fontSize != autoSizing max ({nonAutoSizingFontSize} != {maxSize})"));
                    }
                }
            }
        }

        return results;
    }

    public static List<UnityObjectWithDescription> GetInvalidTextMeshProAlignment(Object[] objects) {

        var results = new List<UnityObjectWithDescription>();
        foreach (var o in objects) {
            if (o is TMP_Text text) {

                var alignmentInt = (int)text.alignment;
                if (alignmentInt == 65535) {
                    results.Add(new UnityObjectWithDescription(o, $"Invalid font alignment (after TMP Downgrade)"));
                }
            }
        }

        return results;
    }
}
