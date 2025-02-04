namespace BGLib.PackagesCore.Editor {

    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.Fields)]
    public record AssemblyDefinitionReferenceFile {

        public readonly string? reference;

        [JsonConstructor]
        public AssemblyDefinitionReferenceFile(string? reference) {
            this.reference = reference;
        }
    }

}
