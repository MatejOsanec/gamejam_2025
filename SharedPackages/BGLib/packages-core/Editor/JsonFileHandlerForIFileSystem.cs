namespace BGLib.PackagesCore.Editor {

    using System;
    using System.IO;
    using System.IO.Abstractions;
    using JsonExtension;
    using Newtonsoft.Json;

    public static class JsonFileHandlerForIFileSystem {

        public static void WriteIndentedWithDefault<T>(
            T content,
            IFileSystem _fileSystem,
            string filePath,
            int indentation = 4
        ) {

            WriteToFile(
                content,
                _fileSystem,
                filePath,
                JsonSettings.readableWithDefault,
                writer => {
                    writer.IndentChar = ' ';
                    writer.Indentation = indentation;
                }
            );
        }

        public static void WriteCompactWithoutDefault<T>(T content, IFileSystem fileSystem, string filePath) {

            WriteToFile(content, fileSystem, filePath, JsonSettings.compactNoDefault);
        }

        public static void WriteToFile<T>(
            T content,
            IFileSystem _fileSystem,
            string filePath,
            JsonSerializerSettings settings,
            Action<JsonTextWriter>? beforeSerialize = null
        ) {

            using FileSystemStream fileStream = _fileSystem.File.Open(filePath, FileMode.OpenOrCreate);
            fileStream.SetLength(0);
            using StreamWriter streamWriter = new StreamWriter(fileStream);
            JsonFileHandler.WriteToText(streamWriter, content, settings, beforeSerialize);
        }

        public static T ReadFromFile<T>(IFileSystem fileSystem, string filePath) {

            return ReadFromFile<T>(fileSystem, filePath, JsonSettings.readableWithDefault);
        }

        public static T ReadFromFile<T>(IFileSystem fileSystem, string filePath, JsonSerializerSettings settings) {

            using FileSystemStream fileStream = fileSystem.File.Open(filePath, FileMode.Open);
            using StreamReader streamReader = new StreamReader(fileStream);
            return JsonFileHandler.ReadFromText<T>(streamReader, settings);
        }
    }
}
