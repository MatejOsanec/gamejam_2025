namespace BGLib.BuildProcess.Editor {

    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public abstract class BaseLeafBuildStage : IBuildStage {

        protected readonly string name;
        private BuildProcessRunner? _runner;
        protected BuildProcessRunner runner {
            get {
                if (_runner == null) {
                    throw new InvalidOperationException("You must get runner inside the Execute method.");
                }
                return _runner;
            }
        }
        private readonly float _estimatedDurationInSeconds;

        public float estimatedDurationInSeconds => _estimatedDurationInSeconds;
        public float startNormalizedProgress { get; private set; }

        protected T GetProcessState<T>() where T : class {

            var processState = runner.processState;
            var result = processState is JObject jObject ? jObject.ToObject<T>() : processState as T;
            if (result == null) {
                throw new InvalidOperationException(
                    $"Provided state could not be casted to '{typeof(T)}' on type '{GetType()}'"
                );
            }
            return result;
        }

        protected BaseLeafBuildStage(string name, float estimatedDurationInSeconds) {

            this.name = name;
            _estimatedDurationInSeconds = estimatedDurationInSeconds;
            _runner = null;
            startNormalizedProgress = 0;
        }

        private void ReportEvaluation() {

            runner.OnStageReport(
                new BuildStageProgressReport(
                    startNormalizedProgress,
                    $"{name}:{runner.handler}"
                )
            );
        }

        public BuildStageResult Continue(
            IList<int> stagesStep,
            BuildProcessRunner processRunner
        ) {

            if (stagesStep.Count != 0) {
                throw new ArgumentException("Stages list should be empty", nameof(stagesStep));
            }
            _runner = processRunner;

            try {
                ReportEvaluation();
                var waiter = Execute();
                return new BuildStageResult(completed: true, waiter);
            }
            catch (BuildProcessException customBuildProcessException) {
                processRunner.OnError(customBuildProcessException);
                return new BuildStageResult(completed: false, IBuildStage.dontWait);
            }
            catch (Exception exception) {
                BuildProcessException newException = new BuildProcessException(
                    name,
                    "There was an unhandled exception on the build stage",
                    exception
                );
                processRunner.OnError(newException);
                return new BuildStageResult(completed: false, IBuildStage.dontWait);
            }
        }

        public void UpdateNormalizedProgress(float start, float end) {

            AssertExtensions.GreaterOrEqual(end, start);
            startNormalizedProgress = start;
        }

        /// <summary>
        /// Step work to be implemented
        /// </summary>
        /// <param name="runner">Runner calling this step</param>
        /// <returns>Returns true if the runner has to wait for the Editor to fully refresh.</returns>
        protected abstract IBuildStageWaiter? Execute();
    }
}
