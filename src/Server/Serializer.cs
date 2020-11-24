using System.IO;
using System.Text.Json;

using Flurl.Http.Configuration;

namespace Brighid.Identity
{
    public class Serializer : ISerializer
    {
        private readonly JsonSerializerOptions options;

        public Serializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public T? Deserialize<T>(string @string)
        {
            return JsonSerializer.Deserialize<T>(@string, options);
        }

#pragma warning disable CA2012
        public T? Deserialize<T>(Stream stream)
        {
            return JsonSerializer.DeserializeAsync<T>(stream, options).Result;
        }
#pragma warning restore CA2012

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
