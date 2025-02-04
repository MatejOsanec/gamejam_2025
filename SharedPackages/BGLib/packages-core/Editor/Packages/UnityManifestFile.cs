namespace BGLib.PackagesCore.Editor.Packages {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    [Serializable]
    public sealed class ScopedRegistry {

        public string? name;
        public string? url;
        public List<string>? scopes;
    }

    [Serializable]
    public class UnityManifestFile {

        public Dictionary<string, string> dependencies = null!;
        public List<ScopedRegistry> scopedRegistries = null!;
        public List<string> testables = null!;
    }
}
