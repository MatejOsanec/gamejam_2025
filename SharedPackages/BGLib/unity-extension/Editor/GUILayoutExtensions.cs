using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public readonly struct DefineSymbolDescription {

    public readonly string symbol;
    public readonly string label;
    public readonly bool persistent;

    public DefineSymbolDescription(string symbol, string label, bool persistent = false) {

        this.symbol = symbol;
        this.label = label;
        this.persistent = persistent;
    }
}

public static class GUILayoutExtensions {

    public static bool FilterableList<T>(string label, string selectedButtonLabel, int minimumFilterSize, in List<T> sourceList, ref T[] filteredArray, ref string[] names, ref string filter, ref int selectedIndex, Func<T, string, bool> filterFunction, Func<T, string> nameFunction) {

        GUILayout.Label("Filter");
        var newFilter = EditorGUILayout.TextField(filter);
        if (newFilter != filter) {
            if (!string.IsNullOrEmpty(newFilter) && newFilter.Length >= Mathf.Max(minimumFilterSize, 1)) {
                filteredArray = sourceList.FindAll(val => filterFunction(val, newFilter)).ToArray();
                names = filteredArray.Select(nameFunction).ToArray();
            }
            selectedIndex = 0;
            filter = newFilter;
        }

        if (names == null || names.Length == 0) {
            return false;
        }
        GUILayout.Label(label);
        selectedIndex = EditorGUILayout.Popup(selectedIndex, names);

        GUILayout.Space(8.0f);

        if (GUILayout.Button(selectedButtonLabel)) {
            return true;
        }
        return false;
    }

    public static void DefineSymbolToggleList(BuildTargetGroup buildTargetGroup, IEnumerable<DefineSymbolDescription> symbols) {

        var currentScriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(";").ToList();

        bool hasChanged = false;

        EditorGUI.BeginDisabledGroup(Application.isPlaying || EditorApplication.isUpdating || EditorApplication.isCompiling);
        foreach (var symbolElement in symbols) {
            if (DefineSymbolToggle(currentScriptingDefineSymbols, symbolElement.label, symbolElement.symbol, symbolElement.persistent)) {
                hasChanged = true;
            }
        }
        EditorGUI.EndDisabledGroup();

        if (hasChanged) {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", currentScriptingDefineSymbols));
        }

        PersistentDefineSymbolsStorage.Save();
    }

    private static bool DefineSymbolToggle(
        List<string> currentScriptingDefineSymbols,
        string label,
        string symbol,
        bool persistent
    ) {

        bool symbolValue = currentScriptingDefineSymbols.Contains(symbol);
        bool oldValue = symbolValue;

        if (persistent) {
            oldValue = PersistentDefineSymbolsStorage.Get(symbol);
        }

        bool newValue = GUILayout.Toggle(oldValue, label);

        if (newValue != symbolValue) {
            if (newValue) {
                currentScriptingDefineSymbols.Add(symbol);
            }
            else {
                currentScriptingDefineSymbols.Remove(symbol);
            }
        }

        var hasChanged = newValue != oldValue;
        if (persistent && hasChanged) {
            PersistentDefineSymbolsStorage.Set(symbol, newValue);
        }

        return hasChanged;
    }
}

