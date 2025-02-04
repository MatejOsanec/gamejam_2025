namespace BGLib.PackagesCore.Editor.Packages {

    using System.Collections.Generic;

    public class PackageBrowser {
        
        /// <summary>
        /// Gets a list of installed packages
        /// Invoking UnityEditor.PackageManager.Client methods might be more relevant,
        /// however it's a complicated asynchronous request with a lot of supporting code.
        /// Parsing manifest.json directly is a bit faster and easier approach
        /// </summary>
        /// <returns>Dictionary of packageId => local path</returns>
        public static Dictionary<string, string> GetInstalledPackages(UnityManifestFile manifestData) {

            var packageRegistry = new Dictionary<string, string>();

            foreach (var keyValuePair in manifestData.dependencies) {
                if (keyValuePair.Value.StartsWith(Constants.kLocalPackagePathPrefix)) {
                    packageRegistry.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return packageRegistry;
        }
        
        /// <summary>
        /// Traverse SharedPackages folder and build a registry of all local packages
        /// Current traversal expects folder structure to look like:
        /// SharedPackages/[GroupName]/[ShortenedPackageId]/package.json
        /// </summary>
        /// <returns>Dictionary of packageId => list of versions</returns>
        public static SharedPackageRegistry GetAvailablePackages() {
                    
            return new SharedPackageRegistry();
        }
    }
}
