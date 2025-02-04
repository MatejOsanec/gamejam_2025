namespace BGLib.PackagesCore.Editor.Packages {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class SharedPackageInfoCollection {

        private readonly List<SharedPackageInfo> _versions = new();

        public readonly string packageId;

        public IReadOnlyList<SharedPackageInfo> versions => _versions;
        public SharedPackageInfo latest => _versions[0];

        public SharedPackageInfoCollection(string packageId) {

            this.packageId = packageId;
        }

        internal void Add(
            string packageGroupName,
            string packageFolderName,
            PackageManifestFile packageData,
            bool isTestable
        ) {

            var sharedPackageInfo = new SharedPackageInfo(
                this,
                packageGroupName,
                packageFolderName,
                packageData.name.StartsWith(Constants.kInternalPackagesPrefix),
                isTestable,
                packageData.version
            );

            _versions.Add(sharedPackageInfo);
            _versions.Sort((a, b) => b.version.CompareTo(a.version));
        }

        public bool TryResolve(
            string version,
            [NotNullWhen(returnValue: true)] out SharedPackageInfo? sharedPackageInfo
        ) {

            if (version == Constants.kLatestVersion) {
                sharedPackageInfo = latest;
                return true;
            }
            
            if (!Version.TryParse(version, out var systemVersion)) {
                sharedPackageInfo = null;
                return false;
            }
            
            sharedPackageInfo = _versions.Find(x => x.version.Equals(systemVersion));
            return sharedPackageInfo != null;
        }
    }
}
