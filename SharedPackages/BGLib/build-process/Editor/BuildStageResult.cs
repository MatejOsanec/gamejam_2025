namespace BGLib.BuildProcess.Editor {

    using System.Collections.Generic;

    public class BuildStageResult {

        public IBuildStageWaiter? waiter { get; }
        public bool completed { get; }
        public IList<int> stageStep { get; }

        public BuildStageResult(bool completed, IBuildStageWaiter? waiter = null) {

            this.completed = completed;
            this.waiter = waiter;
            stageStep = new List<int>();
        }

        public BuildStageResult(bool completed, IBuildStageWaiter? waiter, int startIndex, IEnumerable<int> nextElements) {

            this.completed = completed;
            this.waiter = waiter;
            stageStep = new List<int>();
            stageStep.Add(startIndex);
            foreach (var element in nextElements) {
                stageStep.Add(element);
            }
        }
    }
}
