using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    public class SnsMessageConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type _) => true;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeof(SnsMessageConverter<>).MakeGenericType(new[] { typeToConvert });
            return (JsonConverter)Activator.CreateInstance(type, Array.Empty<object>())!;
        }
    }
}
