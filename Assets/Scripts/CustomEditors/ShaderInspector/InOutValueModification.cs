namespace BGLib.ShaderInspector {

    using System;
    using UnityEngine;

    public static class InOutValueModification {

        public static float OneDividedByX(float x) => 1.0f/x;

        public static Vector4 Round(Vector4 value, int digits) {

            return value._Round(digits);
        }

        public static Vector4 _Round(this Vector4 value, int digits) {

            float multiplication = Mathf.Pow(10.0f, digits);
            float division = 1.0f / multiplication;

            value.x = Mathf.Round(value.x * multiplication) * division;
            value.y = Mathf.Round(value.y * multiplication) * division;
            value.z = Mathf.Round(value.z * multiplication) * division;
            value.w = Mathf.Round(value.w * multiplication) * division;
            return value;
        }

        [UIToMaterialOnlyAllowed]
        public static float Round(float value) {

            return Mathf.Round(value);
        }
        
        [UIToMaterialOnlyAllowed]
        public static Vector4 Round(Vector4 value) {
            
            return new Vector4(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z), Mathf.Round(value.w));
        }
        
        [UIToMaterialOnlyAllowed]
        public static Vector4 Normalize(Vector4 value) {
            
            var inputVector3 = new Vector3(value.x, value.y, value.z);
            var normalizedInput = Vector3.Normalize(inputVector3);
            return new Vector4(normalizedInput.x, normalizedInput.y, normalizedInput.z, value.w);
        }
    }

    // Methods with this attribute will not trigger warning if used for in value modification without out value modification
    [AttributeUsage(AttributeTargets.Method)]
    public class UIToMaterialOnlyAllowedAttribute : Attribute {
        public UIToMaterialOnlyAllowedAttribute() { }
    }
}
