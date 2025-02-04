namespace BGLib.BuildProcess.Editor {
    using UnityEditor.Build.Reporting;

    public class BuildProcessResult {

        public readonly BuildReport buildReport;

        public BuildProcessResult(BuildReport buildReport) {

            this.buildReport = buildReport;
        }
    }
}
