using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.StyleSheets;

internal class MaterialBigHeaderDecorator : MaterialPropertyDrawer {
    private readonly string _header;

    public MaterialBigHeaderDecorator(string header) => _header = header;

    public MaterialBigHeaderDecorator(float headerAsNumber) => _header = headerAsNumber.ToString((IFormatProvider)CultureInfo.InvariantCulture);

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            return 0;
        }
        return 52.0f;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            return;
        }

        position.y += 20.0f;
        GuiLine(position);

        position.y += 4.0f;

        position = EditorGUI.IndentedRect(position);

        GUIStyle myStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter };

        GUI.Label(position, _header, myStyle);

        position.y += 20.0f;
        GuiLine(position);
    }

    private void GuiLine(Rect position, int iHeight = 1) {

        Rect rect = position;
        rect.height = iHeight;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1.0f));

    }
}
