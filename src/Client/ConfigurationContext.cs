using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Client
{
    internal class ConfigurationContext
    {
        public string BaseAddress { get; set; } = "";
        public string ConfigSectionName { get; set; } = "";
    }
}
