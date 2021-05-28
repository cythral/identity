using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Brighid.Identity.Auth
{
    public class IdentityCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        public IdentityCookieAuthenticationEvents()
        {
            OnRedirectToAccessDenied = OnRedirectToAccessDeniedHandler;
            OnRedirectToLogin = OnRedirectToLoginHandler;
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
    }
}
