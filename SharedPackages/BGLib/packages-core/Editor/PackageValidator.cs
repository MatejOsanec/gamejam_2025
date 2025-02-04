namespace BGLib.PackagesCore.Editor {
    
    using System;

    public static class PackageValidator {

        public static void ValidateCommonFields(PackageOperationParams param) {

            ValidateCommonFields(param.name, param.description, param.type, param.author);
        }

        private static void ValidateCommonFields(string name, string description, PackageType type, string author) {

            ValidatePackageName(name);
            if (string.IsNullOrWhiteSpace(description)) {
                throw new ArgumentException("Please, provide a description to your package.", nameof(description));
            }
            if (type == PackageType.ThirdParty && string.IsNullOrWhiteSpace(author)) {
                throw new ArgumentException("Please, provide the author of the third party package.", nameof(author));
            }
        }

        public static void ValidatePackageName(string name) {

            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Please, provide a name to the package.", nameof(name));
            }
            ValidateNameChars(name);
        }

        private static void ValidateNameChars(string name) {

            foreach (char c in name) {
                if (c is >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '-') {
                    continue;
                }
                throw new ArgumentException(
                    $"Name has a invalid character '{c}'. It should only contain lowercase letter, digit, '.' or '-'",
                    nameof(name)
                );
            }
        }
    }
}
