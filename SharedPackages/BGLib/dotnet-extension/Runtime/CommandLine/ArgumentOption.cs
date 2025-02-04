namespace BGLib.DotnetExtension.CommandLine {
    using System;

    public enum ArgumentType {
        Boolean,
        String,
        StringOptional,
        Integer,
        IntegerOptional
    }

    public readonly struct ArgumentOption {

        public readonly string name;
        public readonly string[] identifiers;
        public readonly string hint;
        public readonly ArgumentType type;

        public ArgumentOption(string name, string hint, ArgumentType type, params string[] identifiers) {

            this.name = name;
            this.identifiers = identifiers;
            this.hint = hint;
            this.type = type;
        }

        public bool required => type is ArgumentType.Integer or ArgumentType.String;
        public bool expectsValue => type is ArgumentType.Integer or ArgumentType.IntegerOptional or ArgumentType.String
            or ArgumentType.StringOptional;

        public void ValidateArgumentValue(string value) {

            switch (type) {
                case ArgumentType.String:
                    if (string.IsNullOrEmpty(value)) {
                        throw new ArgumentException(
                            $"Option '{name}' is string, but no value was provided in the command line",
                            nameof(value)
                        );
                    }
                    break;
                case ArgumentType.StringOptional:
                    break;
                case ArgumentType.Integer:
                case ArgumentType.IntegerOptional:
                    if (string.IsNullOrEmpty(value)) {
                        throw new ArgumentException(
                            $"Option '{name}' is int, but no value was provided in the command line",
                            nameof(value)
                        );
                    }
                    if (!int.TryParse(value, out int _)) {
                        throw new ArgumentException(
                            $"Option '{name}' is int, but the value provided '{value}' in the command line is not a number",
                            nameof(value)
                        );
                    }
                    break;
                case ArgumentType.Boolean:
                    throw new ArgumentException(
                        $"Option '{name}' is boolean, it should not have value.",
                        nameof(value)
                    );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
