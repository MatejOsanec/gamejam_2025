namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;

    public class ProjectFiles {

        public enum SearchSpace {
            AllFiles,
            BeatGamesFiles,
            AssetsFolder
        }

        public readonly string projectPath;
        public readonly string assetsPath;
        public readonly string packagesPath;
        public readonly IReadOnlyCollection<string> allFilesRootFolders;
        public readonly IReadOnlyCollection<string> beatGamesFilesRootFolders;
        public readonly IReadOnlyCollection<string> assetsRootFolders;
        public readonly IFileSystem fileSystem;

#if UNITY_EDITOR
        private static ProjectFiles? _defaultProjectFiles;
        public static ProjectFiles Default => _defaultProjectFiles ??= new ProjectFiles();
#endif

        private ProjectFiles() : this(GetDefaultProjectLocation(), new FileSystem()) { }

        public ProjectFiles(string projectPath, IFileSystem fileSystem) {

            this.projectPath = projectPath;
            this.fileSystem = fileSystem;
            assetsPath = fileSystem.Path.Combine(projectPath, "Assets");
            packagesPath = fileSystem.Path.Combine(projectPath, Constants.kPackagesFolder);
            allFilesRootFolders = new[] { assetsPath, packagesPath };
            beatGamesFilesRootFolders = new[]
                { assetsPath, GetPackageRootPath(PackageType.BeatSaber), GetPackageRootPath(PackageType.BGLib) };
            assetsRootFolders = new[] { assetsPath };
        }

        private static string GetDefaultProjectLocation() {

#if UNITY_EDITOR
            return Directory.GetCurrentDirectory();
#else
            throw new System.NotImplementedException("Getting default project location is only supported when running from Unity Editor");
#endif
        }

        public string GetPackageRootPath(PackageType packageType) {

            return fileSystem.Path.Combine(packagesPath, packageType.ToString());
        }

        public IEnumerable<string> GetPackagesByType(PackageType packageType) {

            var packageTypeDirPath = GetPackageRootPath(packageType);
            if (!fileSystem.Directory.Exists(packageTypeDirPath)) {
                yield break;
            }
            var packageDirs = fileSystem.Directory.GetDirectories(packageTypeDirPath);
            foreach (string packageDir in packageDirs) {
                yield return fileSystem.Path.GetFileName(packageDir);
            }
        }

        public string GetPackagePath(PackageType packageType, string folderName) {

            return fileSystem.Path.Combine(packagesPath, packageType.ToString(), folderName);
        }

        public string GetPackageManifestPath(string packagePath) {

            return fileSystem.Path.Combine(packagePath, Constants.kPackageManifestFileName);
        }

        public string GetAssemblyDirectoryPath(string packagePath, AssemblyDefinitionFile.Type assemblyFileType) {

            return fileSystem.Path.Combine(packagePath, AssemblyDefinitionFile.GetSuffixName(assemblyFileType));
        }

        public string GetAssemblyPath(
            string packagePath,
            PackageId packageId,
            AssemblyDefinitionFile.Type assemblyFileType
        ) {

            return GetAssemblyPath(
                GetAssemblyDirectoryPath(packagePath, assemblyFileType),
                packageId.GetAssemblyName(assemblyFileType)
            );
        }

        public string GetAssemblyPath(string assemblyDirPath, string assemblyName) {

            return fileSystem.Path.Combine(assemblyDirPath, $"{assemblyName}.{Constants.kAssemblyDefinitionExtension}");
        }

        public bool HasDirectory(string dirPath) {

            return fileSystem.Directory.Exists(dirPath);
        }

        public void CreateDirectoryIfNotExists(string dirPath) {

            if (!HasDirectory(dirPath)) {
                fileSystem.Directory.CreateDirectory(dirPath);
            }
        }

        public IEnumerable<string> EnumerateFilePath(PackageType packageType, params string[] searchPattern) {

            string packageRoot = GetPackageRootPath(packageType);
            return EnumerateFilePath(new[] { packageRoot }, SearchOption.AllDirectories, searchPattern);
        }

        public IEnumerable<string> EnumerateFilePath(SearchSpace searchSpace, params string[] searchPattern) {

            return EnumerateFilePath(GetRootDirectoriesForSearchSpace(searchSpace), SearchOption.AllDirectories, searchPattern);
        }

        public IEnumerable<string> EnumerateFilePath(IReadOnlyCollection<string> directories, SearchOption searchOption, params string[] searchPattern) {

            foreach (var pattern in searchPattern) {
                foreach (var directory in directories) {
                    foreach (var filePath in fileSystem.Directory.EnumerateFiles(
                                 directory,
                                 pattern,
                                 searchOption
                             )) {
                        yield return filePath;
                    }
                }
            }
        }

        private IReadOnlyCollection<string> GetRootDirectoriesForSearchSpace(SearchSpace searchSpace) {

            return searchSpace switch {
                SearchSpace.AllFiles => allFilesRootFolders,
                SearchSpace.BeatGamesFiles => beatGamesFilesRootFolders,
                SearchSpace.AssetsFolder => assetsRootFolders,
                _ => throw new ArgumentOutOfRangeException(nameof(searchSpace), searchSpace, null)
            };
        }
    }
}
