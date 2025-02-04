namespace BGLib.JsonExtension {

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class JsonSettings {

        [DoesNotRequireDomainReloadInit]
        public static JsonConverter[] jsonConverters = new JsonConverter[] {
            new ColorConverter(),
            new Vector2Converter(),
            new Vector2IntConverter(),
            new Vector3Converter()
        };

        [DoesNotRequireDomainReloadInit]
        public static readonly JsonSerializerSettings compactNoDefault = new JsonSerializerSettings {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = jsonConverters,
            ContractResolver = new DefaultContractResolver {
                IgnoreSerializableAttribute = false
            }
        };

        [DoesNotRequireDomainReloadInit]
        public static readonly JsonSerializerSettings compactWithDefault = new JsonSerializerSettings {
            DefaultValueHandling = DefaultValueHandling.Populate,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = jsonConverters,
            ContractResolver = new DefaultContractResolver {
                IgnoreSerializableAttribute = false,
            }
        };

        [DoesNotRequireDomainReloadInit]
        public static readonly JsonSerializerSettings readableWithDefault = new JsonSerializerSettings {
            DefaultValueHandling = DefaultValueHandling.Populate,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = jsonConverters,
            ContractResolver = new DefaultContractResolver {
                IgnoreSerializableAttribute = false
            }
        };
    }
}
