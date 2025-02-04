namespace BGLib.DotnetExtension {

    using System;
    using System.IO;

    public static class EnvironmentVariableHelper {

        /// <summary>
        /// Return a path stored in an environmental variable
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns>the path stored in the variable and null if it's empty</returns>
        /// <exception cref="DirectoryNotFoundException">Throws when the content of the variable is not a path</exception>
        public static string? GetDirectoryPath(string variableName) {

            var path = Environment.GetEnvironmentVariable(variableName);

            if (string.IsNullOrWhiteSpace(path)) {
                return null;
            }

            if (!Directory.Exists(path)) {
                throw new DirectoryNotFoundException(
                    $"{variableName} environment variable points to an nonexistent directory: {path}"
                );
            }

            return path;
        }

        public static void SetDirectoryPath(string variableName, string path) {

            if (string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException($"Provide a valid path for '{variableName}'", nameof(path));
            }
            if (!Directory.Exists(path)) {
                throw new DirectoryNotFoundException($"Provide a valid directory path '{path}' is not valid");
            }
            Environment.SetEnvironmentVariable(variableName, path);
        }
    }
}
