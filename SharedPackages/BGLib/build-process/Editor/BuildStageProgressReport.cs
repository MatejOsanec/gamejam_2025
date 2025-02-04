namespace BGLib.BuildProcess.Editor {

    public readonly struct BuildStageProgressReport {

        public readonly float percentage;
        public readonly string description;

        public BuildStageProgressReport(float percentage, string description) {

            this.percentage = percentage;
            this.description = description;
        }
    }
}
