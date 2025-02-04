#nullable enable

namespace BGLib.UnityExtension {
    
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class ButtonAttribute : PropertyAttribute {
        
        public readonly string? title;

        public ButtonAttribute(string? title = null) {
            
            this.title = title;
        }
    }
}
