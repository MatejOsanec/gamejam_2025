using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Signal))]
public class EventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        Signal e = target as Signal;
        if (GUILayout.Button("Raise"))
            e.Raise();
    }
}
