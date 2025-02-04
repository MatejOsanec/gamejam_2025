using System;
using System.IO;
using System.Linq;
using BGLib.DotnetExtension.CommandLine;
using NUnit.Framework;
using UnityEngine;

public class CommandLineParserTests {

    private readonly ArgumentOption platformRequired = new ArgumentOption(
        name: "Platform Required",
        hint: string.Empty,
        ArgumentType.String,
        "-platform"
    );

    private readonly ArgumentOption workspaceOptional = new ArgumentOption(
        name: "Workspace Optional",
        hint: string.Empty,
        ArgumentType.StringOptional,
        "-workspace"
    );

    private readonly ArgumentOption codeRequired = new ArgumentOption(
        name: "Code Required",
        hint: string.Empty,
        ArgumentType.Integer,
        "code"
    );

    private readonly ArgumentOption testBoolean = new ArgumentOption(
        name: "Test",
        hint: string.Empty,
        ArgumentType.Boolean,
        "-test"
    );

    private string _programPath;

    [OneTimeSetUp]
    public void PrepareProgramPath() {

        _programPath = Application.persistentDataPath + "Program.exe";
        File.WriteAllText(_programPath, "program code here.");
    }

    [OneTimeTearDown]
    public void DeleteProgramFile() {

        File.Delete(_programPath);
    }

    [Test]
    public void OverlappingIdentifiers_ThrowsInvalidOperationException() {

        var args = new[] {
            _programPath
        };

        Assert.That(
            () => CommandLineParser.ParseCommandLine(
                args,
                new ArgumentOption(name: "Argument one", hint: string.Empty, ArgumentType.String, "-something"),
                new ArgumentOption(name: "Argument two", hint: string.Empty, ArgumentType.String, "-something")
            ),
            Throws.TypeOf<CommandLineParseException>().With.InnerException.TypeOf<InvalidOperationException>()
        );
    }

    [Test]
    public void MissingRequiredField_ThrowsArgumentException() {

        var args = new[] {
            _programPath
        };

        Assert.That(
            () => CommandLineParser.ParseCommandLine(args, platformRequired),
            Throws.TypeOf<CommandLineParseException>().With.InnerException.TypeOf<ArgumentException>()
        );
    }

    [Test]
    public void ApplicationPath_RequiresActualFile() {

        const string inputPlatform = "Rift_Platform";
        const string inputWorkspace = "LightweightDevelopment_Workspace";

        var args = new[] {
            _programPath,
            "-platform",
            inputPlatform,
            "-workspace",
            inputWorkspace
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);
        Assert.AreEqual(_programPath, result.applicationPath);
    }

    [Test]
    public void ApplicationPath_IsNullWhenFirstArgumentIsNotAFile() {

        const string inputPlatform = "Rift_Platform";
        const string inputWorkspace = "LightweightDevelopment_Workspace";
        var args = new[] {
            "anything",
            "-platform",
            inputPlatform,
            "-workspace",
            inputWorkspace
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);
        Assert.IsNull(result.applicationPath);
        Assert.IsFalse(result.Contains("anything"));
    }

    [Test]
    public void RequiredString_AreCapturedByOptionAndIdentifier() {

        const string inputPlatform = "Rift_Platform";
        var args = new[] {
            _programPath,
            "-platform",
            inputPlatform,
        };

        var result = CommandLineParser.ParseCommandLine(args, platformRequired);
        Assert.AreEqual(inputPlatform, result[platformRequired]);
        Assert.AreEqual(inputPlatform, result["-platform"]);
    }

    [Test]
    public void OptionalString_AreCapturedByOptionAndIdentifier() {

        const string inputWorkspace = "My workspace";
        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "-workspace",
            inputWorkspace
        };

        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);
        Assert.AreEqual(inputWorkspace, result[workspaceOptional]);
        Assert.AreEqual(inputWorkspace, result["-workspace"]);
    }

    [Test]
    public void OptionalString_IgnoreMissingValue() {

        var args = new[] {
            _programPath,
            "-workspace",
            "-platform",
            "Rift_Platform",
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);
        Assert.False(result.Contains(workspaceOptional));
    }

    [Test]
    public void MissingOptional_IsNotRequiredByParser() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
        };

        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);

        Assert.IsFalse(result.Contains(workspaceOptional));
    }

    [Test]
    public void IntegerType_ThrowsArgumentExceptionOnNonNumericInput() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "code",
            "not_a_number"
        };
        Assert.That(
            () => CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional, codeRequired),
            Throws.TypeOf<CommandLineParseException>().With.InnerException.TypeOf<ArgumentException>()
        );
    }

    [Test]
    public void BooleanType_ComplementGoesToIgnoredList() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "-test",
            "true",
            "-workspace",
            "workspace",
            "code",
            "123"
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional, codeRequired, testBoolean);

        Assert.AreEqual(1,result.unexpectedArguments.Count);
        Assert.AreEqual("true",result.unexpectedArguments[0]);
    }

    [Test]
    public void BooleanType_IsPresentWhenDefined() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "-test",
            "-workspace",
            "workspace",
            "code",
            "123"
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional, codeRequired, testBoolean);
        Assert.IsTrue(result.Contains(testBoolean));
        Assert.IsTrue(result.Contains("-test"));
    }

    [Test]
    public void BooleanType_IsNotPresentWhenNotDefined() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "-workspace",
            "workspace",
            "code",
            "123"
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional, codeRequired, testBoolean);
        Assert.False(result.Contains(testBoolean));
        Assert.False(result.Contains("-test"));
    }

    [Test]
    public void ValueNotFound_ThrowsArgumentException() {

        var args = new[] {
            _programPath,
            "-platform",
            "-workspace",
            "dummy_workspace"
        };
        Assert.That(
            () => CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional),
            Throws.TypeOf<CommandLineParseException>().With.InnerException.TypeOf<ArgumentException>()
        );
    }

    [Test]
    public void OptionalEndKey_IgnoredWhenValueEmpty() {

        var args = new[] {
            _programPath,
            "-platform",
            "Rift_Platform",
            "-workspace",
        };
        var result = CommandLineParser.ParseCommandLine(args, platformRequired, workspaceOptional);
        Assert.False(result.Contains(workspaceOptional));
    }

    [Test]
    public void ExtraParameters_AreAddedToIgnoreList() {

        var args = new[] {
            _programPath,
            "-buildTarget",
            "Win64",
            "-platform",
            "Rift_Platform",
            "-workspace",
            "Production_Workspace",
            "-project_path",
            @"C:\open\BeatGames\A1\BeatSaber",
            "-EnableCacheServer",
            "-executeMethod",
            "BeatSaber.CustomBuilds.Editor.BuildService.CMDWarmUp",
            "-batchmode",
            "-logFile",
            @"C:\open\BeatGames\A1\BeatSaber\Logs\warmup-1_1_1.log"
        };

        var result = CommandLineParser.ParseCommandLine(args, platformRequired);
        Assert.AreEqual(12, result.unexpectedArguments.Count);
    }

    [Test]
    public void EmptyArgs_ProducesEmptyResult() {

        var args = Array.Empty<string>();
        var result = CommandLineParser.ParseCommandLine(args, workspaceOptional);
        Assert.IsNull(result.applicationPath);
        Assert.IsFalse(result.Contains(workspaceOptional));
        Assert.AreEqual(0, result.unexpectedArguments.Count);
    }
}
