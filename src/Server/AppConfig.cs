namespace Brighid.Identity
{
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets the domain to use for the session cookie.
        /// </summary>
        public string CookieDomain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the session cookie.
        /// </summary>
        public string CookieName { get; set; } = ".Brighid.AccessToken";

        /// <summary>
        /// Gets or sets the redirect uri parameter.
        /// </summary>
        public string RedirectUriParameter { get; set; } = "redirect_uri";

        /// <summary>
        /// Gets or sets a value indicating whether or not to use HTTPS.
        /// </summary>
        public bool UseHttps { get; set; } = true;
    }
}
