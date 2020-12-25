namespace Brighid.Identity
{
    public class OpenIdConfig
    {
        public string CertificatesDirectory { get; set; } = "/certs";

        public string AuthorizationEndpoint { get; set; } = "/oauth2/authorize";

        public string LogoutEndpoint { get; set; } = "/oauth2/logout";

        public string TokenEndpoint { get; set; } = "/oauth2/token";

        public string UserInfoEndpoint { get; set; } = "/oauth2/userinfo";
    }
}
