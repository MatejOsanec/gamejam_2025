using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

public static class FileSystemHelper {

    public static string FindFirstExistedParentPath(string path) {

        while (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path)) {
            path = Path.GetDirectoryName(path);
        }
        return path;
    }

    private static bool HasWritePermission(FileSystemSecurity accessControlList) {

        if (accessControlList == null) {
            return false;
        }

        var writeAllow = false;
        var writeDeny = false;

        var accessRules = accessControlList.GetAccessRules(true, true, typeof(SecurityIdentifier));
        foreach (FileSystemAccessRule rule in accessRules) {
            if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write) {
                continue;
            }

            if (rule.AccessControlType == AccessControlType.Allow) {
                writeAllow = true;
            }
            else if (rule.AccessControlType == AccessControlType.Deny) {
                writeDeny = true;
            }
        }

        return writeAllow && !writeDeny;
    }

    public static bool HasWritePermissionOnDirectory(string path) {

        return HasWritePermission(Directory.GetAccessControl(path));
    }

    public static bool HasWritePermissionOnFile(string path) {

        return HasWritePermission(File.GetAccessControl(path));
    }

    public static bool IsFileWritable(string path) {

        return !new FileInfo(path).Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}
