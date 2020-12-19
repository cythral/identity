using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public class ApplicationRoleConverter : JsonConverter<ApplicationRole>
    {
        public override ApplicationRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var role = new Role { Name = reader.GetString() };
            return new ApplicationRole { Role = role };
        }

        public override void Write(Utf8JsonWriter writer, ApplicationRole value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Role.Name);
        }
    }
}
