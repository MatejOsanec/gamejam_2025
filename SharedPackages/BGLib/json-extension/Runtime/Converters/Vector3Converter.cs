namespace BGLib.JsonExtension {

    using System;
    using Newtonsoft.Json;
    using UnityEngine;

    public class Vector3Converter : JsonConverter<Vector3> {

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {

            var t = serializer.Deserialize(reader);
            return JsonConvert.DeserializeObject<Vector3>(t!.ToString());
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }
    }
}
