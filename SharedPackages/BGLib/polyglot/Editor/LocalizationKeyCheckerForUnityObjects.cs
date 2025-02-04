using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BGLib.Polyglot;
using BGLib.Polyglot.Editor;
using BGLib.UnityExtension.Editor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LocalizationKeyCheckerForUnityObjects {

    private enum CheckerState {
        Initialized,
        Running,
        Completed
    }
    private readonly HashSet<object> _checkedObjects;
    private readonly List<UnityObjectWithDescription> _localizedReferenceDescriptions;
    private readonly HashSet<Object> _rootObjects;
    private readonly HashSet<string> _keysToIgnore;
    private readonly StringBuilder _stringBuilder;
    private readonly IgnoredAssemblies _ignoredAssemblies;
    private CheckerState _state;

    private const string kUnLocalizedKeyValue = "_UNLOCALIZED_";
    private const string kUseNonLocalizedNamePropertyName = "useNonLocalizedName";

    public LocalizationKeyCheckerForUnityObjects(IEnumerable<Object> rootObjects, HashSet<string>? keysToIgnore, IgnoredAssemblies ignoredAssemblies) {

        _keysToIgnore = keysToIgnore ?? new HashSet<string>();
        _stringBuilder = new StringBuilder();
        _checkedObjects = new HashSet<object>();
        _rootObjects = new HashSet<Object>(rootObjects);
        _localizedReferenceDescriptions = new List<UnityObjectWithDescription>();
        _ignoredAssemblies = ignoredAssemblies;
        _state = CheckerState.Initialized;
    }

    public List<UnityObjectWithDescription>? CheckKey() {

        switch (_state) {
            case CheckerState.Completed:
                return _localizedReferenceDescriptions;
            case CheckerState.Running:
                Debug.LogError("Check key is running");
                return null;
        }

        _state = CheckerState.Running;

        foreach (var obj in _rootObjects) {
            Assert.IsNotNull(obj);
            CheckKey(obj, obj.GetType().Name, obj, _ignoredAssemblies);
        }
        _state = CheckerState.Completed;

        return _localizedReferenceDescriptions;
    }

    private bool CheckIfKeyExistsInLanguage(string key) {

        return _keysToIgnore.Contains(key) || EditorLocalization.instance.KeyExist(key);
    }

    private void CheckKey(object obj, string objPath, Object rootObject, IgnoredAssemblies ignoreAssemblies) {

        if (_checkedObjects.Contains(obj)) {
            return;
        }
        _checkedObjects.Add(obj);

        // If it is not in assembly, ignore it.
        if (ignoreAssemblies.IsIgnoredType(obj.GetType())) {
            return;
        }

        var props = ReflectionHelpers.GetInheritanceChain(obj.GetType())
            .SelectMany(x => x.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(p => p.GetCustomAttribute<LocalizationKeyAttribute>() != null));

        foreach (var p in props) {
            var value = p.GetValue(obj);
            switch (value) {
                case string stringValue:
                    if (stringValue == kUnLocalizedKeyValue) {
                        var allProps = obj.GetType().GetProperties();
                        bool useNonLocalizedChecked = false;
                        for (int i = 0; i < allProps.Length; i++) {
                            if (allProps[i].Name == kUseNonLocalizedNamePropertyName) {
                                useNonLocalizedChecked = (bool) allProps[i].GetValue(obj);
                                break;
                            }
                        }
                        if (useNonLocalizedChecked) {
                            break;
                        }
                    }
                    if (!CheckIfKeyExistsInLanguage(stringValue)) {
                        _localizedReferenceDescriptions.Add(new UnityObjectWithDescription(rootObject, GenerateKeyDescription(objPath, stringValue)));
                    }
                    break;
                case IEnumerable<string> enumerableStringValue:
                    foreach (var val in enumerableStringValue) {
                        if (!CheckIfKeyExistsInLanguage(val)) {
                            _localizedReferenceDescriptions.Add(new UnityObjectWithDescription(rootObject, GenerateKeyDescription(objPath, val)));
                        }
                    }
                    break;
                default:
                    _localizedReferenceDescriptions.Add(new UnityObjectWithDescription(rootObject, GenerateKeyNonStringDescription(objPath)));
                    break;
            }
        }

        var serializedReferences = ReflectionHelpers.GetSerializedReferences(obj, rootObject, ignoreAssemblies);

        // Recursive objects.
        foreach (var reference in serializedReferences) {

            switch (reference.fieldContent) {
                case null:
                // Recursion is stopped on root objects.
                case Object unityReference when _rootObjects.Contains(unityReference):
                    continue;
            }

            if (_checkedObjects.Contains(obj)) {
                continue;
            }

            CheckKey(reference, GenerateRecursiveObjPath(objPath, reference.GetType().Name), reference.rootObject, ignoreAssemblies);
        }
    }

    private string GenerateKeyDescription(in string objPath, in string stringValue) {

        _stringBuilder.Append(objPath);
        _stringBuilder.Append(", key:'");
        _stringBuilder.Append(stringValue);
        _stringBuilder.Append("'");
        string result = _stringBuilder.ToString();
        _stringBuilder.Clear();
        return result;
    }

    private string GenerateKeyNonStringDescription(in string objPath) {

        _stringBuilder.Append("Used LocalizationKeyAttribute on non-string property ");
        _stringBuilder.Append(objPath);
        string result = _stringBuilder.ToString();
        _stringBuilder.Clear();
        return result;
    }

    private string GenerateRecursiveObjPath(in string objPath, in string typeName) {

        _stringBuilder.Append(objPath);
        _stringBuilder.Append(".");
        _stringBuilder.Append(typeName);
        string result = _stringBuilder.ToString();
        _stringBuilder.Clear();
        return result;
    }
}
