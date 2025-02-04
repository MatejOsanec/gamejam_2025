using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using BGLib.JsonExtension;
using BGLib.PackagesCore.Editor;
using Newtonsoft.Json;
using NUnit.Framework;

public class PackmanManagerTests {

    private const string kReadMeFileTemplatePath = @"SharedPackages\BGLib\packages-core\Tests\TestReadmeTemplate.md";
    private static ExpectedPackageValues[] expectedPackagesValues = {
        ExpectedPackageValues.kValidBeatSaberPackage, ExpectedPackageValues.kValidBGLibPackage,
        ExpectedPackageValues.kValidThirdPartyPackage, ExpectedPackageValues.kValidBeatSaberWithHiddenAssetsPackage
    };

    private IFileSystem _fileSystem = default!;
    private PackmanManager _packmanManager = default!;
    private Random _random = default!;

    [OneTimeSetUp]
    public void OneTimeSetup() {

        _random = new Random();
    }

    [SetUp]
    public void SetUpTests() {

        _fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> {
                { ExpectedPackageValues.kPackagesPath, new MockDirectoryData() }
            }
        );
        var projectFiles = new ProjectFiles(string.Empty, _fileSystem);
        foreach (var directory in projectFiles.allFilesRootFolders) {
            if (!_fileSystem.Directory.Exists(directory)) {
                _fileSystem.Directory.CreateDirectory(directory);
            }
        }
        _packmanManager = new PackmanManager(
            projectFiles,
            Array.Empty<IPackageList>()
        );
    }

    [Test]
    public void Create_ValidatesPackageName(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        Assert.Catch(
            typeof(ArgumentException),
            () => {
                _packmanManager.Create(
                    new PackageOperationParams(null!, expectedPackage.description, expectedPackage.type)
                );
            }
        );
        Assert.Catch(
            typeof(ArgumentException),
            () => {
                _packmanManager.Create(
                    new PackageOperationParams("", expectedPackage.description, expectedPackage.type)
                );
            }
        );
        Assert.Catch(
            typeof(ArgumentException),
            () => {
                _packmanManager.Create(
                    new PackageOperationParams("  ", expectedPackage.description, expectedPackage.type)
                );
            }
        );
    }

    [Test]
    public void Validator_ChecksPackageNameHasOnlyAllowedCharacters() {

        var invalidRanges = new (char lowerBound, char upperBound)[]
            { ('\u0001', ','), ('/', '/'), (':', '`'), ('{', char.MaxValue) };
        foreach (var invalidRange in invalidRanges) {
            for (char c = invalidRange.lowerBound; c <= invalidRange.upperBound && c > 0; c++) {
                string validPackageName = expectedPackagesValues[_random.Next(expectedPackagesValues.Length)].name;
                string invalidName = validPackageName.Insert(_random.Next(validPackageName.Length), $"{c}");
                Assert.Catch(
                    typeof(ArgumentException),
                    () => { PackageValidator.ValidatePackageName(invalidName); },
                    $"This name should be invalid: {invalidName} in range [{invalidRange.lowerBound}, {invalidRange.upperBound}], char number:{(int)c}"
                );
            }
        }
    }

    [Test]
    public void Create_ValidatesDescription(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        Assert.Catch(
            typeof(ArgumentException),
            () => {
                _packmanManager.Create(new PackageOperationParams(expectedPackage.name, null!, expectedPackage.type));
            }
        );
        Assert.Catch(
            typeof(ArgumentException),
            () => { _packmanManager.Create(new PackageOperationParams(expectedPackage.name, "", expectedPackage.type)); }
        );
        Assert.Catch(
            typeof(ArgumentException),
            () => {
                _packmanManager.Create(new PackageOperationParams(expectedPackage.name, "     ", expectedPackage.type));
            }
        );
    }

    [Test]
    public void Package_HasUniqueName(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        var packageListMock = new PackageListMock();
        _packmanManager.Create(
            new PackageOperationParams(
                expectedPackage.name,
                expectedPackage.description,
                PackageType.BGLib,
                packageList: packageListMock
            )
        );
        Assert.Catch(
            typeof(InvalidOperationException),
            () => {
                _packmanManager.Create(
                    new PackageOperationParams(
                        expectedPackage.name,
                        expectedPackage.description,
                        PackageType.BGLib,
                        packageList: packageListMock
                    )
                );
            }
        );
    }

    private static string ReadFileTemplate() {

        var stream = new FileStream( kReadMeFileTemplatePath, FileMode.Open);
        using (stream) {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    [Test]
    public void Readme_MatchesTheTemplate(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        var readMeAsset = ReadFileTemplate();
        expectedPackage.Create(_packmanManager, readMeTemplate: readMeAsset);
        string packageFolderPath = _fileSystem.Path.Combine(
            ExpectedPackageValues.kPackagesPath,
            expectedPackage.packCategoryFolder,
            expectedPackage.name
        );
        string packageManifestFilePath = _fileSystem.Path.Combine(packageFolderPath, Constants.kPackageManifestFileName);
        string readMeFilePath = _fileSystem.Path.Combine(packageFolderPath, Constants.kReadMeFileName);
        AssertDirectoryExists(_fileSystem, packageFolderPath);
        AssertFileExists(_fileSystem, packageManifestFilePath);
        AssertFileExists(_fileSystem, readMeFilePath);
        using var readMeFileStream = _fileSystem.File.Open(readMeFilePath, FileMode.Open);
        using var readMeFileStreamReader = new StreamReader(readMeFileStream);
        Assert.AreEqual(expectedPackage.readmeTitle, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(expectedPackage.description, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual("## Getting started", ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual("TODO: Describe here how to start using this package", ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual("## Known issues", ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(
            "TODO: Describe here common issues that users might face while using this package",
            ReadNextLine(readMeFileStreamReader)
        );
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual("## Roadmap", ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.AreEqual(
            "TODO: Write a roadmap of features for this tool and how can users collaborate",
            ReadNextLine(readMeFileStreamReader)
        );
        Assert.AreEqual(string.Empty, ReadNextLine(readMeFileStreamReader));
        Assert.IsTrue(readMeFileStreamReader.EndOfStream, "There is more text than expected");
    }

    [Test]
    public void Manifest_IsGeneratedProperly(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager);
        string packageFolderPath = _fileSystem.Path.Combine(
            ExpectedPackageValues.kPackagesPath,
            expectedPackage.packCategoryFolder,
            expectedPackage.name
        );
        string packageManifestFilePath = _fileSystem.Path.Combine(packageFolderPath, Constants.kPackageManifestFileName);
        AssertDirectoryExists(_fileSystem, packageFolderPath);
        AssertFileExists(_fileSystem, packageManifestFilePath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.testsAssemblyPath);
        string packageManifestJson = _fileSystem.File.ReadAllText(packageManifestFilePath);
        var packageManifest = JsonConvert.DeserializeObject<PackageManifestFile>(
            packageManifestJson,
            JsonSettings.readableWithDefault
        );
        Assert.AreEqual(expectedPackage.packageId, packageManifest.name);
        Assert.AreEqual(Constants.kPackageDefaultVersion, packageManifest.version);
        Assert.AreEqual(expectedPackage.displayName, packageManifest.displayName);
        Assert.AreEqual(expectedPackage.description, packageManifest.description);
        Assert.AreEqual(expectedPackage.author, packageManifest.author.name);
        Assert.AreEqual(expectedPackage.hideAssets, packageManifest.hideInEditor);
    }

    [Test]
    public void Create_GeneratesCscFileWhenRequested(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createRuntimeAssembly: true, enableNullableAssemblies: true);
        AssertFileContent(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.runtimeFolderPath, Constants.kCompilerOptionFileName),
            Constants.kCompilerNullableOption
        );
    }

    [Test]
    public void Create_AddsPackageToPackageList(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        var (packageId, packageList) = expectedPackage.Create(
            _packmanManager,
            createRuntimeAssembly: true,
            enableNullableAssemblies: true
        );
        Assert.AreEqual(1, packageList.packageIds.Count);
        Assert.AreEqual(packageId.GetFullName(), packageList.packageIds[0]);
    }

    [Test]
    public void LinkPackageToList_AddsPackageFullNameToEndOfList(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        int listLength = _random.Next(10);
        var list = new string[listLength];
        Array.Fill(list, "any.package-id");
        var stringList = new List<string>(list);

        var (_, packageList) = expectedPackage.Create(
            _packmanManager,
            createRuntimeAssembly: true,
            enableNullableAssemblies: true,
            packageListContent: stringList
        );
        Assert.AreEqual(expectedPackage.packageId, packageList.packageIds[^1]);
    }

    [Test]
    public void Update_GeneratesCscFileWhenRequested(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager);
        var packagesInProject = _packmanManager.packageFileHandler.ListAllPackagesId().ToList();
        Assert.AreEqual(1, packagesInProject.Count);
        var updatablePackage = _packmanManager.packageFileHandler.LoadPackage(packagesInProject[0]);
        Assert.IsFalse(updatablePackage.hasTestsAssembly);
        Assert.IsFalse(updatablePackage.hasEditorAssembly);
        Assert.IsFalse(updatablePackage.hasRuntimeAssembly);
        Assert.IsFalse(updatablePackage.hasPlayTestsAssembly);
        var newUpdatablePackage = expectedPackage.Update(
            _packmanManager,
            updatablePackage,
            createRuntimeAssembly: true,
            createRuntimeTestAssembly: true,
            createEditorAssembly: true,
            createEditorTestAssembly: true,
            out bool hasMovedFiles,
            enableNullableAssemblies: true
        );
        Assert.IsFalse(hasMovedFiles);
        Assert.IsTrue(newUpdatablePackage.hasTestsAssembly);
        Assert.IsTrue(newUpdatablePackage.hasEditorAssembly);
        Assert.IsTrue(newUpdatablePackage.hasRuntimeAssembly);
        Assert.IsTrue(newUpdatablePackage.hasPlayTestsAssembly);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileContent(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.runtimeFolderPath, Constants.kCompilerOptionFileName),
            Constants.kCompilerNullableOption
        );
        AssertFileExists(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileContent(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.editorFolderPath, Constants.kCompilerOptionFileName),
            Constants.kCompilerNullableOption
        );
        AssertFileExists(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileContent(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.playTestsFolderPath, Constants.kCompilerOptionFileName),
            Constants.kCompilerNullableOption
        );
        AssertFileExists(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileContent(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.testsFolderPath, Constants.kCompilerOptionFileName),
            Constants.kCompilerNullableOption
        );
        AssertFileExists(_fileSystem, expectedPackage.testsAssemblyPath);
    }

    [Test]
    public void RuntimeAssembly_IsGeneratedProperly(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createRuntimeAssembly: true);

        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileExists(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileDoesNotExist(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.runtimeFolderPath, Constants.kCompilerOptionFileName)
        );
        AssertFileDoesNotExist(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.testsAssemblyPath);
        var assembly = _packmanManager.assemblyFileHandler.ReadFromFile(expectedPackage.runtimeAssemblyPath);
        Assert.AreEqual(expectedPackage.runtimeAssemblyName, assembly.name);
        Assert.AreEqual(expectedPackage.runtimeAssemblyName, assembly.rootNamespace);
        Assert.AreEqual(Array.Empty<string>(), assembly.references);
        Assert.AreEqual(Array.Empty<string>(), assembly.includePlatforms);
        Assert.AreEqual(Array.Empty<string>(), assembly.excludePlatforms);
        Assert.IsFalse(assembly.allowUnsafeCode);
        Assert.IsTrue(assembly.overrideReferences);
        Assert.AreEqual(Array.Empty<string>(), assembly.precompiledReferences);
        Assert.IsFalse(assembly.autoReferenced);
        Assert.AreEqual(Array.Empty<string>(), assembly.defineConstraints);
        Assert.AreEqual(Array.Empty<AssemblyDefinitionFile.VersionDefine>(), assembly.versionDefines);
        Assert.IsFalse(assembly.noEngineReferences);
    }

    [Test]
    public void EditorAssembly_IsGeneratedProperly(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createEditorAssembly: true);

        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileDoesNotExist(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.editorFolderPath, Constants.kCompilerOptionFileName)
        );
        AssertFileDoesNotExist(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.testsAssemblyPath);
        var assembly = _packmanManager.assemblyFileHandler.ReadFromFile(expectedPackage.editorAssemblyPath);
        Assert.AreEqual(expectedPackage.editorAssemblyName, assembly.name);
        Assert.AreEqual(expectedPackage.editorAssemblyName, assembly.rootNamespace);
        Assert.AreEqual(Array.Empty<string>(), assembly.references);
        Assert.AreEqual(new[] { "Editor" }, assembly.includePlatforms);
        Assert.AreEqual(Array.Empty<string>(), assembly.excludePlatforms);
        Assert.IsFalse(assembly.allowUnsafeCode);
        Assert.IsTrue(assembly.overrideReferences);
        Assert.AreEqual(Array.Empty<string>(), assembly.precompiledReferences);
        Assert.IsFalse(assembly.autoReferenced);
        Assert.AreEqual(Array.Empty<string>(), assembly.defineConstraints);
        Assert.AreEqual(Array.Empty<AssemblyDefinitionFile.VersionDefine>(), assembly.versionDefines);
        Assert.IsFalse(assembly.noEngineReferences);
    }

    [Test]
    public void PlayTestAssembly_IsGeneratedProperly(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createRuntimeTestAssembly: true);

        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileDoesNotExist(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.playTestsFolderPath, Constants.kCompilerOptionFileName)
        );
        AssertFileDoesNotExist(_fileSystem, expectedPackage.testsAssemblyPath);
        var assembly = _packmanManager.assemblyFileHandler.ReadFromFile(expectedPackage.playTestsAssemblyPath);
        Assert.AreEqual(expectedPackage.playTestsAssemblyName, assembly.name);
        Assert.AreEqual(string.Empty, assembly.rootNamespace);
        Assert.AreEqual(new[] { "UnityEditor.TestRunner", "UnityEngine.TestRunner" }, assembly.references);
        Assert.AreEqual(Array.Empty<string>(), assembly.includePlatforms);
        Assert.AreEqual(Array.Empty<string>(), assembly.excludePlatforms);
        Assert.IsFalse(assembly.allowUnsafeCode);
        Assert.IsTrue(assembly.overrideReferences);
        Assert.AreEqual(new[] { "nunit.framework.dll" }, assembly.precompiledReferences);
        Assert.IsFalse(assembly.autoReferenced);
        Assert.AreEqual(new[] { "UNITY_INCLUDE_TESTS" }, assembly.defineConstraints);
        Assert.AreEqual(Array.Empty<AssemblyDefinitionFile.VersionDefine>(), assembly.versionDefines);
        Assert.IsFalse(assembly.noEngineReferences);
    }

    [Test]
    public void TestAssembly_IsGeneratedProperly(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createEditorTestAssembly: true);

        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.testsAssemblyPath);
        AssertFileDoesNotExist(
            _fileSystem,
            _fileSystem.Path.Combine(expectedPackage.testsFolderPath, Constants.kCompilerOptionFileName)
        );
        var assembly = _packmanManager.assemblyFileHandler.ReadFromFile(expectedPackage.testsAssemblyPath);
        Assert.AreEqual(expectedPackage.testsAssemblyName, assembly.name);
        Assert.AreEqual(string.Empty, assembly.rootNamespace);
        Assert.AreEqual(new[] { "UnityEditor.TestRunner", "UnityEngine.TestRunner" }, assembly.references);
        Assert.AreEqual(new[] { "Editor" }, assembly.includePlatforms);
        Assert.AreEqual(Array.Empty<string>(), assembly.excludePlatforms);
        Assert.IsFalse(assembly.allowUnsafeCode);
        Assert.IsTrue(assembly.overrideReferences);
        Assert.AreEqual(new[] { "nunit.framework.dll" }, assembly.precompiledReferences);
        Assert.IsFalse(assembly.autoReferenced);
        Assert.AreEqual(new[] { "UNITY_INCLUDE_TESTS" }, assembly.defineConstraints);
        Assert.AreEqual(Array.Empty<AssemblyDefinitionFile.VersionDefine>(), assembly.versionDefines);
        Assert.IsFalse(assembly.noEngineReferences);
    }

    [Test]
    public void PackageFileHandler_CanLoadUpdatablePackage(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager, createEditorTestAssembly: true);

        var packagesInProject = _packmanManager.packageFileHandler.ListAllPackagesId().ToList();
        Assert.AreEqual(1, packagesInProject.Count);
        var updatablePackage = _packmanManager.packageFileHandler.LoadPackage(packagesInProject[0]);
        Assert.AreEqual(expectedPackage.name, updatablePackage.packageName);
        Assert.AreEqual(expectedPackage.description, updatablePackage.manifest.description);
        Assert.AreEqual(expectedPackage.type, updatablePackage.type);
        Assert.IsTrue(updatablePackage.hasTestsAssembly);
        Assert.IsFalse(updatablePackage.hasEditorAssembly);
        Assert.IsFalse(updatablePackage.hasRuntimeAssembly);
        Assert.IsFalse(updatablePackage.hasPlayTestsAssembly);
    }

    [Test]
    public void Update_CanAddAssemblies(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        expectedPackage.Create(_packmanManager);
        var packagesInProject = _packmanManager.packageFileHandler.ListAllPackagesId().ToList();
        Assert.AreEqual(1, packagesInProject.Count);
        var updatablePackage = _packmanManager.packageFileHandler.LoadPackage(packagesInProject[0]);
        Assert.IsFalse(updatablePackage.hasTestsAssembly);
        Assert.IsFalse(updatablePackage.hasEditorAssembly);
        Assert.IsFalse(updatablePackage.hasRuntimeAssembly);
        Assert.IsFalse(updatablePackage.hasPlayTestsAssembly);
        var newUpdatablePackage = expectedPackage.Update(
            _packmanManager,
            updatablePackage,
            createRuntimeAssembly: true,
            createRuntimeTestAssembly: true,
            createEditorAssembly: true,
            createEditorTestAssembly: true,
            out bool hasMovedFiles
        );
        Assert.IsFalse(hasMovedFiles);
        Assert.IsTrue(newUpdatablePackage.hasTestsAssembly);
        Assert.IsTrue(newUpdatablePackage.hasEditorAssembly);
        Assert.IsTrue(newUpdatablePackage.hasRuntimeAssembly);
        Assert.IsTrue(newUpdatablePackage.hasPlayTestsAssembly);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileExists(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.testsAssemblyPath);
    }

    [Test]
    public void Update_MovesFilesWhenChangePackageName(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        var readMeAsset = ReadFileTemplate();
        string readMeFilePath = _fileSystem.Path.Combine(expectedPackage.path, Constants.kReadMeFileName);
        const string packageOriginalName = "some-name";
        const string packageOriginalDescription = "Some name description";
        const PackageType packageOriginalType = PackageType.BGLib;
        _packmanManager.Create(
            new PackageOperationParams(
                packageOriginalName,
                packageOriginalDescription,
                packageOriginalType,
                createRuntimeAssembly: true,
                createPlayTestsAssembly: true,
                createEditorAssembly: true,
                createTestsAssembly: true,
                readMeTemplate: readMeAsset,
                packageList: new PackageListMock()
            )
        );
        var packagesInProject = _packmanManager.packageFileHandler.ListAllPackagesId().ToList();
        Assert.AreEqual(1, packagesInProject.Count);
        var updatablePackage = _packmanManager.packageFileHandler.LoadPackage(packagesInProject[0]);
        var newUpdatablePackage = expectedPackage.Update(
            _packmanManager,
            updatablePackage,
            createRuntimeAssembly: false,
            createRuntimeTestAssembly: false,
            createEditorAssembly: false,
            createEditorTestAssembly: false,
            out bool hasMovedFiles
        );
        Assert.AreEqual(packageOriginalName, updatablePackage.packageName);
        Assert.AreEqual(packageOriginalDescription, updatablePackage.manifest.description);
        Assert.AreEqual(packageOriginalType, updatablePackage.type);
        Assert.IsTrue(updatablePackage.hasTestsAssembly);
        Assert.IsTrue(updatablePackage.hasEditorAssembly);
        Assert.IsTrue(updatablePackage.hasRuntimeAssembly);
        Assert.IsTrue(updatablePackage.hasPlayTestsAssembly);
        Assert.IsTrue(hasMovedFiles);
        Assert.IsTrue(newUpdatablePackage.hasTestsAssembly);
        Assert.IsTrue(newUpdatablePackage.hasEditorAssembly);
        Assert.IsTrue(newUpdatablePackage.hasRuntimeAssembly);
        Assert.IsTrue(newUpdatablePackage.hasPlayTestsAssembly);
        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, readMeFilePath);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileExists(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileExists(_fileSystem, expectedPackage.testsAssemblyPath);
    }

    [Test]
    public void Update_ChangeReferencesInAssemblyDefinitions(
        [ValueSource(nameof(expectedPackagesValues))] ExpectedPackageValues expectedPackage
    ) {

        const string packAId = "pack-a";
        const string packBId = "pack-b";
        var packageListMock = new PackageListMock();
        var (_, packageAId) = _packmanManager.Create(
            new PackageOperationParams(
                packAId,
                "Some name description",
                PackageType.ThirdParty,
                createRuntimeAssembly: true,
                author: "SomeThirdParty",
                packageList: packageListMock
            )
        );
        var (_, packageBId) = _packmanManager.Create(
            new PackageOperationParams(
                packBId,
                "Some other description",
                PackageType.BGLib,
                createRuntimeAssembly: true,
                packageList: packageListMock
            )
        );
        Assert.AreEqual(2, packageListMock.packageIds.Count);
        Assert.AreEqual(packageAId.GetFullName(), packageListMock.packageIds[0]);
        Assert.AreEqual(packageBId.GetFullName(), packageListMock.packageIds[1]);
        var packagesInProject = _packmanManager.packageFileHandler.ListAllPackagesId().ToList();
        Assert.AreEqual(2, packagesInProject.Count);
        var packAName = packagesInProject.FirstOrDefault(
            p => PackageFileHandler.ExtractPackageInfoFromPackageId(p).folderName == packAId
        );
        NullableAssert.IsNotNull(packAName);
        var packA = _packmanManager.packageFileHandler.LoadPackage(packAName);
        var packBName = packagesInProject.FirstOrDefault(
            p => PackageFileHandler.ExtractPackageInfoFromPackageId(p).folderName == packBId
        );
        NullableAssert.IsNotNull(packBName);
        var packB = _packmanManager.packageFileHandler.LoadPackage(packBName);
        Assert.AreEqual(packAId, packA.packageName);
        Assert.AreEqual(packBId, packB.packageName);
        var packARuntimeAssembly = ReadRuntimeAssemblyFromPackage(_packmanManager, packA);
        string packBRuntimeAssemblyName = packB.packageId.GetRuntimeAssemblyName();
        var packARuntimeAssemblyWithReference =
            packARuntimeAssembly.Update(newReferences: new[] { packBRuntimeAssemblyName });
        _packmanManager.assemblyFileHandler.WritePackageRuntimeAssembly(packA, packARuntimeAssemblyWithReference);

        var updatedPackB = expectedPackage.Update(
            _packmanManager,
            packB,
            createRuntimeAssembly: false,
            createRuntimeTestAssembly: false,
            createEditorAssembly: false,
            createEditorTestAssembly: false,
            out bool hasMovedFiles
        );

        var packARuntimeAssemblyAfterUpdate = ReadRuntimeAssemblyFromPackage(_packmanManager, packA);
        var packBRuntimeAssemblyAfterUpdate = ReadRuntimeAssemblyFromPackage(_packmanManager, updatedPackB);
        var packBRuntimeAssemblyNameAfterUpdate = updatedPackB.packageId.GetRuntimeAssemblyName();
        Assert.IsFalse(updatedPackB.hasTestsAssembly);
        Assert.IsFalse(updatedPackB.hasEditorAssembly);
        Assert.IsTrue(updatedPackB.hasRuntimeAssembly);
        Assert.IsFalse(updatedPackB.hasPlayTestsAssembly);
        Assert.IsTrue(hasMovedFiles);
        AssertDirectoryExists(_fileSystem, expectedPackage.path);
        AssertFileExists(_fileSystem, expectedPackage.manifestPath);
        AssertFileExists(_fileSystem, expectedPackage.runtimeAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.editorAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.playTestsAssemblyPath);
        AssertFileDoesNotExist(_fileSystem, expectedPackage.testsAssemblyPath);
        Assert.AreEqual("BGLib.PackB", packBRuntimeAssemblyName);
        Assert.AreEqual(expectedPackage.runtimeAssemblyName, packBRuntimeAssemblyNameAfterUpdate);
        Assert.AreEqual(expectedPackage.runtimeAssemblyName, packBRuntimeAssemblyAfterUpdate.name);
        Assert.AreEqual(expectedPackage.runtimeAssemblyName, packBRuntimeAssemblyAfterUpdate.rootNamespace);
        Assert.IsNotNull(packARuntimeAssemblyAfterUpdate.references);
        Assert.AreEqual(1, packARuntimeAssemblyAfterUpdate.references.Count);
        Assert.AreEqual(packBRuntimeAssemblyNameAfterUpdate, packARuntimeAssemblyAfterUpdate.references[0]);
    }

    private static AssemblyDefinitionFile ReadRuntimeAssemblyFromPackage(
        PackmanManager packmanManager,
        UpdatablePackage package
    ) {

        var nullableRuntimeAssembly = packmanManager.assemblyFileHandler.ReadPackageRuntimeAssembly(package);
        NullableAssert.IsNotNull(nullableRuntimeAssembly);
        return nullableRuntimeAssembly;
    }

    private static void AssertFileContent(IFileSystem fileSystem, string path, string content) {

        AssertFileExists(fileSystem, path);
        string readContent = fileSystem.File.ReadAllText(path);
        Assert.AreEqual(content, readContent);
    }

    private static void AssertFileExists(IFileSystem fileSystem, string path) {

        if (!fileSystem.File.Exists(path)) {
            Assert.Fail($"File was not found at path: {path}");
        }
    }

    private static void AssertFileDoesNotExist(IFileSystem fileSystem, string path) {

        if (fileSystem.File.Exists(path)) {
            Assert.Fail($"File should not exist at path: {path}");
        }
    }

    private static void AssertDirectoryExists(IFileSystem fileSystem, string path) {

        if (!fileSystem.Directory.Exists(path)) {
            Assert.Fail($"Directory was not found at path: {path}");
        }
    }

    private static string ReadNextLine(TextReader reader) {

        return reader.ReadLine()?.Trim() ?? string.Empty;
    }

}
