namespace BGLib.ShaderInspector {

    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public enum KeywordState {

        Enabled,
        Disabled,
        Mixed
    }

    public static class ShaderInspectorHelper {

        public static KeywordState GetKeywordState(string keyword, MaterialProperty[] properties) {

            if (properties.Length == 0) {
                return KeywordState.Disabled;
            }

            var anyEnabled = false;
            var anyDisabled = false;
            foreach (Material material in properties[0].targets) {
                if (material.IsKeywordEnabled(keyword)) {
                    anyEnabled = true;
                }
                else {
                    anyDisabled = true;
                }
            }

            if (anyDisabled && anyEnabled) {
                return KeywordState.Mixed;
            }
            if (anyEnabled) {
                return KeywordState.Enabled;
            }
            return KeywordState.Disabled;
        }

        public static void SetKeywordEnabled(string keyword, MaterialProperty materialProperty, bool enabled) {

            // Feature without a keyword
            if (string.IsNullOrEmpty(keyword)) {
                return;
            }
            foreach (Material target in materialProperty.targets) {
                if (enabled) {
                    target.EnableKeyword(keyword);
                }
                else {
                    target.DisableKeyword(keyword);
                }
            }
        }

        public static KeywordState GetKeywordState(string keyword, string propertyName, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            if (property == null) {
                return KeywordState.Disabled;
            }

            return GetKeywordState(keyword, property, properties);
        }

        public static KeywordState GetKeywordState(string keyword, MaterialProperty property, MaterialProperty[] properties) {

            // Feature without a keyword
            if (string.IsNullOrEmpty(keyword)) {
                return property.floatValue > 0 ? KeywordState.Enabled : KeywordState.Disabled;
            }
            return GetKeywordState(keyword, properties);
        }

        private static readonly StringBuilder _tooltipStringBuilder = new StringBuilder();

        public static string BuildTooltip(string tooltip, string propertyName = null, string keyword = null) {

            _tooltipStringBuilder.Clear();

            if (!string.IsNullOrEmpty(tooltip)) {
                _tooltipStringBuilder.Append(tooltip);
                _tooltipStringBuilder.AppendLine();
                _tooltipStringBuilder.AppendLine();
            }
            if (!string.IsNullOrEmpty(keyword)) {
                _tooltipStringBuilder.Append(keyword);
                _tooltipStringBuilder.Append(" - keyword");
                _tooltipStringBuilder.AppendLine();
            }
            if (!string.IsNullOrEmpty(propertyName)) {
                _tooltipStringBuilder.Append(propertyName);
                _tooltipStringBuilder.Append(" - property name");
            }

            while (_tooltipStringBuilder.Length > 0 && (_tooltipStringBuilder[^1] == '\n' || _tooltipStringBuilder[^1] == '\r')) {
                _tooltipStringBuilder.Length--;
            }
            return _tooltipStringBuilder.ToString();
        }
    }
}
