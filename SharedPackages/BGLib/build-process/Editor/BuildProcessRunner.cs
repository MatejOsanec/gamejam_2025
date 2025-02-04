namespace BGLib.BuildProcess.Editor {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using JsonExtension;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Assert = UnityEngine.Assertions.Assert;

    public class BuildProcessRunner : IVerboseLogger {

        private const string kStateFilePath = ".buildProcessState.json";

        public static bool isRunning { get; private set; }

        public bool hasBuildSucceeded {
            get => SessionState.GetBool($"{process}_{handler}_BUILD_SUCCEEDED", false);
            private set => SessionState.SetBool($"{process}_{handler}_BUILD_SUCCEEDED", value);
        }

        public readonly BuildProcess process;
        public readonly IBuildProcessHandler handler;
        private readonly BuildProcessSessionState _state;

        public object processState {
            get => _state.processState;
            set => _state.processState = value;
        }

        public bool isFailed { get; private set; }

        public string loggerPrefix => process.name;

        private BuildProcessRunner(
            BuildProcess process,
            IBuildProcessHandler handler,
            List<int> steps,
            object processState,
            int stepErrorCount = 0
        ) {

            this.handler = handler;
            var handlerName = handler.GetType().AssemblyQualifiedName;
            if (handlerName == null) {
                throw new InvalidOperationException(
                    "Could not start process because it the handler does not have an AssemblyQualifiedName"
                );
            }

            this.process = process;
            var processName = process.GetType().AssemblyQualifiedName;
            if (processName == null) {
                throw new InvalidOperationException(
                    "Could not start process because it does not have an AssemblyQualifiedName"
                );
            }
            _state = new BuildProcessSessionState(handlerName, processName, steps, processState, stepErrorCount);
            isFailed = false;
        }

        public IEnumerator Start() {

            hasBuildSucceeded = false;
            this.Log("Starting process");
            SetRunning();
            if (File.Exists(kStateFilePath)) {
                Debug.LogWarning(
                    "There was an existing process file storage in the file system. That could mean that a previous process didn't finish correctly. Deleting the file to start a new process"
                );
                File.Delete(kStateFilePath);
            }
            yield return Continue();
        }

        public IEnumerator ContinueFromState() {

            if (_state.steps.Count == 1 && _state.steps[0] == 0) {
                throw new InvalidOperationException(
                    "This method is used to continue from reinitialization. It should not return to the initial step."
                );
            }
            if (_state.stepErrorCount == 1) {
                this.Log("This step failed once. If it fails again, it will cause the build process to be stopped.");
            } else if (_state.stepErrorCount > 1) {
                this.Log("Same state failed more than once, build process will be stopped.");
                yield break;
            }
            var stagesStep = _state.steps;
            if (stagesStep == null) {
                throw new InvalidOperationException("No persistent stage step found");
            }
            SetRunning();
            yield return Continue();
        }

        public IEnumerator Continue() {

            this.Log($"Continuing step: {Print(_state.steps.ToArray())}");
            BuildStageResult stageResult = process.Continue(_state.steps, this);
            if (isFailed) {
                yield break;
            }
            if (stageResult.waiter == null) {
                Assert.IsTrue(
                    stageResult.completed,
                    "At the runner callstack is expected to not wait editor only when it is completed."
                );
                OnCompleteProcess();
                yield break;
            }

            var nextStagesStep = stageResult.stageStep;
            _state.steps.Clear();
            _state.steps.AddRange(nextStagesStep);
            if (!stageResult.completed) {
                PersistProcessStage();
            }
            else {
                OnCompleteProcess();
            }

            yield return stageResult.waiter.Wait();

            if (!isFailed && !stageResult.completed) {
                yield return Continue();
            }
        }

        private static void SetRunning() {

            if (isRunning) {
                throw new InvalidOperationException("There is already one process running.");
            }
            isRunning = true;
        }

        private string Print(int[] stagesStep) {

            var log = new StringBuilder($"Process {process.fullName}, caller: {handler}, stage path: ");
            IBuildStage? stage = null;

            foreach (var stageStep in stagesStep) {
                var buildStageProcess = stage == null ? process : stage as BuildProcess;
                if (buildStageProcess != null) {
                    stage = buildStageProcess.stages[stageStep];
                }
                log.Append(stageStep);
                log.Append(" ");
            }
            log.Remove(log.Length - 1, 1);
            log.Append(", ");
            log.Append(stage == null ? "stage not found" : $"stage name: {stage.GetType().FullName}");
            return log.ToString();
        }

        internal void OnStageReport(BuildStageProgressReport stageProgressReport) {

            handler.OnStageReport(stageProgressReport);
        }

        internal void OnError(BuildProcessException error) {

            isFailed = true;
            isRunning = false;
            //TODO: Fix process so we can delete the state when it fails
            //  As we stored variables in the Session, it was not deleted after fail and starting would try to continue
            // one more time. Going from Quest to Steam causes temporary compilation issues and if we delete the state.
            //  It is not able to resume and switch the platform.
            //  This is hiding a bigger problem in the compilation order, but for now, it's the only way to make it work.
            //File.Delete(kStateFilePath);

            PersistProcessStage(didStepFailed: true);
            handler.OnProcessError(error);
        }

        public void OnCompleteBuild(BuildProcessResult report) {

            if (report.buildReport != null && report.buildReport.summary.result == BuildResult.Succeeded) {
                hasBuildSucceeded = true;
            }

            handler.OnCompleteBuildProcess(report);
        }

        private void OnCompleteProcess() {

            File.Delete(kStateFilePath);
            handler.OnProcessComplete(this);
            isRunning = false;
        }

        internal void PersistProcessStage(bool didStepFailed = false) {

            if (didStepFailed) {
                _state.stepErrorCount++;
            }
            else {
                _state.stepErrorCount = 0;
            }
            this.Log($"PersistingStage {Print(_state.steps.ToArray())} didFailed: {didStepFailed} stepErrorCount: {_state.stepErrorCount}");
            WriteSessionStateToFile(_state, kStateFilePath);
        }

        public static void WriteSessionStateToFile(BuildProcessSessionState processSessionState, string filePath) {

            JsonFileHandler.WriteIndentedWithDefault(processSessionState, filePath);
        }


        public static BuildProcessRunner CreateFromStartState<TProcessType, THandlerType>(object initialState)
            where TProcessType : BuildProcess where THandlerType : IBuildProcessHandler {

            var process = FromQualifiedName<BuildProcess>(typeof(TProcessType).AssemblyQualifiedName);
            var handler = FromQualifiedName<IBuildProcessHandler>(typeof(THandlerType).AssemblyQualifiedName);
            return new BuildProcessRunner(process, handler, new List<int>(new[] { 0 }), initialState);
        }

        public static BuildProcessRunner? RestoreFromPersistentState() {

            try {
                if (!File.Exists(kStateFilePath)) {
                    return null;
                }
                var loadedState = ReadSessionStateFromFile(kStateFilePath);
                var process = FromQualifiedName<BuildProcess>(loadedState.processFullName);
                var handler = FromQualifiedName<IBuildProcessHandler>(loadedState.handlerFullName);
                return new BuildProcessRunner(process, handler, loadedState.steps, loadedState.processState, loadedState.stepErrorCount);
            }
            catch (Exception e) {
                Debug.LogException(e);
                File.Delete(kStateFilePath);
                return null;
            }
        }

        public static BuildProcessSessionState ReadSessionStateFromFile(string filePath) {

            return  JsonFileHandler.ReadFromFile<BuildProcessSessionState>(filePath);
        }

        private static T FromQualifiedName<T>(string? fullName) where T : class {

            if (fullName == null || string.IsNullOrWhiteSpace(fullName)) {
                throw new ArgumentNullException(nameof(fullName), "Null or empty QualifiedName was provided.");
            }
            Type? type = Type.GetType(fullName);
            if (type == null) {
                throw new ArgumentException($"Type with qualified name '{fullName}' was not found.", nameof(fullName));
            }
            return (T)Activator.CreateInstance(type);
        }
    }
}
