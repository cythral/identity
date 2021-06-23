using System;

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Brighid.Identity
{
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to use HTTPS.
        /// </summary>
        public bool UseHttps { get; set; } = true;

        /// <summary>
        /// Gets or sets the HTTP Protocols to use.
        /// </summary>
        public HttpProtocols Protocols { get; set; } = HttpProtocols.Http2;

        /// <summary>
        /// Gets or sets the port to use for the adapter's HTTP interface.
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// Gets or sets the wait condition handle to post the iac management application to.
        /// </summary>
        public Uri? WaitConditionHandle { get; set; } = null;
    }
}
