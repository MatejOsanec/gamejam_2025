namespace BGLib.PackagesCore.Editor.Packages {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.RegularExpressions;
    using JsonExtension;
    using Newtonsoft.Json;

    public class SharedPackageRegistry {

        private static readonly Regex kVersionRegex = new Regex(@"^\d+\.\d+\.\d+(\.\d+)?");

        private readonly Dictionary<string, SharedPackageInfoCollection> _packages = new();

        public IReadOnlyDictionary<string, SharedPackageInfoCollection> packages => _packages;

        public SharedPackageRegistry() {

            var packageGroups = Directory.GetDirectories(Constants.kPackagesFolder);

            foreach (var packageGroup in packageGroups) {
                var packageGroupName = Path.GetFileName(packageGroup);
                var packageFolders = Directory.GetDirectories(packageGroup);
                foreach (var packageFolder in packageFolders) {

                    var packageFolderName = Path.GetFileName(packageFolder);
                    var packageJsonPath = Path.Combine(packageFolder, "package.json");
                    if (!File.Exists(packageJsonPath)) {
                        continue;
                    }

                    var packageData = JsonConvert.DeserializeObject<PackageManifestFile>(
                        File.ReadAllText(packageJsonPath),
                        JsonSettings.readableWithDefault
                    );
                    if (!_packages.TryGetValue(packageData.name, out var collection)) {
                        collection = new SharedPackageInfoCollection(packageData.name);
                        _packages.Add(packageData.name, collection);
                    }

                    collection.Add(
                        packageGroupName,
                        packageFolderName,
                        packageData,
                        isTestable: Directory.Exists(Path.Combine(packageFolder, "Tests")) ||
                                    Directory.Exists(Path.Combine(packageFolder, "PlayTests"))
                    );
                }

                var packageTarballs = Directory.GetFiles(packageGroup);
                foreach (var tarball in packageTarballs) {
                    if (Path.GetExtension(tarball) != ".tgz") {
                        continue;
                    }

                    var packageChunks = Path.GetFileNameWithoutExtension(tarball).Split('@');
                    var packageId = packageChunks[0];

                    if (packageChunks.Length < 2) {
                        //TODO: Add logger. This will not show inside Unity editor, but write to Editor.log file
                        Console.WriteLine($"[WARNING] Package '{tarball}' does not have a valid version specified, please add a valid version, like com.company.sample@1.0.0.tgz");
                        continue;
                    }
                    var versionMatch = kVersionRegex.Match(packageChunks[1]);
                    if (!versionMatch.Success) {
                        //TODO: Add logger
                        Console.WriteLine($"[WARNING] Package '{tarball}' has invalid version '{packageChunks[1]}' specified, please add a valid version, like com.company.sample@1.0.0.tgz");
                        continue;
                    }
                    var version = versionMatch.Value;

                    if (!_packages.TryGetValue(packageId, out var collection)) {
                        collection = new SharedPackageInfoCollection(packageId);
                        _packages.Add(packageId, collection);
                    }

                    collection.Add(
                        packageGroupName,
                        Path.GetFileName(tarball),
                        new PackageManifestFile(
                            packageId: new PackageId(packageId, PackageType.ThirdParty),
                            description: string.Empty,
                            author: string.Empty,
                            version: version,
                            hideInEditor: true
                        ),
                        isTestable: false
                    );
                }
            }
        }

        public bool TryResolve(
            string packageNameAndVersion,
            [NotNullWhen(returnValue: true)] out SharedPackageInfo? sharedPackageInfo
        ) {

            sharedPackageInfo = null;

            var nameChunks = packageNameAndVersion.Split('@');
            if (nameChunks.Length > 2) {
                return false;
            }

            if (!_packages.TryGetValue(nameChunks[0], out var packageInfoCollection)) {
                return false;
            }

            if (nameChunks.Length == 1) {
                sharedPackageInfo = packageInfoCollection.latest;
                return true;
            }

            return packageInfoCollection.TryResolve(nameChunks[1], out sharedPackageInfo);
        }
    }
}
