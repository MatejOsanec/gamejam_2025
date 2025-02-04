namespace BGLib.BuildProcess.Editor {

    using System.Collections.Generic;

    public interface IBuildStage {

        protected static readonly IBuildStageWaiter? dontWait = null;
        protected static readonly IBuildStageWaiter waitForEditor = EditorBuildStageWaiter.shared;

        public float estimatedDurationInSeconds { get; }
        public float startNormalizedProgress { get; }

        BuildStageResult Continue(IList<int> stagesStep, BuildProcessRunner runner);

        void UpdateNormalizedProgress(float start, float end);
    }
}
