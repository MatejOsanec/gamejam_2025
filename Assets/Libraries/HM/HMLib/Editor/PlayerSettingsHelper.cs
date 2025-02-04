using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PlayerSettingsHelper {

    public static void SetStaticBatchingValue(bool value) {

        PlayerSettings[] playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (playerSettings == null) {
            return;
        }
        SerializedObject playerSettingsSerializedObject = new SerializedObject(playerSettings);
        SerializedProperty batchingSettings = playerSettingsSerializedObject.FindProperty("m_BuildTargetBatching");
        // Not sure how these couldn't exist
        if (batchingSettings == null) {
            return;
        }
        // Iterate over all platforms
        for (int i = 0; i < batchingSettings.arraySize; i++) {
            SerializedProperty batchingArrayValue = batchingSettings.GetArrayElementAtIndex(i);
            if (batchingArrayValue == null) {
                continue;
            }
            IEnumerator batchingEnumerator = batchingArrayValue.GetEnumerator();
            if (batchingEnumerator == null) {
                continue;
            }
            while (batchingEnumerator.MoveNext()) {
                SerializedProperty property = (SerializedProperty)batchingEnumerator.Current;
                if (property != null && property.name == "m_StaticBatching") {
                    property.boolValue = value;
                }
            }
        }
        playerSettingsSerializedObject.ApplyModifiedProperties();
    }

    public static void SetDynamicBatchingValue(bool value) {

        PlayerSettings[] playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (playerSettings == null) {
            return;
        }
        SerializedObject playerSettingsSerializedObject = new SerializedObject(playerSettings);
        SerializedProperty batchingSettings = playerSettingsSerializedObject.FindProperty("m_BuildTargetBatching");
        // Not sure how these couldn't exist
        if (batchingSettings == null) {
            return;
        }
        // Iterate over all platforms
        for (int i = 0; i < batchingSettings.arraySize; i++) {
            SerializedProperty batchingArrayValue = batchingSettings.GetArrayElementAtIndex(i);
            if (batchingArrayValue == null) {
                continue;
            }
            IEnumerator batchingEnumerator = batchingArrayValue.GetEnumerator();
            if (batchingEnumerator == null) {
                continue;
            }
            while (batchingEnumerator.MoveNext()) {
                SerializedProperty property = (SerializedProperty)batchingEnumerator.Current;
                if (property != null && property.name == "m_DynamicBatching") {
                    property.boolValue = value;
                }
            }
        }
        playerSettingsSerializedObject.ApplyModifiedProperties();
    }    
}
