using UnityEditor;

public abstract class ColorSOEditor : Editor {

    protected void OnEnable() {

        Undo.undoRedoPerformed += HandleUndoRedoPerformed;
    }

    protected void OnDisable() {

        Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
    }

    public override void OnInspectorGUI() {

        EditorGUI.BeginChangeCheck();

        DrawInspector();

        if (EditorGUI.EndChangeCheck()) {
            ComponentRefresherRegistry.ForceRefreshComponents();
        }
    }

    protected virtual void DrawInspector() {

        DrawDefaultInspector();
    }

    private void HandleUndoRedoPerformed() {

        ComponentRefresherRegistry.ForceRefreshComponents();
    }
}
