namespace BGLib.DotnetExtension.CommandLine {
    using System;

    public class CommandLineParseException : Exception {

        public CommandLineParseException(string hint, Exception innerException) : base(
            $"Could not parse command line because:\n{innerException.Message}\n{hint}",
            innerException
        ) { }
    }
}
