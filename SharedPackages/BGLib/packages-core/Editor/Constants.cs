namespace BGLib.PackagesCore.Editor {

    public static class Constants {

        public const string kPackagesFolder = "SharedPackages";
        public const string kCommercialPrefix = "com";
        public const string kCompanyName = "beatgames";
        public const string kAuthorName = "Beat Games";
        public const string kBeatSaberName = "beatsaber";
        public const string kBGLibName = "bglib";
        public const string kBGLibDisplayName = "Lib";
        public const string kPackageManifestFileName = "package.json";
        public const string kPackageDefaultVersion = "1.0.0";
        public const string kReadMeFileName = "README.md";
        public const string kAssemblyDefinitionExtension = "asmdef";
        public const string kAssemblyDefinitionSearchPattern = "*."+kAssemblyDefinitionExtension;
        public const string kAssemblyReferenceExtension = "asmref";
        public const string kAssemblyReferenceSearchPattern = "*."+kAssemblyReferenceExtension;
        public const string kCodeFileExtension = "cs";
        public const string kCodeFileSearchPattern = "*."+kCodeFileExtension;
        public const string kUnitySceneFileExtension = "unity";
        public const string kUnitySceneFileSearchPattern = "*."+kUnitySceneFileExtension;


        public const string kLocalPackagePathPrefix = "file:../" + kPackagesFolder;
        public const string kLatestVersion = "latest";
        public const string kInternalPackagesPrefix = "com.beatgames.";

        public const string kCompilerOptionFileName = "csc.rsp";
        public const string kCompilerNullableOption = "-nullable:enable";
        public const string kRuntimeAssemblyName = "Runtime";
        public const string kEditorAssemblyName = "Editor";
        public const string kTestsAssemblyName = "Tests";
        public const string kPlayTestsAssemblyName = "PlayTests";
    }
}
