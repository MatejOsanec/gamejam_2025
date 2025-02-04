namespace BGLib.UnityExtension.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(Object), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : Editor {

        private static Dictionary<Type, (MethodInfo method, string title)[]> _buttonMethodsCache;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            if (target == null) {
                return;
            }

            var buttonMethods = GetButtonMethods(target.GetType());

            foreach (var (methodInfo, buttonTitle) in buttonMethods) {
                if (GUILayout.Button(buttonTitle)) {
                    foreach (var targetInstance in targets) {
                        methodInfo.Invoke(targetInstance, null);
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void NoDomainReloadInit() {

            _buttonMethodsCache = null;
        }

        private static (MethodInfo method, string title)[] GetButtonMethods(Type type) {

            _buttonMethodsCache ??= new();

            if (!_buttonMethodsCache.TryGetValue(type, out var methods)) {
                methods = type.GetMembers(
                        BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static |
                        BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .OfType<MethodInfo>()
                    .Where(methodInfo => Attribute.IsDefined(methodInfo, typeof(ButtonAttribute)))
                    .Select(
                        methodInfo => (
                            methodInfo, methodInfo.GetCustomAttribute<ButtonAttribute>().title ?? methodInfo.Name
                        )
                    )
                    .ToArray();

                _buttonMethodsCache.Add(type, methods);
            }

            return methods;
        }
    }
}
