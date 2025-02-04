namespace BGLib.Polyglot.Editor {

    using UnityEditor;

    [CustomEditor(typeof(LocalizedTextMesh))]
    [CanEditMultipleObjects]
    public class LocalizedTextMeshEditor : LocalizedEditor<LocalizedTextMesh> {
        
        public override void OnInspectorGUI() {
            
            OnInspectorGUI("key");
        }
    }
}
