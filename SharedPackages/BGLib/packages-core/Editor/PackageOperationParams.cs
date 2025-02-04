namespace BGLib.PackagesCore.Editor {

    public readonly struct PackageOperationParams {

        public readonly string name;
        public readonly string description;
        public readonly PackageType type;
        public readonly bool createRuntimeAssembly;
        public readonly bool createPlayTestsAssembly;
        public readonly bool createEditorAssembly;
        public readonly bool createTestsAssembly;
        public readonly string? readMeTemplate;
        public readonly IPackageList? packageList;
        public readonly string author;
        public readonly bool nullableAssemblies;
        public readonly bool hideAssets;

        public PackageOperationParams(
            string name,
            string description,
            PackageType type,
            bool createRuntimeAssembly = false,
            bool createPlayTestsAssembly = false,
            bool createEditorAssembly = false,
            bool createTestsAssembly = false,
            bool nullableAssemblies = true,
            bool hideAssets = true,
            string? author = null,
            string? readMeTemplate = null,
            IPackageList? packageList = null
        ) {
            this.name = name;
            this.description = description;
            this.type = type;
            this.createRuntimeAssembly = createRuntimeAssembly;
            this.createPlayTestsAssembly = createPlayTestsAssembly;
            this.createEditorAssembly = createEditorAssembly;
            this.createTestsAssembly = createTestsAssembly;
            this.readMeTemplate = readMeTemplate;
            this.author = author ?? string.Empty;
            this.nullableAssemblies = nullableAssemblies;
            this.packageList = packageList;
            this.hideAssets = hideAssets;
        }
    }
}
