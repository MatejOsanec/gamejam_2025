using System;
using System.Text;
using NUnit.Framework;

public class ToolDirectoryTest {

    [Test]
    public void AddToToolDirectoryAttributes_AreValid() {

        var toolDefinitions = ToolDefinitionLoader.GetToolDefinitions();

        Assert.IsTrue(toolDefinitions.Count > 0, "No tool definitions found.");

        int errorCount = 0;
        StringBuilder errors = new();
        foreach (var toolDefinition in toolDefinitions) {
            if (!toolDefinition.HasErrors) {
                continue;
            }

            foreach (var error in toolDefinition.parseErrors) {
                errorCount++;
                errors.AppendLine($"{errorCount}: {error}");
            }
        }

        Assert.True(errorCount == 0, $"Errors encountered in loading of tools:\n{errors}");
    }

    [Test]
    public void ToolMaintainerEnum_AreAllDefined() {

        // Exception is thrown when maintainer is not found
        foreach (var toolMaintainer in (ToolMaintainer[])Enum.GetValues(typeof(ToolMaintainer))) {
            if (!ToolMaintainers.TryGetMaintainer(toolMaintainer, out _)) {
                Assert.Fail($"Tool maintainer data not found for {toolMaintainer}");
            }
        }
    }

    [Test]
    public void LabelTypeEnum_AreAllDefined() {

        // Exception is thrown when maintainer is not found
        foreach (var labelType in (LabelType[])Enum.GetValues(typeof(LabelType))) {
            if (!LabelDefinitions.TryGetLabel(labelType, out _)) {
                Assert.Fail($"Label definition not found for {labelType}");
            }
        }
    }
}
