namespace BGLib.Inspections.Editor.Core {

    using System;
    using UnityEngine;

    public abstract class SimpleInspection : IInspection {

        public abstract string name { get; }
        public abstract bool isCritical { get; }

        public InspectionResult Inspect() {

            try {
                return InspectAndFix(inspectOnly: true);
            }
            catch (Exception e) {
                Debug.LogException(e);
                return InspectionResult.NonFixable(e.Message);
            }
        }

        public void Fix() {

            var result = InspectAndFix(inspectOnly: false);
            switch (result.status) {
                case InspectionResult.Status.Ok:
                    return;
                case InspectionResult.Status.Fixable:
                    Debug.LogError("Failed to fix a fixable state, might be a mistake in the inspection");
                    break;
                case InspectionResult.Status.NonFixable:
                    Debug.LogWarning(
                        $"Non-fixable inspection state with message '{result.errorMessage}' discovered, please fix manually"
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract InspectionResult InspectAndFix(bool inspectOnly);
    }

}
