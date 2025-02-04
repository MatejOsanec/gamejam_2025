namespace BGLib.Inspections.Editor.Core {

    using System;
    using System.Collections.Generic;

    public static class InspectionExtensions {

        public static InspectionResult FirstFailed(this IEnumerable<InspectionResult> inspectionResults) {

            foreach (var inspectionResult in inspectionResults) {
                if (!inspectionResult.isOk) {
                    return inspectionResult;
                }
            }

            return InspectionResult.Ok;
        }

        public static InspectionResult Combine(this IEnumerable<InspectionResult> inspectionResults) {

            var resultStatus = InspectionResult.Status.Ok;
            var message = new List<string>();

            foreach (var inspectionResult in inspectionResults) {

                if (inspectionResult.isOk) {
                    continue;
                }
                if (resultStatus == InspectionResult.Status.Ok ||
                    inspectionResult.status == InspectionResult.Status.NonFixable) {
                    resultStatus = inspectionResult.status;
                }
                message.Add(inspectionResult.errorMessage);
            }
            return resultStatus switch {
                InspectionResult.Status.Ok => InspectionResult.Ok,
                InspectionResult.Status.Fixable => InspectionResult.Fixable(string.Join(Environment.NewLine, message)),
                InspectionResult.Status.NonFixable => InspectionResult.NonFixable(
                    string.Join(Environment.NewLine, message)
                ),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
