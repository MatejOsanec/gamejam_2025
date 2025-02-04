using System.Collections.Generic;
using System.Linq;
using BGLib.PackagesCore.Editor;

public class PackageListMock : IPackageList {

    private readonly List<string> _packagesIds;

    public PackageListMock() : this(Enumerable.Empty<string>()) { }

    public PackageListMock(IEnumerable<string> newPackageIds) {

        _packagesIds = new List<string>(newPackageIds);
    }

    public IReadOnlyList<string> packageIds => _packagesIds.ToList();

    public void AddPackage(string newPackageId) {
        _packagesIds.Add(newPackageId);
    }

    public bool ReplacePackage(PackageId oldId, PackageId newId) {

        if (!_packagesIds.Remove(oldId.GetFullName())) {
            return false;
        }
        var newValue = oldId.GetFullName();
        if (_packagesIds.Contains(newValue)) {
            return false;
        }
        _packagesIds.Add(newValue);
        return true;
    }
}
