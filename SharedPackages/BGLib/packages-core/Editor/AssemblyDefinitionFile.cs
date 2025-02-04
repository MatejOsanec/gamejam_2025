namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.Fields)]
    public record AssemblyDefinitionFile {

        public enum Type {
            Runtime,
            Editor,
            Tests,
            PlayTests
        }

        [JsonObject(MemberSerialization.Fields)]
        public record VersionDefine {

            public readonly string name;
            public readonly string expression;
            public readonly string define;

            public VersionDefine(string name, string expression, string define) {

                this.name = name;
                this.expression = expression;
                this.define = define;
            }
        }

        public readonly string name = string.Empty;
        public readonly string rootNamespace = string.Empty;

        public IReadOnlyList<string> references => _references;
        [DefaultValue(new string[0])]
        [JsonProperty(PropertyName = nameof(references))]
        private readonly string[] _references = Array.Empty<string>();

        public IReadOnlyList<string> includePlatforms => _includePlatforms;
        [DefaultValue(new string[0])]
        [JsonProperty(PropertyName = nameof(includePlatforms))]
        private readonly string[] _includePlatforms = Array.Empty<string>();

        public IReadOnlyList<string> excludePlatforms => _excludePlatforms;
        [DefaultValue(new string[0])]
        [JsonProperty(PropertyName = nameof(excludePlatforms))]
        private readonly string[] _excludePlatforms = Array.Empty<string>();

        public readonly bool allowUnsafeCode;
        public readonly bool overrideReferences;
        public IReadOnlyList<string> precompiledReferences => _precompiledReferences;
        [DefaultValue(new string[0])]
        [JsonProperty(PropertyName = nameof(precompiledReferences))]
        private readonly string[] _precompiledReferences = Array.Empty<string>();

        public readonly bool autoReferenced;
        public IReadOnlyList<string> defineConstraints => _defineConstraints;
        [DefaultValue(new string[0])]
        [JsonProperty(PropertyName = nameof(defineConstraints))]
        private readonly string[] _defineConstraints = Array.Empty<string>();

        public IReadOnlyList<VersionDefine> versionDefines => _versionDefines;
        [DefaultValue(new object[0])]
        [JsonProperty(PropertyName = nameof(versionDefines))]
        private readonly VersionDefine[] _versionDefines = Array.Empty<VersionDefine>();

        public readonly bool noEngineReferences;

        public AssemblyDefinitionFile() { }

        public AssemblyDefinitionFile(string assemblyName, Type type, VersionDefine[] versionDefines) : this(
            assemblyName,
            rootNamespace: IsTest(type) ? string.Empty : assemblyName,
            references:
            IsTest(type) ? new[] { "UnityEditor.TestRunner", "UnityEngine.TestRunner" } : Array.Empty<string>(),
            includePlatforms: IsEditor(type) ? new[] { "Editor" } : Array.Empty<string>(),
            excludePlatforms: Array.Empty<string>(),
            allowUnsafeCode: false,
            overrideReferences: true,
            precompiledReferences: IsTest(type) ? new[] { "nunit.framework.dll" } : Array.Empty<string>(),
            autoReferenced: false,
            defineConstraints: IsTest(type) ? new[] { "UNITY_INCLUDE_TESTS" } : Array.Empty<string>(),
            versionDefines: versionDefines,
            noEngineReferences: false
        ) { }

        [JsonConstructor]
        public AssemblyDefinitionFile(
            string name,
            string rootNamespace,
            string[] references,
            string[] includePlatforms,
            string[] excludePlatforms,
            bool allowUnsafeCode,
            bool overrideReferences,
            string[] precompiledReferences,
            bool autoReferenced,
            string[] defineConstraints,
            VersionDefine[] versionDefines,
            bool noEngineReferences
        ) {
            this.name = name;
            this.rootNamespace = rootNamespace;
            _references = references;
            _includePlatforms = includePlatforms;
            _excludePlatforms = excludePlatforms;
            this.allowUnsafeCode = allowUnsafeCode;
            this.overrideReferences = overrideReferences;
            _precompiledReferences = precompiledReferences;
            this.autoReferenced = autoReferenced;
            _defineConstraints = defineConstraints;
            _versionDefines = versionDefines;
            this.noEngineReferences = noEngineReferences;
        }

        public AssemblyDefinitionFile Update(
            string? newName = null,
            string[]? newReferences = null,
            VersionDefine[]? newVersionDefines = null
        ) {

            return new AssemblyDefinitionFile(
                newName ?? name,
                newName ?? rootNamespace,
                newReferences ?? _references,
                _includePlatforms,
                _excludePlatforms,
                allowUnsafeCode,
                overrideReferences,
                _precompiledReferences,
                autoReferenced,
                _defineConstraints,
                newVersionDefines ?? _versionDefines,
                noEngineReferences
            );
        }

        public static string GetSuffixName(Type type) {

            return type switch {
                Type.Runtime => Constants.kRuntimeAssemblyName,
                Type.Editor => Constants.kEditorAssemblyName,
                Type.Tests => Constants.kTestsAssemblyName,
                Type.PlayTests => Constants.kPlayTestsAssemblyName,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static bool IsTest(Type type) {

            return type is Type.Tests or Type.PlayTests;
        }

        private static bool IsEditor(Type type) {

            return type is Type.Editor or Type.Tests;
        }
    }
}
