using System;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyExtensions {

    /// <summary>
    /// Use reflection to get the actual data instance of a SerializedProperty.
    /// Use the optional parameters to search for child fields or siblings fields by going up the hierarchy.
    /// This methods also returns fields and property that are not serialized. 
    /// </summary>
    /// <param name="property">Any serialized property</param>
    /// <param name="hierarchyOffset">number of step up in the hierarchy of the field:
    /// 0 is the object, 1 is direct parent, 2 is the parent of the parent and so on.</param>
    /// <param name="childElements">child elements that you want to append to after moving up in the hierarchy</param>
    /// <returns>Object value of the searched field or property</returns>
    /// <exception cref="ArgumentException">If it's provided an hierarchyOffset over</exception>
    /// <exception cref="InvalidOperationException">If it's provided an invalid path for a property</exception>
    public static object GetValue(this SerializedProperty property, uint hierarchyOffset = 0, params string[] childElements) {
        
        var path = property.propertyPath.Replace(".Array.data[", "[");
        object obj = property.serializedObject.targetObject;
        string[] parts = path.Split('.');
        if (hierarchyOffset > parts.Length) {
            throw new ArgumentException($"It not possible to go ({hierarchyOffset} steps) over the number of elements ({parts.Length}) in the hierarchy.", nameof(hierarchyOffset));
        }
        string[] elements = new string[parts.Length - hierarchyOffset + childElements.Length];
        Array.Copy(parts, elements, length: parts.Length - hierarchyOffset);
        Array.Copy(childElements, sourceIndex: 0, elements,  destinationIndex: parts.Length - hierarchyOffset, length: childElements.Length);
        foreach (string element in elements) {
            int indexOfOpeningBracket = element.IndexOf("[", StringComparison.Ordinal);
            try {
                if (indexOfOpeningBracket >= 0) {
                    var elementName = element[..indexOfOpeningBracket];
                    var indexString = element[(indexOfOpeningBracket + 1)..^1];
                    var index = Convert.ToInt32(indexString);
                    obj = ReflectionHelpers.GetValueIndex(obj, elementName, index);
                }
                else {
                    obj = ReflectionHelpers.GetValue(obj, element);
                }
            }
            catch (ArgumentException argumentException) {
                throw new InvalidOperationException($"Could not find the property '{string.Join(".", elements)}', because {argumentException.Message}", argumentException);
            }
        }
        return obj;
    }

    public static bool TryGetGameObject(this SerializedProperty property, out GameObject gameObject) {

        gameObject = property?.objectReferenceValue as GameObject;
        return gameObject != null;
    }
}
