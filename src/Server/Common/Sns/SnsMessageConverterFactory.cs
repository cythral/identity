using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable SA1313

namespace Brighid.Identity.Sns
{
    public class SnsMessageConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type _)
        {
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeof(SnsMessageConverter<>).MakeGenericType(new[] { typeToConvert });
            return (JsonConverter)Activator.CreateInstance(type, Array.Empty<object>())!;
        }
    }
}
