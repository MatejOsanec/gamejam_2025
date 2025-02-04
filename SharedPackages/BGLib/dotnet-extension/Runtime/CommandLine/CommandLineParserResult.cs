namespace BGLib.DotnetExtension.CommandLine {

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public readonly struct CommandLineParserResult {

        public readonly string? applicationPath;
        private readonly IReadOnlyDictionary<ArgumentOption, string> _parsed;
        public readonly IReadOnlyList<string> unexpectedArguments;

        public CommandLineParserResult(
            string? applicationPath,
            IReadOnlyDictionary<ArgumentOption, string> parsed,
            IReadOnlyList<string> unexpectedArguments
        ) {

            this.applicationPath = applicationPath;
            _parsed = parsed;
            this.unexpectedArguments = unexpectedArguments;
        }

        public string this[ArgumentOption option] => _parsed[option];

        public string this[string identifier] {
            get {

                foreach (var keyValuePair in _parsed) {
                    if (keyValuePair.Key.identifiers.Any(optionIdentifier => optionIdentifier == identifier)) {
                        return keyValuePair.Value;
                    }
                }
                throw new KeyNotFoundException($"Could not find option name {identifier}");
            }
        }

        public string GetValueOrDefault(ArgumentOption option) => _parsed.GetValueOrDefault(option);

        public bool Contains(ArgumentOption option) {

            return _parsed.ContainsKey(option);
        }

        public bool Contains(string identifier) {

            return _parsed.Any(keyValuePair => keyValuePair.Key.identifiers.Contains(identifier));
        }

        public override string ToString() {

            var sb = new StringBuilder();
            sb.AppendLine($"Application Path: {applicationPath}");
            sb.AppendLine("Argument options:");
            foreach (var parsedOption in _parsed) {
                sb.AppendLine($"'{parsedOption.Key.name}': '{parsedOption.Value}'");
            }
            sb.AppendLine("Unexpected arguments:");
            foreach (var unexpectedArgument in unexpectedArguments) {
                sb.AppendLine(unexpectedArgument);
            }
            return sb.ToString();
        }

    }
}
