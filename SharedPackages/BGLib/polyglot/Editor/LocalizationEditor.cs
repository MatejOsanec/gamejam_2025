namespace BGLib.Polyglot {

    using System;
    using Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(Localization))]
    public class LocalizationInspector : UnityEditor.Editor {

        private const string LocalizationAssetName = "Localization";

        #region Prefs

        private static string GetPrefsString(string key, string? defaultString = null) {

            return EditorPrefs.GetString(Application.productName + "." + key, defaultString);
        }

        private static void SetPrefsString(string key, string value) {

            EditorPrefs.SetString(Application.productName + "." + key, value);
        }

        private static int GetPrefsInt(string key, int defaultInt = 0) {

            return EditorPrefs.GetInt(Application.productName + "." + key, defaultInt);
        }

        private static void SetPrefsInt(string key, int value) {

            EditorPrefs.SetInt(Application.productName + "." + key, value);
        }

        private static bool HasPrefsKey(string key) {

            return EditorPrefs.HasKey(Application.productName + "." + key);
        }

        private static void DeletePrefsKey(string key) {

            EditorPrefs.DeleteKey(Application.productName + "." + key);
        }

        #endregion

        private static void DeletePath(string key, int index) {

            var defaultPath = string.Empty;
            if (Localization.Instance.inputFiles.Count > index) {
                defaultPath = AssetDatabase.GetAssetPath(Localization.Instance.inputFiles[index].TextAsset);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(GetPrefsString(key, defaultPath));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!HasPrefsKey(key));
            if (GUILayout.Button("Clear")) {
                DeletePrefsKey(key);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        void UpdateComponents<T>() where T : Component {

            T[] components = Resources.FindObjectsOfTypeAll<T>();
            foreach (T component in components) {
                var localizable = component as ILocalize;
                localizable?.OnLocalize(EditorLocalization.instance);
            }
        }

        public override void OnInspectorGUI() {

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.LabelField("Polyglot Localization Settings", (GUIStyle)"IN TitleText");

            EditorGUILayout.HelpBox(
                "Support to Google spreadsheet was deprecated. Please, edit the csv file directly.",
                MessageType.Warning
            );
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Localization Settings", (GUIStyle)"IN TitleText");
            var iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false) {
                if (iterator.propertyPath.Contains("Document")) continue;

#if !ARABSUPPORT_ENABLED
                if (iterator.propertyPath == "Localize") {
                    using (new EditorGUI.DisabledGroupScope(true)) {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Arabic Support", (GUIStyle)"BoldLabel");
                        EditorGUILayout.HelpBox(
                            "Enable Arabic Support with ARABSUPPORT_ENABLED post processor flag",
                            MessageType.Info
                        );
                        EditorGUILayout.Toggle(
                            new GUIContent(
                                "Show Tashkeel",
                                "Enable Arabic Support with ARABSUPPORT_ENABLED post processor flag"
                            ),
                            true
                        );
                        EditorGUILayout.Toggle(
                            new GUIContent(
                                "Use Hindu Numbers",
                                "Enable Arabic Support with ARABSUPPORT_ENABLED post processor flag"
                            ),
                            false
                        );
                    }
                }
#endif

                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true, Array.Empty<GUILayoutOption>());
            }
#if !ARABSUPPORT_ENABLED
#endif

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) {
                UpdateComponents<LocalizedText>();
                UpdateComponents<LocalizedTextMesh>();
                UpdateComponents<LocalizedTextMeshPro>();
                UpdateComponents<LocalizedTextMeshProUGUI>();
            }
        }
    }
}
