namespace Brighid.Identity.Auth
{
    public class AuthConfig
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
        /// Gets or sets the directory where to find signing certificates.
        /// </summary>
        public string CertificatesDirectory { get; set; } = "/certs";

        /// <summary>
        /// Gets or sets the endpoint to use for oauth2 authorizations.
        /// </summary>
        public string AuthorizationEndpoint { get; set; } = "/oauth2/authorize";

        /// <summary>
        /// Gets or sets the endpoint to use for logouts.
        /// </summary>
        public string LogoutEndpoint { get; set; } = "/oauth2/logout";

        /// <summary>
        /// Gets or sets the endpoint to use for token exchanges.
        /// </summary>
        public string TokenEndpoint { get; set; } = "/oauth2/token";

        /// <summary>
        /// Gets or sets the endpoint to use for obtaining user info.
        /// </summary>
        public string UserInfoEndpoint { get; set; } = "/oauth2/userinfo";

        /// <summary>
        /// Gets or sets the domain name to use.
        /// </summary>
        public string DomainName { get; set; } = string.Empty;
    }
}
