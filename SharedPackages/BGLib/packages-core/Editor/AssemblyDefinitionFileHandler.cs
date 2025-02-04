namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;

    public class AssemblyDefinitionFileHandler {

        private readonly ProjectFiles _projectFiles;
        private readonly IFileSystem _fileSystem;

        public AssemblyDefinitionFileHandler(ProjectFiles projectFiles) {

            _projectFiles = projectFiles;
            _fileSystem = projectFiles.fileSystem;
        }

        public void WriteToFile(AssemblyDefinitionFile assemblyDefinitionContent, string filePath) {

            JsonFileHandlerForIFileSystem.WriteIndentedWithDefault(assemblyDefinitionContent, _fileSystem, filePath);
        }

        public AssemblyDefinitionFile ReadFromFile(string filePath) {

            return JsonFileHandlerForIFileSystem.ReadFromFile<AssemblyDefinitionFile>(_fileSystem, filePath);
        }

        public AssemblyDefinitionReferenceFile ReadReferenceFromFile(string filePath) {

            return JsonFileHandlerForIFileSystem.ReadFromFile<AssemblyDefinitionReferenceFile>(_fileSystem, filePath);
        }

        internal AssemblyDefinitionFile? ReadFromFile(
            string packagePath,
            PackageId packageId,
            AssemblyDefinitionFile.Type assemblyFileType
        ) {

            string filePath = _projectFiles.GetAssemblyPath(packagePath, packageId, assemblyFileType);
            return _fileSystem.File.Exists(filePath) ? ReadFromFile(filePath) : null;
        }

        internal void RenameAssemblyDefinition(string oldName, string newName) {

            foreach (string assemblyFilePath in SearchAllFilesPath()) {
                UpdateNameInAssemblyFile(oldName, newName, assemblyFilePath);
            }
        }

        public IEnumerable<string> SearchAllFilesPath() {

            return _projectFiles.EnumerateFilePath(
                ProjectFiles.SearchSpace.AllFiles,
                Constants.kAssemblyDefinitionSearchPattern
            );
        }

        public IEnumerable<string> SearchAllReferenceFilesPath() {

            return _projectFiles.EnumerateFilePath(
                ProjectFiles.SearchSpace.AllFiles,
                Constants.kAssemblyReferenceSearchPattern
            );
        }

        public IEnumerable<string> SearchPathsInAssetsFolder() {

            return _projectFiles.EnumerateFilePath(
                ProjectFiles.SearchSpace.AssetsFolder,
                Constants.kAssemblyDefinitionSearchPattern
            );
        }

        public IEnumerable<string> SearchPathsInPackageType(PackageType packageType) {

            return _projectFiles.EnumerateFilePath(packageType, Constants.kAssemblyDefinitionSearchPattern);
        }

        private void UpdateNameInAssemblyFile(string oldName, string newName, string assemblyFilePath) {

            var assemblyDef = ReadFromFile(assemblyFilePath);
            AssemblyDefinitionFile? newAssemblyDef;
            if (assemblyDef.name == oldName) {
                newAssemblyDef = assemblyDef.Update(newName);
                string newFilePath = _fileSystem.Path.Combine(
                    _projectFiles.fileSystem.Path.GetDirectoryName(assemblyFilePath) ?? string.Empty,
                    $"{newName}.{Constants.kAssemblyDefinitionExtension}"
                );
                WriteToFile(newAssemblyDef, newFilePath);
                _fileSystem.File.Delete(assemblyFilePath);
                return;
            }
            bool changed = false;
            var newReferences = new string[assemblyDef.references.Count];
            for (int i = 0; i < assemblyDef.references.Count; i++) {
                string reference = assemblyDef.references[i];
                if (reference != oldName) {
                    newReferences[i] = reference;
                    continue;
                }
                changed = true;
                newReferences[i] = newName;
            }
            if (!changed) {
                return;
            }
            newAssemblyDef = assemblyDef.Update(newReferences: newReferences);
            WriteToFile(newAssemblyDef, assemblyFilePath);
        }

        public AssemblyDefinitionFile? ReadPackageRuntimeAssembly(UpdatablePackage package) {

            if (!package.hasRuntimeAssembly) {
                return null;
            }
            var filePath = _projectFiles.GetAssemblyPath(
                package.path,
                package.packageId,
                AssemblyDefinitionFile.Type.Runtime
            );
            return ReadFromFile(filePath);
        }

        public void WritePackageRuntimeAssembly(
            UpdatablePackage package,
            AssemblyDefinitionFile assemblyDefinitionFile
        ) {

            var filePath = _projectFiles.GetAssemblyPath(
                package.path,
                package.packageId,
                AssemblyDefinitionFile.Type.Runtime
            );
            WriteToFile(assemblyDefinitionFile, filePath);
        }

        public Dictionary<string, List<string>> FindAllAssemblyDirectories() {

            var allAssembliesFilePaths = SearchAllFilesPath().ToArray();
            var visitedDirectories = new HashSet<string>(allAssembliesFilePaths.Length);
            var assemblyDirectories = new Dictionary<string, List<string>>(allAssembliesFilePaths.Length);
            var nameToPaths = new Dictionary<string, List<string>>(allAssembliesFilePaths.Length);

            foreach (var assemblyFilePath in allAssembliesFilePaths) {
                var assemblyDirectory = _fileSystem.Path.GetDirectoryName(assemblyFilePath) ?? "";
                visitedDirectories.Add(assemblyDirectory);
                assemblyDirectories.Add(assemblyFilePath, new List<string> { assemblyDirectory });
                var assembly = ReadFromFile(assemblyFilePath);
                if (nameToPaths.TryGetValue(assembly.name, out List<string> paths)) {
                    paths.Add(assemblyFilePath);
                }
                else {
                    nameToPaths.Add(assembly.name, new List<string> { assemblyFilePath });
                }
            }

            var allAssembliesReferencePaths = SearchAllReferenceFilesPath().ToArray();
            foreach (var assemblyReferencePath in allAssembliesReferencePaths) {
                var assemblyReferenceFile = ReadReferenceFromFile(assemblyReferencePath);
                var assemblyReferenceDirectory = _fileSystem.Path.GetDirectoryName(assemblyReferencePath) ?? "";
                if (assemblyReferenceFile.reference != null) {
                    if (nameToPaths.TryGetValue(assemblyReferenceFile.reference, out var assembliesPath)) {
                        foreach (var assemblyPath in assembliesPath) {
                            assemblyDirectories[assemblyPath].Add(assemblyReferenceDirectory);
                        }
                    }
                }
                visitedDirectories.Add(assemblyReferenceDirectory);
            }

            foreach (var assemblyFilePath in allAssembliesFilePaths) {

                var rootChildren = assemblyDirectories[assemblyFilePath];
                var childDirs = new Stack<IDirectoryInfo>(
                    rootChildren.SelectMany(child => _fileSystem.DirectoryInfo.New(child).GetDirectories())
                );

                while (childDirs.TryPop(out var childDir)) {
                    if (visitedDirectories.Contains(childDir.FullName)) {
                        continue;
                    }
                    //exclude hidden directories
                    if (childDir.Name.EndsWith("~")) {
                        continue;
                    }
                    rootChildren.Add(childDir.FullName);
                    foreach (var subChildDir in childDir.GetDirectories()) {
                        childDirs.Push(subChildDir);
                    }
                }
            }

            return assemblyDirectories;
        }
    }
}
