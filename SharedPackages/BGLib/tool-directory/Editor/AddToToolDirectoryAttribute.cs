using System;

/// <summary>
/// Adds a reference to this class to the Tools Directory window.
/// Use "openToolFunctionName" as such: "nameof(MyStaticOpenToolFunction)", this function is validated and called via reflection.
/// </summary>
public class AddToToolDirectoryAttribute : Attribute {

    public string displayName { get; }
    public string description { get; }
    public string packageName { get; }
    public ToolMaintainer maintainer { get; }
    public string openToolFunctionName { get; }
    public LabelType labelTypes { get; }

    /// <summary>
    /// Format: {button name}={link}. Example: Documentation=https://mywebsite.com
    /// </summary>
    public string[] links { get; }

    public AddToToolDirectoryAttribute(
        string displayName,
        string description,
        string packageName,
        ToolMaintainer maintainer,
        string openToolFunctionName,
        LabelType labelTypes,
        string[] links = null
    ) {

        this.displayName = displayName;
        this.description = description;
        this.packageName = packageName;
        this.maintainer = maintainer;
        this.openToolFunctionName = openToolFunctionName;
        this.labelTypes = labelTypes;
        this.links = links ?? Array.Empty<string>();
    }
}
