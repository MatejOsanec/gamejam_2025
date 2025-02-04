namespace BGLib.BuildProcess.Editor {
    using System.Collections.Generic;

    public record BuildProcessSessionState {

        public readonly string handlerFullName;
        public readonly string processFullName;
        public readonly List<int> steps;
        public object processState;
        public int stepErrorCount;

        public BuildProcessSessionState(
            string handlerFullName,
            string processFullName,
            List<int> steps,
            object processState,
            int stepErrorCount = 0
        ) {
            this.handlerFullName = handlerFullName;
            this.processFullName = processFullName;
            this.steps = steps;
            this.processState = processState;
            this.stepErrorCount = stepErrorCount;
        }
    }
}
