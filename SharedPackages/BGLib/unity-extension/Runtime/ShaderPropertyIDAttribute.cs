namespace BGLib.UnityExtension {

    using System;
    using UnityEngine;

    public abstract class ShaderPropertyIDAttribute : PropertyAttribute {

        public readonly ShaderPropertyAttributeFilter filter;

        public ShaderPropertyIDAttribute(
            string nameFilter = "",
            ShaderPropertyAttributeFilter.PropType filterPropType = ShaderPropertyAttributeFilter.PropType.Any
        ) {

            filter = new ShaderPropertyAttributeFilter(nameFilter, filterPropType);
        }

        public abstract string GetTargetName();
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyIDFromGameObjectAttribute : ShaderPropertyIDAttribute {

        private const string kTargetName = "GameObject";
        
        public ShaderPropertyIDFromGameObjectAttribute(
            string nameFilter = "",
            ShaderPropertyAttributeFilter.PropType filterPropType = ShaderPropertyAttributeFilter.PropType.Any
        ) : base(nameFilter, filterPropType) {
        }

        public override string GetTargetName() => kTargetName;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyIDFromRendererAttribute : ShaderPropertyIDAttribute {

        public readonly string propertyName;

        public ShaderPropertyIDFromRendererAttribute(
            string propertyName,
            string nameFilter = "",
            ShaderPropertyAttributeFilter.PropType filterPropType = ShaderPropertyAttributeFilter.PropType.Any
        ) : base(nameFilter, filterPropType) {

            this.propertyName = propertyName;
        }

        public override string GetTargetName() => propertyName;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyIDFromPropertyAttribute : ShaderPropertyIDFromRendererAttribute {

        public readonly string nestedPropertyName;

        public ShaderPropertyIDFromPropertyAttribute(
            string propertyName,
            string nestedPropertyName,
            string nameFilter = "",
            ShaderPropertyAttributeFilter.PropType filterPropType = ShaderPropertyAttributeFilter.PropType.Any
        ) : base(propertyName, nameFilter, filterPropType) {

            this.nestedPropertyName = nestedPropertyName;
        }

        public override string GetTargetName() {

            return $"'{nestedPropertyName}' in '{base.GetTargetName()}'";
        }
    }
}
