namespace BGLib.DotnetExtension.CommandLine {

    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class CommandLineParser {

        private const string kArgumentIdentifierPattern = @"^(?>\w|-|_)+$";

        /// <summary>
        /// Get command line arguments passed to the game.
        /// This is a wrapper as some platforms treat CLI arguments differently
        /// (eg Android it's all shoved into one argument for Unity :()
        /// This behaviour was not seeing in recent Android builds, so adding a custom logic for this edge case
        /// </summary>
        public static string[] GetCommandLineArgs() {

            const char kArgumentSeparator = ',';
            var environmentCommandLines = Environment.GetCommandLineArgs();
            if (environmentCommandLines.Length == 1 && environmentCommandLines[0].Contains(kArgumentSeparator)) {
                return environmentCommandLines[0].Split(kArgumentSeparator);
            }
            return environmentCommandLines;
        }

        public static CommandLineParserResult ParseEnvironmentCommandLine(params ArgumentOption[] options) {

            var commandLineArgs = GetCommandLineArgs();
            return ParseCommandLine(commandLineArgs, options);
        }

        public static CommandLineParserResult ParseCommandLine(
            IReadOnlyList<string> args,
            params ArgumentOption[] options
        ) {

            try {
                var argumentIdentifierRegex = new Regex(kArgumentIdentifierPattern, RegexOptions.Compiled);
                var optionsMap = GenerateOptionsMap(options, argumentIdentifierRegex);
                string? applicationPath = null;
                int startIndex = 0;
                if (args.Count > 0 && File.Exists(args[0])) {
                    applicationPath = args[0];
                    startIndex = 1;
                }
                var required = SelectRequiredOptions(options);
                ParseArgs(args, startIndex, required, optionsMap, out var parsedOption, out var extraOptions);
                return new CommandLineParserResult(applicationPath, parsedOption, extraOptions);
            }
            catch (Exception e) {
                throw new CommandLineParseException(GenerateHint(options), e);
            }
        }

        private static string GenerateHint(IEnumerable<ArgumentOption> options) {

            var sb = new StringBuilder();
            foreach (var option in options) {
                sb.AppendLine(
                    $"{string.Join(", ", option.identifiers)}: {option.name}. {option.hint} Type={option.type}"
                );
            }
            return sb.ToString();
        }

        private static HashSet<ArgumentOption> SelectRequiredOptions(IEnumerable<ArgumentOption> options) {

            var required = new HashSet<ArgumentOption>();
            foreach (var option in options) {
                if (option.required) {
                    required.Add(option);
                }
            }
            return required;
        }

        private static Dictionary<string, ArgumentOption> GenerateOptionsMap(
            IEnumerable<ArgumentOption> options,
            Regex argumentIdentifierRegex
        ) {

            var result = new Dictionary<string, ArgumentOption>();
            foreach (var option in options) {
                if (option.identifiers.Length == 0) {
                    throw new InvalidOperationException($"Option '{option.name}' does not have identifiers.");
                }
                foreach (var id in option.identifiers) {
                    if (string.IsNullOrWhiteSpace(id)) {
                        throw new InvalidOperationException($"Id {id} in option '{option.name}' is empty");
                    }
                    if (!argumentIdentifierRegex.IsMatch(id)) {
                        throw new InvalidOperationException(
                            $"Identifier '{id}' in option '{option.name}' does not match the argument regex {kArgumentIdentifierPattern}"
                        );
                    }
                    if (!result.TryAdd(id, option)) {
                        throw new InvalidOperationException(
                            $"Id {id} in option '{option.name}' is already used by another option"
                        );
                    }
                }
            }
            return result;
        }

        private static void ParseArgs(
            IReadOnlyList<string> args,
            int startIndex,
            IEnumerable<ArgumentOption> requiredOptions,
            IReadOnlyDictionary<string, ArgumentOption> optionsMap,
            out Dictionary<ArgumentOption, string> parsedOption,
            out List<string> ignored
        ) {

            var requiredFound = new HashSet<ArgumentOption>();
            parsedOption = new Dictionary<ArgumentOption, string>();
            ignored = new List<string>();

            ArgumentOption? lastArg = null;
            for (int i = startIndex; i < args.Count; i++) {
                var element = args[i];
                if (lastArg == null) {
                    if (!optionsMap.TryGetValue(element, out var argumentOption)) {
                        ignored.Add(element);
                        continue;
                    }
                    if (argumentOption.expectsValue) {
                        lastArg = argumentOption;
                    }
                    else {
                        parsedOption.AddParsedOption(argumentOption, string.Empty);
                    }
                }
                else {
                    var lastArgValue = lastArg.Value;

                    if (optionsMap.ContainsKey(element)) {
                        if (lastArgValue.required) {
                            throw new ArgumentException(
                                $"Required argument {lastArgValue.name} needs a value, but argument {element} comes after.", nameof(args));
                        }

                        lastArg = null;
                        i--; // Decrement to re-evaluate the element as a key instead of a value
                        continue;
                    }

                    lastArgValue.ValidateArgumentValue(element);
                    parsedOption.AddParsedOption(lastArgValue, element);
                    if (lastArgValue.required) {
                        bool wasRequiredAlreadyAdded = !requiredFound.Add(lastArgValue);
                        //TODO: Use an assertion Library
                        if (wasRequiredAlreadyAdded) {
                            throw new InvalidOperationException(
                                $"Option '{lastArgValue.name}' has more than one identifier in the command line. This should not happen since the parsedOption should be unique at this point."
                            );
                        }
                    }
                    lastArg = null;
                }
            }

            if (lastArg != null && lastArg.Value.required) {
                throw new ArgumentException($"Required argument {lastArg.Value.name} needs a value, but nothing was provided.", nameof(args));
            }

            var missingRequired = requiredOptions.Where(
                option => option.type != ArgumentType.Boolean && !requiredFound.Contains(option)
            ).ToList();
            if (missingRequired.Count <= 0) {
                return;
            }
            var missingString = string.Join(", ", missingRequired.Select(option => option.name));
            throw new ArgumentException($"Missing required flags: {missingString}", nameof(args));
        }

        private static void AddParsedOption(
            this Dictionary<ArgumentOption, string> parsedOption,
            ArgumentOption option,
            string value
        ) {
            if (!parsedOption.TryAdd(option, value)) {
                throw new ArgumentException(
                    $"Option with name '{option.name}' has already being added by a different or the same identifier.",
                    nameof(option)
                );
            }
        }
    }
}
