namespace BGLib.PackagesCore.Editor {
    using System;

    public class UpdatablePackage {

        public readonly PackageId packageId;
        public readonly PackageManifestFile manifest;
        public readonly string folderName;
        public readonly string path;
        public readonly AssemblyDefinitionFile? runtimeAssembly;
        public readonly AssemblyDefinitionFile? playTestsAssembly;
        public readonly AssemblyDefinitionFile? editorAssembly;
        public readonly AssemblyDefinitionFile? testsAssembly;


        public bool hasRuntimeAssembly => runtimeAssembly != null;
        public bool hasPlayTestsAssembly => playTestsAssembly != null;
        public bool hasEditorAssembly => editorAssembly != null;
        public bool hasTestsAssembly => testsAssembly != null;

        public string packageName => packageId.ToString();
        public PackageType type => packageId.type;

        public UpdatablePackage(
            PackageType type,
            string folderName,
            PackageManifestFile manifest,
            string path,
            AssemblyDefinitionFile? runtimeAssembly,
            AssemblyDefinitionFile? playTestsAssembly,
            AssemblyDefinitionFile? editorAssembly,
            AssemblyDefinitionFile? testsAssembly
        ) {

            packageId = new PackageId(manifest.packageName, type);
            this.folderName = folderName;
            this.manifest = manifest;
            this.path = path;
            this.runtimeAssembly = runtimeAssembly;
            this.playTestsAssembly = playTestsAssembly;
            this.editorAssembly = editorAssembly;
            this.testsAssembly = testsAssembly;
        }

        public UpdatablePackage CloneUpdatingBasicInfo(
            PackageType newType,
            string newPackageName,
            PackageManifestFile newManifest,
            string newPath
        ) {

            return new UpdatablePackage(
                newType,
                newPackageName,
                newManifest,
                newPath,
                runtimeAssembly,
                playTestsAssembly,
                editorAssembly,
                testsAssembly
            );
        }

        public UpdatablePackage CloneUpdatingAssemblies(
            AssemblyDefinitionFile? runtimeAssembly,
            AssemblyDefinitionFile? playTestsAssembly,
            AssemblyDefinitionFile? editorAssembly,
            AssemblyDefinitionFile? testsAssembly
        ) {

            return new UpdatablePackage(
                packageId.type,
                folderName,
                manifest,
                path,
                runtimeAssembly,
                playTestsAssembly,
                editorAssembly,
                testsAssembly
            );
        }


        public AssemblyDefinitionFile? GetAssemblyDefinitionFile(AssemblyDefinitionFile.Type fileType) {

            return fileType switch {
                AssemblyDefinitionFile.Type.Runtime => runtimeAssembly,
                AssemblyDefinitionFile.Type.Editor => editorAssembly,
                AssemblyDefinitionFile.Type.Tests => testsAssembly,
                AssemblyDefinitionFile.Type.PlayTests => playTestsAssembly,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
