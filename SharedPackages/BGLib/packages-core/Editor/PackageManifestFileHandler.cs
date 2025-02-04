namespace BGLib.PackagesCore.Editor {

    public class PackageManifestFileHandler {

        private readonly ProjectFiles _projectFiles;

        public PackageManifestFileHandler(ProjectFiles projectFiles) {

            _projectFiles = projectFiles;
        }

        public PackageManifestFile ReadPackageManifestFromPackagePath(string packagePath) {

            return ReadPackageManifest(_projectFiles.GetPackageManifestPath(packagePath));
        }

        public PackageManifestFile ReadPackageManifest(string manifestPath) {

            return JsonFileHandlerForIFileSystem.ReadFromFile<PackageManifestFile>(_projectFiles.fileSystem, manifestPath);
        }

        public void WritePackageManifestFromPackagePath(PackageManifestFile manifestFile, string packagePath) {

            WritePackageManifest(manifestFile, _projectFiles.GetPackageManifestPath(packagePath));
        }

        public void WritePackageManifest(PackageManifestFile manifestFile, string manifestPath) {

            JsonFileHandlerForIFileSystem.WriteIndentedWithDefault(manifestFile, _projectFiles.fileSystem, manifestPath);
        }
    }
}
