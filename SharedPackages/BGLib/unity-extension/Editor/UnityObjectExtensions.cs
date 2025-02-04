using UnityEngine;
using UnityEditor;
using System.IO;

public static class UnityObjectExtensions {

    public static string GetHierarchyPath(this Transform transform) {

        var path = transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }

    public static string GetHierarchyPath(this GameObject gameObject)
        => gameObject.transform.GetHierarchyPath();

    /// <summary>
    /// Gets an AssetDatabase-formatted asset path of this Object <br/>
    /// EG: Packages/com.unity.images-library/Example/Images/image.png
    /// </summary>
    public static string GetAssetPath(this Object obj)
        => AssetDatabase.GetAssetPath(obj);

    /// <summary>
    /// Gets the full path on disk of an asset <br/>
    /// EG: C:\open\beat-saber\SharedPackages\BGLib\unity-extension\bever.png
    /// </summary>
    public static string GetAbsoluteAssetPath(this Object obj)
        => Path.GetFullPath(AssetDatabase.GetAssetPath(obj));

    /// <summary>
    /// Gets an AssetDatabase GUID of this object
    /// </summary>
    public static GUID GetAssetGUID(this Object obj)
        => AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(obj));
}
