#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BGLib.UnityExtension.Editor;
using UnityEngine.Assertions;

public static class ReflectionHelpers {

    const BindingFlags kFieldsBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
                                             BindingFlags.DeclaredOnly;

    public static IEnumerable<ReflectionReferenceInfo> GetSerializedReferences(
        object obj,
        UnityEngine.Object rootObject,
        IgnoredAssemblies ignoreAssemblies
    ) {
        var type = obj.GetType();
        if (ignoreAssemblies.IsIgnoredType(type)) {
            yield break;
        }
        if (obj is Component objAsComponent) {
            var prefabType = PrefabUtility.GetPrefabAssetType(objAsComponent);
            if (prefabType != PrefabAssetType.NotAPrefab && !objAsComponent.gameObject.scene.IsValid()) {
                var newRootObject = objAsComponent.gameObject;
                var components = newRootObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (var component in components) {
                    yield return new ReflectionReferenceInfo(newRootObject, component, newRootObject.name);
                }
            }
        }
        // Array
        if (type.IsArray) {
            var array = (Array)obj;
            for (int i = 0; i < array.Length; i++) {
                var element = array.GetValue(i);
                if (element == null || element.Equals(null)) {
                    yield return new ReflectionReferenceInfo(rootObject, $"[{i}]");
                    continue;
                }
                if (!IsSimpleType(element)) {
                    yield return new ReflectionReferenceInfo(rootObject, element, $"[{i}]");
                }
            }
            yield break;
        }
        // List
        if (type.IsGenericList()) {
            var list = (IList)obj;
            for (int i = 0; i < list.Count; i++) {
                var element = list[i];
                if (element == null || element.Equals(null)) {
                    yield return new ReflectionReferenceInfo(rootObject, $"[{i}]");
                    continue;
                }
                if (!IsSimpleType(element)) {
                    yield return new ReflectionReferenceInfo(rootObject, element, $"[{i}]");
                }
            }
            yield break;
        }
        foreach (var typeIterator in IterateOverBaseTypes(type)) {
            if (ignoreAssemblies.IsIgnoredType(typeIterator)) {
                continue;
            }
            var fields = typeIterator.GetFields(kFieldsBindingFlags);
            foreach (var field in fields) {
                // Ignore fields which are not serialized.
                if (field.IsNotSerialized) {
                    continue;
                }
                object[] customAttributes = field.GetCustomAttributes(inherit: false);
                bool hasSerializedField = SearchForSerializedField(customAttributes);
                if (!field.IsPublic && !hasSerializedField) {
                    continue;
                }
                // Check if the variable is a null reference.
                object referencedObj = field.GetValue(obj);
                if (referencedObj == null || referencedObj.Equals(null)) {
                    yield return new ReflectionReferenceInfo(rootObject, null, field.Name, customAttributes);
                }
                else if (!IsSimpleType(referencedObj)) {
                    yield return new ReflectionReferenceInfo(rootObject, referencedObj, field.Name, customAttributes);
                }
            }
        }
    }

    private static bool IsSimpleType(object obj) {

        var type = obj.GetType();
        return type.IsPrimitive || type.IsEnum;
    }

    public static bool SearchForSerializedField(in object[] customAttributes) {

        foreach (var attribute in customAttributes) {
            switch (attribute) {
                case SerializeField:
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Walks through whole inheritance chain up.
    /// </summary>
    /// <param name="type">Type to find base classes of</param>
    /// <param name="stopAtUnity">Whether to stop at Unity classes and namespaces</param>
    /// <returns>Enumerable of base types</returns>
    public static IEnumerable<Type> GetInheritanceChain(Type type, bool stopAtUnity = true) {

        var current = type;
        while (current != null && current.FullName != null && !current.FullName.StartsWith("Unity")) {
            yield return current;
            current = current.BaseType;
        }
    }

    public static bool IsGenericList(this Type type) {

        return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));
    }

    /// <summary>
    /// Get the object value of a field or property in a object.
    /// It looks on the object type and all the base types.
    /// </summary>
    /// <param name="sourceObject">Source object</param>
    /// <param name="fieldOrPropertyName">Name of the field or property</param>
    /// <param name="bindings">Bindings used to search the field or property</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If it's provided an invalid field or property name</exception>
    public static object GetValue(
        object sourceObject,
        string fieldOrPropertyName,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
    ) {

        foreach (var typeIter in IterateOverBaseTypes(sourceObject.GetType())) {
            FieldInfo? field = typeIter.GetField(fieldOrPropertyName, bindings);
            if (field != null) {
                return field.GetValue(sourceObject);
            }
            PropertyInfo? property = typeIter.GetProperty(fieldOrPropertyName, bindings);
            if (property != null) {
                return property.GetValue(sourceObject, null);
            }
        }
        throw new ArgumentException(
            $"FieldOrPropertyName was not found: {fieldOrPropertyName} on object {sourceObject.GetType().FullName}",
            nameof(fieldOrPropertyName)
        );
    }

    /// <summary>
    /// Searches the collection to find the element in "index".
    /// This is a linear search O(index).
    /// </summary>
    /// <param name="source">any object</param>
    /// <param name="fieldOrPropertyName">name of the field or property</param>
    /// <param name="index">index in the collection</param>
    /// <returns></returns>
    public static object? GetValueIndex(object source, string fieldOrPropertyName, int index) {

        if (GetValue(source, fieldOrPropertyName) is not IEnumerable enumerable) {
            return null;
        }
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0) {
            enm.MoveNext();
        }
        return enm.Current;
    }

    private static IEnumerable<Type> IterateOverBaseTypes(Type type) {

        var typeIterator = type;
        do {
            yield return typeIterator;
            typeIterator = typeIterator.BaseType;
        } while (typeIterator != null);
    }

    public static List<Type> GetAllTypesSortedByNameWhere(Func<Type, bool> where) {

        var result = new List<Type>();
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in allAssemblies) {
            // Get all classes derived from ScriptableObject
            var plainClasses = assembly.GetTypes().Where(where);
            result.AddRange(plainClasses);
        }

        result.Sort((a, b) => (string.Compare(a.FullName, b.FullName, StringComparison.InvariantCultureIgnoreCase)));

        return result;
    }

    public static bool IsAnonymous(this Type type)  {

        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }
}
