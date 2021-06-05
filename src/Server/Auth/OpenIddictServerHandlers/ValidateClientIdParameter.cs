using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Brighid.Identity.Auth
{
    public class ValidateClientIdParameter : IOpenIddictServerHandler<ValidateTokenRequestContext>
    {
        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<ValidateTokenRequestContext>()
                .UseSingletonHandler<ValidateClientIdParameter>()
                .SetOrder(OpenIddictServerHandlers.Exchange.ValidateGrantType.Descriptor.Order + 1_000)
                .SetType(OpenIddictServerHandlerType.Custom)
                .Build();

        /// <inheritdoc/>
        public ValueTask HandleAsync(ValidateTokenRequestContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!string.IsNullOrEmpty(context.ClientId))
            {
                return default;
            }

            // Impersonate uses the existing access token instead of client credentials.
            if (context.Request.GrantType == Constants.GrantTypes.Impersonate)
            {
                return default;
            }

            if (!context.Options.AcceptAnonymousClients || context.Request.IsAuthorizationCodeGrantType())
            {
                context.Logger.LogInformation(SR.GetResourceString(SR.ID6077), Parameters.ClientId);

                context.Reject(
                    error: Errors.InvalidClient,
                    description: SR.FormatID2029(Parameters.ClientId),
                    uri: SR.FormatID8000(SR.ID2029));

                return default;
            }

            return default;
        }
    }
}
