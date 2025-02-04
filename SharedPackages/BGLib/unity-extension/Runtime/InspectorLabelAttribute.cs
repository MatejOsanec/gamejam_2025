namespace BeatSaber.InspectorName {

    using UnityEngine;

    public class InspectorLabelAttribute : PropertyAttribute {

        public readonly string CustomLabel;

        public InspectorLabelAttribute(string customLabel) {

            CustomLabel = customLabel;
        }
    }
}
