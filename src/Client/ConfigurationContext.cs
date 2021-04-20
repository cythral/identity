using System;
namespace Brighid.Identity.Client
{
    internal class ConfigurationContext
    {
        public string BaseAddress { get; set; } = "";
        public string ConfigSectionName { get; set; } = "";
        public Uri IdentityServerUri { get; set; } = new Uri("https://identity.brigh.id/");
    }
}
