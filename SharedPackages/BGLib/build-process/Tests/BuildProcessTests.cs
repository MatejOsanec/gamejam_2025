using System;
using System.Collections;
using BGLib.BuildProcess.Editor;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

public class CustomBuildProcessTest {

    private class TestSuccessfulProcess : BuildProcess {

        public TestSuccessfulProcess() : base(
            new IBuildStage[] {
                new DoNothingAndWaitStep("A1"),
                new DoNothingAndNotWaitStep("A2"),
                new InnerTestProcess(),
                new DoNothingAndNotWaitStep("A3"),
            }, BuildProcessSuccessCondition.NoFailures, new SimpleWaiter()
        ) { }
    }

    private class TestFailedProcess : BuildProcess {

        public TestFailedProcess() : base(
            new IBuildStage[] {
                new DoNothingAndWaitStep("C1"),
                new DoNothingAndNotWaitStep("C2"),
                new TestSuccessfulProcess(),
                new DoNothingAndNotWaitStep("C3"),
                new ThrowExceptionStep("C3"),
                new DoNothingAndWaitStep("C4"),
                new InnerTestProcess()
            },  BuildProcessSuccessCondition.NoFailures, new SimpleWaiter()
        ) { }
    }

    private class InnerTestProcess : BuildProcess {

        public InnerTestProcess() : base(
            new IBuildStage[] {
                new DoNothingAndWaitStep("B1"),
                new DoNothingAndNotWaitStep("B2")
            }, BuildProcessSuccessCondition.NoFailures, new SimpleWaiter()
        ) { }
    }

    private class SimpleWaiter : IBuildStageWaiter {

        public IEnumerator Wait() {

            yield return null;
        }
    }

    private class DoNothingAndWaitStep : BaseLeafBuildStage {

        public DoNothingAndWaitStep(string name) : base(name, estimatedDurationInSeconds: 5) { }

        protected override IBuildStageWaiter Execute() {

            return new SimpleWaiter();
        }
    }

    private class DoNothingAndNotWaitStep : BaseLeafBuildStage {

        public DoNothingAndNotWaitStep(string name) : base(name, estimatedDurationInSeconds: 4) { }

        protected override IBuildStageWaiter? Execute() {
            return IBuildStage.dontWait;
        }
    }

    private class ThrowExceptionStep : BaseLeafBuildStage {

        public ThrowExceptionStep(string name) : base(name, estimatedDurationInSeconds: 2) { }

        protected override IBuildStageWaiter Execute() {

            throw new Exception($"Exception: {name}");
        }
    }

    private class TestHandler : IBuildProcessHandler {

        public int stageReportCount;
        public int errorCount;
        public int completeProcessCount;
        public int completeBuildCount;

        public void OnStageReport(BuildStageProgressReport report) {
            stageReportCount += 1;
        }

        public void OnProcessError(BuildProcessException exception) {
            errorCount += 1;
        }

        public void OnProcessComplete(BuildProcessRunner runner) {
            completeProcessCount += 1;
        }

        public void OnCompleteBuildProcess(BuildProcessResult buildResult) {
            completeBuildCount += 1;
        }
    }

    [UnityTest]
    public IEnumerator SuccessfulBuildProcess_ReportsSuccess() {

        var runner = BuildProcessRunner.CreateFromStartState<TestSuccessfulProcess, TestHandler>(new object());
        var testHandler = (TestHandler) runner.handler;

        yield return runner.Start();

        Assert.IsFalse(runner.isFailed);
        Assert.IsFalse(runner.hasBuildSucceeded);
        Assert.AreEqual(5, testHandler.stageReportCount);
        Assert.AreEqual(0, testHandler.errorCount);
        Assert.AreEqual(0, testHandler.completeBuildCount);
        Assert.AreEqual(1, testHandler.completeProcessCount);
    }

    [UnityTest]
    public IEnumerator FailedBuildProcess_ReportsFailure() {


        var runner = BuildProcessRunner.CreateFromStartState<TestFailedProcess, TestHandler>(new object());
        var testHandler = (TestHandler) runner.handler;
        yield return runner.Start();

        Assert.IsTrue(runner.isFailed);
        Assert.IsFalse(runner.hasBuildSucceeded);
        Assert.AreEqual(9, testHandler.stageReportCount);
        Assert.AreEqual(1, testHandler.errorCount);
        Assert.AreEqual(0, testHandler.completeBuildCount);
        Assert.AreEqual(0, testHandler.completeProcessCount);
    }

    [Test]
    public void BuildProcessRunner_RestoresFromPersistentState() {


        var runner = BuildProcessRunner.CreateFromStartState<TestSuccessfulProcess, TestHandler>(new object());
        var _ = runner.Start();
        runner.PersistProcessStage();

        var restoredRunner = BuildProcessRunner.RestoreFromPersistentState();
        Assert.IsNotNull(restoredRunner);
        Assert.IsTrue(runner.process is TestSuccessfulProcess);
        Assert.IsTrue(runner.handler is TestHandler);
    }

    [Test]
    public void StartAndEndNormalizedProgress_AreCalculatedProperly() {

        var process = new TestSuccessfulProcess();
        Assert.AreEqual(0, process.startNormalizedProgress);
        Assert.AreEqual(22, process.estimatedDurationInSeconds);
        Assert.AreEqual(0, process.stages[0].startNormalizedProgress);
        Assert.AreEqual(5, process.stages[0].estimatedDurationInSeconds);
        Assert.AreEqual(0.227272734f, process.stages[1].startNormalizedProgress);
        Assert.AreEqual(4, process.stages[1].estimatedDurationInSeconds);

        var innerProcess = process.stages[2] as BuildProcess;
        NullableAssert.IsNotNull(innerProcess);
        Assert.AreEqual(0.409090906f, innerProcess.startNormalizedProgress);
        Assert.AreEqual(9, innerProcess.estimatedDurationInSeconds);
        Assert.AreEqual(0.409090906f, innerProcess.stages[0].startNormalizedProgress);
        Assert.AreEqual(5, innerProcess.stages[0].estimatedDurationInSeconds);
        Assert.AreEqual(0.636363626f, innerProcess.stages[1].startNormalizedProgress);
        Assert.AreEqual(4, innerProcess.stages[1].estimatedDurationInSeconds);
        Assert.AreEqual(0.818181813f, innerProcess.endNormalizedProgress);

        Assert.AreEqual(0.818181813f, process.stages[3].startNormalizedProgress);
        Assert.AreEqual(4, process.stages[3].estimatedDurationInSeconds);
        Assert.AreEqual(1, process.endNormalizedProgress);
    }
}
