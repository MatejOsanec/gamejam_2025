namespace BGLib.AppFlow.Editor {

    using System;
    using System.Linq;
    using DotnetExtension.CommandLine;
    using Initialization;
    using JsonExtension;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Windows;

    [InitializeOnLoad]
    public static class CustomizableEnvironmentCommandLineArgsProviderEditor {

        private const string kStateFilePath = ".customizableEnvironmentCommandLineArgsProviderState.json";

        static CustomizableEnvironmentCommandLineArgsProviderEditor() {

            LoadState();
            CustomizableEnvironmentCommandLineArgsProvider.injectArgumentsResult = GetCommandLineArgsHandler;
        }

        private static string[] GetCommandLineArgsHandler() {

            var commandLineArgs = _state.useEnvironmentCommandLineArgs
                ? CommandLineParser.GetCommandLineArgs()
                : Array.Empty<string>();
            if (_state.useCustomCommandLineArgs && !string.IsNullOrWhiteSpace(_state.customCommandLineArgs)) {
                commandLineArgs = commandLineArgs.Concat(_state.customCommandLineArgs.Split(" ")).ToArray();
            }
            return commandLineArgs;
        }

        private class State {

            public bool useEnvironmentCommandLineArgs;
            public bool useCustomCommandLineArgs;
            public string customCommandLineArgs = string.Empty;
        }

        private static State _state = new State();

        private static void LoadState() {

            if (File.Exists(kStateFilePath)) {
                _state = JsonFileHandler.ReadFromFile<State>(kStateFilePath);
            }
            else {
                WriteState();
            }
        }

        private static void WriteState() {

            JsonFileHandler.WriteIndentedWithDefault(_state, kStateFilePath, indentation: 2);
        }

        public static void OnGUI() {

            EditorGUI.BeginChangeCheck();
            _state.useEnvironmentCommandLineArgs = GUILayout.Toggle(
                _state.useEnvironmentCommandLineArgs,
                "Use environment command line args"
            );
            _state.useCustomCommandLineArgs = GUILayout.Toggle(
                _state.useCustomCommandLineArgs,
                "Use Custom Command Line Args"
            );

            if (_state.useCustomCommandLineArgs) {
                _state.customCommandLineArgs = GUILayout.TextField(_state.customCommandLineArgs);
            }
            if (EditorGUI.EndChangeCheck()) {
                WriteState();
            }
        }

        public static void UpdateState(string commandLineArgs, bool useCustomCommandLineArgs) {

            _state.customCommandLineArgs = commandLineArgs;
            _state.useCustomCommandLineArgs = useCustomCommandLineArgs;
            WriteState();
        }
    }
}
