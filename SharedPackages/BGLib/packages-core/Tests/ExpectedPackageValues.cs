using System.Collections.Generic;
using System.IO;
using System.Linq;
using BGLib.PackagesCore.Editor;

public class ExpectedPackageValues {

    public static readonly string kPackagesPath = Constants.kPackagesFolder + Path.DirectorySeparatorChar;

    public readonly string packageId;
    public readonly string name;
    public readonly string description;
    public readonly PackageType type;
    public readonly string packCategoryFolder;
    public readonly string path;
    public readonly string manifestPath;
    public readonly string runtimeAssemblyName;
    public readonly string runtimeFolderPath;
    public readonly string runtimeAssemblyPath;
    public readonly string editorAssemblyName;
    public readonly string editorFolderPath;
    public readonly string editorAssemblyPath;
    public readonly string playTestsAssemblyName;
    public readonly string playTestsFolderPath;
    public readonly string playTestsAssemblyPath;
    public readonly string testsAssemblyName;
    public readonly string testsFolderPath;
    public readonly string testsAssemblyPath;
    public readonly string displayName;
    public readonly string readmeTitle;
    public readonly string author;
    public readonly bool hideAssets;

    public static readonly ExpectedPackageValues kValidBeatSaberPackage = new ExpectedPackageValues(
        packageId: "com.beatgames.beatsaber.valid.package-name",
        name: "valid.package-name",
        description: "A valid description for the Valid package for test that you should not use the name.",
        type: PackageType.BeatSaber,
        packCategoryFolder: "BeatSaber",
        runtimeAssemblyName: "BeatSaber.Valid.PackageName",
        editorAssemblyName: "BeatSaber.Valid.PackageName.Editor",
        playTestsAssemblyName: "BeatSaber.Valid.PackageName.PlayTests",
        testsAssemblyName: "BeatSaber.Valid.PackageName.Tests",
        hideAssets: false,
        displayName: "_/Valid/PackageName",
        readmeTitle: "# Valid | Package Name"
    );

    public static readonly ExpectedPackageValues kValidBeatSaberWithHiddenAssetsPackage = new ExpectedPackageValues(
        packageId: "com.beatgames.beatsaber.valid.package-name",
        name: "valid.package-name",
        description: "A valid description for the Valid package for test that you should not use the name.",
        type: PackageType.BeatSaber,
        packCategoryFolder: "BeatSaber",
        runtimeAssemblyName: "BeatSaber.Valid.PackageName",
        editorAssemblyName: "BeatSaber.Valid.PackageName.Editor",
        playTestsAssemblyName: "BeatSaber.Valid.PackageName.PlayTests",
        testsAssemblyName: "BeatSaber.Valid.PackageName.Tests",
        hideAssets: true,
        displayName: "_/Valid/PackageName",
        readmeTitle: "# Valid | Package Name"
    );
    public static readonly ExpectedPackageValues kValidBGLibPackage = new ExpectedPackageValues(
        packageId: "com.beatgames.bglib.useful-library",
        name: "useful-library",
        description: "A valid description for an useful library.",
        type: PackageType.BGLib,
        packCategoryFolder: "BGLib",
        runtimeAssemblyName: "BGLib.UsefulLibrary",
        editorAssemblyName: "BGLib.UsefulLibrary.Editor",
        playTestsAssemblyName: "BGLib.UsefulLibrary.PlayTests",
        testsAssemblyName: "BGLib.UsefulLibrary.Tests",
        hideAssets: false,
        displayName: "_Lib/UsefulLibrary",
        readmeTitle: "# Useful Library"
    );
    public static readonly ExpectedPackageValues kValidThirdPartyPackage = new ExpectedPackageValues(
        packageId: "com.meta.oculus.awesome-sdk",
        name: "com.meta.oculus.awesome-sdk",
        description: "A awesome sdk by Oculus.",
        type: PackageType.ThirdParty,
        packCategoryFolder: "ThirdParty",
        runtimeAssemblyName: "Com.Meta.Oculus.AwesomeSdk",
        editorAssemblyName: "Com.Meta.Oculus.AwesomeSdk.Editor",
        playTestsAssemblyName: "Com.Meta.Oculus.AwesomeSdk.PlayTests",
        testsAssemblyName: "Com.Meta.Oculus.AwesomeSdk.Tests",
        hideAssets: false,
        displayName: "AwesomeSdk",
        readmeTitle: "# Awesome Sdk",
        author: "Oculus"
    );

    private ExpectedPackageValues(
        string packageId,
        string name,
        string description,
        PackageType type,
        string packCategoryFolder,
        string runtimeAssemblyName,
        string editorAssemblyName,
        string playTestsAssemblyName,
        string testsAssemblyName,
        bool hideAssets,
        string displayName,
        string readmeTitle,
        string? author = null
    ) {

        this.packageId = packageId;
        this.name = name;
        this.description = description;
        this.type = type;
        this.packCategoryFolder = packCategoryFolder;
        path = Path.Combine(kPackagesPath, type.ToString(), name);
        manifestPath = Path.Combine(path, Constants.kPackageManifestFileName);
        this.runtimeAssemblyName = runtimeAssemblyName;
        runtimeFolderPath = Path.Combine(path, "Runtime");
        runtimeAssemblyPath = Path.Combine(
            runtimeFolderPath,
            $"{runtimeAssemblyName}.{Constants.kAssemblyDefinitionExtension}"
        );
        this.editorAssemblyName = editorAssemblyName;
        editorFolderPath = Path.Combine(path, "Editor");
        editorAssemblyPath = Path.Combine(
            editorFolderPath,
            $"{editorAssemblyName}.{Constants.kAssemblyDefinitionExtension}"
        );
        this.playTestsAssemblyName = playTestsAssemblyName;
        playTestsFolderPath = Path.Combine(path, "PlayTests");
        playTestsAssemblyPath = Path.Combine(
            playTestsFolderPath,
            $"{playTestsAssemblyName}.{Constants.kAssemblyDefinitionExtension}"
        );
        this.testsAssemblyName = testsAssemblyName;
        testsFolderPath = Path.Combine(path, "Tests");
        testsAssemblyPath = Path.Combine(
            testsFolderPath,
            $"{testsAssemblyName}.{Constants.kAssemblyDefinitionExtension}"
        );
        this.displayName = displayName;
        this.readmeTitle = readmeTitle;
        this.author = author ?? Constants.kAuthorName;
        this.hideAssets = hideAssets;
    }

    public (PackageId packageId, IPackageList packageList) Create(
        PackmanManager packmanManager,
        bool createRuntimeAssembly = false,
        bool createRuntimeTestAssembly = false,
        bool createEditorAssembly = false,
        bool createEditorTestAssembly = false,
        bool enableNullableAssemblies = false,
        string? readMeTemplate = null,
        IEnumerable<string>? packageListContent = null
    ) {

        packageListContent ??= Enumerable.Empty<string>();
        var packageList = new PackageListMock(packageListContent);
        var createResult = packmanManager.Create(
            new PackageOperationParams(
                name,
                description,
                type,
                createRuntimeAssembly,
                createRuntimeTestAssembly,
                createEditorAssembly,
                createEditorTestAssembly,
                enableNullableAssemblies,
                hideAssets,
                author,
                readMeTemplate,
                packageList
            )
        );
        return (createResult.packageId, packageList);
    }

    internal UpdatablePackage Update(
        PackmanManager packmanManager,
        UpdatablePackage updatablePackage,
        bool createRuntimeAssembly,
        bool createRuntimeTestAssembly,
        bool createEditorAssembly,
        bool createEditorTestAssembly,
        out bool hasMovedFiles,
        bool enableNullableAssemblies = false
    ) {

        return packmanManager.Update(
            updatablePackage,
            new PackageOperationParams(
                name,
                description,
                type,
                createRuntimeAssembly,
                createRuntimeTestAssembly,
                createEditorAssembly,
                createEditorTestAssembly,
                enableNullableAssemblies,
                hideAssets,
                author
            ),
            out hasMovedFiles
        );
    }
}
