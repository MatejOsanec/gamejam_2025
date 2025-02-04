namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;

    public class PackmanManager {

        public readonly ProjectFiles projectFiles;
        public readonly PackageFileHandler packageFileHandler;
        public readonly AssemblyDefinitionFileHandler assemblyFileHandler;
        public readonly PackageManifestFileHandler packageManifestFileHandler;
        private readonly IReadOnlyList<IPackageList> _packageLists;

        public PackmanManager(
            ProjectFiles projectFiles,
            IReadOnlyList<IPackageList> packageLists
        ) {

            this.projectFiles = projectFiles;
            packageManifestFileHandler = new PackageManifestFileHandler(this.projectFiles);
            assemblyFileHandler = new AssemblyDefinitionFileHandler(this.projectFiles);
            packageFileHandler = new PackageFileHandler(this.projectFiles, assemblyFileHandler, packageManifestFileHandler);
            _packageLists = packageLists;
        }

        public (string packagePath, PackageId packageId) Create(PackageOperationParams param) {

            PackageValidator.ValidateCommonFields(param);
            if (param.packageList is null) {
                throw new ArgumentException("Please, provide a package list.", nameof(param.packageList));
            }
            string packagePath = projectFiles.GetPackagePath(param.type, param.name);
            PackageId packageId = new PackageId(param.name, param.type);
            packageFileHandler.CreatePackageFolder(packagePath);
            var manifestFile = new PackageManifestFile(packageId, param.description, param.author, param.hideAssets);
            packageManifestFileHandler.WritePackageManifestFromPackagePath(manifestFile, packagePath);
            CreateAssemblies(packagePath, packageId, param);
            if (param.readMeTemplate != null) {
                packageFileHandler.CreateReadMe(packagePath, packageId, param.readMeTemplate, param.description);
            }
            param.packageList.AddPackage(packageId.GetFullName());
            return (packagePath, packageId);
        }

        public UpdatablePackage Update(
            UpdatablePackage updatablePackage,
            PackageOperationParams param,
            out bool hasMovedFiles
        ) {

            hasMovedFiles = false;
            PackageValidator.ValidateCommonFields(param);
            var newPackage = UpdateBasicInfo(
                updatablePackage,
                param.type,
                param.name,
                param.description,
                param.author,
                param.hideAssets
            );
            if (updatablePackage.path != newPackage.path) {
                MoveFiles(updatablePackage, newPackage);
                hasMovedFiles = true;
            }
            var result = AddRequestedAssemblies(
                newPackage,
                param.createRuntimeAssembly,
                param.createPlayTestsAssembly,
                param.createEditorAssembly,
                param.createTestsAssembly,
                param.nullableAssemblies
            );
            if (hasMovedFiles) {
                UpdatePackageIdInPackagesList(updatablePackage.packageId, result.packageId);
            }
            return result;
        }

        private void CreateAssemblies(string packagePath, PackageId packageId, PackageOperationParams param) {

            if (param.createRuntimeAssembly) {
                CreateAssembly(packageId, packagePath, AssemblyDefinitionFile.Type.Runtime, param.nullableAssemblies);
            }
            if (param.createPlayTestsAssembly) {
                CreateAssembly(packageId, packagePath, AssemblyDefinitionFile.Type.PlayTests, param.nullableAssemblies);
            }
            if (param.createEditorAssembly) {
                CreateAssembly(packageId, packagePath, AssemblyDefinitionFile.Type.Editor, param.nullableAssemblies);
            }
            if (param.createTestsAssembly) {
                CreateAssembly(packageId, packagePath, AssemblyDefinitionFile.Type.Tests, param.nullableAssemblies);
            }
        }

        private AssemblyDefinitionFile CreateAssembly(
            PackageId packageId,
            string packagePath,
            AssemblyDefinitionFile.Type assemblyFileType,
            bool isNullable
        ) {

            string assemblyDirPath = projectFiles.GetAssemblyDirectoryPath(packagePath, assemblyFileType);
            projectFiles.CreateDirectoryIfNotExists(assemblyDirPath);
            string assemblyName = packageId.GetAssemblyName(assemblyFileType);
            string assemblyPath = projectFiles.GetAssemblyPath(assemblyDirPath, assemblyName);
            var assemblyDefinitionContent = new AssemblyDefinitionFile(assemblyName, assemblyFileType, Array.Empty<AssemblyDefinitionFile.VersionDefine>());
            assemblyFileHandler.WriteToFile(assemblyDefinitionContent, assemblyPath);
            if (isNullable) {
                string compilerOptionFilePath = projectFiles.fileSystem.Path.Combine(assemblyDirPath, Constants.kCompilerOptionFileName);
                projectFiles.fileSystem.File.WriteAllText(compilerOptionFilePath, Constants.kCompilerNullableOption);
            }
            return assemblyDefinitionContent;
        }

        private UpdatablePackage UpdateBasicInfo(
            UpdatablePackage package,
            PackageType newType,
            string newPackageName,
            string newDescription,
            string newAuthor,
            bool newHideAssets
        ) {

            bool hasToMoveFiles = newPackageName != package.packageId.ToString() || newType != package.packageId.type;
            string newAuthorForManifest = PackageManifestFile.CreateAuthorForManifest(newType, newAuthor);
            if (!hasToMoveFiles && newDescription == package.manifest.description &&
                newAuthorForManifest == package.manifest.author.name &&
                newHideAssets == package.manifest.hideInEditor) {
                return package;
            }
            string newPath = projectFiles.GetPackagePath(newType, newPackageName);
            var newPackageId = new PackageId(newPackageName, newType);
            var newManifest = new PackageManifestFile(newPackageId, newDescription, newAuthor, newHideAssets);
            packageManifestFileHandler.WritePackageManifestFromPackagePath(newManifest, package.path);
            var result = package.CloneUpdatingBasicInfo(newType, newPackageName, newManifest, newPath);
            return result;
        }

        private void MoveFiles(UpdatablePackage oldPackage, UpdatablePackage newPackage) {

            UpdateAssemblies(oldPackage, newPackage.packageId);
            var parentDirectory = projectFiles.fileSystem.Directory.GetParent(newPackage.path);
            if (parentDirectory != null) {
                string newPathParentDirectory = parentDirectory.FullName;
                projectFiles.CreateDirectoryIfNotExists(newPathParentDirectory);
            }
            projectFiles.fileSystem.Directory.Move(oldPackage.path, newPackage.path);
        }

        private void UpdatePackageIdInPackagesList(PackageId oldId, PackageId newId) {

            foreach (var packageList in _packageLists) {
                packageList.ReplacePackage(oldId, newId);
            }
        }

        private void UpdateAssemblies(UpdatablePackage oldPackage, PackageId newPackageId) {

            if (oldPackage.hasRuntimeAssembly) {
                UpdateAssembly(oldPackage.packageId, newPackageId, AssemblyDefinitionFile.Type.Runtime);
            }
            if (oldPackage.hasPlayTestsAssembly) {
                UpdateAssembly(oldPackage.packageId, newPackageId, AssemblyDefinitionFile.Type.PlayTests);
            }
            if (oldPackage.hasEditorAssembly) {
                UpdateAssembly(oldPackage.packageId, newPackageId, AssemblyDefinitionFile.Type.Editor);
            }
            if (oldPackage.hasTestsAssembly) {
                UpdateAssembly(oldPackage.packageId, newPackageId, AssemblyDefinitionFile.Type.Tests);
            }
        }

        private void UpdateAssembly(
            PackageId oldPackageId,
            PackageId newPackageId,
            AssemblyDefinitionFile.Type assemblyFileType
        ) {

            string oldAssemblyName = oldPackageId.GetAssemblyName(assemblyFileType);
            string newAssemblyName = newPackageId.GetAssemblyName(assemblyFileType);
            assemblyFileHandler.RenameAssemblyDefinition(oldAssemblyName, newAssemblyName);
        }

        private UpdatablePackage AddRequestedAssemblies(
            UpdatablePackage updatablePackage,
            bool addRuntimeAssembly,
            bool addPlayTestsAssembly,
            bool addEditorAssembly,
            bool addTestsAssembly,
            bool nullableAssembly
        ) {
            if ((addRuntimeAssembly || addPlayTestsAssembly || addEditorAssembly || addTestsAssembly) == false) {
                return updatablePackage;
            }
            var runtimeAssembly = addRuntimeAssembly
                ? AddAssembly(updatablePackage, AssemblyDefinitionFile.Type.Runtime, nullableAssembly)
                : updatablePackage.runtimeAssembly;
            var playTestsAssembly = addPlayTestsAssembly
                ? AddAssembly(updatablePackage, AssemblyDefinitionFile.Type.PlayTests, nullableAssembly)
                : updatablePackage.playTestsAssembly;
            var editorAssembly = addEditorAssembly
                ? AddAssembly(updatablePackage, AssemblyDefinitionFile.Type.Editor, nullableAssembly)
                : updatablePackage.editorAssembly;
            var testsAssembly = addTestsAssembly
                ? AddAssembly(updatablePackage, AssemblyDefinitionFile.Type.Tests, nullableAssembly)
                : updatablePackage.testsAssembly;
            return updatablePackage.CloneUpdatingAssemblies(
                runtimeAssembly,
                playTestsAssembly,
                editorAssembly,
                testsAssembly
            );
        }

        private AssemblyDefinitionFile AddAssembly(
            UpdatablePackage updatablePackage,
            AssemblyDefinitionFile.Type assemblyFileType,
            bool isNullable
        ) {

            return CreateAssembly(updatablePackage.packageId, updatablePackage.path, assemblyFileType, isNullable);
        }
    }
}
