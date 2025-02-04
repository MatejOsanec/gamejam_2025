namespace BGLib.JsonExtension {

    using System;
    using System.IO;
    using Newtonsoft.Json;

    public static class JsonFileHandler {

        public static void WriteIndentedWithDefault<T>(
            T content,
            string filePath,
            int indentation = 4
        ) {

            WriteToFile(
                content,
                filePath,
                JsonSettings.readableWithDefault,
                writer => {
                    writer.IndentChar = ' ';
                    writer.Indentation = indentation;
                }
            );
        }

        public static void WriteCompactWithoutDefault<T>(T content, string filePath) {

            WriteToFile(content, filePath, JsonSettings.compactNoDefault);
        }


        public static void WriteToFile<T>(
            T content,
            string filePath,
            JsonSerializerSettings settings,
            Action<JsonTextWriter>? beforeSerialize = null
        ) {

            using FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            fileStream.SetLength(0);
            using StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.NewLine = "\n";
            WriteToText(streamWriter, content, settings, beforeSerialize);
        }

        public static void WriteToText<T>(
            TextWriter writer,
            T content,
            JsonSerializerSettings settings,
            Action<JsonTextWriter>? beforeSerialize = null
        ) {

            var jsonSerializer = JsonSerializer.CreateDefault(settings);
            using JsonTextWriter jsonTextWriter = new JsonTextWriter(writer);
            jsonTextWriter.Formatting = settings.Formatting;
            beforeSerialize?.Invoke(jsonTextWriter);
            jsonSerializer.Serialize(jsonTextWriter, content, typeof(T));
        }

        public static T ReadFromFile<T>(string filePath) {

            return ReadFromFile<T>(filePath, JsonSettings.readableWithDefault);
        }

        public static T ReadFromFile<T>(string filePath, JsonSerializerSettings settings) {

            using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader streamReader = new StreamReader(fileStream);
            return ReadFromText<T>(streamReader, settings);
        }

        public static T ReadFromText<T>(TextReader textReader, JsonSerializerSettings settings) {

            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
            jsonSerializer.CheckAdditionalContent = true;
            using JsonTextReader jsonTextReader = new JsonTextReader(textReader);
            object? deserialize = jsonSerializer.Deserialize(jsonTextReader, typeof(T));
            return deserialize switch {
                null => throw new JsonReaderException("Json Serializer return a null reference."),
                T result => result,
                _ => throw new JsonReaderException($"Could not convert object {deserialize} to type {typeof(T)}")
            };
        }
    }
}
