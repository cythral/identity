using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0060

namespace Brighid.Identity.Users
{
    public class UserFlagsConverter : JsonConverter<UserFlags>
    {
        public override UserFlags Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var number = reader.GetInt64();
            return (UserFlags)number;
        }

        public override void Write(Utf8JsonWriter writer, UserFlags value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((long)value);
        }
    }
}
