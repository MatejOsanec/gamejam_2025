namespace BGLib.Inspections.Editor.Core {
    using System.Collections.Generic;
    using System.Linq;

    public class Inspector {

        public readonly struct Result {

            public static readonly Result empty = new Result(0, 0, 0);

            public readonly int passedCount;
            public readonly int fixableCount;
            public readonly int nonFixableCount;

            public int failedCount => fixableCount + nonFixableCount;

            public Result(int passedCount, int fixableCount, int nonFixableCount) {
                this.passedCount = passedCount;
                this.fixableCount = fixableCount;
                this.nonFixableCount = nonFixableCount;
            }
        }

        private readonly IInspectionGroup[] _inspectionGroups;

        public Inspector(params IInspectionGroup[] inspectionGroups) {
            _inspectionGroups = inspectionGroups;
        }

        public IEnumerable<IInspectionGroup> groups => _inspectionGroups;

        public int Count() {

            return _inspectionGroups.Select(inspectionGroup => inspectionGroup.inspections.Count()).Sum();
        }

        public Result Inspect() {

            var passed = 0;
            var fixable = 0;
            var nonFixable = 0;

            foreach (var inspectionGroup in _inspectionGroups) {
                foreach (var inspection in inspectionGroup.inspections) {
                    var result = inspection.Inspect();
                    switch (result.status) {
                        case InspectionResult.Status.Ok:
                            passed++;
                            break;
                        case InspectionResult.Status.Fixable:
                            fixable++;
                            break;
                        default:
                            nonFixable++;
                            break;
                    }
                }
            }

            return new Result(passed, fixable, nonFixable);
        }

        public void Fix() {

            foreach (var inspectionGroup in _inspectionGroups) {
                foreach (var inspection in inspectionGroup.inspections) {
                    inspection.Fix();
                }
            }
        }
    }
}
