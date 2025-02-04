namespace BGLib.Inspections.Editor.UI {

    using Core;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    public static class InspectionGUILayout {

        public static void DrawInspectionGroups(IEnumerable<IInspectionGroup> inspectionGroups, bool showFailOnly = false) {

            foreach (var inspectionGroup in inspectionGroups) {
                DrawInspectionGroup(inspectionGroup, showFailOnly);
            }
        }

        private static void DrawInspectionGroup(IInspectionGroup inspectionGroup, bool showFailOnly) {

            if (showFailOnly && inspectionGroup.inspections.All(inspection => inspection.Inspect().isOk)) {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(inspectionGroup.name, EditorStyles.boldLabel);
            foreach (var inspection in inspectionGroup.inspections) {
                DrawInspection(inspection, showFailOnly);
            }
        }

        private static void DrawInspection(IInspection inspection, bool showFailOnly) {

            var labelContent = new GUIContent(inspection.name);
            var validateResult = inspection.Inspect();

            if (validateResult.isOk) {
                if (!showFailOnly) {
                    EditorGUILayout.LabelField(labelContent, EditorGUIUtility.IconContent("Installed"));
                }
                return;
            }

            var icon = inspection.isCritical
                ? EditorGUIUtility.IconContent("Error").image
                : EditorGUIUtility.IconContent("Warning").image;

            if (validateResult.status == InspectionResult.Status.NonFixable) {
                EditorGUILayout.LabelField(
                    labelContent,
                    new GUIContent(validateResult.errorMessage,icon)
                );
                return;
            }

            const float kMaxButtonWidth = 45;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                labelContent,
                new GUIContent(validateResult.errorMessage, icon)
            );
            if (GUILayout.Button("Fix", GUILayout.MaxWidth(kMaxButtonWidth))) {
                inspection.Fix();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
