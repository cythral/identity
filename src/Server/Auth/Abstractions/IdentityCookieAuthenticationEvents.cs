using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Brighid.Identity.Auth
{
    public class IdentityCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly CookieOptions cookieOptions;

        public IdentityCookieAuthenticationEvents(AuthConfig authConfig)
        {
            OnRedirectToAccessDenied = OnRedirectToAccessDeniedHandler;
            OnRedirectToLogin = OnRedirectToLoginHandler;
            OnSigningIn = OnSigningInHandler;
            OnSignedIn = OnSignedInHandler;
            cookieOptions = new CookieOptions { Domain = authConfig.CookieDomain };
        }

        public Task OnRedirectToAccessDeniedHandler(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }

            return Task.FromResult(0);
        }

        public Task OnRedirectToLoginHandler(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }

            return Task.FromResult(0);
        }

        public Task OnSigningInHandler(CookieSigningInContext context)
        {
            var token = (from authToken in context.Properties.GetTokens()
                         where authToken.Name == "id_token"
                         select authToken.Value).First();

            context.HttpContext.Response.Cookies.Append(".Brighid.IdentityToken", token, cookieOptions);
            return Task.FromResult(0);
        }

        public Task OnSignedInHandler(CookieSignedInContext context)
        {
            if (context.Properties.RedirectUri != null)
            {
                context.Response.Redirect(context.Properties.RedirectUri);
            }

            return Task.FromResult(0);
        }
    }
}
