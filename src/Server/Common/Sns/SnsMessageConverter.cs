using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    public class SnsMessageConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
            {
                return default;
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(stringValue, options);

                return result;
            }
            catch (JsonException)
            {
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var stringValue = JsonSerializer.Serialize(value, options);
            writer.WriteStringValue(stringValue);
        }
    }
}
