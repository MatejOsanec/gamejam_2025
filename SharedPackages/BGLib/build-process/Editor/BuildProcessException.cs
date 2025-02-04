namespace BGLib.BuildProcess.Editor {

    using System;

    public class BuildProcessException : Exception {

        public readonly string stage;

        public BuildProcessException(string stage, string message, Exception? innerException = null)
            : base(message, innerException) {

            this.stage = stage;
        }
    }
}
