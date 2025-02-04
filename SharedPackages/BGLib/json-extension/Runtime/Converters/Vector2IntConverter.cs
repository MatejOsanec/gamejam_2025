namespace BGLib.JsonExtension {

    using System;
    using Newtonsoft.Json;
    using UnityEngine;

    public class Vector2IntConverter : JsonConverter<Vector2Int> {

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer) {

            var t = serializer.Deserialize(reader);
            return JsonConvert.DeserializeObject<Vector2Int>(t!.ToString());
        }

        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer) {

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }
}
