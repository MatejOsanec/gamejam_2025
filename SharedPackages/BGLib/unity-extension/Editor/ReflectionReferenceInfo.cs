#nullable enable

using System;

public readonly struct ReflectionReferenceInfo {

    public readonly UnityEngine.Object rootObject;
    public readonly object? fieldContent;
    public readonly string fieldName;
    public readonly object[] customAttributes;

    public ReflectionReferenceInfo(UnityEngine.Object rootObject, string fieldName) : this(
        rootObject,
        null,
        fieldName
    ) { }

    public ReflectionReferenceInfo(UnityEngine.Object rootObject, object? fieldContent, string fieldName) : this(
        rootObject,
        fieldContent,
        fieldName,
        Array.Empty<object>()
    ) { }

    public ReflectionReferenceInfo(
        UnityEngine.Object rootObject,
        object? fieldContent,
        string fieldName,
        object[] customAttributes
    ) {

        this.rootObject = rootObject;
        this.fieldContent = fieldContent;
        this.fieldName = fieldName;
        this.customAttributes = customAttributes;
    }
}
