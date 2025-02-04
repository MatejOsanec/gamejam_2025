namespace BGLib.PackagesCore.Editor {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ProjectAssemblyContext {

        public readonly AssemblyDefinitionFileHandler assemblyDefinitionFileHandler;

        private Dictionary<string, List<string>>? _pathToDirectories;

        private (string groupName, IReadOnlyCollection<string> assemblyPaths)[]? _assemblyGroups;
        public IReadOnlyCollection<(string groupName, IReadOnlyCollection<string> assemblyPaths)> assemblyGroups =>
            _assemblyGroups ??= CreateAssemblyGroups();
        public IReadOnlyDictionary<string, List<string>> pathToDirectories =>
            _pathToDirectories ??= assemblyDefinitionFileHandler.FindAllAssemblyDirectories();

        public ProjectAssemblyContext(AssemblyDefinitionFileHandler assemblyDefinitionFileHandler) {

            this.assemblyDefinitionFileHandler = assemblyDefinitionFileHandler;
        }

        public void ClearCache() {

            _assemblyGroups = null;
        }

        private (string groupName, IReadOnlyCollection<string> assemblyPaths)[] CreateAssemblyGroups() {

            var packageTypeValues = Enum.GetValues(typeof(PackageType));
            var newAssemblyGroups =
                new (string groupName, IReadOnlyCollection<string> assemblyPaths)[packageTypeValues.Length + 1];
            newAssemblyGroups[0] = ("Assets Folder",
                assemblyDefinitionFileHandler.SearchPathsInAssetsFolder().ToArray());
            int i = 1;
            foreach (PackageType packageType in packageTypeValues) {
                newAssemblyGroups[i] = (packageType.ToString(),
                    assemblyDefinitionFileHandler.SearchPathsInPackageType(packageType).ToArray());
                i++;
            }
            return newAssemblyGroups;
        }

    }
}
