using UnityEditor;

public class ScriptableObjectFactory {

    [MenuItem("Assets/Create/ScriptableObject")]
    public static void CreateScriptableObject() {

        // Show the selection window.
        ScriptableObjectWindow.Present();
    }
}