using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ColorLibrary {

    public class ColorLibraryEditor : EditorWindow {

        public const string kAlphaSOPath = "Packages/com.beatgames.beatsaber.tours.core/UI/Common/SO/AlphaSeparatedColors/AlphaValues/";

        private int _selectedTab;

        private GUIContent _refreshIcon;

        private readonly List<(string tabName, IColorLibraryTab colorLibraryTab)> tabs = new List<(string tabName, IColorLibraryTab colorLibraryTab)>() {
            ("Color editor", new UIColorEditor()),
            ("Alpha SO values", new AlphaValuesTabEditor()),
            ("Text styles", new TextStyleEditor()),
            ("Simple Colors (legacy)", new SimpleColorTabEditor()),
        };

        protected void OnEnable() {

            _refreshIcon = EditorGUIUtility.IconContent("d_Refresh@2x");
            foreach (var tab in tabs) {
                tab.colorLibraryTab.Initialize();
            }

            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
        }

        protected void OnDisable() {

            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
        }

        private void HandleUndoRedoPerformed() {

            ComponentRefresherRegistry.ForceRefreshComponents();
        }

        protected void OnGUI() {

            EditorGUILayout.Space();
            var rect = EditorGUILayout.GetControlRect(false, 25.0f, GUIStyle.none);

            var refreshButtonRect = new Rect(rect.width - 30.0f, rect.y, 25.0f, 20.0f);

            bool forceCacheRefresh = GUI.Button(refreshButtonRect, _refreshIcon);

            EditorGUILayout.Space();

            var newSelectedTab = GUILayout.Toolbar(_selectedTab, tabs.Select(tab => tab.tabName).ToArray());
            forceCacheRefresh |= newSelectedTab != _selectedTab;
            _selectedTab = newSelectedTab;

            if (forceCacheRefresh) {
                tabs[_selectedTab].colorLibraryTab.RefreshCache();
            }
            tabs[_selectedTab].colorLibraryTab.OnGUI();
        }

        [MenuItem("Beat Saber/UI Style Editor", isValidateFunction: false, 'C' - 64)]
        public static void ShowWindow() {

            GetWindow<ColorLibraryEditor>("UI Style Editor");
        }
    }
}
