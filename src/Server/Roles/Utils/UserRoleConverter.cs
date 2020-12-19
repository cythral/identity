using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Roles
{
    public class UserRoleConverter : JsonConverter<UserRole>
    {
        public override UserRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var role = new Role { Name = reader.GetString() };
            return new UserRole { Role = role };
        }

        public override void Write(Utf8JsonWriter writer, UserRole value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Role.Name);
        }
    }
}
