using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ColorLibrary {

    public class AlphaValuesTabEditor : IColorLibraryTab {

        private List<CacheDataWithSerializedProperty> _cachedAlphaObjects;

        private Vector2 _scrollPosition;

        public void Initialize() {

            RefreshCache();
        }

        public void OnGUI() {

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var alphaObject in _cachedAlphaObjects) {
                RowAlphaGUI(alphaObject);
            }

            // New alpha SO button
            var rect = EditorGUILayout.GetControlRect(false, 25.0f, GUIStyle.none);

            if (GUI.Button(rect, "Add new alpha SO")) {
                EditorApplication.delayCall += CreateNewAlphaSO;
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshCache() {

            var alphaObjects = AssetDatabase.FindAssets($"t:{typeof(AlphaSO)}").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<AlphaSO>).OrderBy(alpha => alpha.alphaValue).Select(alpha => new SerializedObject(alpha));

            _cachedAlphaObjects = new List<CacheDataWithSerializedProperty>();
            foreach (var serializedObject in alphaObjects) {
                var alphaObjectName = serializedObject.FindProperty("m_Name").stringValue;
                var alphaPath = $"{ColorLibraryEditor.kAlphaSOPath}{alphaObjectName}.asset";
                var alphaVariationCacheData = new CacheDataWithSerializedProperty(serializedObject, alphaObjectName, serializedObject.FindProperty("alphaValue"), alphaPath);
                _cachedAlphaObjects.Add(alphaVariationCacheData);
            }
        }

        private void RowAlphaGUI(CacheDataWithSerializedProperty data) {

            data.serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(data.name);
            EditorGUILayout.DelayedFloatField(data.serializedProperty, GUIContent.none);

            if (EditorGUI.EndChangeCheck()) {
                ComponentRefresherRegistry.ForceRefreshComponents();
            }

            if (GUILayout.Button("Remove")) {
                EditorApplication.delayCall += () => DeleteAlphaSO(data.serializedObject.targetObject as AlphaSO);
            }

            EditorGUILayout.EndHorizontal();

            if (data.serializedObject.hasModifiedProperties) {
                if (IsAlphaSoValueValid(data.serializedProperty.floatValue)) {
                    data.serializedObject.ApplyModifiedProperties();
                    var assetName = $"alpha_{data.serializedProperty.floatValue * 100:0}";
                    var currentPath = AssetDatabase.GetAssetPath(data.serializedObject.targetObject);
                    var newPath = $"{ColorLibraryEditor.kAlphaSOPath}{assetName}.asset";
                    var uniqueNewPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                    AssetDatabase.MoveAsset(currentPath, uniqueNewPath);
                    RefreshCache();
                }
                else {
                    data.serializedObject.Update();
                }
            }
        }

        private void CreateNewAlphaSO() {

            bool isValid = TextInputModalDialog.Show(
                "Create new alpha SO",
                "Enter an alpha value between 0.0 and 1.0.\nAlready existing alpha values are invalid.",
                IsAlphaSoValueValid,
                out string alphaStringValue
            );

            if (!isValid) {
                return;
            }

            float alphaValue = float.Parse(alphaStringValue);
            int alphaNameValue = Mathf.RoundToInt(alphaValue * 100);

            AlphaSO asset = ScriptableObject.CreateInstance<AlphaSO>();
            var assetName = $"{ColorLibraryEditor.kAlphaSOPath}alpha_{alphaNameValue}.asset";
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(assetName);
            asset.alphaValue = alphaValue;
            AssetDatabase.CreateAsset(asset, uniqueAssetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;

            RefreshCache();
        }


        private void DeleteAlphaSO(AlphaSO alphaSo) {

            bool proceedDelete = EditorUtility.DisplayDialog(
                title: "Delete alpha SO?",
                message: $"Are you sure you want to delete {alphaSo.name}?\n" +
                         $"Objects referencing this SO will break.",
                ok: "Confirm",
                cancel: "Cancel"
            );

            if (!proceedDelete) {
                return;
            }

            AssetDatabase.DeleteAsset(alphaSo.GetAssetPath());
            AssetDatabase.Refresh();

            RefreshCache();
        }

        private bool IsAlphaSoValueValid(string textInput) {

            if (!float.TryParse(textInput, out float alphaValue)) {
                return false;
            }

            return IsAlphaSoValueValid(alphaValue);
        }

        private bool IsAlphaSoValueValid(float alphaValue) {

            if (alphaValue < 0 || alphaValue > 1) {
                return false;
            }

            if (_cachedAlphaObjects.Any(property => Mathf.Approximately(property.serializedProperty.floatValue, alphaValue))) {
                return false;
            }

            return true;
        }
    }
}
