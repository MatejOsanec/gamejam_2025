namespace BGLib.BuildProcess.Editor {

    public interface IBuildProcessHandler {
        void OnStageReport(BuildStageProgressReport report);
        void OnProcessError(BuildProcessException exception);
        void OnProcessComplete(BuildProcessRunner runner);
        void OnCompleteBuildProcess(BuildProcessResult buildResult);
    }
}
