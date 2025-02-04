namespace BGLib.JsonExtension {

    using System;
    using Newtonsoft.Json;
    using UnityEngine;

    public class ColorConverter : JsonConverter<Color> {

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {

            var t = serializer.Deserialize(reader);
            return JsonConvert.DeserializeObject<Color>(t!.ToString());
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {

            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }
    }
}
