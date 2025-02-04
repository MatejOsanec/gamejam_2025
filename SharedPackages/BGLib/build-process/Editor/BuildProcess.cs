namespace BGLib.BuildProcess.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class BuildProcess : IBuildStage {

        public readonly string fullName;
        public readonly string name;

        internal readonly IReadOnlyList<IBuildStage> stages;
        private readonly BuildProcessSuccessCondition _successCondition;

        private static readonly IBuildStageWaiter? _dontWait = null;
        private readonly IBuildStageWaiter? _defaultWaiter;

        public float startNormalizedProgress { get; private set; }
        public float endNormalizedProgress { get; private set; }

        public BuildProcessSuccessCondition successCondition => _successCondition;

        protected BuildProcess(
            IBuildStage[] stages,
            BuildProcessSuccessCondition successCondition,
            IBuildStageWaiter defaultWaiter
        ) {

            this.stages = stages;
            _successCondition = successCondition;
            fullName = GetType().FullName ??
                       throw new InvalidOperationException("Could not find type full name for Build Process");
            name = GetType().Name;
            _defaultWaiter = defaultWaiter;
            estimatedDurationInSeconds = stages.Sum(stage => stage.estimatedDurationInSeconds);
            UpdateNormalizedProgress(0, 1);
        }

        protected BuildProcess(IBuildStage[] stages, BuildProcessSuccessCondition successCondition) : this(
            stages,
            successCondition,
            EditorBuildStageWaiter.shared
        ) { }

        public float estimatedDurationInSeconds { get; }

        public BuildStageResult Continue(IList<int> stagesStep, BuildProcessRunner runner) {

            List<int> childStageSteps = new List<int>(stagesStep.Count == 0 ? 0 : stagesStep.Count - 1);
            int index;
            if (stagesStep.Count == 0) {
                index = 0;
            }
            else {
                index = stagesStep[0];
                for (int i = 1; i < stagesStep.Count; i++) {
                    childStageSteps.Add(stagesStep[i]);
                }
            }
            AssertExtensions.LessThan(index, stages.Count);
            for (; index < stages.Count; index++) {
                var lastResult = stages[index].Continue(childStageSteps, runner);
                if (!lastResult.completed) {
                    return new BuildStageResult(completed: false, _defaultWaiter, index, lastResult.stageStep);
                }
                if (lastResult.waiter == null) {
                    childStageSteps.Clear();
                    continue;
                }
                int nextIndex = index + 1;
                return nextIndex >= stages.Count
                    ? new BuildStageResult(completed: true, lastResult.waiter)
                    : new BuildStageResult(completed: false, lastResult.waiter, nextIndex, lastResult.stageStep);
            }
            return new BuildStageResult(completed: true, _dontWait);
        }

        public void UpdateNormalizedProgress(float start, float end) {

            startNormalizedProgress = start;
            endNormalizedProgress = end;
            float counter = start;
            float range = end - start;
            float nextCounter = counter;
            foreach (var stage in stages) {
                nextCounter += stage.estimatedDurationInSeconds * range / estimatedDurationInSeconds;
                stage.UpdateNormalizedProgress(counter, nextCounter);
                counter = nextCounter;
            }
        }
    }
}
