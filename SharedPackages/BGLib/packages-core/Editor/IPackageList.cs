using System.Collections.Generic;

namespace BGLib.PackagesCore.Editor {

    public interface IPackageList {

        public IReadOnlyList<string> packageIds { get; }
        void AddPackage(string newPackageId);
        bool ReplacePackage(PackageId oldId, PackageId newId);
    }
}
