using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    public class CloudFormationRequestTypeConverter : JsonConverter<CloudFormationRequestType>
    {
        public override CloudFormationRequestType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString()?.ToUpper(CultureInfo.InvariantCulture);
            return stringValue switch
            {
                "CREATE" => CloudFormationRequestType.Create,
                "UPDATE" => CloudFormationRequestType.Update,
                "DELETE" => CloudFormationRequestType.Delete,
                _ => throw new Exception("Invalid value given"),
            };
        }

        public override void Write(Utf8JsonWriter writer, CloudFormationRequestType value, JsonSerializerOptions options)
        {
            var stringValue = JsonSerializer.Serialize(value, options);
            writer.WriteStringValue(stringValue);
        }
    }
}
