using System;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Client
{
    public class Token
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public uint ExpiresIn { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreationDate { get; } = DateTimeOffset.Now;

        [JsonIgnore]
        public bool HasExpired => DateTimeOffset.Now >= (CreationDate + TimeSpan.FromSeconds(ExpiresIn));
    }
}
