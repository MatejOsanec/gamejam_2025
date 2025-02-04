using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BGLib.PackagesCore.Editor;
using Newtonsoft.Json;
using NUnit.Framework;

public class AssetValidationTests {

    private ProjectFiles _projectFiles = default!;
    private AssemblyDefinitionFileHandler _assemblyFileHandler = default!;

    [SetUp]
    public void SetUpTests() {

        _projectFiles = ProjectFiles.Default;
        _assemblyFileHandler = new AssemblyDefinitionFileHandler(_projectFiles);
    }

    [Test]
    public void LoadedAssemblies_AreValid() {

        var assemblies = _assemblyFileHandler.SearchAllFilesPath().ToArray();
        Assert.That(
            assemblies.Length,
            Is.GreaterThan(10),
            "Number of assemblies failed sanity check that the project has more than 10 assemblies."
        );
        foreach (var assemblyFilePath in assemblies) {
            var assemblyDefinitionFile = _assemblyFileHandler.ReadFromFile(assemblyFilePath);
            Assert.IsNotNull(
                assemblyDefinitionFile.references,
                GetNullFieldMessage(assemblyDefinitionFile, "references")
            );
            Assert.IsNotNull(
                assemblyDefinitionFile.versionDefines,
                GetNullFieldMessage(assemblyDefinitionFile, "versionDefines")
            );
            Assert.IsNotNull(
                assemblyDefinitionFile.precompiledReferences,
                GetNullFieldMessage(assemblyDefinitionFile, "precompiledReferences")
            );
            Assert.IsNotNull(
                assemblyDefinitionFile.defineConstraints,
                GetNullFieldMessage(assemblyDefinitionFile, "defineConstraints")
            );
            Assert.IsNotNull(
                assemblyDefinitionFile.excludePlatforms,
                GetNullFieldMessage(assemblyDefinitionFile, "excludePlatforms")
            );
            Assert.IsNotNull(
                assemblyDefinitionFile.includePlatforms,
                GetNullFieldMessage(assemblyDefinitionFile, "includePlatforms")
            );
        }
    }

    private static string GetNullFieldMessage(AssemblyDefinitionFile assemblyDefinitionFile, string fieldName) {
        return $"Assembly with name '{assemblyDefinitionFile.name}' has null value on the field: {fieldName}";
    }

    private static readonly HashSet<string> _referencedAssembliesAllowlist = new() {
        "Unity.Addressables",
        "Unity.Addressables.Editor",
        "UnityEditor.UI",
        "UnityEngine.UI",
        "DataModels",
        "Main"
    };

    [Test]
    public void AssemblyDefinitionReference_IsOnlyUsedForAllowlistedPackages() {

        var assemblyReferences = _assemblyFileHandler.SearchAllReferenceFilesPath().ToList();
        foreach (var filePath in assemblyReferences) {

            var localPath = Path.GetRelativePath(ProjectFiles.Default.projectPath, filePath);

            var assemblyReferenceFile = JsonFileHandlerForIFileSystem.ReadFromFile<AssemblyDefinitionReferenceFile>(_projectFiles.fileSystem, filePath);

            if (assemblyReferenceFile.reference == null) {
                Assert.Fail($"Assembly file with empty reference found: {filePath}");
                continue;
            }

            string packageName = assemblyReferenceFile.reference;
            if (assemblyReferenceFile.reference.StartsWith("GUID")) {
                Assert.Fail($"Assembly reference using GUID found: {filePath}. Using guid is not allowed in this project, " +
                            $"in order to fix either uncheck and apply 'use guid' box in assembly reference or remove it completely if it's " +
                            $"not being used.");
                continue;
            }

            if (!_referencedAssembliesAllowlist.Contains(packageName)) {
                Assert.Fail($"Package {packageName} is not allowlisted but contains assembly reference. File can be found at: {localPath}");
            }
        }
    }
}
