namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [JsonObject(MemberSerialization.Fields)]
    public readonly struct PackageManifestFile {

        public readonly struct Author {

            public readonly string name;
            public readonly string url;

            public Author(string name) : this(name, string.Empty) { }

            [JsonConstructor]
            public Author(string name, string url) {

                this.name = name;
                this.url = url;
            }
        }

        /// <summary>
        /// Allows author field to be a string with the author name or an object with name and url.
        /// Other fields could be added to the Author, but this converter is necessary to handle when the field is a string.
        /// </summary>
        private class AuthorConverter : JsonConverter {
            public override bool CanConvert(Type objectType) {

                return objectType == typeof(Author);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object? existingValue,
                JsonSerializer serializer
            ) {

                JToken token = JToken.Load(reader);
                return token.Type == JTokenType.Object
                    ? token.ToObject<Author>(serializer)
                    : new Author(token.ToString());
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {

                serializer.Serialize(writer, value);
            }
        }

        public readonly string name;
        public readonly string version;
        public readonly string displayName;
        public readonly string description;
        public readonly bool hideInEditor;
        [JsonConverter(typeof(AuthorConverter))]
        public readonly Author author;

        [JsonConstructor]
        public PackageManifestFile(string name, string version, string displayName, string description, Author author, bool? hideInEditor) {

            this.name = name;
            this.version = version;
            this.displayName = displayName;
            this.description = description;
            this.author = author;
            // Unity uses hide in editor as true when it's not defined
            this.hideInEditor = hideInEditor ?? true;
        }

        public PackageManifestFile(
            PackageId packageId,
            string description,
            string author,
            bool hideInEditor,
            string? version = null
        ) : this(
            name: packageId.GetFullName(),
            version: version ?? Constants.kPackageDefaultVersion,
            displayName: CreateDisplayNameForManifest(packageId.type, packageId.GetDisplayName()),
            description: description,
            author: new Author(CreateAuthorForManifest(packageId.type, author)),
            hideInEditor: hideInEditor
        ) { }

        private static string CreateDisplayNameForManifest(PackageType type, string displayName) {

            return type switch {
                PackageType.BeatSaber => $"_/{displayName}",
                PackageType.BGLib => $"_{Constants.kBGLibDisplayName}/{displayName}",
                _ => displayName
            };
        }

        public static string CreateAuthorForManifest(PackageType type, string author) {

            return type == PackageType.ThirdParty && !string.IsNullOrWhiteSpace(author)
                ? author
                : Constants.kAuthorName;
        }

        public string packageName {
            get {
                var nameParts = name.Split(".");
                if (nameParts.Length < 4 || nameParts[0] != Constants.kCommercialPrefix ||
                    nameParts[1] != Constants.kCompanyName || (nameParts[2] != Constants.kBeatSaberName &&
                                                               nameParts[2] != Constants.kBGLibName)) {
                    return name;
                }
                return string.Join(".", nameParts.Skip(3));
            }
        }
    }
}
