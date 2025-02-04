using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class ToolDefinitionLoader {

    public const char kLinkSeparatorCharacter = '=';

    public static List<ToolDefinition> GetToolDefinitions() {

        List<ToolDefinition> toolDefinitions = new();

        FindAndParseToolAttributes(toolDefinitions);
        AddExternalTools(toolDefinitions);

        return toolDefinitions.OrderBy(definition => definition.labelSortOrder)
            .ThenBy(definition => definition.displayName).ToList();
    }

    private static void FindAndParseToolAttributes(List<ToolDefinition> output) {

        Dictionary<Type, AddToToolDirectoryAttribute> foundToolDirectoryAttributes = new();
        var typesWithAttribute = TypeCache.GetTypesWithAttribute<AddToToolDirectoryAttribute>();
        foreach (Type type in typesWithAttribute) {
            var attributes = type.GetCustomAttributes(typeof(AddToToolDirectoryAttribute), false);
            foreach (var attribute in attributes) {
                foundToolDirectoryAttributes.Add(type, attribute as AddToToolDirectoryAttribute);
            }
        }

        foreach (var (type, toolDirectoryAttribute) in foundToolDirectoryAttributes) {
            output.Add(CreateToolDefinition(
                displayName: toolDirectoryAttribute.displayName,
                description: toolDirectoryAttribute.description,
                packageName: toolDirectoryAttribute.packageName,
                labelTypes: toolDirectoryAttribute.labelTypes,
                maintainer: toolDirectoryAttribute.maintainer,
                toolType: type,
                openToolFunctionName: toolDirectoryAttribute.openToolFunctionName,
                links: toolDirectoryAttribute.links
            ));
        }
    }

    private static void AddExternalTools(List<ToolDefinition> output) {

        output.Add(
            CreateToolDefinition(
                displayName: "Beat Launcher",
                description:
                "A Windows application that lives in the tray to quickly execute preset commands such as launching Unity, website bookmarks, and fixing common issues outside of Unity.",
                packageName: "Beat Launcher application",
                labelTypes: LabelType.Core,
                maintainer: ToolMaintainer.RandySchouten,
                toolType: null,
                openToolFunctionName: null,
                links: new[] {
                    $"Documentation{kLinkSeparatorCharacter}https://beatgames.atlassian.net/wiki/spaces/BS/pages/732332040/Beat+Launcher"
                }
            )
        );
        output.Add(
            CreateToolDefinition(
                displayName: "Jira Bridge",
                description:
                "Press pg-up or Y 5x in Beat Saber menus to directly report a bug to Jira during playtests.",
                packageName: "BGLib.JiraBridge & Tours.Core",
                labelTypes: LabelType.Tours,
                maintainer: ToolMaintainer.DannyDeBruijne,
                toolType: null,
                openToolFunctionName: null,
                links: new[] {
                    $"Documentation{kLinkSeparatorCharacter}https://github.com/facebookexternal/beat-saber/blob/main/SharedPackages/BGLib/jira-bridge/README.md"
                }
            )
        );
    }

    private static ToolDefinition CreateToolDefinition(
        string displayName,
        string description,
        string packageName,
        LabelType labelTypes,
        ToolMaintainer maintainer,
        Type toolType,
        string openToolFunctionName,
        string[] links
    ) {

        List<string> errors = new();

        string menuItemPath = string.Empty;
        Action callback = null;

        if (toolType != null && !string.IsNullOrEmpty(openToolFunctionName)) {
            var staticMethods = toolType.GetMethods(
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                ).Where(methodInfo => methodInfo.Name.Equals(openToolFunctionName))
                .ToList();

            if (staticMethods.Count == 0) {
                errors.Add(
                    $"openToolFunctionName is given, but no valid static function is found for tool '{displayName}': {openToolFunctionName}"
                );
            }

            MethodInfo validMethod = null;
            foreach (var methodInfo in staticMethods) {
                if (methodInfo.GetParameters().Length == 0) {
                    validMethod = methodInfo;
                    break;
                }
            }

            if (validMethod == null) {
                errors.Add(
                    $"openToolFunctionName has no valid parameterless static functions for tool '{displayName}': {openToolFunctionName}"
                );
            }

            callback = () => validMethod!.Invoke(null, null);

            var menuItemAttribute = validMethod.GetCustomAttribute<MenuItem>();
            if (menuItemAttribute != null) {
                menuItemPath = $"Main Menu/{menuItemAttribute.menuItem}";
            }
        }

        foreach (string attributeLink in links) {
            if (!attributeLink.Contains(kLinkSeparatorCharacter)) {
                errors.Add(
                    $"Link does not contain separator character '{kLinkSeparatorCharacter}' in tool {displayName}: {attributeLink}"
                );
            }
        }

        ToolMaintainers.TryGetMaintainer(maintainer, out var maintainerData);
        ToolDefinition toolDefinition = new ToolDefinition(labelTypes) {
            displayName = displayName,
            description = description,
            packageName = packageName,
            maintainer = maintainerData,
            openToolCallback = callback,
            menuItemPath = menuItemPath,
            links = links,
            parseErrors = errors,
        };

        return toolDefinition;
    }
}
