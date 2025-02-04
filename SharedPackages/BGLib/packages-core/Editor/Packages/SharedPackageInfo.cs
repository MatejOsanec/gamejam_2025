namespace BGLib.PackagesCore.Editor.Packages {

    using System;

    public class SharedPackageInfo {

        private readonly SharedPackageInfoCollection _collection;
        private readonly string _version;

        public readonly string path;
        public readonly string menuPath;
        public readonly bool isInternal;
        public readonly bool isTestable;
        public readonly Version version;

        public string id => _collection.packageId;

        public string referenceName => _collection.versions.Count > 1
            ? $"{_collection.packageId}@{_version}"
            : _collection.packageId;

        internal SharedPackageInfo(
            SharedPackageInfoCollection collection,
            string packageGroupName,
            string packageFolderName,
            bool isInternal,
            bool isTestable,
            string version
        ) {
            _version = version;
            _collection = collection;

            menuPath = isInternal
                ? $"{packageGroupName}/{packageFolderName.Replace('.', '/')}"
                : $"{packageGroupName}/{packageFolderName.Replace('@', '/').Replace(".tgz", "")}";
            path = $"{Constants.kLocalPackagePathPrefix}/{packageGroupName}/{packageFolderName}";

            this.isInternal = isInternal;
            this.isTestable = isTestable;
            this.version = new Version(version);
        }
    }
}
