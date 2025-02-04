namespace BGLib.Inspections.Editor.Core {
    using System;

    public readonly struct InspectionResult {

        public enum Status {
            Ok,
            Fixable,
            NonFixable
        }

        public static readonly InspectionResult Ok = new InspectionResult(Status.Ok, string.Empty);

        public readonly Status status;
        public readonly string errorMessage;

        public bool isOk => status == Status.Ok;

        private InspectionResult(Status status, string errorMessage) {

            this.status = status;
            this.errorMessage = errorMessage;
        }

        public static InspectionResult Fixable(string errorMessage) {

            return new InspectionResult(Status.Fixable, errorMessage);
        }

        public static InspectionResult NonFixable(string errorMessage) {

            return new InspectionResult(Status.NonFixable, errorMessage);
        }

        public override string ToString() {

            return status switch {
                Status.Ok => "OK",
                Status.Fixable => "Fixable: " + errorMessage,
                Status.NonFixable => "NonFixable: " + errorMessage,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
