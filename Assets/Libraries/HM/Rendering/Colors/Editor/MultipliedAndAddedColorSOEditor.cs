using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MultipliedAndAddedColorSO))]
public class MultipliedAndAddedColorSOEditor : ColorSOEditor  {

    protected override void DrawInspector() {

		var color = target as MultipliedAndAddedColorSO;

		DrawDefaultInspector();

		Rect rt = GUILayoutUtility.GetRect(60.0f, 20.0f);

        Color fullColor = color.color;
        fullColor.a = 1.0f;

        EditorGUI.DrawRect(rt, fullColor);

		rt.position = new Vector2(rt.position.x, rt.position.y + rt.height);
		rt.height = 4.0f;
		EditorGUI.DrawRect(rt, Color.black);
		rt.width *= color.color.a;
		EditorGUI.DrawRect(rt, Color.white);
    }
}
