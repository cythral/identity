using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Abstractions;

using ValidateTokenRequestContext = OpenIddict.Server.OpenIddictServerEvents.ValidateTokenRequestContext;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthService
    {
        /// <summary>
        /// Performs a client credentials exchange for a token.
        /// </summary>
        /// <param name="request">The OpenIddict request containing the client credentials.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting ticket, which will contain tokens.</returns>
        Task<AuthenticationTicket> ClientExchange(OpenIddictRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs an impersonate exchange, which exchanges a client application's token for a user token.
        /// </summary>
        /// <param name="request">The OpenIddict request containing the client token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting ticket, which will contain the user token.</returns>
        Task<AuthenticationTicket> ImpersonateExchange(OpenIddictRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a email/password exchange for a user token.
        /// </summary>
        /// <param name="email">The email of the user to get a token for.</param>
        /// <param name="password">The password of the user to get a token for.</param>
        /// <param name="redirectUri">The URI to redirect to after obtaining a token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting ticket, which will contain the user token.</returns>
        Task<AuthenticationTicket> PasswordExchange(string email, string password, Uri redirectUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extract the User Principal/Identity from the given <paramref name="context" />.
        /// </summary>
        /// <param name="context">The context to extract the user principal from.</param>
        /// <returns>The resulting User Principal.</returns>
        ClaimsPrincipal ExtractPrincipalFromRequestContext(ValidateTokenRequestContext context);
    }
}
