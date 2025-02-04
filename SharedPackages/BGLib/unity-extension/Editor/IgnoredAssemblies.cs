namespace BGLib.UnityExtension.Editor {

using System;
using System.Collections.Generic;
using System.Linq;

public class IgnoredAssemblies {

    private readonly HashSet<string> _ignoredAssemblies;
    private readonly IReadOnlyList<string> _ignoredAssembliesPrefixes;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    // We might want to initialize those data structure differently based on the constructor of this class
    public IgnoredAssemblies() {

        _ignoredAssemblies = new HashSet<string> {
            "Oculus.VR",
            "OculusPlatform",
            "FinalIK",
            "LIV",
            "Cinemachine",
            "Zenject",
            "Steamworks.NET",
            "LiteNetLib",
            "Oculus.Platform",
            "JsonSubtype",
            "Tayx.Graphy",
            "Oculus.AvatarSDK2",
            "FBxPlat",
            "UIToolkitUtilities",
            "Oculus.AvatarSDK2",
            "Oculus.Haptics",
            "OvrMetrics",
            "mscorlib",
            "BGLib.HierarchyIcons",// Package is set up as independent from beat code, cant add annotations.
            "BGLib.HierarchyIcons.Editor"
        };
        _ignoredAssembliesPrefixes = new List<string>() {
            "Unity.",
            "UnityEngine",
            "Meta.XR"
        };
    }

    public bool IsIgnored(string assemblyName) {

        return _ignoredAssemblies.Contains(assemblyName) || _ignoredAssembliesPrefixes.Any(assemblyName.StartsWith);
    }

    public bool IsIgnoredType(Type type) {

        // I believe we do that because using the file name is faster than trying to access Assembly.GetName().Name
        string assemblyNameFromFilePath = System.IO.Path.GetFileNameWithoutExtension(type.Assembly.CodeBase);
        return IsIgnored(assemblyNameFromFilePath);
    }


}

}
