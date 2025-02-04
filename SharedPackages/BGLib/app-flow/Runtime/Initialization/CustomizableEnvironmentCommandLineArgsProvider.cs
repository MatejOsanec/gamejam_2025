namespace BGLib.AppFlow.Initialization {

    using System;
    using DotnetExtension.CommandLine;

    public static class CustomizableEnvironmentCommandLineArgsProvider {

#if UNITY_EDITOR
        [DoesNotRequireDomainReloadInit]
        internal static Func<string[]> injectArgumentsResult;
#endif

        public static string[] GetCommandLineArgs() {

#if UNITY_EDITOR
            return injectArgumentsResult != null ? injectArgumentsResult() : CommandLineParser.GetCommandLineArgs();
#else
            return CommandLineParser.GetCommandLineArgs();
#endif
        }
    }
}
