namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;

    public class PackageFileHandler {

        private readonly ProjectFiles _projectFiles;
        private readonly AssemblyDefinitionFileHandler _assemblyFileHandler;
        private readonly PackageManifestFileHandler _packageManifestFileHandler;

        public PackageFileHandler(
            ProjectFiles projectFiles,
            AssemblyDefinitionFileHandler assemblyFileHandler,
            PackageManifestFileHandler packageManifestFileHandler
        ) {

            _projectFiles = projectFiles;
            _assemblyFileHandler = assemblyFileHandler;
            _packageManifestFileHandler = packageManifestFileHandler;
        }

        public IEnumerable<string> ListAllPackagesId() {

            foreach (PackageType packageType in Enum.GetValues(typeof(PackageType))) {
                foreach (string packageName in _projectFiles.GetPackagesByType(packageType)) {
                    yield return GetPackageId(packageType, packageName);
                }
            }
        }

        public static string GetPackageId(PackageType packageType, string folderName) {

            return $"{packageType.ToString()}\\{folderName}";
        }

        public static (PackageType type, string folderName) ExtractPackageInfoFromPackageId(string packageId) {

            var idSplit = packageId.Split("\\");
            var packageType = (PackageType)Enum.Parse(typeof(PackageType), idSplit[0]);
            var folderName = idSplit[1];
            return (packageType, folderName);
        }

        public UpdatablePackage LoadPackage(string packageIdString) {

            (var packageType, string folderName) = ExtractPackageInfoFromPackageId(packageIdString);
            string packagePath = _projectFiles.GetPackagePath(packageType, folderName);
            var manifest = _packageManifestFileHandler.ReadPackageManifestFromPackagePath(packagePath);

            var packageId = new PackageId(manifest.packageName, packageType);

            var runtimeAssembly = _assemblyFileHandler.ReadFromFile(
                packagePath,
                packageId,
                AssemblyDefinitionFile.Type.Runtime
            );
            var playTestsAssembly = _assemblyFileHandler.ReadFromFile(
                packagePath,
                packageId,
                AssemblyDefinitionFile.Type.PlayTests
            );
            var editorAssembly = _assemblyFileHandler.ReadFromFile(
                packagePath,
                packageId,
                AssemblyDefinitionFile.Type.Editor
            );
            var testsAssembly = _assemblyFileHandler.ReadFromFile(
                packagePath,
                packageId,
                AssemblyDefinitionFile.Type.Tests
            );
            return new UpdatablePackage(
                packageType,
                folderName,
                manifest,
                packagePath,
                runtimeAssembly,
                playTestsAssembly,
                editorAssembly,
                testsAssembly
            );
        }

        internal void CreatePackageFolder(string packagePath) {

            if (_projectFiles.HasDirectory(packagePath)) {
                throw new InvalidOperationException($"There is already a package on the same path: {packagePath}");
            }
            _projectFiles.fileSystem.Directory.CreateDirectory(packagePath);
        }

        public void CreateReadMe(string packagePath, PackageId packageId, string readMeTemplate, string description) {

            if (string.IsNullOrWhiteSpace(readMeTemplate)) {
                throw new ArgumentException("Provided a non-empty template", nameof(readMeTemplate));
            }
            string readMeFilePath = _projectFiles.fileSystem.Path.Combine(packagePath, Constants.kReadMeFileName);
            string title = packageId.GetTitle();
            var result = readMeTemplate!.Replace("{{package_name}}", title);
            result = result.Replace("{{description}}", description);
            _projectFiles.fileSystem.File.WriteAllText(readMeFilePath, result);
        }
    }
}
