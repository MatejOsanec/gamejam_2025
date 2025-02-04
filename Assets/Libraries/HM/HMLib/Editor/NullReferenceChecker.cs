#nullable enable

using System.Collections.Generic;
using BGLib.UnityExtension.Editor;

public class NullReferencesChecker {

    public static List<UnityObjectWithDescription> FindNullReferencesInObjects(
        IEnumerable<UnityEngine.Object> objects,
        NullAllowed.Context nullAllowedContext = NullAllowed.Context.Everywhere
    ) {

        var checker = new NullReferencesChecker(objects, new IgnoredAssemblies(), nullAllowedContext);
        return checker.Check();
    }

    private readonly HashSet<UnityEngine.Object> _rootObjects;
    private readonly HashSet<object> _checkedObjects;
    private readonly List<UnityObjectWithDescription> _nullReferenceDescriptions;
    private readonly IgnoredAssemblies _ignoredAssemblies;
    private readonly NullAllowed.Context _initialNullAllowedContext;

    private NullReferencesChecker(
        IEnumerable<UnityEngine.Object> objects,
        IgnoredAssemblies ignoredAssemblies,
        NullAllowed.Context initialNullAllowedContext
    ) {

        _rootObjects = new HashSet<UnityEngine.Object>(objects);
        _ignoredAssemblies = ignoredAssemblies;
        _initialNullAllowedContext = initialNullAllowedContext;
        _nullReferenceDescriptions = new List<UnityObjectWithDescription>();
        _checkedObjects = new HashSet<object>();
    }

    private List<UnityObjectWithDescription> Check() {

        foreach (var obj in _rootObjects) {
            if (obj != null && !CheckSpecialObject(obj, obj.GetType().Name, obj)) {
                Check(obj, obj.GetType().Name, obj, _initialNullAllowedContext);
            }
        }

        return _nullReferenceDescriptions;
    }

    private void Check(
        object obj,
        string objPath,
        UnityEngine.Object rootObject,
        NullAllowed.Context nullAllowedContext
    ) {

        if (_ignoredAssemblies.IsIgnoredType(obj.GetType())) {
            return;
        }

        // Checked already.
        if (!_checkedObjects.Add(obj)) {
            return;
        }

        var serializedReferences = ReflectionHelpers.GetSerializedReferences(obj, rootObject, _ignoredAssemblies);

        // Recursive objects.
        foreach (var referenceInfo in serializedReferences) {

            var reference = referenceInfo.fieldContent;
            switch (reference) {
                case null: {
                    if (!CheckIfFieldAllowsNull(obj, referenceInfo.customAttributes, nullAllowedContext)) {
                        _nullReferenceDescriptions.Add(
                            new UnityObjectWithDescription(rootObject, $"{objPath}.{referenceInfo.fieldName}")
                        );
                    }
                    continue;
                }
                // Recursion is stopped on root objects.
                case UnityEngine.Object unityObject: {
                    if (_rootObjects.Contains(unityObject)) {
                        continue;
                    }
                    break;
                }
            }
            // Special objects are not inspected using reflection.
            if (CheckSpecialObject(reference, objPath, referenceInfo.rootObject)) {
                continue;
            }

            if (SearchForPrefabTemplate(referenceInfo.customAttributes)) {
                nullAllowedContext = NullAllowed.Context.Prefab;
            }
            Check(
                reference,
                $"{objPath}.{referenceInfo.fieldName}",
                referenceInfo.rootObject,
                nullAllowedContext
            );
        }
    }

    private bool CheckSpecialObject(object obj, string objPath, UnityEngine.Object rootObject) {

        var objType = obj.GetType();

        // UnityEvent
        if (typeof(UnityEngine.Events.UnityEvent).IsAssignableFrom(objType)) {
            var unityEvent = (UnityEngine.Events.UnityEvent)obj;
            if (DoesUnityEventContainNullReferences(unityEvent)) {
                _nullReferenceDescriptions.Add(
                    new UnityObjectWithDescription(rootObject, $"{objPath}.UnityEvent.MissingReferences")
                );
            }
            return true;
        }
        // UnityEngine.UI.Button
        if (typeof(UnityEngine.UI.Button).IsAssignableFrom(objType)) {
            var button = (UnityEngine.UI.Button)obj;
            if (DoesUnityEventContainNullReferences(button.onClick)) {
                _nullReferenceDescriptions.Add(
                    new UnityObjectWithDescription(rootObject, $"{objPath}.Button.OnClick.MissingReferences")
                );
            }
            return true;
        }

        return false;
    }

    private static bool DoesUnityEventContainNullReferences(UnityEngine.Events.UnityEvent unityEvent) {

        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
            var target = unityEvent.GetPersistentTarget(i);
            var methodName = unityEvent.GetPersistentMethodName(i);
            if (target == null || string.IsNullOrEmpty(methodName)) {
                return true;
            }
        }
        return false;
    }

    private static bool SearchForPrefabTemplate(object[] customAttributes) {

        foreach (var attribute in customAttributes) {
            switch (attribute) {
                case PrefabTemplate:
                    return true;
            }
        }
        return false;
    }

    public static bool CheckIfFieldAllowsNull(
        object obj,
        object[] customAttributes,
        NullAllowed.Context nullAllowedContext
    ) {

        // Starting from end to beginning, in our code style, NullAllowed comes in the end, so it increase the changes
        // of hitting in the first try
        for (int i = customAttributes.Length - 1; i >= 0; i--) {
            object attribute = customAttributes[i];
            switch (attribute) {
                case NullAllowedIf nullAllowedIfAttribute:
                    var value = ReflectionHelpers.GetValue(obj, nullAllowedIfAttribute.propertyName);
                    return nullAllowedIfAttribute.IsNullAllowedFor(value, nullAllowedContext);
                case NullAllowed nullAllowedAttribute:
                    return nullAllowedAttribute.IsNullAllowedFor(nullAllowedContext);
            }
        }
        return false;
    }
}
